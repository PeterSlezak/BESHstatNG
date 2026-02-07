<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Ui11PCA
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
        Me.TabPageOptionsSPM = New System.Windows.Forms.TabPage()
        Me.ckShowRegressionLines = New System.Windows.Forms.CheckBox()
        Me.ckDisplayCorrelCoef = New System.Windows.Forms.CheckBox()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.ckFirstRow = New System.Windows.Forms.CheckBox()
        Me.lbXs = New System.Windows.Forms.ListBox()
        Me.cbSheetsList = New System.Windows.Forms.ComboBox()
        Me.btReload = New System.Windows.Forms.Button()
        Me.btRemoveX = New System.Windows.Forms.Button()
        Me.btAddX = New System.Windows.Forms.Button()
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.lblX = New System.Windows.Forms.Label()
        Me.lblSelectedSheet = New System.Windows.Forms.Label()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPageOptionsPCA = New System.Windows.Forms.TabPage()
        Me.grpExtract = New System.Windows.Forms.GroupBox()
        Me.spinBtnExtractEigen = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnExtractComp = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnExtractVariance = New System.Windows.Forms.NumericUpDown()
        Me.optExtractVariance = New System.Windows.Forms.RadioButton()
        Me.optExtractFixed = New System.Windows.Forms.RadioButton()
        Me.optExtractEigen = New System.Windows.Forms.RadioButton()
        Me.grpAnalyzeType = New System.Windows.Forms.GroupBox()
        Me.optCovar = New System.Windows.Forms.RadioButton()
        Me.optCorr = New System.Windows.Forms.RadioButton()
        Me.grpIterOptions = New System.Windows.Forms.GroupBox()
        Me.tbMaxIter = New System.Windows.Forms.TextBox()
        Me.lblMaxIter = New System.Windows.Forms.Label()
        Me.lblEps = New System.Windows.Forms.Label()
        Me.tbEps = New System.Windows.Forms.TextBox()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCalculate = New System.Windows.Forms.Button()
        Me.TabPageOptionsSPM.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPageOptionsPCA.SuspendLayout()
        Me.grpExtract.SuspendLayout()
        CType(Me.spinBtnExtractEigen, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnExtractComp, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnExtractVariance, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpAnalyzeType.SuspendLayout()
        Me.grpIterOptions.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabPageOptionsSPM
        '
        Me.TabPageOptionsSPM.Controls.Add(Me.ckShowRegressionLines)
        Me.TabPageOptionsSPM.Controls.Add(Me.ckDisplayCorrelCoef)
        Me.TabPageOptionsSPM.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsSPM.Name = "TabPageOptionsSPM"
        Me.TabPageOptionsSPM.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptionsSPM.TabIndex = 2
        Me.TabPageOptionsSPM.Text = "Options"
        Me.TabPageOptionsSPM.UseVisualStyleBackColor = True
        '
        'ckShowRegressionLines
        '
        Me.ckShowRegressionLines.AutoSize = True
        Me.ckShowRegressionLines.Checked = True
        Me.ckShowRegressionLines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowRegressionLines.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckShowRegressionLines.Location = New System.Drawing.Point(19, 45)
        Me.ckShowRegressionLines.Name = "ckShowRegressionLines"
        Me.ckShowRegressionLines.Size = New System.Drawing.Size(170, 20)
        Me.ckShowRegressionLines.TabIndex = 6
        Me.ckShowRegressionLines.Text = "Show Regression Lines"
        Me.ckShowRegressionLines.UseVisualStyleBackColor = True
        '
        'ckDisplayCorrelCoef
        '
        Me.ckDisplayCorrelCoef.AutoSize = True
        Me.ckDisplayCorrelCoef.Checked = True
        Me.ckDisplayCorrelCoef.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDisplayCorrelCoef.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckDisplayCorrelCoef.Location = New System.Drawing.Point(19, 19)
        Me.ckDisplayCorrelCoef.Name = "ckDisplayCorrelCoef"
        Me.ckDisplayCorrelCoef.Size = New System.Drawing.Size(215, 20)
        Me.ckDisplayCorrelCoef.TabIndex = 5
        Me.ckDisplayCorrelCoef.Text = "Display Correlation Coefficients"
        Me.ckDisplayCorrelCoef.UseVisualStyleBackColor = True
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.ckFirstRow)
        Me.TabPage1.Controls.Add(Me.lbXs)
        Me.TabPage1.Controls.Add(Me.cbSheetsList)
        Me.TabPage1.Controls.Add(Me.btReload)
        Me.TabPage1.Controls.Add(Me.btRemoveX)
        Me.TabPage1.Controls.Add(Me.btAddX)
        Me.TabPage1.Controls.Add(Me.lbAllColumns)
        Me.TabPage1.Controls.Add(Me.lblAllColumns)
        Me.TabPage1.Controls.Add(Me.lblX)
        Me.TabPage1.Controls.Add(Me.lblSelectedSheet)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(836, 465)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Select Variables"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'ckFirstRow
        '
        Me.ckFirstRow.Checked = True
        Me.ckFirstRow.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckFirstRow.Location = New System.Drawing.Point(232, 74)
        Me.ckFirstRow.Name = "ckFirstRow"
        Me.ckFirstRow.Size = New System.Drawing.Size(84, 94)
        Me.ckFirstRow.TabIndex = 25
        Me.ckFirstRow.Text = "1st Row Contains Variable Names"
        Me.ckFirstRow.UseVisualStyleBackColor = True
        Me.ckFirstRow.Visible = False
        '
        'lbXs
        '
        Me.lbXs.FormattingEnabled = True
        Me.lbXs.ItemHeight = 16
        Me.lbXs.Location = New System.Drawing.Point(322, 23)
        Me.lbXs.Name = "lbXs"
        Me.lbXs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbXs.Size = New System.Drawing.Size(240, 420)
        Me.lbXs.TabIndex = 17
        '
        'cbSheetsList
        '
        Me.cbSheetsList.FormattingEnabled = True
        Me.cbSheetsList.Location = New System.Drawing.Point(579, 23)
        Me.cbSheetsList.Name = "cbSheetsList"
        Me.cbSheetsList.Size = New System.Drawing.Size(240, 24)
        Me.cbSheetsList.TabIndex = 21
        Me.cbSheetsList.Text = "Select Sheet"
        '
        'btReload
        '
        Me.btReload.Location = New System.Drawing.Point(579, 53)
        Me.btReload.Name = "btReload"
        Me.btReload.Size = New System.Drawing.Size(130, 23)
        Me.btReload.TabIndex = 20
        Me.btReload.Text = "Reload Sheet Data"
        Me.btReload.UseVisualStyleBackColor = True
        '
        'btRemoveX
        '
        Me.btRemoveX.Location = New System.Drawing.Point(277, 22)
        Me.btRemoveX.Name = "btRemoveX"
        Me.btRemoveX.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveX.TabIndex = 16
        Me.btRemoveX.Text = "<<"
        Me.btRemoveX.UseVisualStyleBackColor = True
        '
        'btAddX
        '
        Me.btAddX.Location = New System.Drawing.Point(232, 22)
        Me.btAddX.Name = "btAddX"
        Me.btAddX.Size = New System.Drawing.Size(39, 23)
        Me.btAddX.TabIndex = 15
        Me.btAddX.Text = ">>"
        Me.btAddX.UseVisualStyleBackColor = True
        '
        'lbAllColumns
        '
        Me.lbAllColumns.FormattingEnabled = True
        Me.lbAllColumns.ItemHeight = 16
        Me.lbAllColumns.Location = New System.Drawing.Point(5, 22)
        Me.lbAllColumns.Name = "lbAllColumns"
        Me.lbAllColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbAllColumns.Size = New System.Drawing.Size(221, 420)
        Me.lbAllColumns.TabIndex = 0
        '
        'lblAllColumns
        '
        Me.lblAllColumns.AutoSize = True
        Me.lblAllColumns.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAllColumns.Location = New System.Drawing.Point(15, 3)
        Me.lblAllColumns.Name = "lblAllColumns"
        Me.lblAllColumns.Size = New System.Drawing.Size(144, 16)
        Me.lblAllColumns.TabIndex = 1
        Me.lblAllColumns.Text = "Worksheet Columns"
        '
        'lblX
        '
        Me.lblX.AutoSize = True
        Me.lblX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblX.Location = New System.Drawing.Point(322, 3)
        Me.lblX.Name = "lblX"
        Me.lblX.Size = New System.Drawing.Size(150, 16)
        Me.lblX.TabIndex = 18
        Me.lblX.Text = "Selected Variable(s)"
        '
        'lblSelectedSheet
        '
        Me.lblSelectedSheet.AutoSize = True
        Me.lblSelectedSheet.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedSheet.Location = New System.Drawing.Point(576, 3)
        Me.lblSelectedSheet.Name = "lblSelectedSheet"
        Me.lblSelectedSheet.Size = New System.Drawing.Size(132, 16)
        Me.lblSelectedSheet.TabIndex = 24
        Me.lblSelectedSheet.Text = "Active Worksheet:"
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsSPM)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsPCA)
        Me.TabControl1.Location = New System.Drawing.Point(3, 2)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(844, 494)
        Me.TabControl1.TabIndex = 3
        '
        'TabPageOptionsPCA
        '
        Me.TabPageOptionsPCA.Controls.Add(Me.grpExtract)
        Me.TabPageOptionsPCA.Controls.Add(Me.grpAnalyzeType)
        Me.TabPageOptionsPCA.Controls.Add(Me.grpIterOptions)
        Me.TabPageOptionsPCA.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsPCA.Name = "TabPageOptionsPCA"
        Me.TabPageOptionsPCA.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptionsPCA.TabIndex = 3
        Me.TabPageOptionsPCA.Text = "Options"
        Me.TabPageOptionsPCA.UseVisualStyleBackColor = True
        '
        'grpExtract
        '
        Me.grpExtract.Controls.Add(Me.spinBtnExtractEigen)
        Me.grpExtract.Controls.Add(Me.spinBtnExtractComp)
        Me.grpExtract.Controls.Add(Me.spinBtnExtractVariance)
        Me.grpExtract.Controls.Add(Me.optExtractVariance)
        Me.grpExtract.Controls.Add(Me.optExtractFixed)
        Me.grpExtract.Controls.Add(Me.optExtractEigen)
        Me.grpExtract.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpExtract.Location = New System.Drawing.Point(14, 134)
        Me.grpExtract.Name = "grpExtract"
        Me.grpExtract.Size = New System.Drawing.Size(312, 143)
        Me.grpExtract.TabIndex = 5
        Me.grpExtract.TabStop = False
        Me.grpExtract.Text = "Extract"
        '
        'spinBtnExtractEigen
        '
        Me.spinBtnExtractEigen.DecimalPlaces = 2
        Me.spinBtnExtractEigen.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnExtractEigen.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.spinBtnExtractEigen.Location = New System.Drawing.Point(237, 29)
        Me.spinBtnExtractEigen.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnExtractEigen.Name = "spinBtnExtractEigen"
        Me.spinBtnExtractEigen.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnExtractEigen.TabIndex = 7
        Me.spinBtnExtractEigen.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'spinBtnExtractComp
        '
        Me.spinBtnExtractComp.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnExtractComp.Location = New System.Drawing.Point(237, 61)
        Me.spinBtnExtractComp.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnExtractComp.Name = "spinBtnExtractComp"
        Me.spinBtnExtractComp.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnExtractComp.TabIndex = 6
        Me.spinBtnExtractComp.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'spinBtnExtractVariance
        '
        Me.spinBtnExtractVariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnExtractVariance.Location = New System.Drawing.Point(237, 96)
        Me.spinBtnExtractVariance.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        Me.spinBtnExtractVariance.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnExtractVariance.Name = "spinBtnExtractVariance"
        Me.spinBtnExtractVariance.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnExtractVariance.TabIndex = 5
        Me.spinBtnExtractVariance.Value = New Decimal(New Integer() {80, 0, 0, 0})
        '
        'optExtractVariance
        '
        Me.optExtractVariance.AutoSize = True
        Me.optExtractVariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optExtractVariance.Location = New System.Drawing.Point(16, 98)
        Me.optExtractVariance.Name = "optExtractVariance"
        Me.optExtractVariance.Size = New System.Drawing.Size(168, 20)
        Me.optExtractVariance.TabIndex = 2
        Me.optExtractVariance.Text = "Variance Explained [%]"
        Me.optExtractVariance.UseVisualStyleBackColor = True
        '
        'optExtractFixed
        '
        Me.optExtractFixed.AutoSize = True
        Me.optExtractFixed.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optExtractFixed.Location = New System.Drawing.Point(16, 63)
        Me.optExtractFixed.Name = "optExtractFixed"
        Me.optExtractFixed.Size = New System.Drawing.Size(205, 20)
        Me.optExtractFixed.TabIndex = 1
        Me.optExtractFixed.Text = "Fixed Number of Components"
        Me.optExtractFixed.UseVisualStyleBackColor = True
        '
        'optExtractEigen
        '
        Me.optExtractEigen.AutoSize = True
        Me.optExtractEigen.Checked = True
        Me.optExtractEigen.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optExtractEigen.Location = New System.Drawing.Point(16, 31)
        Me.optExtractEigen.Name = "optExtractEigen"
        Me.optExtractEigen.Size = New System.Drawing.Size(185, 20)
        Me.optExtractEigen.TabIndex = 0
        Me.optExtractEigen.TabStop = True
        Me.optExtractEigen.Text = "Based on Eigenvalue  (>=)"
        Me.optExtractEigen.UseVisualStyleBackColor = True
        '
        'grpAnalyzeType
        '
        Me.grpAnalyzeType.Controls.Add(Me.optCovar)
        Me.grpAnalyzeType.Controls.Add(Me.optCorr)
        Me.grpAnalyzeType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpAnalyzeType.Location = New System.Drawing.Point(332, 18)
        Me.grpAnalyzeType.Name = "grpAnalyzeType"
        Me.grpAnalyzeType.Size = New System.Drawing.Size(312, 110)
        Me.grpAnalyzeType.TabIndex = 4
        Me.grpAnalyzeType.TabStop = False
        Me.grpAnalyzeType.Text = "Analyze"
        '
        'optCovar
        '
        Me.optCovar.AutoSize = True
        Me.optCovar.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCovar.Location = New System.Drawing.Point(16, 63)
        Me.optCovar.Name = "optCovar"
        Me.optCovar.Size = New System.Drawing.Size(135, 20)
        Me.optCovar.TabIndex = 1
        Me.optCovar.Text = "Covariance Matrix"
        Me.optCovar.UseVisualStyleBackColor = True
        '
        'optCorr
        '
        Me.optCorr.AutoSize = True
        Me.optCorr.Checked = True
        Me.optCorr.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCorr.Location = New System.Drawing.Point(16, 31)
        Me.optCorr.Name = "optCorr"
        Me.optCorr.Size = New System.Drawing.Size(131, 20)
        Me.optCorr.TabIndex = 0
        Me.optCorr.TabStop = True
        Me.optCorr.Text = "Correlation Matrix"
        Me.optCorr.UseVisualStyleBackColor = True
        '
        'grpIterOptions
        '
        Me.grpIterOptions.Controls.Add(Me.tbMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblEps)
        Me.grpIterOptions.Controls.Add(Me.tbEps)
        Me.grpIterOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpIterOptions.Location = New System.Drawing.Point(14, 18)
        Me.grpIterOptions.Name = "grpIterOptions"
        Me.grpIterOptions.Size = New System.Drawing.Size(312, 110)
        Me.grpIterOptions.TabIndex = 1
        Me.grpIterOptions.TabStop = False
        Me.grpIterOptions.Text = "Convergence Options"
        '
        'tbMaxIter
        '
        Me.tbMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMaxIter.Location = New System.Drawing.Point(171, 63)
        Me.tbMaxIter.Name = "tbMaxIter"
        Me.tbMaxIter.Size = New System.Drawing.Size(125, 22)
        Me.tbMaxIter.TabIndex = 3
        Me.tbMaxIter.Text = "50"
        '
        'lblMaxIter
        '
        Me.lblMaxIter.AutoSize = True
        Me.lblMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMaxIter.Location = New System.Drawing.Point(15, 69)
        Me.lblMaxIter.Name = "lblMaxIter"
        Me.lblMaxIter.Size = New System.Drawing.Size(92, 16)
        Me.lblMaxIter.TabIndex = 2
        Me.lblMaxIter.Text = "Max. Iterations"
        '
        'lblEps
        '
        Me.lblEps.AutoSize = True
        Me.lblEps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEps.Location = New System.Drawing.Point(15, 37)
        Me.lblEps.Name = "lblEps"
        Me.lblEps.Size = New System.Drawing.Size(140, 16)
        Me.lblEps.TabIndex = 1
        Me.lblEps.Text = "Convergence Criterion"
        '
        'tbEps
        '
        Me.tbEps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbEps.Location = New System.Drawing.Point(171, 31)
        Me.tbEps.Name = "tbEps"
        Me.tbEps.Size = New System.Drawing.Size(125, 22)
        Me.tbEps.TabIndex = 1
        Me.tbEps.Text = "0.000001"
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(687, 502)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 6
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCalculate
        '
        Me.btCalculate.Location = New System.Drawing.Point(768, 502)
        Me.btCalculate.Name = "btCalculate"
        Me.btCalculate.Size = New System.Drawing.Size(75, 23)
        Me.btCalculate.TabIndex = 5
        Me.btCalculate.Text = "Fit"
        Me.btCalculate.UseVisualStyleBackColor = True
        '
        'Ui11PCA
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(856, 539)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCalculate)
        Me.Controls.Add(Me.TabControl1)
        Me.Name = "Ui11PCA"
        Me.ShowIcon = False
        Me.Text = "Form1"
        Me.TabPageOptionsSPM.ResumeLayout(False)
        Me.TabPageOptionsSPM.PerformLayout()
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPageOptionsPCA.ResumeLayout(False)
        Me.grpExtract.ResumeLayout(False)
        Me.grpExtract.PerformLayout()
        CType(Me.spinBtnExtractEigen, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnExtractComp, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnExtractVariance, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpAnalyzeType.ResumeLayout(False)
        Me.grpAnalyzeType.PerformLayout()
        Me.grpIterOptions.ResumeLayout(False)
        Me.grpIterOptions.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabPageOptionsSPM As Windows.Forms.TabPage
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents lbXs As Windows.Forms.ListBox
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents btRemoveX As Windows.Forms.Button
    Friend WithEvents btAddX As Windows.Forms.Button
    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents lblX As Windows.Forms.Label
    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents ckShowRegressionLines As Windows.Forms.CheckBox
    Friend WithEvents ckDisplayCorrelCoef As Windows.Forms.CheckBox
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCalculate As Windows.Forms.Button
    Friend WithEvents lblSelectedSheet As Windows.Forms.Label
    Friend WithEvents TabPageOptionsPCA As Windows.Forms.TabPage
    Friend WithEvents grpAnalyzeType As Windows.Forms.GroupBox
    Friend WithEvents optCovar As Windows.Forms.RadioButton
    Friend WithEvents optCorr As Windows.Forms.RadioButton
    Friend WithEvents grpIterOptions As Windows.Forms.GroupBox
    Friend WithEvents tbMaxIter As Windows.Forms.TextBox
    Friend WithEvents lblMaxIter As Windows.Forms.Label
    Friend WithEvents lblEps As Windows.Forms.Label
    Friend WithEvents tbEps As Windows.Forms.TextBox
    Friend WithEvents grpExtract As Windows.Forms.GroupBox
    Friend WithEvents optExtractVariance As Windows.Forms.RadioButton
    Friend WithEvents optExtractFixed As Windows.Forms.RadioButton
    Friend WithEvents optExtractEigen As Windows.Forms.RadioButton
    Friend WithEvents spinBtnExtractVariance As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnExtractComp As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnExtractEigen As Windows.Forms.NumericUpDown
    Friend WithEvents ckFirstRow As Windows.Forms.CheckBox
End Class
