<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UiTwoInputRefedits
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UiTwoInputRefedits))
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.RefEdit2 = New Excel2007RefEdit()
        Me.RefEdit1 = New Excel2007RefEdit()
        Me.lblRefedit2 = New System.Windows.Forms.Label()
        Me.lblRefedit1 = New System.Windows.Forms.Label()
        Me.TabPageOptions = New System.Windows.Forms.TabPage()
        Me.lblErrorRatio = New System.Windows.Forms.Label()
        Me.spinBtnErrorRatio = New System.Windows.Forms.NumericUpDown()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.spinBtnAlphaDeming = New System.Windows.Forms.NumericUpDown()
        Me.grpCItype = New System.Windows.Forms.GroupBox()
        Me.optAnalyticalClosedForm = New System.Windows.Forms.RadioButton()
        Me.optJackknife = New System.Windows.Forms.RadioButton()
        Me.optAnalyticalLinnet = New System.Windows.Forms.RadioButton()
        Me.ckSignTest = New System.Windows.Forms.CheckBox()
        Me.ckDescriptiveStatistics = New System.Windows.Forms.CheckBox()
        Me.TabPageOptionsHotteling = New System.Windows.Forms.TabPage()
        Me.lblAlphaOutliers = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.grpHottelingTestType = New System.Windows.Forms.GroupBox()
        Me.optIndependent = New System.Windows.Forms.RadioButton()
        Me.optPaired = New System.Windows.Forms.RadioButton()
        Me.optSingle = New System.Windows.Forms.RadioButton()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.progressBarExactCalc = New System.Windows.Forms.ProgressBar()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPageOptions.SuspendLayout()
        CType(Me.spinBtnErrorRatio, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnAlphaDeming, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpCItype.SuspendLayout()
        Me.TabPageOptionsHotteling.SuspendLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpHottelingTestType.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1)
        Me.TabMultipage.Controls.Add(Me.TabPageOptions)
        Me.TabMultipage.Controls.Add(Me.TabPageOptionsHotteling)
        Me.TabMultipage.Location = New System.Drawing.Point(6, 11)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(454, 364)
        Me.TabMultipage.TabIndex = 0
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.grpOutput)
        Me.TabPage1.Controls.Add(Me.grpInput)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(446, 335)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Input"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(2, 168)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(429, 130)
        Me.grpOutput.TabIndex = 3
        Me.grpOutput.TabStop = False
        Me.grpOutput.Text = "Output"
        '
        'RefEditOutput
        '
        Me.RefEditOutput.Address = ""
        Me.RefEditOutput.BackColor = System.Drawing.Color.Transparent
        Me.RefEditOutput.Enabled = False
        Me.RefEditOutput.ExcelConnector = Nothing
        Me.RefEditOutput.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEditOutput.ImageMinimized = CType(resources.GetObject("RefEditOutput.ImageMinimized"), System.Drawing.Image)
        Me.RefEditOutput.Location = New System.Drawing.Point(155, 16)
        Me.RefEditOutput.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEditOutput.Name = "RefEditOutput"
        Me.RefEditOutput.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEditOutput.Size = New System.Drawing.Size(260, 32)
        Me.RefEditOutput.TabIndex = 3
        '
        'optWorkbook
        '
        Me.optWorkbook.AutoSize = True
        Me.optWorkbook.Location = New System.Drawing.Point(19, 80)
        Me.optWorkbook.Name = "optWorkbook"
        Me.optWorkbook.Size = New System.Drawing.Size(121, 20)
        Me.optWorkbook.TabIndex = 2
        Me.optWorkbook.Text = "New Workbook"
        Me.optWorkbook.UseVisualStyleBackColor = True
        '
        'optWorksheet
        '
        Me.optWorksheet.AutoSize = True
        Me.optWorksheet.Checked = True
        Me.optWorksheet.Location = New System.Drawing.Point(19, 54)
        Me.optWorksheet.Name = "optWorksheet"
        Me.optWorksheet.Size = New System.Drawing.Size(123, 20)
        Me.optWorksheet.TabIndex = 1
        Me.optWorksheet.TabStop = True
        Me.optWorksheet.Text = "New Worksheet"
        Me.optWorksheet.UseVisualStyleBackColor = True
        '
        'optOutputRange
        '
        Me.optOutputRange.AutoSize = True
        Me.optOutputRange.Location = New System.Drawing.Point(20, 28)
        Me.optOutputRange.Name = "optOutputRange"
        Me.optOutputRange.Size = New System.Drawing.Size(110, 20)
        Me.optOutputRange.TabIndex = 0
        Me.optOutputRange.Text = "Output Range"
        Me.optOutputRange.UseVisualStyleBackColor = True
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.RefEdit2)
        Me.grpInput.Controls.Add(Me.RefEdit1)
        Me.grpInput.Controls.Add(Me.lblRefedit2)
        Me.grpInput.Controls.Add(Me.lblRefedit1)
        Me.grpInput.Location = New System.Drawing.Point(6, 6)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(426, 158)
        Me.grpInput.TabIndex = 2
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'RefEdit2
        '
        Me.RefEdit2.Address = ""
        Me.RefEdit2.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit2.ExcelConnector = Nothing
        Me.RefEdit2.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit2.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit2.Location = New System.Drawing.Point(136, 96)
        Me.RefEdit2.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit2.Name = "RefEdit2"
        Me.RefEdit2.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit2.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit2.TabIndex = 5
        '
        'RefEdit1
        '
        Me.RefEdit1.Address = ""
        Me.RefEdit1.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit1.ExcelConnector = Nothing
        Me.RefEdit1.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit1.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit1.Location = New System.Drawing.Point(136, 42)
        Me.RefEdit1.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit1.Name = "RefEdit1"
        Me.RefEdit1.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit1.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit1.TabIndex = 4
        '
        'lblRefedit2
        '
        Me.lblRefedit2.AutoSize = True
        Me.lblRefedit2.Location = New System.Drawing.Point(13, 96)
        Me.lblRefedit2.Name = "lblRefedit2"
        Me.lblRefedit2.Size = New System.Drawing.Size(89, 16)
        Me.lblRefedit2.TabIndex = 3
        Me.lblRefedit2.Text = "Data: Group 2"
        '
        'lblRefedit1
        '
        Me.lblRefedit1.Location = New System.Drawing.Point(13, 42)
        Me.lblRefedit1.Name = "lblRefedit1"
        Me.lblRefedit1.Size = New System.Drawing.Size(123, 34)
        Me.lblRefedit1.TabIndex = 2
        Me.lblRefedit1.Text = "Data: Group 1"
        '
        'TabPageOptions
        '
        Me.TabPageOptions.Controls.Add(Me.lblErrorRatio)
        Me.TabPageOptions.Controls.Add(Me.spinBtnErrorRatio)
        Me.TabPageOptions.Controls.Add(Me.lblAlpha)
        Me.TabPageOptions.Controls.Add(Me.spinBtnAlphaDeming)
        Me.TabPageOptions.Controls.Add(Me.grpCItype)
        Me.TabPageOptions.Controls.Add(Me.ckSignTest)
        Me.TabPageOptions.Controls.Add(Me.ckDescriptiveStatistics)
        Me.TabPageOptions.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptions.Name = "TabPageOptions"
        Me.TabPageOptions.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageOptions.Size = New System.Drawing.Size(446, 335)
        Me.TabPageOptions.TabIndex = 1
        Me.TabPageOptions.Text = "Options"
        Me.TabPageOptions.UseVisualStyleBackColor = True
        '
        'lblErrorRatio
        '
        Me.lblErrorRatio.AutoSize = True
        Me.lblErrorRatio.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblErrorRatio.Location = New System.Drawing.Point(16, 231)
        Me.lblErrorRatio.Name = "lblErrorRatio"
        Me.lblErrorRatio.Size = New System.Drawing.Size(123, 16)
        Me.lblErrorRatio.TabIndex = 9
        Me.lblErrorRatio.Text = "Error Ratio (σx²/σy²)"
        '
        'spinBtnErrorRatio
        '
        Me.spinBtnErrorRatio.DecimalPlaces = 3
        Me.spinBtnErrorRatio.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnErrorRatio.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnErrorRatio.Location = New System.Drawing.Point(145, 226)
        Me.spinBtnErrorRatio.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnErrorRatio.Name = "spinBtnErrorRatio"
        Me.spinBtnErrorRatio.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnErrorRatio.TabIndex = 8
        Me.spinBtnErrorRatio.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblAlpha
        '
        Me.lblAlpha.AutoSize = True
        Me.lblAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlpha.Location = New System.Drawing.Point(97, 206)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(42, 16)
        Me.lblAlpha.TabIndex = 7
        Me.lblAlpha.Text = "Alpha"
        '
        'spinBtnAlphaDeming
        '
        Me.spinBtnAlphaDeming.DecimalPlaces = 3
        Me.spinBtnAlphaDeming.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlphaDeming.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaDeming.Location = New System.Drawing.Point(145, 200)
        Me.spinBtnAlphaDeming.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlphaDeming.Name = "spinBtnAlphaDeming"
        Me.spinBtnAlphaDeming.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlphaDeming.TabIndex = 6
        Me.spinBtnAlphaDeming.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'grpCItype
        '
        Me.grpCItype.Controls.Add(Me.optAnalyticalClosedForm)
        Me.grpCItype.Controls.Add(Me.optJackknife)
        Me.grpCItype.Controls.Add(Me.optAnalyticalLinnet)
        Me.grpCItype.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpCItype.Location = New System.Drawing.Point(19, 69)
        Me.grpCItype.Name = "grpCItype"
        Me.grpCItype.Size = New System.Drawing.Size(325, 123)
        Me.grpCItype.TabIndex = 5
        Me.grpCItype.TabStop = False
        Me.grpCItype.Text = "Confidence Interval Construction"
        '
        'optAnalyticalClosedForm
        '
        Me.optAnalyticalClosedForm.AutoSize = True
        Me.optAnalyticalClosedForm.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optAnalyticalClosedForm.Location = New System.Drawing.Point(17, 82)
        Me.optAnalyticalClosedForm.Name = "optAnalyticalClosedForm"
        Me.optAnalyticalClosedForm.Size = New System.Drawing.Size(248, 20)
        Me.optAnalyticalClosedForm.TabIndex = 2
        Me.optAnalyticalClosedForm.Text = "Analytical (closed form / linearization)"
        Me.optAnalyticalClosedForm.UseVisualStyleBackColor = True
        '
        'optJackknife
        '
        Me.optJackknife.AutoSize = True
        Me.optJackknife.Checked = True
        Me.optJackknife.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optJackknife.Location = New System.Drawing.Point(17, 30)
        Me.optJackknife.Name = "optJackknife"
        Me.optJackknife.Size = New System.Drawing.Size(85, 20)
        Me.optJackknife.TabIndex = 1
        Me.optJackknife.TabStop = True
        Me.optJackknife.Text = "Jackknife"
        Me.optJackknife.UseVisualStyleBackColor = True
        '
        'optAnalyticalLinnet
        '
        Me.optAnalyticalLinnet.AutoSize = True
        Me.optAnalyticalLinnet.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optAnalyticalLinnet.Location = New System.Drawing.Point(17, 56)
        Me.optAnalyticalLinnet.Name = "optAnalyticalLinnet"
        Me.optAnalyticalLinnet.Size = New System.Drawing.Size(289, 20)
        Me.optAnalyticalLinnet.TabIndex = 0
        Me.optAnalyticalLinnet.Text = "Analytical – Linnet (jackknife pseudo-values)"
        Me.optAnalyticalLinnet.UseVisualStyleBackColor = True
        '
        'ckSignTest
        '
        Me.ckSignTest.AutoSize = True
        Me.ckSignTest.Checked = True
        Me.ckSignTest.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckSignTest.Location = New System.Drawing.Point(19, 43)
        Me.ckSignTest.Name = "ckSignTest"
        Me.ckSignTest.Size = New System.Drawing.Size(86, 20)
        Me.ckSignTest.TabIndex = 4
        Me.ckSignTest.Text = "Sign Test"
        Me.ckSignTest.UseVisualStyleBackColor = True
        '
        'ckDescriptiveStatistics
        '
        Me.ckDescriptiveStatistics.AutoSize = True
        Me.ckDescriptiveStatistics.Checked = True
        Me.ckDescriptiveStatistics.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDescriptiveStatistics.Location = New System.Drawing.Point(19, 17)
        Me.ckDescriptiveStatistics.Name = "ckDescriptiveStatistics"
        Me.ckDescriptiveStatistics.Size = New System.Drawing.Size(177, 20)
        Me.ckDescriptiveStatistics.TabIndex = 3
        Me.ckDescriptiveStatistics.Text = "Full Descriptive Statistics"
        Me.ckDescriptiveStatistics.UseVisualStyleBackColor = True
        '
        'TabPageOptionsHotteling
        '
        Me.TabPageOptionsHotteling.Controls.Add(Me.lblAlphaOutliers)
        Me.TabPageOptionsHotteling.Controls.Add(Me.spinBtnAlpha)
        Me.TabPageOptionsHotteling.Controls.Add(Me.grpHottelingTestType)
        Me.TabPageOptionsHotteling.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsHotteling.Name = "TabPageOptionsHotteling"
        Me.TabPageOptionsHotteling.Size = New System.Drawing.Size(446, 335)
        Me.TabPageOptionsHotteling.TabIndex = 2
        Me.TabPageOptionsHotteling.Text = "Options"
        Me.TabPageOptionsHotteling.UseVisualStyleBackColor = True
        '
        'lblAlphaOutliers
        '
        Me.lblAlphaOutliers.AutoSize = True
        Me.lblAlphaOutliers.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaOutliers.Location = New System.Drawing.Point(21, 165)
        Me.lblAlphaOutliers.Name = "lblAlphaOutliers"
        Me.lblAlphaOutliers.Size = New System.Drawing.Size(42, 16)
        Me.lblAlphaOutliers.TabIndex = 5
        Me.lblAlphaOutliers.Text = "Alpha"
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(69, 163)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 4
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'grpHottelingTestType
        '
        Me.grpHottelingTestType.Controls.Add(Me.optIndependent)
        Me.grpHottelingTestType.Controls.Add(Me.optPaired)
        Me.grpHottelingTestType.Controls.Add(Me.optSingle)
        Me.grpHottelingTestType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHottelingTestType.Location = New System.Drawing.Point(24, 26)
        Me.grpHottelingTestType.Name = "grpHottelingTestType"
        Me.grpHottelingTestType.Size = New System.Drawing.Size(274, 121)
        Me.grpHottelingTestType.TabIndex = 0
        Me.grpHottelingTestType.TabStop = False
        Me.grpHottelingTestType.Text = "Analysis Type"
        '
        'optIndependent
        '
        Me.optIndependent.AutoSize = True
        Me.optIndependent.Checked = True
        Me.optIndependent.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optIndependent.Location = New System.Drawing.Point(17, 83)
        Me.optIndependent.Name = "optIndependent"
        Me.optIndependent.Size = New System.Drawing.Size(189, 20)
        Me.optIndependent.TabIndex = 2
        Me.optIndependent.TabStop = True
        Me.optIndependent.Text = "Two Independent Samples"
        Me.optIndependent.UseVisualStyleBackColor = True
        '
        'optPaired
        '
        Me.optPaired.AutoSize = True
        Me.optPaired.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optPaired.Location = New System.Drawing.Point(17, 57)
        Me.optPaired.Name = "optPaired"
        Me.optPaired.Size = New System.Drawing.Size(125, 20)
        Me.optPaired.TabIndex = 1
        Me.optPaired.TabStop = True
        Me.optPaired.Text = "Paired Samples"
        Me.optPaired.UseVisualStyleBackColor = True
        '
        'optSingle
        '
        Me.optSingle.AutoSize = True
        Me.optSingle.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optSingle.Location = New System.Drawing.Point(17, 31)
        Me.optSingle.Name = "optSingle"
        Me.optSingle.Size = New System.Drawing.Size(116, 20)
        Me.optSingle.TabIndex = 0
        Me.optSingle.TabStop = True
        Me.optSingle.Text = "Single Sample"
        Me.optSingle.UseVisualStyleBackColor = True
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(297, 380)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 4
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(378, 380)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 3
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'progressBarExactCalc
        '
        Me.progressBarExactCalc.Location = New System.Drawing.Point(10, 381)
        Me.progressBarExactCalc.Name = "progressBarExactCalc"
        Me.progressBarExactCalc.Size = New System.Drawing.Size(270, 23)
        Me.progressBarExactCalc.TabIndex = 5
        '
        'UiTwoInputRefedits
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(465, 415)
        Me.Controls.Add(Me.progressBarExactCalc)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.TabMultipage)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimumSize = New System.Drawing.Size(483, 457)
        Me.Name = "UiTwoInputRefedits"
        Me.ShowIcon = False
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPageOptions.ResumeLayout(False)
        Me.TabPageOptions.PerformLayout()
        CType(Me.spinBtnErrorRatio, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnAlphaDeming, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpCItype.ResumeLayout(False)
        Me.grpCItype.PerformLayout()
        Me.TabPageOptionsHotteling.ResumeLayout(False)
        Me.TabPageOptionsHotteling.PerformLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpHottelingTestType.ResumeLayout(False)
        Me.grpHottelingTestType.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents TabPageOptions As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents RefEdit2 As Excel2007RefEdit
    Friend WithEvents RefEdit1 As Excel2007RefEdit
    Friend WithEvents lblRefedit2 As Windows.Forms.Label
    Friend WithEvents lblRefedit1 As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents ckDescriptiveStatistics As Windows.Forms.CheckBox
    Friend WithEvents ckSignTest As Windows.Forms.CheckBox
    Friend WithEvents progressBarExactCalc As Windows.Forms.ProgressBar
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents TabPageOptionsHotteling As Windows.Forms.TabPage
    Friend WithEvents grpHottelingTestType As Windows.Forms.GroupBox
    Friend WithEvents optSingle As Windows.Forms.RadioButton
    Friend WithEvents optIndependent As Windows.Forms.RadioButton
    Friend WithEvents optPaired As Windows.Forms.RadioButton
    Friend WithEvents lblAlphaOutliers As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents grpCItype As Windows.Forms.GroupBox
    Friend WithEvents optJackknife As Windows.Forms.RadioButton
    Friend WithEvents optAnalyticalLinnet As Windows.Forms.RadioButton
    Friend WithEvents lblErrorRatio As Windows.Forms.Label
    Friend WithEvents spinBtnErrorRatio As Windows.Forms.NumericUpDown
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlphaDeming As Windows.Forms.NumericUpDown
    Friend WithEvents optAnalyticalClosedForm As Windows.Forms.RadioButton
End Class
