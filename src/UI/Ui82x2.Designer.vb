<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui82x2
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui82x2))
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.spinBtnD = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnC = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnB = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnA = New System.Windows.Forms.NumericUpDown()
        Me.lblC2 = New System.Windows.Forms.Label()
        Me.lblC1 = New System.Windows.Forms.Label()
        Me.lblR2 = New System.Windows.Forms.Label()
        Me.lblR1 = New System.Windows.Forms.Label()
        Me.RefEdit1_WorksheetData = New Excel2007RefEdit()
        Me.optWorksheetData = New System.Windows.Forms.RadioButton()
        Me.optScreenData = New System.Windows.Forms.RadioButton()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.grpOptions = New System.Windows.Forms.GroupBox()
        Me.ckRR = New System.Windows.Forms.CheckBox()
        Me.ckOR = New System.Windows.Forms.CheckBox()
        Me.ckLiddel = New System.Windows.Forms.CheckBox()
        Me.ckAssociation = New System.Windows.Forms.CheckBox()
        Me.ckChi2 = New System.Windows.Forms.CheckBox()
        Me.ckFisher = New System.Windows.Forms.CheckBox()
        Me.grpInput.SuspendLayout()
        CType(Me.spinBtnD, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnC, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnB, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnA, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpOutput.SuspendLayout()
        Me.grpOptions.SuspendLayout()
        Me.SuspendLayout()
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.spinBtnD)
        Me.grpInput.Controls.Add(Me.spinBtnC)
        Me.grpInput.Controls.Add(Me.spinBtnB)
        Me.grpInput.Controls.Add(Me.spinBtnA)
        Me.grpInput.Controls.Add(Me.lblC2)
        Me.grpInput.Controls.Add(Me.lblC1)
        Me.grpInput.Controls.Add(Me.lblR2)
        Me.grpInput.Controls.Add(Me.lblR1)
        Me.grpInput.Controls.Add(Me.RefEdit1_WorksheetData)
        Me.grpInput.Controls.Add(Me.optWorksheetData)
        Me.grpInput.Controls.Add(Me.optScreenData)
        Me.grpInput.Location = New System.Drawing.Point(23, 16)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(523, 162)
        Me.grpInput.TabIndex = 0
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'spinBtnD
        '
        Me.spinBtnD.Location = New System.Drawing.Point(137, 122)
        Me.spinBtnD.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnD.Name = "spinBtnD"
        Me.spinBtnD.Size = New System.Drawing.Size(94, 22)
        Me.spinBtnD.TabIndex = 17
        Me.spinBtnD.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'spinBtnC
        '
        Me.spinBtnC.Location = New System.Drawing.Point(36, 122)
        Me.spinBtnC.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnC.Name = "spinBtnC"
        Me.spinBtnC.Size = New System.Drawing.Size(94, 22)
        Me.spinBtnC.TabIndex = 16
        Me.spinBtnC.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'spinBtnB
        '
        Me.spinBtnB.Location = New System.Drawing.Point(137, 92)
        Me.spinBtnB.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnB.Name = "spinBtnB"
        Me.spinBtnB.Size = New System.Drawing.Size(94, 22)
        Me.spinBtnB.TabIndex = 15
        Me.spinBtnB.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'spinBtnA
        '
        Me.spinBtnA.Location = New System.Drawing.Point(36, 92)
        Me.spinBtnA.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnA.Name = "spinBtnA"
        Me.spinBtnA.Size = New System.Drawing.Size(95, 22)
        Me.spinBtnA.TabIndex = 14
        Me.spinBtnA.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'lblC2
        '
        Me.lblC2.AutoSize = True
        Me.lblC2.Location = New System.Drawing.Point(119, 67)
        Me.lblC2.Name = "lblC2"
        Me.lblC2.Size = New System.Drawing.Size(23, 16)
        Me.lblC2.TabIndex = 13
        Me.lblC2.Text = "C2"
        '
        'lblC1
        '
        Me.lblC1.AutoSize = True
        Me.lblC1.Location = New System.Drawing.Point(33, 67)
        Me.lblC1.Name = "lblC1"
        Me.lblC1.Size = New System.Drawing.Size(23, 16)
        Me.lblC1.TabIndex = 12
        Me.lblC1.Text = "C1"
        '
        'lblR2
        '
        Me.lblR2.AutoSize = True
        Me.lblR2.Location = New System.Drawing.Point(6, 124)
        Me.lblR2.Name = "lblR2"
        Me.lblR2.Size = New System.Drawing.Size(24, 16)
        Me.lblR2.TabIndex = 11
        Me.lblR2.Text = "R2"
        '
        'lblR1
        '
        Me.lblR1.AutoSize = True
        Me.lblR1.Location = New System.Drawing.Point(6, 94)
        Me.lblR1.Name = "lblR1"
        Me.lblR1.Size = New System.Drawing.Size(24, 16)
        Me.lblR1.TabIndex = 10
        Me.lblR1.Text = "R1"
        '
        'RefEdit1_WorksheetData
        '
        Me.RefEdit1_WorksheetData.Address = ""
        Me.RefEdit1_WorksheetData.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit1_WorksheetData.Enabled = False
        Me.RefEdit1_WorksheetData.ExcelConnector = Nothing
        Me.RefEdit1_WorksheetData.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit1_WorksheetData.ImageMinimized = CType(resources.GetObject("RefEdit1_WorksheetData.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit1_WorksheetData.Location = New System.Drawing.Point(243, 92)
        Me.RefEdit1_WorksheetData.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit1_WorksheetData.Name = "RefEdit1_WorksheetData"
        Me.RefEdit1_WorksheetData.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit1_WorksheetData.Size = New System.Drawing.Size(267, 32)
        Me.RefEdit1_WorksheetData.TabIndex = 9
        '
        'optWorksheetData
        '
        Me.optWorksheetData.AutoSize = True
        Me.optWorksheetData.Location = New System.Drawing.Point(243, 34)
        Me.optWorksheetData.Name = "optWorksheetData"
        Me.optWorksheetData.Size = New System.Drawing.Size(125, 20)
        Me.optWorksheetData.TabIndex = 1
        Me.optWorksheetData.Text = "Worksheet Data"
        Me.optWorksheetData.UseVisualStyleBackColor = True
        '
        'optScreenData
        '
        Me.optScreenData.AutoSize = True
        Me.optScreenData.Checked = True
        Me.optScreenData.Location = New System.Drawing.Point(28, 34)
        Me.optScreenData.Name = "optScreenData"
        Me.optScreenData.Size = New System.Drawing.Size(103, 20)
        Me.optScreenData.TabIndex = 0
        Me.optScreenData.TabStop = True
        Me.optScreenData.Text = "Screen Data"
        Me.optScreenData.UseVisualStyleBackColor = True
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(23, 303)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(442, 115)
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
        Me.btnHelp.Location = New System.Drawing.Point(471, 368)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 8
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(471, 395)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 7
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'grpOptions
        '
        Me.grpOptions.Controls.Add(Me.ckRR)
        Me.grpOptions.Controls.Add(Me.ckOR)
        Me.grpOptions.Controls.Add(Me.ckLiddel)
        Me.grpOptions.Controls.Add(Me.ckAssociation)
        Me.grpOptions.Controls.Add(Me.ckChi2)
        Me.grpOptions.Controls.Add(Me.ckFisher)
        Me.grpOptions.Location = New System.Drawing.Point(23, 184)
        Me.grpOptions.Name = "grpOptions"
        Me.grpOptions.Size = New System.Drawing.Size(442, 113)
        Me.grpOptions.TabIndex = 9
        Me.grpOptions.TabStop = False
        Me.grpOptions.Text = "Options"
        '
        'ckRR
        '
        Me.ckRR.AutoSize = True
        Me.ckRR.Checked = True
        Me.ckRR.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckRR.Location = New System.Drawing.Point(197, 84)
        Me.ckRR.Name = "ckRR"
        Me.ckRR.Size = New System.Drawing.Size(91, 20)
        Me.ckRR.TabIndex = 5
        Me.ckRR.Text = "Risk Ratio"
        Me.ckRR.UseVisualStyleBackColor = True
        '
        'ckOR
        '
        Me.ckOR.AutoSize = True
        Me.ckOR.Checked = True
        Me.ckOR.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckOR.Location = New System.Drawing.Point(197, 59)
        Me.ckOR.Name = "ckOR"
        Me.ckOR.Size = New System.Drawing.Size(97, 20)
        Me.ckOR.TabIndex = 4
        Me.ckOR.Text = "Odds Ratio"
        Me.ckOR.UseVisualStyleBackColor = True
        '
        'ckLiddel
        '
        Me.ckLiddel.AutoSize = True
        Me.ckLiddel.Checked = True
        Me.ckLiddel.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckLiddel.Location = New System.Drawing.Point(197, 33)
        Me.ckLiddel.Name = "ckLiddel"
        Me.ckLiddel.Size = New System.Drawing.Size(101, 20)
        Me.ckLiddel.TabIndex = 3
        Me.ckLiddel.Text = "Paired Data"
        Me.ckLiddel.UseVisualStyleBackColor = True
        '
        'ckAssociation
        '
        Me.ckAssociation.AutoSize = True
        Me.ckAssociation.Checked = True
        Me.ckAssociation.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckAssociation.Location = New System.Drawing.Point(8, 84)
        Me.ckAssociation.Name = "ckAssociation"
        Me.ckAssociation.Size = New System.Drawing.Size(177, 20)
        Me.ckAssociation.TabIndex = 2
        Me.ckAssociation.Text = "Cramer's V, Phi, Pearson"
        Me.ckAssociation.UseVisualStyleBackColor = True
        '
        'ckChi2
        '
        Me.ckChi2.AutoSize = True
        Me.ckChi2.Checked = True
        Me.ckChi2.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckChi2.Location = New System.Drawing.Point(7, 59)
        Me.ckChi2.Name = "ckChi2"
        Me.ckChi2.Size = New System.Drawing.Size(85, 20)
        Me.ckChi2.TabIndex = 1
        Me.ckChi2.Text = "Chi2 Test"
        Me.ckChi2.UseVisualStyleBackColor = True
        '
        'ckFisher
        '
        Me.ckFisher.AutoSize = True
        Me.ckFisher.Checked = True
        Me.ckFisher.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckFisher.Location = New System.Drawing.Point(8, 33)
        Me.ckFisher.Name = "ckFisher"
        Me.ckFisher.Size = New System.Drawing.Size(132, 20)
        Me.ckFisher.TabIndex = 0
        Me.ckFisher.Text = "Fisher Exact Test"
        Me.ckFisher.UseVisualStyleBackColor = True
        '
        'Ui82x2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(555, 430)
        Me.Controls.Add(Me.grpOptions)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.grpOutput)
        Me.Controls.Add(Me.grpInput)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MinimizeBox = False
        Me.Name = "Ui82x2"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "2x2 Table"
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        CType(Me.spinBtnD, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnC, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnB, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnA, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpOptions.ResumeLayout(False)
        Me.grpOptions.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents optWorksheetData As Windows.Forms.RadioButton
    Friend WithEvents optScreenData As Windows.Forms.RadioButton
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents spinBtnA As Windows.Forms.NumericUpDown
    Friend WithEvents lblC2 As Windows.Forms.Label
    Friend WithEvents lblC1 As Windows.Forms.Label
    Friend WithEvents lblR2 As Windows.Forms.Label
    Friend WithEvents lblR1 As Windows.Forms.Label
    Friend WithEvents RefEdit1_WorksheetData As Excel2007RefEdit
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents spinBtnD As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnC As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnB As Windows.Forms.NumericUpDown
    Friend WithEvents grpOptions As Windows.Forms.GroupBox
    Friend WithEvents ckChi2 As Windows.Forms.CheckBox
    Friend WithEvents ckFisher As Windows.Forms.CheckBox
    Friend WithEvents ckRR As Windows.Forms.CheckBox
    Friend WithEvents ckOR As Windows.Forms.CheckBox
    Friend WithEvents ckLiddel As Windows.Forms.CheckBox
    Friend WithEvents ckAssociation As Windows.Forms.CheckBox
End Class
