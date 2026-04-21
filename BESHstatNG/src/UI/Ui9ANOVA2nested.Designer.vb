<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Ui9ANOVA2nested
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui9ANOVA2nested))
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPageInput = New System.Windows.Forms.TabPage()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New Global.BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.RefEdit1_Group = New Global.BESHStatNG.Excel2007RefEdit()
        Me.RefEdit2_nested = New Global.BESHStatNG.Excel2007RefEdit()
        Me.RefEdit3_Data = New Global.BESHStatNG.Excel2007RefEdit()
        Me.lblRefedit2_Nested = New System.Windows.Forms.Label()
        Me.lblRefedit3_Data = New System.Windows.Forms.Label()
        Me.lblRefedit1_Group = New System.Windows.Forms.Label()
        Me.TabPageOptionsBlandAltman = New System.Windows.Forms.TabPage()
        Me.grpBlandGeneral = New System.Windows.Forms.GroupBox()
        Me.spinBtnBlandMinPairs = New System.Windows.Forms.NumericUpDown()
        Me.lblBlandMinPairs = New System.Windows.Forms.Label()
        Me.spinBtnBlandMinSubjects = New System.Windows.Forms.NumericUpDown()
        Me.lblBlandMinSubjects = New System.Windows.Forms.Label()
        Me.ckBlandAllowFallback = New System.Windows.Forms.CheckBox()
        Me.ckBlandExcludeSingletonSubjects = New System.Windows.Forms.CheckBox()
        Me.ckBlandCheckProportionalBias = New System.Windows.Forms.CheckBox()
        Me.ckBlandUseTDistribution = New System.Windows.Forms.CheckBox()
        Me.cmbBlandPlotMode = New System.Windows.Forms.ComboBox()
        Me.lblBlandPlotMode = New System.Windows.Forms.Label()
        Me.cmbBlandXAxis = New System.Windows.Forms.ComboBox()
        Me.lblBlandXAxis = New System.Windows.Forms.Label()
        Me.cmbBlandScale = New System.Windows.Forms.ComboBox()
        Me.lblBlandScale = New System.Windows.Forms.Label()
        Me.cmbBlandMode = New System.Windows.Forms.ComboBox()
        Me.lblBlandMode = New System.Windows.Forms.Label()
        Me.grpBlandCI = New System.Windows.Forms.GroupBox()
        Me.optBlandJackknife = New System.Windows.Forms.RadioButton()
        Me.tbBlandBootstrapReps = New System.Windows.Forms.NumericUpDown()
        Me.lblBlandBootstrapReps = New System.Windows.Forms.Label()
        Me.optBlandBootstrapBCa = New System.Windows.Forms.RadioButton()
        Me.optBlandBootstrap = New System.Windows.Forms.RadioButton()
        Me.optBlandAnalytical = New System.Windows.Forms.RadioButton()
        Me.lblBlandAlpha = New System.Windows.Forms.Label()
        Me.spinBtnBlandAlpha = New System.Windows.Forms.NumericUpDown()
        Me.TabPageDecisionLimitsBlandAltman = New System.Windows.Forms.TabPage()
        Me.grpBlandDecisionLimits = New System.Windows.Forms.GroupBox()
        Me.lblBlandDecisionLimitsHelp = New System.Windows.Forms.Label()
        Me.tbBlandUpperAllowable = New System.Windows.Forms.TextBox()
        Me.lblBlandUpperAllowable = New System.Windows.Forms.Label()
        Me.tbBlandLowerAllowable = New System.Windows.Forms.TextBox()
        Me.ckBlandDecisionLimitsEnable = New System.Windows.Forms.CheckBox()
        Me.lblBlandLowerAllowable = New System.Windows.Forms.Label()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabControl1.SuspendLayout()
        Me.TabPageInput.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPageOptionsBlandAltman.SuspendLayout()
        Me.grpBlandGeneral.SuspendLayout()
        CType(Me.spinBtnBlandMinPairs, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnBlandMinSubjects, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpBlandCI.SuspendLayout()
        CType(Me.tbBlandBootstrapReps, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnBlandAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageDecisionLimitsBlandAltman.SuspendLayout()
        Me.grpBlandDecisionLimits.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPageInput)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsBlandAltman)
        Me.TabControl1.Controls.Add(Me.TabPageDecisionLimitsBlandAltman)
        Me.TabControl1.Location = New System.Drawing.Point(-3, -3)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(476, 400)
        Me.TabControl1.TabIndex = 9
        '
        'TabPageInput
        '
        Me.TabPageInput.Controls.Add(Me.grpOutput)
        Me.TabPageInput.Controls.Add(Me.grpInput)
        Me.TabPageInput.Location = New System.Drawing.Point(4, 25)
        Me.TabPageInput.Name = "TabPageInput"
        Me.TabPageInput.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageInput.Size = New System.Drawing.Size(468, 371)
        Me.TabPageInput.TabIndex = 0
        Me.TabPageInput.Text = "Input"
        Me.TabPageInput.UseVisualStyleBackColor = True
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(11, 223)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(448, 130)
        Me.grpOutput.TabIndex = 13
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
        Me.RefEditOutput.Location = New System.Drawing.Point(174, 16)
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
        Me.grpInput.Controls.Add(Me.RefEdit1_Group)
        Me.grpInput.Controls.Add(Me.RefEdit2_nested)
        Me.grpInput.Controls.Add(Me.RefEdit3_Data)
        Me.grpInput.Controls.Add(Me.lblRefedit2_Nested)
        Me.grpInput.Controls.Add(Me.lblRefedit3_Data)
        Me.grpInput.Controls.Add(Me.lblRefedit1_Group)
        Me.grpInput.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpInput.Location = New System.Drawing.Point(6, 24)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(453, 165)
        Me.grpInput.TabIndex = 12
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'RefEdit1_Group
        '
        Me.RefEdit1_Group.Address = ""
        Me.RefEdit1_Group.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit1_Group.ExcelConnector = Nothing
        Me.RefEdit1_Group.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit1_Group.ImageMinimized = CType(resources.GetObject("RefEdit1_Group.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit1_Group.Location = New System.Drawing.Point(155, 37)
        Me.RefEdit1_Group.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit1_Group.Name = "RefEdit1_Group"
        Me.RefEdit1_Group.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit1_Group.Size = New System.Drawing.Size(284, 32)
        Me.RefEdit1_Group.TabIndex = 10
        '
        'RefEdit2_nested
        '
        Me.RefEdit2_nested.Address = ""
        Me.RefEdit2_nested.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit2_nested.ExcelConnector = Nothing
        Me.RefEdit2_nested.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit2_nested.ImageMinimized = CType(resources.GetObject("RefEdit2_nested.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit2_nested.Location = New System.Drawing.Point(155, 77)
        Me.RefEdit2_nested.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit2_nested.Name = "RefEdit2_nested"
        Me.RefEdit2_nested.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit2_nested.Size = New System.Drawing.Size(284, 32)
        Me.RefEdit2_nested.TabIndex = 9
        '
        'RefEdit3_Data
        '
        Me.RefEdit3_Data.Address = ""
        Me.RefEdit3_Data.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit3_Data.ExcelConnector = Nothing
        Me.RefEdit3_Data.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit3_Data.ImageMinimized = CType(resources.GetObject("RefEdit3_Data.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit3_Data.Location = New System.Drawing.Point(155, 117)
        Me.RefEdit3_Data.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit3_Data.Name = "RefEdit3_Data"
        Me.RefEdit3_Data.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit3_Data.Size = New System.Drawing.Size(284, 32)
        Me.RefEdit3_Data.TabIndex = 8
        '
        'lblRefedit2_Nested
        '
        Me.lblRefedit2_Nested.AutoSize = True
        Me.lblRefedit2_Nested.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRefedit2_Nested.Location = New System.Drawing.Point(9, 77)
        Me.lblRefedit2_Nested.Name = "lblRefedit2_Nested"
        Me.lblRefedit2_Nested.Size = New System.Drawing.Size(92, 16)
        Me.lblRefedit2_Nested.TabIndex = 7
        Me.lblRefedit2_Nested.Text = "Nested Factor"
        '
        'lblRefedit3_Data
        '
        Me.lblRefedit3_Data.AutoSize = True
        Me.lblRefedit3_Data.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRefedit3_Data.Location = New System.Drawing.Point(9, 117)
        Me.lblRefedit3_Data.Name = "lblRefedit3_Data"
        Me.lblRefedit3_Data.Size = New System.Drawing.Size(39, 16)
        Me.lblRefedit3_Data.TabIndex = 3
        Me.lblRefedit3_Data.Text = "Data:"
        '
        'lblRefedit1_Group
        '
        Me.lblRefedit1_Group.AutoSize = True
        Me.lblRefedit1_Group.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRefedit1_Group.Location = New System.Drawing.Point(9, 37)
        Me.lblRefedit1_Group.Name = "lblRefedit1_Group"
        Me.lblRefedit1_Group.Size = New System.Drawing.Size(85, 16)
        Me.lblRefedit1_Group.TabIndex = 2
        Me.lblRefedit1_Group.Text = "Group Factor"
        '
        'TabPageOptionsBlandAltman
        '
        Me.TabPageOptionsBlandAltman.Controls.Add(Me.grpBlandGeneral)
        Me.TabPageOptionsBlandAltman.Controls.Add(Me.grpBlandCI)
        Me.TabPageOptionsBlandAltman.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsBlandAltman.Name = "TabPageOptionsBlandAltman"
        Me.TabPageOptionsBlandAltman.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageOptionsBlandAltman.Size = New System.Drawing.Size(468, 371)
        Me.TabPageOptionsBlandAltman.TabIndex = 1
        Me.TabPageOptionsBlandAltman.Text = "Options"
        Me.TabPageOptionsBlandAltman.UseVisualStyleBackColor = True
        '
        'grpBlandGeneral
        '
        Me.grpBlandGeneral.Controls.Add(Me.spinBtnBlandMinPairs)
        Me.grpBlandGeneral.Controls.Add(Me.lblBlandMinPairs)
        Me.grpBlandGeneral.Controls.Add(Me.spinBtnBlandMinSubjects)
        Me.grpBlandGeneral.Controls.Add(Me.lblBlandMinSubjects)
        Me.grpBlandGeneral.Controls.Add(Me.ckBlandAllowFallback)
        Me.grpBlandGeneral.Controls.Add(Me.ckBlandExcludeSingletonSubjects)
        Me.grpBlandGeneral.Controls.Add(Me.ckBlandCheckProportionalBias)
        Me.grpBlandGeneral.Controls.Add(Me.ckBlandUseTDistribution)
        Me.grpBlandGeneral.Controls.Add(Me.cmbBlandPlotMode)
        Me.grpBlandGeneral.Controls.Add(Me.lblBlandPlotMode)
        Me.grpBlandGeneral.Controls.Add(Me.cmbBlandXAxis)
        Me.grpBlandGeneral.Controls.Add(Me.lblBlandXAxis)
        Me.grpBlandGeneral.Controls.Add(Me.cmbBlandScale)
        Me.grpBlandGeneral.Controls.Add(Me.lblBlandScale)
        Me.grpBlandGeneral.Controls.Add(Me.cmbBlandMode)
        Me.grpBlandGeneral.Controls.Add(Me.lblBlandMode)
        Me.grpBlandGeneral.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpBlandGeneral.Location = New System.Drawing.Point(7, 6)
        Me.grpBlandGeneral.Name = "grpBlandGeneral"
        Me.grpBlandGeneral.Size = New System.Drawing.Size(455, 207)
        Me.grpBlandGeneral.TabIndex = 28
        Me.grpBlandGeneral.TabStop = False
        Me.grpBlandGeneral.Text = "General"
        '
        'spinBtnBlandMinPairs
        '
        Me.spinBtnBlandMinPairs.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnBlandMinPairs.Location = New System.Drawing.Point(371, 105)
        Me.spinBtnBlandMinPairs.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.spinBtnBlandMinPairs.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinBtnBlandMinPairs.Name = "spinBtnBlandMinPairs"
        Me.spinBtnBlandMinPairs.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnBlandMinPairs.TabIndex = 42
        Me.spinBtnBlandMinPairs.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'lblBlandMinPairs
        '
        Me.lblBlandMinPairs.AutoSize = True
        Me.lblBlandMinPairs.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandMinPairs.Location = New System.Drawing.Point(324, 83)
        Me.lblBlandMinPairs.Name = "lblBlandMinPairs"
        Me.lblBlandMinPairs.Size = New System.Drawing.Size(114, 16)
        Me.lblBlandMinPairs.TabIndex = 41
        Me.lblBlandMinPairs.Text = "Min pairs / subject"
        '
        'spinBtnBlandMinSubjects
        '
        Me.spinBtnBlandMinSubjects.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnBlandMinSubjects.Location = New System.Drawing.Point(371, 34)
        Me.spinBtnBlandMinSubjects.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.spinBtnBlandMinSubjects.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinBtnBlandMinSubjects.Name = "spinBtnBlandMinSubjects"
        Me.spinBtnBlandMinSubjects.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnBlandMinSubjects.TabIndex = 40
        Me.spinBtnBlandMinSubjects.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'lblBlandMinSubjects
        '
        Me.lblBlandMinSubjects.AutoSize = True
        Me.lblBlandMinSubjects.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandMinSubjects.Location = New System.Drawing.Point(357, 15)
        Me.lblBlandMinSubjects.Name = "lblBlandMinSubjects"
        Me.lblBlandMinSubjects.Size = New System.Drawing.Size(81, 16)
        Me.lblBlandMinSubjects.TabIndex = 39
        Me.lblBlandMinSubjects.Text = "Min subjects"
        '
        'ckBlandAllowFallback
        '
        Me.ckBlandAllowFallback.AutoSize = True
        Me.ckBlandAllowFallback.Checked = True
        Me.ckBlandAllowFallback.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBlandAllowFallback.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBlandAllowFallback.Location = New System.Drawing.Point(8, 173)
        Me.ckBlandAllowFallback.Name = "ckBlandAllowFallback"
        Me.ckBlandAllowFallback.Size = New System.Drawing.Size(221, 20)
        Me.ckBlandAllowFallback.TabIndex = 38
        Me.ckBlandAllowFallback.Text = "Allow fallback to simple analysis"
        Me.ckBlandAllowFallback.UseVisualStyleBackColor = True
        '
        'ckBlandExcludeSingletonSubjects
        '
        Me.ckBlandExcludeSingletonSubjects.AutoSize = True
        Me.ckBlandExcludeSingletonSubjects.Checked = True
        Me.ckBlandExcludeSingletonSubjects.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBlandExcludeSingletonSubjects.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBlandExcludeSingletonSubjects.Location = New System.Drawing.Point(235, 173)
        Me.ckBlandExcludeSingletonSubjects.Name = "ckBlandExcludeSingletonSubjects"
        Me.ckBlandExcludeSingletonSubjects.Size = New System.Drawing.Size(187, 20)
        Me.ckBlandExcludeSingletonSubjects.TabIndex = 37
        Me.ckBlandExcludeSingletonSubjects.Text = "Exclude singleton subjects"
        Me.ckBlandExcludeSingletonSubjects.UseVisualStyleBackColor = True
        '
        'ckBlandCheckProportionalBias
        '
        Me.ckBlandCheckProportionalBias.AutoSize = True
        Me.ckBlandCheckProportionalBias.Checked = True
        Me.ckBlandCheckProportionalBias.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBlandCheckProportionalBias.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBlandCheckProportionalBias.Location = New System.Drawing.Point(235, 147)
        Me.ckBlandCheckProportionalBias.Name = "ckBlandCheckProportionalBias"
        Me.ckBlandCheckProportionalBias.Size = New System.Drawing.Size(171, 20)
        Me.ckBlandCheckProportionalBias.TabIndex = 34
        Me.ckBlandCheckProportionalBias.Text = "Check proportional bias"
        Me.ckBlandCheckProportionalBias.UseVisualStyleBackColor = True
        '
        'ckBlandUseTDistribution
        '
        Me.ckBlandUseTDistribution.AutoSize = True
        Me.ckBlandUseTDistribution.Checked = True
        Me.ckBlandUseTDistribution.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBlandUseTDistribution.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBlandUseTDistribution.Location = New System.Drawing.Point(8, 147)
        Me.ckBlandUseTDistribution.Name = "ckBlandUseTDistribution"
        Me.ckBlandUseTDistribution.Size = New System.Drawing.Size(220, 20)
        Me.ckBlandUseTDistribution.TabIndex = 33
        Me.ckBlandUseTDistribution.Text = "Use t distribution for analytical CI"
        Me.ckBlandUseTDistribution.UseVisualStyleBackColor = True
        '
        'cmbBlandPlotMode
        '
        Me.cmbBlandPlotMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBlandPlotMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbBlandPlotMode.Location = New System.Drawing.Point(55, 105)
        Me.cmbBlandPlotMode.Name = "cmbBlandPlotMode"
        Me.cmbBlandPlotMode.Size = New System.Drawing.Size(225, 24)
        Me.cmbBlandPlotMode.TabIndex = 32
        '
        'lblBlandPlotMode
        '
        Me.lblBlandPlotMode.AutoSize = True
        Me.lblBlandPlotMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandPlotMode.Location = New System.Drawing.Point(6, 108)
        Me.lblBlandPlotMode.Name = "lblBlandPlotMode"
        Me.lblBlandPlotMode.Size = New System.Drawing.Size(30, 16)
        Me.lblBlandPlotMode.TabIndex = 31
        Me.lblBlandPlotMode.Text = "Plot"
        '
        'cmbBlandXAxis
        '
        Me.cmbBlandXAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBlandXAxis.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbBlandXAxis.Location = New System.Drawing.Point(55, 75)
        Me.cmbBlandXAxis.Name = "cmbBlandXAxis"
        Me.cmbBlandXAxis.Size = New System.Drawing.Size(225, 24)
        Me.cmbBlandXAxis.TabIndex = 30
        '
        'lblBlandXAxis
        '
        Me.lblBlandXAxis.AutoSize = True
        Me.lblBlandXAxis.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandXAxis.Location = New System.Drawing.Point(6, 78)
        Me.lblBlandXAxis.Name = "lblBlandXAxis"
        Me.lblBlandXAxis.Size = New System.Drawing.Size(43, 16)
        Me.lblBlandXAxis.TabIndex = 29
        Me.lblBlandXAxis.Text = "X-axis"
        '
        'cmbBlandScale
        '
        Me.cmbBlandScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBlandScale.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbBlandScale.Location = New System.Drawing.Point(54, 45)
        Me.cmbBlandScale.Name = "cmbBlandScale"
        Me.cmbBlandScale.Size = New System.Drawing.Size(226, 24)
        Me.cmbBlandScale.TabIndex = 28
        '
        'lblBlandScale
        '
        Me.lblBlandScale.AutoSize = True
        Me.lblBlandScale.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandScale.Location = New System.Drawing.Point(6, 48)
        Me.lblBlandScale.Name = "lblBlandScale"
        Me.lblBlandScale.Size = New System.Drawing.Size(42, 16)
        Me.lblBlandScale.TabIndex = 27
        Me.lblBlandScale.Text = "Scale"
        '
        'cmbBlandMode
        '
        Me.cmbBlandMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBlandMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbBlandMode.Location = New System.Drawing.Point(54, 15)
        Me.cmbBlandMode.Name = "cmbBlandMode"
        Me.cmbBlandMode.Size = New System.Drawing.Size(226, 24)
        Me.cmbBlandMode.TabIndex = 26
        '
        'lblBlandMode
        '
        Me.lblBlandMode.AutoSize = True
        Me.lblBlandMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandMode.Location = New System.Drawing.Point(6, 18)
        Me.lblBlandMode.Name = "lblBlandMode"
        Me.lblBlandMode.Size = New System.Drawing.Size(42, 16)
        Me.lblBlandMode.TabIndex = 25
        Me.lblBlandMode.Text = "Mode"
        '
        'grpBlandCI
        '
        Me.grpBlandCI.Controls.Add(Me.optBlandJackknife)
        Me.grpBlandCI.Controls.Add(Me.tbBlandBootstrapReps)
        Me.grpBlandCI.Controls.Add(Me.lblBlandBootstrapReps)
        Me.grpBlandCI.Controls.Add(Me.optBlandBootstrapBCa)
        Me.grpBlandCI.Controls.Add(Me.optBlandBootstrap)
        Me.grpBlandCI.Controls.Add(Me.optBlandAnalytical)
        Me.grpBlandCI.Controls.Add(Me.lblBlandAlpha)
        Me.grpBlandCI.Controls.Add(Me.spinBtnBlandAlpha)
        Me.grpBlandCI.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpBlandCI.Location = New System.Drawing.Point(7, 219)
        Me.grpBlandCI.Name = "grpBlandCI"
        Me.grpBlandCI.Size = New System.Drawing.Size(455, 146)
        Me.grpBlandCI.TabIndex = 27
        Me.grpBlandCI.TabStop = False
        Me.grpBlandCI.Text = "Confidence Intervals"
        '
        'optBlandJackknife
        '
        Me.optBlandJackknife.AutoSize = True
        Me.optBlandJackknife.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optBlandJackknife.Location = New System.Drawing.Point(9, 56)
        Me.optBlandJackknife.Name = "optBlandJackknife"
        Me.optBlandJackknife.Size = New System.Drawing.Size(85, 20)
        Me.optBlandJackknife.TabIndex = 10
        Me.optBlandJackknife.Text = "Jackknife"
        Me.optBlandJackknife.UseVisualStyleBackColor = True
        '
        'tbBlandBootstrapReps
        '
        Me.tbBlandBootstrapReps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbBlandBootstrapReps.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        Me.tbBlandBootstrapReps.Location = New System.Drawing.Point(327, 82)
        Me.tbBlandBootstrapReps.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.tbBlandBootstrapReps.Minimum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.tbBlandBootstrapReps.Name = "tbBlandBootstrapReps"
        Me.tbBlandBootstrapReps.Size = New System.Drawing.Size(93, 22)
        Me.tbBlandBootstrapReps.TabIndex = 9
        Me.tbBlandBootstrapReps.Value = New Decimal(New Integer() {2000, 0, 0, 0})
        '
        'lblBlandBootstrapReps
        '
        Me.lblBlandBootstrapReps.AutoSize = True
        Me.lblBlandBootstrapReps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandBootstrapReps.Location = New System.Drawing.Point(188, 84)
        Me.lblBlandBootstrapReps.Name = "lblBlandBootstrapReps"
        Me.lblBlandBootstrapReps.Size = New System.Drawing.Size(133, 16)
        Me.lblBlandBootstrapReps.TabIndex = 8
        Me.lblBlandBootstrapReps.Text = "Bootstrap Replicates"
        '
        'optBlandBootstrapBCa
        '
        Me.optBlandBootstrapBCa.AutoSize = True
        Me.optBlandBootstrapBCa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optBlandBootstrapBCa.Location = New System.Drawing.Point(9, 108)
        Me.optBlandBootstrapBCa.Name = "optBlandBootstrapBCa"
        Me.optBlandBootstrapBCa.Size = New System.Drawing.Size(115, 20)
        Me.optBlandBootstrapBCa.TabIndex = 5
        Me.optBlandBootstrapBCa.Text = "Bootstrap BCa"
        Me.optBlandBootstrapBCa.UseVisualStyleBackColor = True
        '
        'optBlandBootstrap
        '
        Me.optBlandBootstrap.AutoSize = True
        Me.optBlandBootstrap.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optBlandBootstrap.Location = New System.Drawing.Point(9, 82)
        Me.optBlandBootstrap.Name = "optBlandBootstrap"
        Me.optBlandBootstrap.Size = New System.Drawing.Size(149, 20)
        Me.optBlandBootstrap.TabIndex = 4
        Me.optBlandBootstrap.Text = "Bootstrap Percentile"
        Me.optBlandBootstrap.UseVisualStyleBackColor = True
        '
        'optBlandAnalytical
        '
        Me.optBlandAnalytical.AutoSize = True
        Me.optBlandAnalytical.Checked = True
        Me.optBlandAnalytical.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optBlandAnalytical.Location = New System.Drawing.Point(8, 31)
        Me.optBlandAnalytical.Name = "optBlandAnalytical"
        Me.optBlandAnalytical.Size = New System.Drawing.Size(86, 20)
        Me.optBlandAnalytical.TabIndex = 3
        Me.optBlandAnalytical.TabStop = True
        Me.optBlandAnalytical.Text = "Analytical"
        Me.optBlandAnalytical.UseVisualStyleBackColor = True
        '
        'lblBlandAlpha
        '
        Me.lblBlandAlpha.AutoSize = True
        Me.lblBlandAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandAlpha.Location = New System.Drawing.Point(305, 21)
        Me.lblBlandAlpha.Name = "lblBlandAlpha"
        Me.lblBlandAlpha.Size = New System.Drawing.Size(41, 16)
        Me.lblBlandAlpha.TabIndex = 36
        Me.lblBlandAlpha.Text = "alpha"
        '
        'spinBtnBlandAlpha
        '
        Me.spinBtnBlandAlpha.DecimalPlaces = 3
        Me.spinBtnBlandAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnBlandAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnBlandAlpha.Location = New System.Drawing.Point(353, 19)
        Me.spinBtnBlandAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnBlandAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnBlandAlpha.Name = "spinBtnBlandAlpha"
        Me.spinBtnBlandAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnBlandAlpha.TabIndex = 35
        Me.spinBtnBlandAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'TabPageDecisionLimitsBlandAltman
        '
        Me.TabPageDecisionLimitsBlandAltman.Controls.Add(Me.grpBlandDecisionLimits)
        Me.TabPageDecisionLimitsBlandAltman.Location = New System.Drawing.Point(4, 25)
        Me.TabPageDecisionLimitsBlandAltman.Name = "TabPageDecisionLimitsBlandAltman"
        Me.TabPageDecisionLimitsBlandAltman.Size = New System.Drawing.Size(468, 371)
        Me.TabPageDecisionLimitsBlandAltman.TabIndex = 2
        Me.TabPageDecisionLimitsBlandAltman.Text = "Decision Limits"
        Me.TabPageDecisionLimitsBlandAltman.UseVisualStyleBackColor = True
        '
        'grpBlandDecisionLimits
        '
        Me.grpBlandDecisionLimits.Controls.Add(Me.lblBlandDecisionLimitsHelp)
        Me.grpBlandDecisionLimits.Controls.Add(Me.tbBlandUpperAllowable)
        Me.grpBlandDecisionLimits.Controls.Add(Me.lblBlandUpperAllowable)
        Me.grpBlandDecisionLimits.Controls.Add(Me.tbBlandLowerAllowable)
        Me.grpBlandDecisionLimits.Controls.Add(Me.ckBlandDecisionLimitsEnable)
        Me.grpBlandDecisionLimits.Controls.Add(Me.lblBlandLowerAllowable)
        Me.grpBlandDecisionLimits.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpBlandDecisionLimits.Location = New System.Drawing.Point(11, 18)
        Me.grpBlandDecisionLimits.Name = "grpBlandDecisionLimits"
        Me.grpBlandDecisionLimits.Size = New System.Drawing.Size(437, 181)
        Me.grpBlandDecisionLimits.TabIndex = 28
        Me.grpBlandDecisionLimits.TabStop = False
        Me.grpBlandDecisionLimits.Text = "Allowable Bias / Decision Limits"
        '
        'lblBlandDecisionLimitsHelp
        '
        Me.lblBlandDecisionLimitsHelp.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandDecisionLimitsHelp.Location = New System.Drawing.Point(6, 128)
        Me.lblBlandDecisionLimitsHelp.Name = "lblBlandDecisionLimitsHelp"
        Me.lblBlandDecisionLimitsHelp.Size = New System.Drawing.Size(425, 41)
        Me.lblBlandDecisionLimitsHelp.TabIndex = 41
        Me.lblBlandDecisionLimitsHelp.Text = "Uses the original (test - reference) difference scale. For the first UI, enable t" &
    "his only for ordinary paired Bland–Altman analysis on the Raw difference scale."
        '
        'tbBlandUpperAllowable
        '
        Me.tbBlandUpperAllowable.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbBlandUpperAllowable.Location = New System.Drawing.Point(188, 91)
        Me.tbBlandUpperAllowable.Name = "tbBlandUpperAllowable"
        Me.tbBlandUpperAllowable.Size = New System.Drawing.Size(100, 22)
        Me.tbBlandUpperAllowable.TabIndex = 40
        Me.tbBlandUpperAllowable.Text = "5"
        '
        'lblBlandUpperAllowable
        '
        Me.lblBlandUpperAllowable.AutoSize = True
        Me.lblBlandUpperAllowable.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandUpperAllowable.Location = New System.Drawing.Point(6, 94)
        Me.lblBlandUpperAllowable.Name = "lblBlandUpperAllowable"
        Me.lblBlandUpperAllowable.Size = New System.Drawing.Size(178, 16)
        Me.lblBlandUpperAllowable.TabIndex = 39
        Me.lblBlandUpperAllowable.Text = "Upper acceptable difference"
        '
        'tbBlandLowerAllowable
        '
        Me.tbBlandLowerAllowable.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbBlandLowerAllowable.Location = New System.Drawing.Point(188, 63)
        Me.tbBlandLowerAllowable.Name = "tbBlandLowerAllowable"
        Me.tbBlandLowerAllowable.Size = New System.Drawing.Size(100, 22)
        Me.tbBlandLowerAllowable.TabIndex = 38
        Me.tbBlandLowerAllowable.Text = "-5"
        '
        'ckBlandDecisionLimitsEnable
        '
        Me.ckBlandDecisionLimitsEnable.AutoSize = True
        Me.ckBlandDecisionLimitsEnable.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBlandDecisionLimitsEnable.Location = New System.Drawing.Point(6, 33)
        Me.ckBlandDecisionLimitsEnable.Name = "ckBlandDecisionLimitsEnable"
        Me.ckBlandDecisionLimitsEnable.Size = New System.Drawing.Size(209, 20)
        Me.ckBlandDecisionLimitsEnable.TabIndex = 37
        Me.ckBlandDecisionLimitsEnable.Text = "Enable decision-limit reporting"
        Me.ckBlandDecisionLimitsEnable.UseVisualStyleBackColor = True
        '
        'lblBlandLowerAllowable
        '
        Me.lblBlandLowerAllowable.AutoSize = True
        Me.lblBlandLowerAllowable.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBlandLowerAllowable.Location = New System.Drawing.Point(6, 66)
        Me.lblBlandLowerAllowable.Name = "lblBlandLowerAllowable"
        Me.lblBlandLowerAllowable.Size = New System.Drawing.Size(176, 16)
        Me.lblBlandLowerAllowable.TabIndex = 8
        Me.lblBlandLowerAllowable.Text = "Lower acceptable difference"
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(310, 403)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 11
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(391, 403)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 10
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'Ui9ANOVA2nested
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(472, 438)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(490, 485)
        Me.MinimumSize = New System.Drawing.Size(490, 485)
        Me.Name = "Ui9ANOVA2nested"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Ui9ANOVA2nested"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPageInput.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPageOptionsBlandAltman.ResumeLayout(False)
        Me.grpBlandGeneral.ResumeLayout(False)
        Me.grpBlandGeneral.PerformLayout()
        CType(Me.spinBtnBlandMinPairs, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnBlandMinSubjects, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpBlandCI.ResumeLayout(False)
        Me.grpBlandCI.PerformLayout()
        CType(Me.tbBlandBootstrapReps, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnBlandAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageDecisionLimitsBlandAltman.ResumeLayout(False)
        Me.grpBlandDecisionLimits.ResumeLayout(False)
        Me.grpBlandDecisionLimits.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents TabPageInput As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents lblRefedit2_Nested As Windows.Forms.Label
    Friend WithEvents lblRefedit3_Data As Windows.Forms.Label
    Friend WithEvents lblRefedit1_Group As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents TabPageOptionsBlandAltman As Windows.Forms.TabPage
    Friend WithEvents grpBlandCI As Windows.Forms.GroupBox
    Friend WithEvents tbBlandBootstrapReps As Windows.Forms.NumericUpDown
    Friend WithEvents lblBlandBootstrapReps As Windows.Forms.Label
    Friend WithEvents optBlandBootstrapBCa As Windows.Forms.RadioButton
    Friend WithEvents optBlandBootstrap As Windows.Forms.RadioButton
    Friend WithEvents optBlandAnalytical As Windows.Forms.RadioButton
    Friend WithEvents cmbBlandMode As Windows.Forms.ComboBox
    Friend WithEvents lblBlandMode As Windows.Forms.Label
    Friend WithEvents optBlandJackknife As Windows.Forms.RadioButton
    Friend WithEvents grpBlandGeneral As Windows.Forms.GroupBox
    Friend WithEvents cmbBlandScale As Windows.Forms.ComboBox
    Friend WithEvents lblBlandScale As Windows.Forms.Label
    Friend WithEvents cmbBlandXAxis As Windows.Forms.ComboBox
    Friend WithEvents lblBlandXAxis As Windows.Forms.Label
    Friend WithEvents cmbBlandPlotMode As Windows.Forms.ComboBox
    Friend WithEvents lblBlandPlotMode As Windows.Forms.Label
    Friend WithEvents ckBlandUseTDistribution As Windows.Forms.CheckBox
    Friend WithEvents ckBlandCheckProportionalBias As Windows.Forms.CheckBox
    Friend WithEvents lblBlandAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnBlandAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents ckBlandAllowFallback As Windows.Forms.CheckBox
    Friend WithEvents ckBlandExcludeSingletonSubjects As Windows.Forms.CheckBox
    Friend WithEvents spinBtnBlandMinPairs As Windows.Forms.NumericUpDown
    Friend WithEvents lblBlandMinPairs As Windows.Forms.Label
    Friend WithEvents spinBtnBlandMinSubjects As Windows.Forms.NumericUpDown
    Friend WithEvents lblBlandMinSubjects As Windows.Forms.Label
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents RefEdit2_nested As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents RefEdit3_Data As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents RefEdit1_Group As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents TabPageDecisionLimitsBlandAltman As Windows.Forms.TabPage
    Friend WithEvents grpBlandDecisionLimits As Windows.Forms.GroupBox
    Friend WithEvents ckBlandDecisionLimitsEnable As Windows.Forms.CheckBox
    Friend WithEvents lblBlandLowerAllowable As Windows.Forms.Label
    Friend WithEvents lblBlandDecisionLimitsHelp As Windows.Forms.Label
    Friend WithEvents tbBlandUpperAllowable As Windows.Forms.TextBox
    Friend WithEvents lblBlandUpperAllowable As Windows.Forms.Label
    Friend WithEvents tbBlandLowerAllowable As Windows.Forms.TextBox
End Class
