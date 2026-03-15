<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Ui0OneRefeditMulticol
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui0OneRefeditMulticol))
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.ckLabels = New System.Windows.Forms.CheckBox()
        Me.lblRefedit1 = New System.Windows.Forms.Label()
        Me.RefEdit1 = New Excel2007RefEdit()
        Me.TabPage_Options = New System.Windows.Forms.TabPage()
        Me.grpMCP = New System.Windows.Forms.GroupBox()
        Me.ckTukey = New System.Windows.Forms.CheckBox()
        Me.grpRmANOVAsphericity = New System.Windows.Forms.GroupBox()
        Me.ckGreenhouse = New System.Windows.Forms.CheckBox()
        Me.ckHuyhn = New System.Windows.Forms.CheckBox()
        Me.ckMauchly = New System.Windows.Forms.CheckBox()
        Me.ckBoxPlot = New System.Windows.Forms.CheckBox()
        Me.ckDescriptiveStatistics = New System.Windows.Forms.CheckBox()
        Me.TabPage_OptionsRxC = New System.Windows.Forms.TabPage()
        Me.ckFFH = New System.Windows.Forms.CheckBox()
        Me.ckCochranArmitage = New System.Windows.Forms.CheckBox()
        Me.ckNominalAssociation = New System.Windows.Forms.CheckBox()
        Me.ckOrdinal = New System.Windows.Forms.CheckBox()
        Me.TabPage_OptionsICC = New System.Windows.Forms.TabPage()
        Me.ckRepeatabilityCoefficient = New System.Windows.Forms.CheckBox()
        Me.lblAlphaICC = New System.Windows.Forms.Label()
        Me.spinBtnAlphaICC = New System.Windows.Forms.NumericUpDown()
        Me.grpICCtype = New System.Windows.Forms.GroupBox()
        Me.optICC3k = New System.Windows.Forms.RadioButton()
        Me.optICC31 = New System.Windows.Forms.RadioButton()
        Me.optICC2k = New System.Windows.Forms.RadioButton()
        Me.optICC21 = New System.Windows.Forms.RadioButton()
        Me.optICC1k = New System.Windows.Forms.RadioButton()
        Me.optICC11 = New System.Windows.Forms.RadioButton()
        Me.progressBarExactCalc = New System.Windows.Forms.ProgressBar()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPage_Options.SuspendLayout()
        Me.grpMCP.SuspendLayout()
        Me.grpRmANOVAsphericity.SuspendLayout()
        Me.TabPage_OptionsRxC.SuspendLayout()
        Me.TabPage_OptionsICC.SuspendLayout()
        CType(Me.spinBtnAlphaICC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpICCtype.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1)
        Me.TabMultipage.Controls.Add(Me.TabPage_Options)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsRxC)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsICC)
        Me.TabMultipage.Location = New System.Drawing.Point(12, 13)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(454, 364)
        Me.TabMultipage.TabIndex = 4
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
        Me.grpOutput.Location = New System.Drawing.Point(6, 177)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(429, 130)
        Me.grpOutput.TabIndex = 4
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
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.ckLabels)
        Me.grpInput.Controls.Add(Me.lblRefedit1)
        Me.grpInput.Controls.Add(Me.RefEdit1)
        Me.grpInput.Location = New System.Drawing.Point(6, 6)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(430, 158)
        Me.grpInput.TabIndex = 1
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'ckLabels
        '
        Me.ckLabels.AutoSize = True
        Me.ckLabels.Location = New System.Drawing.Point(140, 100)
        Me.ckLabels.Name = "ckLabels"
        Me.ckLabels.Size = New System.Drawing.Size(245, 20)
        Me.ckLabels.TabIndex = 5
        Me.ckLabels.Text = "Rows and Columns Labels Selected"
        Me.ckLabels.UseVisualStyleBackColor = True
        Me.ckLabels.Visible = False
        '
        'lblRefedit1
        '
        Me.lblRefedit1.AutoSize = True
        Me.lblRefedit1.Location = New System.Drawing.Point(28, 53)
        Me.lblRefedit1.Name = "lblRefedit1"
        Me.lblRefedit1.Size = New System.Drawing.Size(39, 16)
        Me.lblRefedit1.TabIndex = 2
        Me.lblRefedit1.Text = "Data:"
        '
        'RefEdit1
        '
        Me.RefEdit1.Address = ""
        Me.RefEdit1.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit1.ExcelConnector = Nothing
        Me.RefEdit1.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit1.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit1.Location = New System.Drawing.Point(140, 37)
        Me.RefEdit1.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit1.Name = "RefEdit1"
        Me.RefEdit1.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit1.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit1.TabIndex = 4
        '
        'TabPage_Options
        '
        Me.TabPage_Options.Controls.Add(Me.grpMCP)
        Me.TabPage_Options.Controls.Add(Me.grpRmANOVAsphericity)
        Me.TabPage_Options.Controls.Add(Me.ckBoxPlot)
        Me.TabPage_Options.Controls.Add(Me.ckDescriptiveStatistics)
        Me.TabPage_Options.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_Options.Name = "TabPage_Options"
        Me.TabPage_Options.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage_Options.Size = New System.Drawing.Size(446, 335)
        Me.TabPage_Options.TabIndex = 1
        Me.TabPage_Options.Text = "Options"
        Me.TabPage_Options.UseVisualStyleBackColor = True
        '
        'grpMCP
        '
        Me.grpMCP.Controls.Add(Me.ckTukey)
        Me.grpMCP.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpMCP.Location = New System.Drawing.Point(18, 202)
        Me.grpMCP.Name = "grpMCP"
        Me.grpMCP.Size = New System.Drawing.Size(257, 69)
        Me.grpMCP.TabIndex = 4
        Me.grpMCP.TabStop = False
        Me.grpMCP.Text = "Multiple Comparisons"
        Me.grpMCP.Visible = False
        '
        'ckTukey
        '
        Me.ckTukey.AutoSize = True
        Me.ckTukey.Checked = True
        Me.ckTukey.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckTukey.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckTukey.Location = New System.Drawing.Point(18, 31)
        Me.ckTukey.Name = "ckTukey"
        Me.ckTukey.Size = New System.Drawing.Size(67, 20)
        Me.ckTukey.TabIndex = 0
        Me.ckTukey.Text = "Tukey"
        Me.ckTukey.UseVisualStyleBackColor = True
        '
        'grpRmANOVAsphericity
        '
        Me.grpRmANOVAsphericity.Controls.Add(Me.ckGreenhouse)
        Me.grpRmANOVAsphericity.Controls.Add(Me.ckHuyhn)
        Me.grpRmANOVAsphericity.Controls.Add(Me.ckMauchly)
        Me.grpRmANOVAsphericity.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpRmANOVAsphericity.Location = New System.Drawing.Point(18, 75)
        Me.grpRmANOVAsphericity.Name = "grpRmANOVAsphericity"
        Me.grpRmANOVAsphericity.Size = New System.Drawing.Size(257, 121)
        Me.grpRmANOVAsphericity.TabIndex = 3
        Me.grpRmANOVAsphericity.TabStop = False
        Me.grpRmANOVAsphericity.Text = "Sphericity"
        Me.grpRmANOVAsphericity.Visible = False
        '
        'ckGreenhouse
        '
        Me.ckGreenhouse.AutoSize = True
        Me.ckGreenhouse.Checked = True
        Me.ckGreenhouse.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckGreenhouse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckGreenhouse.Location = New System.Drawing.Point(18, 83)
        Me.ckGreenhouse.Name = "ckGreenhouse"
        Me.ckGreenhouse.Size = New System.Drawing.Size(218, 20)
        Me.ckGreenhouse.TabIndex = 2
        Me.ckGreenhouse.Text = "Greenhouse-Geisser Correction"
        Me.ckGreenhouse.UseVisualStyleBackColor = True
        '
        'ckHuyhn
        '
        Me.ckHuyhn.AutoSize = True
        Me.ckHuyhn.Checked = True
        Me.ckHuyhn.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckHuyhn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckHuyhn.Location = New System.Drawing.Point(18, 57)
        Me.ckHuyhn.Name = "ckHuyhn"
        Me.ckHuyhn.Size = New System.Drawing.Size(165, 20)
        Me.ckHuyhn.TabIndex = 1
        Me.ckHuyhn.Text = "Huyhn-Feldt Correction"
        Me.ckHuyhn.UseVisualStyleBackColor = True
        '
        'ckMauchly
        '
        Me.ckMauchly.AutoSize = True
        Me.ckMauchly.Checked = True
        Me.ckMauchly.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMauchly.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMauchly.Location = New System.Drawing.Point(18, 31)
        Me.ckMauchly.Name = "ckMauchly"
        Me.ckMauchly.Size = New System.Drawing.Size(195, 20)
        Me.ckMauchly.TabIndex = 0
        Me.ckMauchly.Text = "Mauchly's Test of Sphericity"
        Me.ckMauchly.UseVisualStyleBackColor = True
        '
        'ckBoxPlot
        '
        Me.ckBoxPlot.AutoSize = True
        Me.ckBoxPlot.Checked = True
        Me.ckBoxPlot.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBoxPlot.Location = New System.Drawing.Point(15, 44)
        Me.ckBoxPlot.Name = "ckBoxPlot"
        Me.ckBoxPlot.Size = New System.Drawing.Size(163, 20)
        Me.ckBoxPlot.TabIndex = 2
        Me.ckBoxPlot.Text = "Box and Whiskers Plot"
        Me.ckBoxPlot.UseVisualStyleBackColor = True
        Me.ckBoxPlot.Visible = False
        '
        'ckDescriptiveStatistics
        '
        Me.ckDescriptiveStatistics.AutoSize = True
        Me.ckDescriptiveStatistics.Checked = True
        Me.ckDescriptiveStatistics.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDescriptiveStatistics.Location = New System.Drawing.Point(15, 18)
        Me.ckDescriptiveStatistics.Name = "ckDescriptiveStatistics"
        Me.ckDescriptiveStatistics.Size = New System.Drawing.Size(177, 20)
        Me.ckDescriptiveStatistics.TabIndex = 1
        Me.ckDescriptiveStatistics.Text = "Full Descriptive Statistics"
        Me.ckDescriptiveStatistics.UseVisualStyleBackColor = True
        Me.ckDescriptiveStatistics.Visible = False
        '
        'TabPage_OptionsRxC
        '
        Me.TabPage_OptionsRxC.Controls.Add(Me.ckFFH)
        Me.TabPage_OptionsRxC.Controls.Add(Me.ckCochranArmitage)
        Me.TabPage_OptionsRxC.Controls.Add(Me.ckNominalAssociation)
        Me.TabPage_OptionsRxC.Controls.Add(Me.ckOrdinal)
        Me.TabPage_OptionsRxC.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsRxC.Name = "TabPage_OptionsRxC"
        Me.TabPage_OptionsRxC.Size = New System.Drawing.Size(446, 335)
        Me.TabPage_OptionsRxC.TabIndex = 2
        Me.TabPage_OptionsRxC.Text = "Options"
        Me.TabPage_OptionsRxC.UseVisualStyleBackColor = True
        '
        'ckFFH
        '
        Me.ckFFH.AutoSize = True
        Me.ckFFH.Checked = True
        Me.ckFFH.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckFFH.Location = New System.Drawing.Point(20, 103)
        Me.ckFFH.Name = "ckFFH"
        Me.ckFFH.Size = New System.Drawing.Size(256, 20)
        Me.ckFFH.TabIndex = 3
        Me.ckFFH.Text = "Try Fisher-Freeman-Halton Exact Test"
        Me.ckFFH.UseVisualStyleBackColor = True
        '
        'ckCochranArmitage
        '
        Me.ckCochranArmitage.AutoSize = True
        Me.ckCochranArmitage.Checked = True
        Me.ckCochranArmitage.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckCochranArmitage.Location = New System.Drawing.Point(20, 77)
        Me.ckCochranArmitage.Name = "ckCochranArmitage"
        Me.ckCochranArmitage.Size = New System.Drawing.Size(238, 20)
        Me.ckCochranArmitage.TabIndex = 2
        Me.ckCochranArmitage.Text = "Cochran-Armitage Test (2xK Table)"
        Me.ckCochranArmitage.UseVisualStyleBackColor = True
        '
        'ckNominalAssociation
        '
        Me.ckNominalAssociation.AutoSize = True
        Me.ckNominalAssociation.Checked = True
        Me.ckNominalAssociation.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckNominalAssociation.Location = New System.Drawing.Point(20, 51)
        Me.ckNominalAssociation.Name = "ckNominalAssociation"
        Me.ckNominalAssociation.Size = New System.Drawing.Size(177, 20)
        Me.ckNominalAssociation.TabIndex = 1
        Me.ckNominalAssociation.Text = "Cramer's V, Phi, Pearson"
        Me.ckNominalAssociation.UseVisualStyleBackColor = True
        '
        'ckOrdinal
        '
        Me.ckOrdinal.AutoSize = True
        Me.ckOrdinal.Checked = True
        Me.ckOrdinal.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckOrdinal.Location = New System.Drawing.Point(20, 25)
        Me.ckOrdinal.Name = "ckOrdinal"
        Me.ckOrdinal.Size = New System.Drawing.Size(145, 20)
        Me.ckOrdinal.TabIndex = 0
        Me.ckOrdinal.Text = "Ordinal Association"
        Me.ckOrdinal.UseVisualStyleBackColor = True
        '
        'TabPage_OptionsICC
        '
        Me.TabPage_OptionsICC.Controls.Add(Me.ckRepeatabilityCoefficient)
        Me.TabPage_OptionsICC.Controls.Add(Me.lblAlphaICC)
        Me.TabPage_OptionsICC.Controls.Add(Me.spinBtnAlphaICC)
        Me.TabPage_OptionsICC.Controls.Add(Me.grpICCtype)
        Me.TabPage_OptionsICC.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsICC.Name = "TabPage_OptionsICC"
        Me.TabPage_OptionsICC.Size = New System.Drawing.Size(446, 335)
        Me.TabPage_OptionsICC.TabIndex = 3
        Me.TabPage_OptionsICC.Text = "Options"
        Me.TabPage_OptionsICC.UseVisualStyleBackColor = True
        '
        'ckRepeatabilityCoefficient
        '
        Me.ckRepeatabilityCoefficient.AutoSize = True
        Me.ckRepeatabilityCoefficient.Checked = True
        Me.ckRepeatabilityCoefficient.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckRepeatabilityCoefficient.Location = New System.Drawing.Point(216, 82)
        Me.ckRepeatabilityCoefficient.Name = "ckRepeatabilityCoefficient"
        Me.ckRepeatabilityCoefficient.Size = New System.Drawing.Size(174, 20)
        Me.ckRepeatabilityCoefficient.TabIndex = 10
        Me.ckRepeatabilityCoefficient.Text = "Repeatability Coefficient"
        Me.ckRepeatabilityCoefficient.UseVisualStyleBackColor = True
        '
        'lblAlphaICC
        '
        Me.lblAlphaICC.AutoSize = True
        Me.lblAlphaICC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaICC.Location = New System.Drawing.Point(211, 43)
        Me.lblAlphaICC.Name = "lblAlphaICC"
        Me.lblAlphaICC.Size = New System.Drawing.Size(42, 16)
        Me.lblAlphaICC.TabIndex = 9
        Me.lblAlphaICC.Text = "Alpha"
        '
        'spinBtnAlphaICC
        '
        Me.spinBtnAlphaICC.DecimalPlaces = 3
        Me.spinBtnAlphaICC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlphaICC.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaICC.Location = New System.Drawing.Point(259, 41)
        Me.spinBtnAlphaICC.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlphaICC.Name = "spinBtnAlphaICC"
        Me.spinBtnAlphaICC.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlphaICC.TabIndex = 8
        Me.spinBtnAlphaICC.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'grpICCtype
        '
        Me.grpICCtype.Controls.Add(Me.optICC3k)
        Me.grpICCtype.Controls.Add(Me.optICC31)
        Me.grpICCtype.Controls.Add(Me.optICC2k)
        Me.grpICCtype.Controls.Add(Me.optICC21)
        Me.grpICCtype.Controls.Add(Me.optICC1k)
        Me.grpICCtype.Controls.Add(Me.optICC11)
        Me.grpICCtype.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpICCtype.Location = New System.Drawing.Point(20, 12)
        Me.grpICCtype.Name = "grpICCtype"
        Me.grpICCtype.Size = New System.Drawing.Size(174, 199)
        Me.grpICCtype.TabIndex = 2
        Me.grpICCtype.TabStop = False
        Me.grpICCtype.Text = "ICC Type"
        '
        'optICC3k
        '
        Me.optICC3k.AutoSize = True
        Me.optICC3k.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optICC3k.Location = New System.Drawing.Point(17, 161)
        Me.optICC3k.Name = "optICC3k"
        Me.optICC3k.Size = New System.Drawing.Size(74, 20)
        Me.optICC3k.TabIndex = 5
        Me.optICC3k.Text = "ICC(3,k)"
        Me.optICC3k.UseVisualStyleBackColor = True
        '
        'optICC31
        '
        Me.optICC31.AutoSize = True
        Me.optICC31.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optICC31.Location = New System.Drawing.Point(17, 135)
        Me.optICC31.Name = "optICC31"
        Me.optICC31.Size = New System.Drawing.Size(74, 20)
        Me.optICC31.TabIndex = 4
        Me.optICC31.Text = "ICC(3,1)"
        Me.optICC31.UseVisualStyleBackColor = True
        '
        'optICC2k
        '
        Me.optICC2k.AutoSize = True
        Me.optICC2k.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optICC2k.Location = New System.Drawing.Point(17, 109)
        Me.optICC2k.Name = "optICC2k"
        Me.optICC2k.Size = New System.Drawing.Size(74, 20)
        Me.optICC2k.TabIndex = 3
        Me.optICC2k.Text = "ICC(2,k)"
        Me.optICC2k.UseVisualStyleBackColor = True
        '
        'optICC21
        '
        Me.optICC21.AutoSize = True
        Me.optICC21.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optICC21.Location = New System.Drawing.Point(17, 83)
        Me.optICC21.Name = "optICC21"
        Me.optICC21.Size = New System.Drawing.Size(74, 20)
        Me.optICC21.TabIndex = 2
        Me.optICC21.Text = "ICC(2,1)"
        Me.optICC21.UseVisualStyleBackColor = True
        '
        'optICC1k
        '
        Me.optICC1k.AutoSize = True
        Me.optICC1k.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optICC1k.Location = New System.Drawing.Point(17, 57)
        Me.optICC1k.Name = "optICC1k"
        Me.optICC1k.Size = New System.Drawing.Size(74, 20)
        Me.optICC1k.TabIndex = 1
        Me.optICC1k.Text = "ICC(1,k)"
        Me.optICC1k.UseVisualStyleBackColor = True
        '
        'optICC11
        '
        Me.optICC11.AutoSize = True
        Me.optICC11.Checked = True
        Me.optICC11.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optICC11.Location = New System.Drawing.Point(17, 31)
        Me.optICC11.Name = "optICC11"
        Me.optICC11.Size = New System.Drawing.Size(74, 20)
        Me.optICC11.TabIndex = 0
        Me.optICC11.TabStop = True
        Me.optICC11.Text = "ICC(1,1)"
        Me.optICC11.UseVisualStyleBackColor = True
        '
        'progressBarExactCalc
        '
        Me.progressBarExactCalc.Location = New System.Drawing.Point(12, 382)
        Me.progressBarExactCalc.Name = "progressBarExactCalc"
        Me.progressBarExactCalc.Size = New System.Drawing.Size(288, 23)
        Me.progressBarExactCalc.TabIndex = 7
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(306, 383)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 6
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(387, 383)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 5
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'Ui0OneRefeditMulticol
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(467, 417)
        Me.Controls.Add(Me.progressBarExactCalc)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.TabMultipage)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(485, 464)
        Me.MinimumSize = New System.Drawing.Size(485, 464)
        Me.Name = "Ui0OneRefeditMulticol"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Ui0OneRefeditMulticol"
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPage_Options.ResumeLayout(False)
        Me.TabPage_Options.PerformLayout()
        Me.grpMCP.ResumeLayout(False)
        Me.grpMCP.PerformLayout()
        Me.grpRmANOVAsphericity.ResumeLayout(False)
        Me.grpRmANOVAsphericity.PerformLayout()
        Me.TabPage_OptionsRxC.ResumeLayout(False)
        Me.TabPage_OptionsRxC.PerformLayout()
        Me.TabPage_OptionsICC.ResumeLayout(False)
        Me.TabPage_OptionsICC.PerformLayout()
        CType(Me.spinBtnAlphaICC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpICCtype.ResumeLayout(False)
        Me.grpICCtype.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents lblRefedit1 As Windows.Forms.Label
    Friend WithEvents RefEdit1 As Excel2007RefEdit
    Friend WithEvents TabPage_Options As Windows.Forms.TabPage
    Friend WithEvents ckBoxPlot As Windows.Forms.CheckBox
    Friend WithEvents ckDescriptiveStatistics As Windows.Forms.CheckBox
    Friend WithEvents progressBarExactCalc As Windows.Forms.ProgressBar
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents grpRmANOVAsphericity As Windows.Forms.GroupBox
    Friend WithEvents grpMCP As Windows.Forms.GroupBox
    Friend WithEvents ckTukey As Windows.Forms.CheckBox
    Friend WithEvents ckGreenhouse As Windows.Forms.CheckBox
    Friend WithEvents ckHuyhn As Windows.Forms.CheckBox
    Friend WithEvents ckMauchly As Windows.Forms.CheckBox
    Friend WithEvents TabPage_OptionsRxC As Windows.Forms.TabPage
    Friend WithEvents ckOrdinal As Windows.Forms.CheckBox
    Friend WithEvents ckFFH As Windows.Forms.CheckBox
    Friend WithEvents ckCochranArmitage As Windows.Forms.CheckBox
    Friend WithEvents ckNominalAssociation As Windows.Forms.CheckBox
    Friend WithEvents ckLabels As Windows.Forms.CheckBox
    Friend WithEvents TabPage_OptionsICC As Windows.Forms.TabPage
    Friend WithEvents grpICCtype As Windows.Forms.GroupBox
    Friend WithEvents optICC21 As Windows.Forms.RadioButton
    Friend WithEvents optICC1k As Windows.Forms.RadioButton
    Friend WithEvents optICC11 As Windows.Forms.RadioButton
    Friend WithEvents optICC3k As Windows.Forms.RadioButton
    Friend WithEvents optICC31 As Windows.Forms.RadioButton
    Friend WithEvents optICC2k As Windows.Forms.RadioButton
    Friend WithEvents lblAlphaICC As Windows.Forms.Label
    Friend WithEvents spinBtnAlphaICC As Windows.Forms.NumericUpDown
    Friend WithEvents ckRepeatabilityCoefficient As Windows.Forms.CheckBox
End Class
