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
        Me.lblHypothesisType = New System.Windows.Forms.Label()
        Me.cbHypothesisType = New System.Windows.Forms.ComboBox()
        Me.tbCustom4 = New System.Windows.Forms.TextBox()
        Me.lblCustom4 = New System.Windows.Forms.Label()
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
        Me.grpInput.Controls.Add(Me.lblHypothesisType)
        Me.grpInput.Controls.Add(Me.cbHypothesisType)
        Me.grpInput.Controls.Add(Me.tbCustom4)
        Me.grpInput.Controls.Add(Me.lblCustom4)
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
        'lblHypothesisType
        '
        Me.lblHypothesisType.AutoSize = True
        Me.lblHypothesisType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHypothesisType.Location = New System.Drawing.Point(379, 81)
        Me.lblHypothesisType.Name = "lblHypothesisType"
        Me.lblHypothesisType.Size = New System.Drawing.Size(75, 16)
        Me.lblHypothesisType.TabIndex = 14
        Me.lblHypothesisType.Text = "Hypothesis"
        '
        'cbHypothesisType
        '
        Me.cbHypothesisType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHypothesisType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHypothesisType.FormattingEnabled = True
        Me.cbHypothesisType.Location = New System.Drawing.Point(379, 103)
        Me.cbHypothesisType.Name = "cbHypothesisType"
        Me.cbHypothesisType.Size = New System.Drawing.Size(141, 24)
        Me.cbHypothesisType.TabIndex = 13
        '
        'tbCustom4
        '
        Me.tbCustom4.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbCustom4.Location = New System.Drawing.Point(277, 103)
        Me.tbCustom4.Name = "tbCustom4"
        Me.tbCustom4.Size = New System.Drawing.Size(72, 22)
        Me.tbCustom4.TabIndex = 12
        '
        'lblCustom4
        '
        Me.lblCustom4.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCustom4.Location = New System.Drawing.Point(6, 100)
        Me.lblCustom4.Name = "lblCustom4"
        Me.lblCustom4.Size = New System.Drawing.Size(265, 41)
        Me.lblCustom4.TabIndex = 11
        Me.lblCustom4.TextAlign = System.Drawing.ContentAlignment.TopRight
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
        Me.lblBeta.Location = New System.Drawing.Point(412, 52)
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
        Me.spinBtnBeta.Location = New System.Drawing.Point(453, 50)
        Me.spinBtnBeta.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnBeta.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnBeta.Name = "spinBtnBeta"
        Me.spinBtnBeta.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnBeta.TabIndex = 8
        Me.spinBtnBeta.Value = New Decimal(New Integer() {20, 0, 0, 131072})
        '
        'lblAlpha
        '
        Me.lblAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlpha.Location = New System.Drawing.Point(355, 16)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(92, 30)
        Me.lblAlpha.TabIndex = 7
        Me.lblAlpha.Text = "Alpha"
        Me.lblAlpha.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(453, 16)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 6
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'tbKappa
        '
        Me.tbKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbKappa.Location = New System.Drawing.Point(277, 75)
        Me.tbKappa.Name = "tbKappa"
        Me.tbKappa.Size = New System.Drawing.Size(72, 22)
        Me.tbKappa.TabIndex = 5
        '
        'lblKappa
        '
        Me.lblKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblKappa.Location = New System.Drawing.Point(12, 72)
        Me.lblKappa.Name = "lblKappa"
        Me.lblKappa.Size = New System.Drawing.Size(259, 25)
        Me.lblKappa.TabIndex = 4
        Me.lblKappa.Text = "Ratio of control to experimental subjects"
        Me.lblKappa.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'tbSD
        '
        Me.tbSD.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbSD.Location = New System.Drawing.Point(277, 47)
        Me.tbSD.Name = "tbSD"
        Me.tbSD.Size = New System.Drawing.Size(72, 22)
        Me.tbSD.TabIndex = 3
        '
        'lblSD
        '
        Me.lblSD.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSD.Location = New System.Drawing.Point(15, 50)
        Me.lblSD.Name = "lblSD"
        Me.lblSD.Size = New System.Drawing.Size(256, 22)
        Me.lblSD.TabIndex = 2
        Me.lblSD.Text = "Standard Deviation"
        Me.lblSD.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'tbMeanDiff
        '
        Me.tbMeanDiff.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMeanDiff.Location = New System.Drawing.Point(277, 19)
        Me.tbMeanDiff.Name = "tbMeanDiff"
        Me.tbMeanDiff.Size = New System.Drawing.Size(72, 22)
        Me.tbMeanDiff.TabIndex = 1
        '
        'lblMeanDiff
        '
        Me.lblMeanDiff.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMeanDiff.Location = New System.Drawing.Point(15, 22)
        Me.lblMeanDiff.Name = "lblMeanDiff"
        Me.lblMeanDiff.Size = New System.Drawing.Size(256, 24)
        Me.lblMeanDiff.TabIndex = 0
        Me.lblMeanDiff.Text = "Mean Difference"
        Me.lblMeanDiff.TextAlign = System.Drawing.ContentAlignment.TopRight
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
    Friend WithEvents tbCustom4 As Windows.Forms.TextBox
    Friend WithEvents lblCustom4 As Windows.Forms.Label
    Friend WithEvents lblHypothesisType As Windows.Forms.Label
    Friend WithEvents cbHypothesisType As Windows.Forms.ComboBox
End Class
