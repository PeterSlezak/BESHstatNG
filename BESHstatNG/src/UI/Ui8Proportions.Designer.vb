<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui8Proportions
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui8Proportions))
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New Global.BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.lblMarginHint_IndProp = New System.Windows.Forms.Label()
        Me.spinBtnMargin_IndProp = New System.Windows.Forms.NumericUpDown()
        Me.lblMargin = New System.Windows.Forms.Label()
        Me.lblHypothesisType = New System.Windows.Forms.Label()
        Me.cbHypothesisType = New System.Windows.Forms.ComboBox()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.optPaired = New System.Windows.Forms.RadioButton()
        Me.spinBtnD = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnC = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnB = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnA = New System.Windows.Forms.NumericUpDown()
        Me.lbl2 = New System.Windows.Forms.Label()
        Me.lbl1 = New System.Windows.Forms.Label()
        Me.lbl4 = New System.Windows.Forms.Label()
        Me.lbl3 = New System.Windows.Forms.Label()
        Me.optIndependent = New System.Windows.Forms.RadioButton()
        Me.optSingle = New System.Windows.Forms.RadioButton()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        CType(Me.spinBtnMargin_IndProp, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnD, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnC, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnB, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnA, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(5, 271)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(442, 113)
        Me.grpOutput.TabIndex = 5
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
        Me.RefEditOutput.Location = New System.Drawing.Point(168, 16)
        Me.RefEditOutput.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEditOutput.Name = "RefEditOutput"
        Me.RefEditOutput.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEditOutput.Size = New System.Drawing.Size(267, 32)
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
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(291, 390)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 12
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(372, 390)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 11
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.lblMarginHint_IndProp)
        Me.grpInput.Controls.Add(Me.spinBtnMargin_IndProp)
        Me.grpInput.Controls.Add(Me.lblMargin)
        Me.grpInput.Controls.Add(Me.lblHypothesisType)
        Me.grpInput.Controls.Add(Me.cbHypothesisType)
        Me.grpInput.Controls.Add(Me.spinBtnAlpha)
        Me.grpInput.Controls.Add(Me.lblAlpha)
        Me.grpInput.Controls.Add(Me.optPaired)
        Me.grpInput.Controls.Add(Me.spinBtnD)
        Me.grpInput.Controls.Add(Me.spinBtnC)
        Me.grpInput.Controls.Add(Me.spinBtnB)
        Me.grpInput.Controls.Add(Me.spinBtnA)
        Me.grpInput.Controls.Add(Me.lbl2)
        Me.grpInput.Controls.Add(Me.lbl1)
        Me.grpInput.Controls.Add(Me.lbl4)
        Me.grpInput.Controls.Add(Me.lbl3)
        Me.grpInput.Controls.Add(Me.optIndependent)
        Me.grpInput.Controls.Add(Me.optSingle)
        Me.grpInput.Location = New System.Drawing.Point(5, 4)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(442, 261)
        Me.grpInput.TabIndex = 10
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'lblMarginHint_IndProp
        '
        Me.lblMarginHint_IndProp.Location = New System.Drawing.Point(11, 217)
        Me.lblMarginHint_IndProp.Name = "lblMarginHint_IndProp"
        Me.lblMarginHint_IndProp.Size = New System.Drawing.Size(424, 44)
        Me.lblMarginHint_IndProp.TabIndex = 25
        Me.lblMarginHint_IndProp.Text = "Margin is entered as a positive magnitude. Sample 1 is Control / Reference and Sa" &
    "mple 2 is Experimental / Test"
        Me.lblMarginHint_IndProp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'spinBtnMargin_IndProp
        '
        Me.spinBtnMargin_IndProp.DecimalPlaces = 4
        Me.spinBtnMargin_IndProp.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnMargin_IndProp.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnMargin_IndProp.Location = New System.Drawing.Point(146, 192)
        Me.spinBtnMargin_IndProp.Maximum = New Decimal(New Integer() {9999, 0, 0, 262144})
        Me.spinBtnMargin_IndProp.Minimum = New Decimal(New Integer() {1, 0, 0, 262144})
        Me.spinBtnMargin_IndProp.Name = "spinBtnMargin_IndProp"
        Me.spinBtnMargin_IndProp.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnMargin_IndProp.TabIndex = 24
        Me.spinBtnMargin_IndProp.Value = New Decimal(New Integer() {1, 0, 0, 65536})
        '
        'lblMargin
        '
        Me.lblMargin.AutoSize = True
        Me.lblMargin.Location = New System.Drawing.Point(11, 194)
        Me.lblMargin.Name = "lblMargin"
        Me.lblMargin.Size = New System.Drawing.Size(129, 16)
        Me.lblMargin.TabIndex = 23
        Me.lblMargin.Text = "Noninferiority margin"
        '
        'lblHypothesisType
        '
        Me.lblHypothesisType.AutoSize = True
        Me.lblHypothesisType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHypothesisType.Location = New System.Drawing.Point(185, 171)
        Me.lblHypothesisType.Name = "lblHypothesisType"
        Me.lblHypothesisType.Size = New System.Drawing.Size(75, 16)
        Me.lblHypothesisType.TabIndex = 22
        Me.lblHypothesisType.Text = "Hypothesis"
        '
        'cbHypothesisType
        '
        Me.cbHypothesisType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHypothesisType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHypothesisType.FormattingEnabled = True
        Me.cbHypothesisType.Location = New System.Drawing.Point(266, 168)
        Me.cbHypothesisType.Name = "cbHypothesisType"
        Me.cbHypothesisType.Size = New System.Drawing.Size(169, 24)
        Me.cbHypothesisType.TabIndex = 21
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(112, 166)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 20
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'lblAlpha
        '
        Me.lblAlpha.Location = New System.Drawing.Point(6, 171)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(100, 16)
        Me.lblAlpha.TabIndex = 19
        Me.lblAlpha.Text = "alpha"
        Me.lblAlpha.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'optPaired
        '
        Me.optPaired.AutoSize = True
        Me.optPaired.Location = New System.Drawing.Point(321, 21)
        Me.optPaired.Name = "optPaired"
        Me.optPaired.Size = New System.Drawing.Size(68, 20)
        Me.optPaired.TabIndex = 18
        Me.optPaired.Text = "Paired"
        Me.optPaired.UseVisualStyleBackColor = True
        '
        'spinBtnD
        '
        Me.spinBtnD.Location = New System.Drawing.Point(14, 132)
        Me.spinBtnD.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnD.Name = "spinBtnD"
        Me.spinBtnD.Size = New System.Drawing.Size(94, 22)
        Me.spinBtnD.TabIndex = 17
        Me.spinBtnD.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'spinBtnC
        '
        Me.spinBtnC.Location = New System.Drawing.Point(14, 104)
        Me.spinBtnC.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnC.Name = "spinBtnC"
        Me.spinBtnC.Size = New System.Drawing.Size(94, 22)
        Me.spinBtnC.TabIndex = 16
        Me.spinBtnC.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'spinBtnB
        '
        Me.spinBtnB.Location = New System.Drawing.Point(14, 76)
        Me.spinBtnB.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnB.Name = "spinBtnB"
        Me.spinBtnB.Size = New System.Drawing.Size(94, 22)
        Me.spinBtnB.TabIndex = 15
        Me.spinBtnB.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'spinBtnA
        '
        Me.spinBtnA.Location = New System.Drawing.Point(14, 47)
        Me.spinBtnA.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnA.Name = "spinBtnA"
        Me.spinBtnA.Size = New System.Drawing.Size(95, 22)
        Me.spinBtnA.TabIndex = 14
        Me.spinBtnA.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'lbl2
        '
        Me.lbl2.AutoSize = True
        Me.lbl2.Location = New System.Drawing.Point(122, 82)
        Me.lbl2.Name = "lbl2"
        Me.lbl2.Size = New System.Drawing.Size(23, 16)
        Me.lbl2.TabIndex = 13
        Me.lbl2.Text = "C2"
        '
        'lbl1
        '
        Me.lbl1.AutoSize = True
        Me.lbl1.Location = New System.Drawing.Point(122, 53)
        Me.lbl1.Name = "lbl1"
        Me.lbl1.Size = New System.Drawing.Size(23, 16)
        Me.lbl1.TabIndex = 12
        Me.lbl1.Text = "C1"
        '
        'lbl4
        '
        Me.lbl4.AutoSize = True
        Me.lbl4.Location = New System.Drawing.Point(122, 138)
        Me.lbl4.Name = "lbl4"
        Me.lbl4.Size = New System.Drawing.Size(24, 16)
        Me.lbl4.TabIndex = 11
        Me.lbl4.Text = "R2"
        '
        'lbl3
        '
        Me.lbl3.AutoSize = True
        Me.lbl3.Location = New System.Drawing.Point(122, 110)
        Me.lbl3.Name = "lbl3"
        Me.lbl3.Size = New System.Drawing.Size(24, 16)
        Me.lbl3.TabIndex = 10
        Me.lbl3.Text = "R1"
        '
        'optIndependent
        '
        Me.optIndependent.AutoSize = True
        Me.optIndependent.Location = New System.Drawing.Point(137, 21)
        Me.optIndependent.Name = "optIndependent"
        Me.optIndependent.Size = New System.Drawing.Size(132, 20)
        Me.optIndependent.TabIndex = 1
        Me.optIndependent.Text = "Two Independent"
        Me.optIndependent.UseVisualStyleBackColor = True
        '
        'optSingle
        '
        Me.optSingle.AutoSize = True
        Me.optSingle.Checked = True
        Me.optSingle.Location = New System.Drawing.Point(26, 21)
        Me.optSingle.Name = "optSingle"
        Me.optSingle.Size = New System.Drawing.Size(66, 20)
        Me.optSingle.TabIndex = 0
        Me.optSingle.TabStop = True
        Me.optSingle.Text = "Single"
        Me.optSingle.UseVisualStyleBackColor = True
        '
        'Ui8Proportions
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(454, 417)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.grpInput)
        Me.Controls.Add(Me.grpOutput)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(472, 464)
        Me.MinimumSize = New System.Drawing.Size(472, 464)
        Me.Name = "Ui8Proportions"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Proportions"
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        CType(Me.spinBtnMargin_IndProp, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnD, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnC, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnB, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnA, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents optPaired As Windows.Forms.RadioButton
    Friend WithEvents spinBtnD As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnC As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnB As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnA As Windows.Forms.NumericUpDown
    Friend WithEvents lbl2 As Windows.Forms.Label
    Friend WithEvents lbl1 As Windows.Forms.Label
    Friend WithEvents lbl4 As Windows.Forms.Label
    Friend WithEvents lbl3 As Windows.Forms.Label
    Friend WithEvents optIndependent As Windows.Forms.RadioButton
    Friend WithEvents optSingle As Windows.Forms.RadioButton
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnMargin_IndProp As Windows.Forms.NumericUpDown
    Friend WithEvents lblMargin As Windows.Forms.Label
    Friend WithEvents lblHypothesisType As Windows.Forms.Label
    Friend WithEvents cbHypothesisType As Windows.Forms.ComboBox
    Friend WithEvents lblMarginHint_IndProp As Windows.Forms.Label
End Class
