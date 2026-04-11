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
        Me.RefEditOutput = New Global.BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.ckFirstRow = New System.Windows.Forms.CheckBox()
        Me.RefEdit2 = New Global.BESHStatNG.Excel2007RefEdit()
        Me.RefEdit1 = New Global.BESHStatNG.Excel2007RefEdit()
        Me.lblRefedit2 = New System.Windows.Forms.Label()
        Me.lblRefedit1 = New System.Windows.Forms.Label()
        Me.TabPageOptions = New System.Windows.Forms.TabPage()
        Me.lblAlphaGlobal = New System.Windows.Forms.Label()
        Me.spinBtnAlphaGlobal = New System.Windows.Forms.NumericUpDown()
        Me.ckSignTest = New System.Windows.Forms.CheckBox()
        Me.ckDescriptiveStatistics = New System.Windows.Forms.CheckBox()
        Me.TabPageOptionsHotteling = New System.Windows.Forms.TabPage()
        Me.lblAlphaHottelings = New System.Windows.Forms.Label()
        Me.spinBtnAlphaHottelings = New System.Windows.Forms.NumericUpDown()
        Me.grpHottelingTestType = New System.Windows.Forms.GroupBox()
        Me.optIndependent = New System.Windows.Forms.RadioButton()
        Me.optPaired = New System.Windows.Forms.RadioButton()
        Me.optSingle = New System.Windows.Forms.RadioButton()
        Me.TabPageOptionsLinCCC = New System.Windows.Forms.TabPage()
        Me.lblLinCCCInfo = New System.Windows.Forms.Label()
        Me.lblNullConcordanceLinCCC = New System.Windows.Forms.Label()
        Me.spinBtnNullConcordanceLinCCC = New System.Windows.Forms.NumericUpDown()
        Me.grpCItypeLinCCC = New System.Windows.Forms.GroupBox()
        Me.spinBtnBootstrapReplicatesLinCCC = New System.Windows.Forms.NumericUpDown()
        Me.lblBootstrapReplicatesLinCCC = New System.Windows.Forms.Label()
        Me.lblAlphaLinCCC = New System.Windows.Forms.Label()
        Me.spinBtnAlphaLinCCC = New System.Windows.Forms.NumericUpDown()
        Me.optLinCCCBootstrapBCa = New System.Windows.Forms.RadioButton()
        Me.optLinCCCBootstrapPercentile = New System.Windows.Forms.RadioButton()
        Me.optLinCCCAnalytical = New System.Windows.Forms.RadioButton()
        Me.TabPageOptionsKappa = New System.Windows.Forms.TabPage()
        Me.grpCItypeKappa = New System.Windows.Forms.GroupBox()
        Me.spinBtnBootstrapReplicatesKappa = New System.Windows.Forms.NumericUpDown()
        Me.lblBootstrapReplicatesKappa = New System.Windows.Forms.Label()
        Me.lblAlphaKappa = New System.Windows.Forms.Label()
        Me.spinBtnAlphaKappa = New System.Windows.Forms.NumericUpDown()
        Me.optKappaBootstrapBCa = New System.Windows.Forms.RadioButton()
        Me.optKappaBootstrapPercentile = New System.Windows.Forms.RadioButton()
        Me.optKappaAnalytical = New System.Windows.Forms.RadioButton()
        Me.cmbWeightingSchemeKappa = New System.Windows.Forms.ComboBox()
        Me.lblWeightingSchemeKappa = New System.Windows.Forms.Label()
        Me.lblKappaInfo = New System.Windows.Forms.Label()
        Me.TabPageOptionsDeming = New System.Windows.Forms.TabPage()
        Me.lblDemingSDy = New System.Windows.Forms.Label()
        Me.lblDemingSDx = New System.Windows.Forms.Label()
        Me.lblDemingCVy = New System.Windows.Forms.Label()
        Me.spinBtnDemingCVy = New System.Windows.Forms.NumericUpDown()
        Me.lblDemingCVx = New System.Windows.Forms.Label()
        Me.spinBtnDemingCVx = New System.Windows.Forms.NumericUpDown()
        Me.cmbDemingVarianceModel = New System.Windows.Forms.ComboBox()
        Me.lblDemingVarianceModel = New System.Windows.Forms.Label()
        Me.ckDemingFitIntercept = New System.Windows.Forms.CheckBox()
        Me.lblErrorRatio = New System.Windows.Forms.Label()
        Me.spinBtnErrorRatio = New System.Windows.Forms.NumericUpDown()
        Me.grpDemingCItype = New System.Windows.Forms.GroupBox()
        Me.lblAlphaDeming = New System.Windows.Forms.Label()
        Me.spinBtnAlphaDeming = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnBootstrapReplicatesDeming = New System.Windows.Forms.NumericUpDown()
        Me.lblBootstrapReplicatesDeming = New System.Windows.Forms.Label()
        Me.optDemingBootstrapBCa = New System.Windows.Forms.RadioButton()
        Me.optDemingBootstrapPercentile = New System.Windows.Forms.RadioButton()
        Me.optAnalyticalClosedForm = New System.Windows.Forms.RadioButton()
        Me.optJackknife = New System.Windows.Forms.RadioButton()
        Me.optAnalyticalLinnet = New System.Windows.Forms.RadioButton()
        Me.RefEditDemingSDy = New Global.BESHStatNG.Excel2007RefEdit()
        Me.RefEditDemingSDx = New Global.BESHStatNG.Excel2007RefEdit()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.progressBarExactCalc = New System.Windows.Forms.ProgressBar()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPageOptions.SuspendLayout()
        CType(Me.spinBtnAlphaGlobal, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageOptionsHotteling.SuspendLayout()
        CType(Me.spinBtnAlphaHottelings, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpHottelingTestType.SuspendLayout()
        Me.TabPageOptionsLinCCC.SuspendLayout()
        CType(Me.spinBtnNullConcordanceLinCCC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpCItypeLinCCC.SuspendLayout()
        CType(Me.spinBtnBootstrapReplicatesLinCCC, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnAlphaLinCCC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageOptionsKappa.SuspendLayout()
        Me.grpCItypeKappa.SuspendLayout()
        CType(Me.spinBtnBootstrapReplicatesKappa, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnAlphaKappa, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageOptionsDeming.SuspendLayout()
        CType(Me.spinBtnDemingCVy, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnDemingCVx, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnErrorRatio, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpDemingCItype.SuspendLayout()
        CType(Me.spinBtnAlphaDeming, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnBootstrapReplicatesDeming, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1)
        Me.TabMultipage.Controls.Add(Me.TabPageOptions)
        Me.TabMultipage.Controls.Add(Me.TabPageOptionsHotteling)
        Me.TabMultipage.Controls.Add(Me.TabPageOptionsLinCCC)
        Me.TabMultipage.Controls.Add(Me.TabPageOptionsKappa)
        Me.TabMultipage.Controls.Add(Me.TabPageOptionsDeming)
        Me.TabMultipage.Location = New System.Drawing.Point(-1, 0)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(456, 432)
        Me.TabMultipage.TabIndex = 0
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.grpOutput)
        Me.TabPage1.Controls.Add(Me.grpInput)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(448, 403)
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
        Me.grpOutput.Location = New System.Drawing.Point(9, 208)
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
        Me.grpInput.Controls.Add(Me.ckFirstRow)
        Me.grpInput.Controls.Add(Me.RefEdit2)
        Me.grpInput.Controls.Add(Me.RefEdit1)
        Me.grpInput.Controls.Add(Me.lblRefedit2)
        Me.grpInput.Controls.Add(Me.lblRefedit1)
        Me.grpInput.Location = New System.Drawing.Point(9, 19)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(426, 158)
        Me.grpInput.TabIndex = 2
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'ckFirstRow
        '
        Me.ckFirstRow.AutoSize = True
        Me.ckFirstRow.Checked = True
        Me.ckFirstRow.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckFirstRow.Location = New System.Drawing.Point(136, 15)
        Me.ckFirstRow.Name = "ckFirstRow"
        Me.ckFirstRow.Size = New System.Drawing.Size(232, 20)
        Me.ckFirstRow.TabIndex = 26
        Me.ckFirstRow.Text = "1st Row Contains Variable Names"
        Me.ckFirstRow.UseVisualStyleBackColor = True
        Me.ckFirstRow.Visible = False
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
        Me.TabPageOptions.Controls.Add(Me.lblAlphaGlobal)
        Me.TabPageOptions.Controls.Add(Me.spinBtnAlphaGlobal)
        Me.TabPageOptions.Controls.Add(Me.ckSignTest)
        Me.TabPageOptions.Controls.Add(Me.ckDescriptiveStatistics)
        Me.TabPageOptions.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptions.Name = "TabPageOptions"
        Me.TabPageOptions.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageOptions.Size = New System.Drawing.Size(448, 403)
        Me.TabPageOptions.TabIndex = 1
        Me.TabPageOptions.Text = "Options"
        Me.TabPageOptions.UseVisualStyleBackColor = True
        '
        'lblAlphaGlobal
        '
        Me.lblAlphaGlobal.AutoSize = True
        Me.lblAlphaGlobal.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaGlobal.Location = New System.Drawing.Point(16, 75)
        Me.lblAlphaGlobal.Name = "lblAlphaGlobal"
        Me.lblAlphaGlobal.Size = New System.Drawing.Size(41, 16)
        Me.lblAlphaGlobal.TabIndex = 7
        Me.lblAlphaGlobal.Text = "alpha"
        '
        'spinBtnAlphaGlobal
        '
        Me.spinBtnAlphaGlobal.DecimalPlaces = 3
        Me.spinBtnAlphaGlobal.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlphaGlobal.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaGlobal.Location = New System.Drawing.Point(64, 73)
        Me.spinBtnAlphaGlobal.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlphaGlobal.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaGlobal.Name = "spinBtnAlphaGlobal"
        Me.spinBtnAlphaGlobal.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlphaGlobal.TabIndex = 6
        Me.spinBtnAlphaGlobal.Value = New Decimal(New Integer() {5, 0, 0, 131072})
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
        Me.TabPageOptionsHotteling.Controls.Add(Me.lblAlphaHottelings)
        Me.TabPageOptionsHotteling.Controls.Add(Me.spinBtnAlphaHottelings)
        Me.TabPageOptionsHotteling.Controls.Add(Me.grpHottelingTestType)
        Me.TabPageOptionsHotteling.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsHotteling.Name = "TabPageOptionsHotteling"
        Me.TabPageOptionsHotteling.Size = New System.Drawing.Size(448, 403)
        Me.TabPageOptionsHotteling.TabIndex = 2
        Me.TabPageOptionsHotteling.Text = "Options"
        Me.TabPageOptionsHotteling.UseVisualStyleBackColor = True
        '
        'lblAlphaHottelings
        '
        Me.lblAlphaHottelings.AutoSize = True
        Me.lblAlphaHottelings.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaHottelings.Location = New System.Drawing.Point(21, 165)
        Me.lblAlphaHottelings.Name = "lblAlphaHottelings"
        Me.lblAlphaHottelings.Size = New System.Drawing.Size(41, 16)
        Me.lblAlphaHottelings.TabIndex = 5
        Me.lblAlphaHottelings.Text = "alpha"
        '
        'spinBtnAlphaHottelings
        '
        Me.spinBtnAlphaHottelings.DecimalPlaces = 3
        Me.spinBtnAlphaHottelings.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlphaHottelings.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaHottelings.Location = New System.Drawing.Point(69, 163)
        Me.spinBtnAlphaHottelings.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlphaHottelings.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaHottelings.Name = "spinBtnAlphaHottelings"
        Me.spinBtnAlphaHottelings.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlphaHottelings.TabIndex = 4
        Me.spinBtnAlphaHottelings.Value = New Decimal(New Integer() {5, 0, 0, 131072})
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
        'TabPageOptionsLinCCC
        '
        Me.TabPageOptionsLinCCC.Controls.Add(Me.lblLinCCCInfo)
        Me.TabPageOptionsLinCCC.Controls.Add(Me.lblNullConcordanceLinCCC)
        Me.TabPageOptionsLinCCC.Controls.Add(Me.spinBtnNullConcordanceLinCCC)
        Me.TabPageOptionsLinCCC.Controls.Add(Me.grpCItypeLinCCC)
        Me.TabPageOptionsLinCCC.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsLinCCC.Name = "TabPageOptionsLinCCC"
        Me.TabPageOptionsLinCCC.Size = New System.Drawing.Size(448, 403)
        Me.TabPageOptionsLinCCC.TabIndex = 3
        Me.TabPageOptionsLinCCC.Text = "Options"
        Me.TabPageOptionsLinCCC.UseVisualStyleBackColor = True
        '
        'lblLinCCCInfo
        '
        Me.lblLinCCCInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLinCCCInfo.Location = New System.Drawing.Point(6, 31)
        Me.lblLinCCCInfo.Name = "lblLinCCCInfo"
        Me.lblLinCCCInfo.Size = New System.Drawing.Size(436, 38)
        Me.lblLinCCCInfo.TabIndex = 10
        Me.lblLinCCCInfo.Text = "Lin CCC uses two paired numeric columns and reports concordance, Pearson r, bias-" &
    "correction factor, and an identity-line plot."
        '
        'lblNullConcordanceLinCCC
        '
        Me.lblNullConcordanceLinCCC.AutoSize = True
        Me.lblNullConcordanceLinCCC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNullConcordanceLinCCC.Location = New System.Drawing.Point(21, 268)
        Me.lblNullConcordanceLinCCC.Name = "lblNullConcordanceLinCCC"
        Me.lblNullConcordanceLinCCC.Size = New System.Drawing.Size(114, 16)
        Me.lblNullConcordanceLinCCC.TabIndex = 9
        Me.lblNullConcordanceLinCCC.Text = "Null Concordance"
        '
        'spinBtnNullConcordanceLinCCC
        '
        Me.spinBtnNullConcordanceLinCCC.DecimalPlaces = 2
        Me.spinBtnNullConcordanceLinCCC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnNullConcordanceLinCCC.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.spinBtnNullConcordanceLinCCC.Location = New System.Drawing.Point(141, 266)
        Me.spinBtnNullConcordanceLinCCC.Maximum = New Decimal(New Integer() {99, 0, 0, 131072})
        Me.spinBtnNullConcordanceLinCCC.Minimum = New Decimal(New Integer() {99, 0, 0, -2147352576})
        Me.spinBtnNullConcordanceLinCCC.Name = "spinBtnNullConcordanceLinCCC"
        Me.spinBtnNullConcordanceLinCCC.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnNullConcordanceLinCCC.TabIndex = 8
        '
        'grpCItypeLinCCC
        '
        Me.grpCItypeLinCCC.Controls.Add(Me.spinBtnBootstrapReplicatesLinCCC)
        Me.grpCItypeLinCCC.Controls.Add(Me.lblBootstrapReplicatesLinCCC)
        Me.grpCItypeLinCCC.Controls.Add(Me.lblAlphaLinCCC)
        Me.grpCItypeLinCCC.Controls.Add(Me.spinBtnAlphaLinCCC)
        Me.grpCItypeLinCCC.Controls.Add(Me.optLinCCCBootstrapBCa)
        Me.grpCItypeLinCCC.Controls.Add(Me.optLinCCCBootstrapPercentile)
        Me.grpCItypeLinCCC.Controls.Add(Me.optLinCCCAnalytical)
        Me.grpCItypeLinCCC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpCItypeLinCCC.Location = New System.Drawing.Point(9, 88)
        Me.grpCItypeLinCCC.Name = "grpCItypeLinCCC"
        Me.grpCItypeLinCCC.Size = New System.Drawing.Size(433, 160)
        Me.grpCItypeLinCCC.TabIndex = 0
        Me.grpCItypeLinCCC.TabStop = False
        Me.grpCItypeLinCCC.Text = "Confidence Interval Type"
        '
        'spinBtnBootstrapReplicatesLinCCC
        '
        Me.spinBtnBootstrapReplicatesLinCCC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnBootstrapReplicatesLinCCC.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesLinCCC.Location = New System.Drawing.Point(334, 57)
        Me.spinBtnBootstrapReplicatesLinCCC.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesLinCCC.Minimum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesLinCCC.Name = "spinBtnBootstrapReplicatesLinCCC"
        Me.spinBtnBootstrapReplicatesLinCCC.Size = New System.Drawing.Size(93, 22)
        Me.spinBtnBootstrapReplicatesLinCCC.TabIndex = 9
        Me.spinBtnBootstrapReplicatesLinCCC.Value = New Decimal(New Integer() {2000, 0, 0, 0})
        '
        'lblBootstrapReplicatesLinCCC
        '
        Me.lblBootstrapReplicatesLinCCC.AutoSize = True
        Me.lblBootstrapReplicatesLinCCC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBootstrapReplicatesLinCCC.Location = New System.Drawing.Point(195, 59)
        Me.lblBootstrapReplicatesLinCCC.Name = "lblBootstrapReplicatesLinCCC"
        Me.lblBootstrapReplicatesLinCCC.Size = New System.Drawing.Size(133, 16)
        Me.lblBootstrapReplicatesLinCCC.TabIndex = 8
        Me.lblBootstrapReplicatesLinCCC.Text = "Bootstrap Replicates"
        '
        'lblAlphaLinCCC
        '
        Me.lblAlphaLinCCC.AutoSize = True
        Me.lblAlphaLinCCC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaLinCCC.Location = New System.Drawing.Point(13, 125)
        Me.lblAlphaLinCCC.Name = "lblAlphaLinCCC"
        Me.lblAlphaLinCCC.Size = New System.Drawing.Size(41, 16)
        Me.lblAlphaLinCCC.TabIndex = 7
        Me.lblAlphaLinCCC.Text = "alpha"
        '
        'spinBtnAlphaLinCCC
        '
        Me.spinBtnAlphaLinCCC.DecimalPlaces = 3
        Me.spinBtnAlphaLinCCC.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlphaLinCCC.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaLinCCC.Location = New System.Drawing.Point(61, 123)
        Me.spinBtnAlphaLinCCC.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlphaLinCCC.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaLinCCC.Name = "spinBtnAlphaLinCCC"
        Me.spinBtnAlphaLinCCC.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlphaLinCCC.TabIndex = 6
        Me.spinBtnAlphaLinCCC.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'optLinCCCBootstrapBCa
        '
        Me.optLinCCCBootstrapBCa.AutoSize = True
        Me.optLinCCCBootstrapBCa.Enabled = False
        Me.optLinCCCBootstrapBCa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optLinCCCBootstrapBCa.Location = New System.Drawing.Point(16, 83)
        Me.optLinCCCBootstrapBCa.Name = "optLinCCCBootstrapBCa"
        Me.optLinCCCBootstrapBCa.Size = New System.Drawing.Size(115, 20)
        Me.optLinCCCBootstrapBCa.TabIndex = 5
        Me.optLinCCCBootstrapBCa.Text = "Bootstrap BCa"
        Me.optLinCCCBootstrapBCa.UseVisualStyleBackColor = True
        '
        'optLinCCCBootstrapPercentile
        '
        Me.optLinCCCBootstrapPercentile.AutoSize = True
        Me.optLinCCCBootstrapPercentile.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optLinCCCBootstrapPercentile.Location = New System.Drawing.Point(16, 57)
        Me.optLinCCCBootstrapPercentile.Name = "optLinCCCBootstrapPercentile"
        Me.optLinCCCBootstrapPercentile.Size = New System.Drawing.Size(149, 20)
        Me.optLinCCCBootstrapPercentile.TabIndex = 4
        Me.optLinCCCBootstrapPercentile.Text = "Bootstrap Percentile"
        Me.optLinCCCBootstrapPercentile.UseVisualStyleBackColor = True
        '
        'optLinCCCAnalytical
        '
        Me.optLinCCCAnalytical.AutoSize = True
        Me.optLinCCCAnalytical.Checked = True
        Me.optLinCCCAnalytical.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optLinCCCAnalytical.Location = New System.Drawing.Point(16, 31)
        Me.optLinCCCAnalytical.Name = "optLinCCCAnalytical"
        Me.optLinCCCAnalytical.Size = New System.Drawing.Size(231, 20)
        Me.optLinCCCAnalytical.TabIndex = 3
        Me.optLinCCCAnalytical.TabStop = True
        Me.optLinCCCAnalytical.Text = "Analytical (Fisher z approximation)"
        Me.optLinCCCAnalytical.UseVisualStyleBackColor = True
        '
        'TabPageOptionsKappa
        '
        Me.TabPageOptionsKappa.Controls.Add(Me.grpCItypeKappa)
        Me.TabPageOptionsKappa.Controls.Add(Me.cmbWeightingSchemeKappa)
        Me.TabPageOptionsKappa.Controls.Add(Me.lblWeightingSchemeKappa)
        Me.TabPageOptionsKappa.Controls.Add(Me.lblKappaInfo)
        Me.TabPageOptionsKappa.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsKappa.Name = "TabPageOptionsKappa"
        Me.TabPageOptionsKappa.Size = New System.Drawing.Size(448, 403)
        Me.TabPageOptionsKappa.TabIndex = 4
        Me.TabPageOptionsKappa.Text = "Options"
        Me.TabPageOptionsKappa.UseVisualStyleBackColor = True
        '
        'grpCItypeKappa
        '
        Me.grpCItypeKappa.Controls.Add(Me.spinBtnBootstrapReplicatesKappa)
        Me.grpCItypeKappa.Controls.Add(Me.lblBootstrapReplicatesKappa)
        Me.grpCItypeKappa.Controls.Add(Me.lblAlphaKappa)
        Me.grpCItypeKappa.Controls.Add(Me.spinBtnAlphaKappa)
        Me.grpCItypeKappa.Controls.Add(Me.optKappaBootstrapBCa)
        Me.grpCItypeKappa.Controls.Add(Me.optKappaBootstrapPercentile)
        Me.grpCItypeKappa.Controls.Add(Me.optKappaAnalytical)
        Me.grpCItypeKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpCItypeKappa.Location = New System.Drawing.Point(7, 144)
        Me.grpCItypeKappa.Name = "grpCItypeKappa"
        Me.grpCItypeKappa.Size = New System.Drawing.Size(433, 160)
        Me.grpCItypeKappa.TabIndex = 24
        Me.grpCItypeKappa.TabStop = False
        Me.grpCItypeKappa.Text = "Confidence Interval Type"
        '
        'spinBtnBootstrapReplicatesKappa
        '
        Me.spinBtnBootstrapReplicatesKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnBootstrapReplicatesKappa.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesKappa.Location = New System.Drawing.Point(334, 57)
        Me.spinBtnBootstrapReplicatesKappa.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesKappa.Minimum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesKappa.Name = "spinBtnBootstrapReplicatesKappa"
        Me.spinBtnBootstrapReplicatesKappa.Size = New System.Drawing.Size(93, 22)
        Me.spinBtnBootstrapReplicatesKappa.TabIndex = 9
        Me.spinBtnBootstrapReplicatesKappa.Value = New Decimal(New Integer() {2000, 0, 0, 0})
        '
        'lblBootstrapReplicatesKappa
        '
        Me.lblBootstrapReplicatesKappa.AutoSize = True
        Me.lblBootstrapReplicatesKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBootstrapReplicatesKappa.Location = New System.Drawing.Point(195, 59)
        Me.lblBootstrapReplicatesKappa.Name = "lblBootstrapReplicatesKappa"
        Me.lblBootstrapReplicatesKappa.Size = New System.Drawing.Size(133, 16)
        Me.lblBootstrapReplicatesKappa.TabIndex = 8
        Me.lblBootstrapReplicatesKappa.Text = "Bootstrap Replicates"
        '
        'lblAlphaKappa
        '
        Me.lblAlphaKappa.AutoSize = True
        Me.lblAlphaKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaKappa.Location = New System.Drawing.Point(13, 125)
        Me.lblAlphaKappa.Name = "lblAlphaKappa"
        Me.lblAlphaKappa.Size = New System.Drawing.Size(41, 16)
        Me.lblAlphaKappa.TabIndex = 7
        Me.lblAlphaKappa.Text = "alpha"
        '
        'spinBtnAlphaKappa
        '
        Me.spinBtnAlphaKappa.DecimalPlaces = 3
        Me.spinBtnAlphaKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlphaKappa.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaKappa.Location = New System.Drawing.Point(61, 123)
        Me.spinBtnAlphaKappa.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlphaKappa.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaKappa.Name = "spinBtnAlphaKappa"
        Me.spinBtnAlphaKappa.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlphaKappa.TabIndex = 6
        Me.spinBtnAlphaKappa.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'optKappaBootstrapBCa
        '
        Me.optKappaBootstrapBCa.AutoSize = True
        Me.optKappaBootstrapBCa.Enabled = False
        Me.optKappaBootstrapBCa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optKappaBootstrapBCa.Location = New System.Drawing.Point(16, 83)
        Me.optKappaBootstrapBCa.Name = "optKappaBootstrapBCa"
        Me.optKappaBootstrapBCa.Size = New System.Drawing.Size(115, 20)
        Me.optKappaBootstrapBCa.TabIndex = 5
        Me.optKappaBootstrapBCa.Text = "Bootstrap BCa"
        Me.optKappaBootstrapBCa.UseVisualStyleBackColor = True
        '
        'optKappaBootstrapPercentile
        '
        Me.optKappaBootstrapPercentile.AutoSize = True
        Me.optKappaBootstrapPercentile.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optKappaBootstrapPercentile.Location = New System.Drawing.Point(16, 57)
        Me.optKappaBootstrapPercentile.Name = "optKappaBootstrapPercentile"
        Me.optKappaBootstrapPercentile.Size = New System.Drawing.Size(149, 20)
        Me.optKappaBootstrapPercentile.TabIndex = 4
        Me.optKappaBootstrapPercentile.Text = "Bootstrap Percentile"
        Me.optKappaBootstrapPercentile.UseVisualStyleBackColor = True
        '
        'optKappaAnalytical
        '
        Me.optKappaAnalytical.AutoSize = True
        Me.optKappaAnalytical.Checked = True
        Me.optKappaAnalytical.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optKappaAnalytical.Location = New System.Drawing.Point(16, 31)
        Me.optKappaAnalytical.Name = "optKappaAnalytical"
        Me.optKappaAnalytical.Size = New System.Drawing.Size(383, 20)
        Me.optKappaAnalytical.TabIndex = 3
        Me.optKappaAnalytical.TabStop = True
        Me.optKappaAnalytical.Text = "Analytical (asymptotic normal / delta-method approximation)"
        Me.optKappaAnalytical.UseVisualStyleBackColor = True
        '
        'cmbWeightingSchemeKappa
        '
        Me.cmbWeightingSchemeKappa.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbWeightingSchemeKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbWeightingSchemeKappa.Location = New System.Drawing.Point(130, 103)
        Me.cmbWeightingSchemeKappa.Name = "cmbWeightingSchemeKappa"
        Me.cmbWeightingSchemeKappa.Size = New System.Drawing.Size(168, 24)
        Me.cmbWeightingSchemeKappa.TabIndex = 23
        '
        'lblWeightingSchemeKappa
        '
        Me.lblWeightingSchemeKappa.AutoSize = True
        Me.lblWeightingSchemeKappa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblWeightingSchemeKappa.Location = New System.Drawing.Point(6, 106)
        Me.lblWeightingSchemeKappa.Name = "lblWeightingSchemeKappa"
        Me.lblWeightingSchemeKappa.Size = New System.Drawing.Size(118, 16)
        Me.lblWeightingSchemeKappa.TabIndex = 12
        Me.lblWeightingSchemeKappa.Text = "Weighting scheme"
        '
        'lblKappaInfo
        '
        Me.lblKappaInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblKappaInfo.Location = New System.Drawing.Point(6, 24)
        Me.lblKappaInfo.Name = "lblKappaInfo"
        Me.lblKappaInfo.Size = New System.Drawing.Size(436, 51)
        Me.lblKappaInfo.TabIndex = 11
        Me.lblKappaInfo.Text = "Cohen's Kappa uses two paired categorical columns. Choose unweighted kappa for cl" &
    "assic Cohen's Kappa or another weighting scheme for ordered categories."
        '
        'TabPageOptionsDeming
        '
        Me.TabPageOptionsDeming.Controls.Add(Me.lblDemingSDy)
        Me.TabPageOptionsDeming.Controls.Add(Me.lblDemingSDx)
        Me.TabPageOptionsDeming.Controls.Add(Me.lblDemingCVy)
        Me.TabPageOptionsDeming.Controls.Add(Me.spinBtnDemingCVy)
        Me.TabPageOptionsDeming.Controls.Add(Me.lblDemingCVx)
        Me.TabPageOptionsDeming.Controls.Add(Me.spinBtnDemingCVx)
        Me.TabPageOptionsDeming.Controls.Add(Me.cmbDemingVarianceModel)
        Me.TabPageOptionsDeming.Controls.Add(Me.lblDemingVarianceModel)
        Me.TabPageOptionsDeming.Controls.Add(Me.ckDemingFitIntercept)
        Me.TabPageOptionsDeming.Controls.Add(Me.lblErrorRatio)
        Me.TabPageOptionsDeming.Controls.Add(Me.spinBtnErrorRatio)
        Me.TabPageOptionsDeming.Controls.Add(Me.grpDemingCItype)
        Me.TabPageOptionsDeming.Controls.Add(Me.RefEditDemingSDy)
        Me.TabPageOptionsDeming.Controls.Add(Me.RefEditDemingSDx)
        Me.TabPageOptionsDeming.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsDeming.Name = "TabPageOptionsDeming"
        Me.TabPageOptionsDeming.Size = New System.Drawing.Size(448, 403)
        Me.TabPageOptionsDeming.TabIndex = 5
        Me.TabPageOptionsDeming.Text = "Options"
        Me.TabPageOptionsDeming.UseVisualStyleBackColor = True
        '
        'lblDemingSDy
        '
        Me.lblDemingSDy.AutoSize = True
        Me.lblDemingSDy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDemingSDy.Location = New System.Drawing.Point(98, 364)
        Me.lblDemingSDy.Name = "lblDemingSDy"
        Me.lblDemingSDy.Size = New System.Drawing.Size(33, 16)
        Me.lblDemingSDy.TabIndex = 44
        Me.lblDemingSDy.Text = "SDy"
        '
        'lblDemingSDx
        '
        Me.lblDemingSDx.AutoSize = True
        Me.lblDemingSDx.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDemingSDx.Location = New System.Drawing.Point(99, 324)
        Me.lblDemingSDx.Name = "lblDemingSDx"
        Me.lblDemingSDx.Size = New System.Drawing.Size(32, 16)
        Me.lblDemingSDx.TabIndex = 43
        Me.lblDemingSDx.Text = "SDx"
        '
        'lblDemingCVy
        '
        Me.lblDemingCVy.AutoSize = True
        Me.lblDemingCVy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDemingCVy.Location = New System.Drawing.Point(231, 295)
        Me.lblDemingCVy.Name = "lblDemingCVy"
        Me.lblDemingCVy.Size = New System.Drawing.Size(32, 16)
        Me.lblDemingCVy.TabIndex = 40
        Me.lblDemingCVy.Text = "CVy"
        '
        'spinBtnDemingCVy
        '
        Me.spinBtnDemingCVy.DecimalPlaces = 3
        Me.spinBtnDemingCVy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnDemingCVy.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnDemingCVy.Location = New System.Drawing.Point(269, 295)
        Me.spinBtnDemingCVy.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnDemingCVy.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnDemingCVy.Name = "spinBtnDemingCVy"
        Me.spinBtnDemingCVy.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnDemingCVy.TabIndex = 39
        Me.spinBtnDemingCVy.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'lblDemingCVx
        '
        Me.lblDemingCVx.AutoSize = True
        Me.lblDemingCVx.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDemingCVx.Location = New System.Drawing.Point(101, 297)
        Me.lblDemingCVx.Name = "lblDemingCVx"
        Me.lblDemingCVx.Size = New System.Drawing.Size(31, 16)
        Me.lblDemingCVx.TabIndex = 38
        Me.lblDemingCVx.Text = "CVx"
        '
        'spinBtnDemingCVx
        '
        Me.spinBtnDemingCVx.DecimalPlaces = 3
        Me.spinBtnDemingCVx.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnDemingCVx.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnDemingCVx.Location = New System.Drawing.Point(138, 295)
        Me.spinBtnDemingCVx.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnDemingCVx.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnDemingCVx.Name = "spinBtnDemingCVx"
        Me.spinBtnDemingCVx.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnDemingCVx.TabIndex = 37
        Me.spinBtnDemingCVx.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'cmbDemingVarianceModel
        '
        Me.cmbDemingVarianceModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDemingVarianceModel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbDemingVarianceModel.Location = New System.Drawing.Point(117, 220)
        Me.cmbDemingVarianceModel.Name = "cmbDemingVarianceModel"
        Me.cmbDemingVarianceModel.Size = New System.Drawing.Size(168, 24)
        Me.cmbDemingVarianceModel.TabIndex = 36
        '
        'lblDemingVarianceModel
        '
        Me.lblDemingVarianceModel.AutoSize = True
        Me.lblDemingVarianceModel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDemingVarianceModel.Location = New System.Drawing.Point(9, 223)
        Me.lblDemingVarianceModel.Name = "lblDemingVarianceModel"
        Me.lblDemingVarianceModel.Size = New System.Drawing.Size(102, 16)
        Me.lblDemingVarianceModel.TabIndex = 35
        Me.lblDemingVarianceModel.Text = "Variance Model"
        '
        'ckDemingFitIntercept
        '
        Me.ckDemingFitIntercept.AutoSize = True
        Me.ckDemingFitIntercept.Checked = True
        Me.ckDemingFitIntercept.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDemingFitIntercept.Location = New System.Drawing.Point(314, 224)
        Me.ckDemingFitIntercept.Name = "ckDemingFitIntercept"
        Me.ckDemingFitIntercept.Size = New System.Drawing.Size(97, 20)
        Me.ckDemingFitIntercept.TabIndex = 34
        Me.ckDemingFitIntercept.Text = "Fit Intercept"
        Me.ckDemingFitIntercept.UseVisualStyleBackColor = True
        '
        'lblErrorRatio
        '
        Me.lblErrorRatio.AutoSize = True
        Me.lblErrorRatio.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblErrorRatio.Location = New System.Drawing.Point(9, 265)
        Me.lblErrorRatio.Name = "lblErrorRatio"
        Me.lblErrorRatio.Size = New System.Drawing.Size(123, 16)
        Me.lblErrorRatio.TabIndex = 33
        Me.lblErrorRatio.Text = "Error Ratio (σx²/σy²)"
        '
        'spinBtnErrorRatio
        '
        Me.spinBtnErrorRatio.DecimalPlaces = 3
        Me.spinBtnErrorRatio.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnErrorRatio.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnErrorRatio.Location = New System.Drawing.Point(138, 260)
        Me.spinBtnErrorRatio.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnErrorRatio.Name = "spinBtnErrorRatio"
        Me.spinBtnErrorRatio.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnErrorRatio.TabIndex = 32
        Me.spinBtnErrorRatio.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'grpDemingCItype
        '
        Me.grpDemingCItype.Controls.Add(Me.lblAlphaDeming)
        Me.grpDemingCItype.Controls.Add(Me.spinBtnAlphaDeming)
        Me.grpDemingCItype.Controls.Add(Me.spinBtnBootstrapReplicatesDeming)
        Me.grpDemingCItype.Controls.Add(Me.lblBootstrapReplicatesDeming)
        Me.grpDemingCItype.Controls.Add(Me.optDemingBootstrapBCa)
        Me.grpDemingCItype.Controls.Add(Me.optDemingBootstrapPercentile)
        Me.grpDemingCItype.Controls.Add(Me.optAnalyticalClosedForm)
        Me.grpDemingCItype.Controls.Add(Me.optJackknife)
        Me.grpDemingCItype.Controls.Add(Me.optAnalyticalLinnet)
        Me.grpDemingCItype.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpDemingCItype.Location = New System.Drawing.Point(9, 12)
        Me.grpDemingCItype.Name = "grpDemingCItype"
        Me.grpDemingCItype.Size = New System.Drawing.Size(421, 202)
        Me.grpDemingCItype.TabIndex = 29
        Me.grpDemingCItype.TabStop = False
        Me.grpDemingCItype.Text = "Confidence Interval Type"
        '
        'lblAlphaDeming
        '
        Me.lblAlphaDeming.AutoSize = True
        Me.lblAlphaDeming.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaDeming.Location = New System.Drawing.Point(14, 174)
        Me.lblAlphaDeming.Name = "lblAlphaDeming"
        Me.lblAlphaDeming.Size = New System.Drawing.Size(41, 16)
        Me.lblAlphaDeming.TabIndex = 33
        Me.lblAlphaDeming.Text = "alpha"
        '
        'spinBtnAlphaDeming
        '
        Me.spinBtnAlphaDeming.DecimalPlaces = 3
        Me.spinBtnAlphaDeming.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlphaDeming.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaDeming.Location = New System.Drawing.Point(62, 168)
        Me.spinBtnAlphaDeming.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlphaDeming.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaDeming.Name = "spinBtnAlphaDeming"
        Me.spinBtnAlphaDeming.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlphaDeming.TabIndex = 32
        Me.spinBtnAlphaDeming.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'spinBtnBootstrapReplicatesDeming
        '
        Me.spinBtnBootstrapReplicatesDeming.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnBootstrapReplicatesDeming.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesDeming.Location = New System.Drawing.Point(321, 108)
        Me.spinBtnBootstrapReplicatesDeming.Maximum = New Decimal(New Integer() {1000000, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesDeming.Minimum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.spinBtnBootstrapReplicatesDeming.Name = "spinBtnBootstrapReplicatesDeming"
        Me.spinBtnBootstrapReplicatesDeming.Size = New System.Drawing.Size(93, 22)
        Me.spinBtnBootstrapReplicatesDeming.TabIndex = 11
        Me.spinBtnBootstrapReplicatesDeming.Value = New Decimal(New Integer() {2000, 0, 0, 0})
        '
        'lblBootstrapReplicatesDeming
        '
        Me.lblBootstrapReplicatesDeming.AutoSize = True
        Me.lblBootstrapReplicatesDeming.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBootstrapReplicatesDeming.Location = New System.Drawing.Point(182, 110)
        Me.lblBootstrapReplicatesDeming.Name = "lblBootstrapReplicatesDeming"
        Me.lblBootstrapReplicatesDeming.Size = New System.Drawing.Size(133, 16)
        Me.lblBootstrapReplicatesDeming.TabIndex = 10
        Me.lblBootstrapReplicatesDeming.Text = "Bootstrap Replicates"
        '
        'optDemingBootstrapBCa
        '
        Me.optDemingBootstrapBCa.AutoSize = True
        Me.optDemingBootstrapBCa.Enabled = False
        Me.optDemingBootstrapBCa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optDemingBootstrapBCa.Location = New System.Drawing.Point(17, 134)
        Me.optDemingBootstrapBCa.Name = "optDemingBootstrapBCa"
        Me.optDemingBootstrapBCa.Size = New System.Drawing.Size(115, 20)
        Me.optDemingBootstrapBCa.TabIndex = 4
        Me.optDemingBootstrapBCa.Text = "Bootstrap BCa"
        Me.optDemingBootstrapBCa.UseVisualStyleBackColor = True
        '
        'optDemingBootstrapPercentile
        '
        Me.optDemingBootstrapPercentile.AutoSize = True
        Me.optDemingBootstrapPercentile.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optDemingBootstrapPercentile.Location = New System.Drawing.Point(17, 108)
        Me.optDemingBootstrapPercentile.Name = "optDemingBootstrapPercentile"
        Me.optDemingBootstrapPercentile.Size = New System.Drawing.Size(149, 20)
        Me.optDemingBootstrapPercentile.TabIndex = 3
        Me.optDemingBootstrapPercentile.Text = "Bootstrap Percentile"
        Me.optDemingBootstrapPercentile.UseVisualStyleBackColor = True
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
        'RefEditDemingSDy
        '
        Me.RefEditDemingSDy.Address = ""
        Me.RefEditDemingSDy.BackColor = System.Drawing.Color.Transparent
        Me.RefEditDemingSDy.ExcelConnector = Nothing
        Me.RefEditDemingSDy.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEditDemingSDy.ImageMinimized = CType(resources.GetObject("RefEditDemingSDy.ImageMinimized"), System.Drawing.Image)
        Me.RefEditDemingSDy.Location = New System.Drawing.Point(138, 364)
        Me.RefEditDemingSDy.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEditDemingSDy.Name = "RefEditDemingSDy"
        Me.RefEditDemingSDy.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEditDemingSDy.Size = New System.Drawing.Size(260, 32)
        Me.RefEditDemingSDy.TabIndex = 42
        '
        'RefEditDemingSDx
        '
        Me.RefEditDemingSDx.Address = ""
        Me.RefEditDemingSDx.BackColor = System.Drawing.Color.Transparent
        Me.RefEditDemingSDx.ExcelConnector = Nothing
        Me.RefEditDemingSDx.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEditDemingSDx.ImageMinimized = CType(resources.GetObject("RefEditDemingSDx.ImageMinimized"), System.Drawing.Image)
        Me.RefEditDemingSDx.Location = New System.Drawing.Point(138, 324)
        Me.RefEditDemingSDx.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEditDemingSDx.Name = "RefEditDemingSDx"
        Me.RefEditDemingSDx.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEditDemingSDx.Size = New System.Drawing.Size(260, 32)
        Me.RefEditDemingSDx.TabIndex = 41
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(294, 438)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 4
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(375, 438)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 3
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'progressBarExactCalc
        '
        Me.progressBarExactCalc.Location = New System.Drawing.Point(7, 439)
        Me.progressBarExactCalc.Name = "progressBarExactCalc"
        Me.progressBarExactCalc.Size = New System.Drawing.Size(270, 23)
        Me.progressBarExactCalc.TabIndex = 5
        '
        'UiTwoInputRefedits
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(455, 473)
        Me.Controls.Add(Me.progressBarExactCalc)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.TabMultipage)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
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
        CType(Me.spinBtnAlphaGlobal, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageOptionsHotteling.ResumeLayout(False)
        Me.TabPageOptionsHotteling.PerformLayout()
        CType(Me.spinBtnAlphaHottelings, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpHottelingTestType.ResumeLayout(False)
        Me.grpHottelingTestType.PerformLayout()
        Me.TabPageOptionsLinCCC.ResumeLayout(False)
        Me.TabPageOptionsLinCCC.PerformLayout()
        CType(Me.spinBtnNullConcordanceLinCCC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpCItypeLinCCC.ResumeLayout(False)
        Me.grpCItypeLinCCC.PerformLayout()
        CType(Me.spinBtnBootstrapReplicatesLinCCC, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnAlphaLinCCC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageOptionsKappa.ResumeLayout(False)
        Me.TabPageOptionsKappa.PerformLayout()
        Me.grpCItypeKappa.ResumeLayout(False)
        Me.grpCItypeKappa.PerformLayout()
        CType(Me.spinBtnBootstrapReplicatesKappa, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnAlphaKappa, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageOptionsDeming.ResumeLayout(False)
        Me.TabPageOptionsDeming.PerformLayout()
        CType(Me.spinBtnDemingCVy, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnDemingCVx, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnErrorRatio, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpDemingCItype.ResumeLayout(False)
        Me.grpDemingCItype.PerformLayout()
        CType(Me.spinBtnAlphaDeming, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnBootstrapReplicatesDeming, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents TabPageOptions As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents RefEdit2 As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents RefEdit1 As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents lblRefedit2 As Windows.Forms.Label
    Friend WithEvents lblRefedit1 As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents ckDescriptiveStatistics As Windows.Forms.CheckBox
    Friend WithEvents ckSignTest As Windows.Forms.CheckBox
    Friend WithEvents progressBarExactCalc As Windows.Forms.ProgressBar
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents TabPageOptionsHotteling As Windows.Forms.TabPage
    Friend WithEvents grpHottelingTestType As Windows.Forms.GroupBox
    Friend WithEvents optSingle As Windows.Forms.RadioButton
    Friend WithEvents optIndependent As Windows.Forms.RadioButton
    Friend WithEvents optPaired As Windows.Forms.RadioButton
    Friend WithEvents lblAlphaHottelings As Windows.Forms.Label
    Friend WithEvents spinBtnAlphaHottelings As Windows.Forms.NumericUpDown
    Friend WithEvents TabPageOptionsLinCCC As Windows.Forms.TabPage
    Friend WithEvents grpCItypeLinCCC As Windows.Forms.GroupBox
    Friend WithEvents lblBootstrapReplicatesLinCCC As Windows.Forms.Label
    Friend WithEvents lblAlphaLinCCC As Windows.Forms.Label
    Friend WithEvents spinBtnAlphaLinCCC As Windows.Forms.NumericUpDown
    Friend WithEvents optLinCCCBootstrapBCa As Windows.Forms.RadioButton
    Friend WithEvents optLinCCCBootstrapPercentile As Windows.Forms.RadioButton
    Friend WithEvents optLinCCCAnalytical As Windows.Forms.RadioButton
    Friend WithEvents lblLinCCCInfo As Windows.Forms.Label
    Friend WithEvents lblNullConcordanceLinCCC As Windows.Forms.Label
    Friend WithEvents spinBtnNullConcordanceLinCCC As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnBootstrapReplicatesLinCCC As Windows.Forms.NumericUpDown
    Friend WithEvents TabPageOptionsKappa As Windows.Forms.TabPage
    Friend WithEvents lblKappaInfo As Windows.Forms.Label
    Friend WithEvents lblWeightingSchemeKappa As Windows.Forms.Label
    Friend WithEvents cmbWeightingSchemeKappa As Windows.Forms.ComboBox
    Friend WithEvents grpCItypeKappa As Windows.Forms.GroupBox
    Friend WithEvents spinBtnBootstrapReplicatesKappa As Windows.Forms.NumericUpDown
    Friend WithEvents lblBootstrapReplicatesKappa As Windows.Forms.Label
    Friend WithEvents lblAlphaKappa As Windows.Forms.Label
    Friend WithEvents spinBtnAlphaKappa As Windows.Forms.NumericUpDown
    Friend WithEvents optKappaBootstrapBCa As Windows.Forms.RadioButton
    Friend WithEvents optKappaBootstrapPercentile As Windows.Forms.RadioButton
    Friend WithEvents optKappaAnalytical As Windows.Forms.RadioButton
    Friend WithEvents TabPageOptionsDeming As Windows.Forms.TabPage
    Friend WithEvents lblDemingCVy As Windows.Forms.Label
    Friend WithEvents spinBtnDemingCVy As Windows.Forms.NumericUpDown
    Friend WithEvents lblDemingCVx As Windows.Forms.Label
    Friend WithEvents spinBtnDemingCVx As Windows.Forms.NumericUpDown
    Friend WithEvents cmbDemingVarianceModel As Windows.Forms.ComboBox
    Friend WithEvents lblDemingVarianceModel As Windows.Forms.Label
    Friend WithEvents ckDemingFitIntercept As Windows.Forms.CheckBox
    Friend WithEvents lblErrorRatio As Windows.Forms.Label
    Friend WithEvents spinBtnErrorRatio As Windows.Forms.NumericUpDown
    Friend WithEvents grpDemingCItype As Windows.Forms.GroupBox
    Friend WithEvents lblAlphaDeming As Windows.Forms.Label
    Friend WithEvents spinBtnAlphaDeming As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnBootstrapReplicatesDeming As Windows.Forms.NumericUpDown
    Friend WithEvents lblBootstrapReplicatesDeming As Windows.Forms.Label
    Friend WithEvents optDemingBootstrapBCa As Windows.Forms.RadioButton
    Friend WithEvents optDemingBootstrapPercentile As Windows.Forms.RadioButton
    Friend WithEvents optAnalyticalClosedForm As Windows.Forms.RadioButton
    Friend WithEvents optJackknife As Windows.Forms.RadioButton
    Friend WithEvents optAnalyticalLinnet As Windows.Forms.RadioButton
    Friend WithEvents lblDemingSDy As Windows.Forms.Label
    Friend WithEvents lblDemingSDx As Windows.Forms.Label
    Friend WithEvents RefEditDemingSDy As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents RefEditDemingSDx As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents lblAlphaGlobal As Windows.Forms.Label
    Friend WithEvents spinBtnAlphaGlobal As Windows.Forms.NumericUpDown
    Friend WithEvents ckFirstRow As Windows.Forms.CheckBox
End Class
