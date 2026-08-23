<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui22MultivariateControlCharts
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
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.ProgressBar = New System.Windows.Forms.ProgressBar()
        Me.btnInterrupt = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1_ChartData = New System.Windows.Forms.TabPage()
        Me.grpWorksheet = New System.Windows.Forms.GroupBox()
        Me.btClearDataRoles = New System.Windows.Forms.Button()
        Me.lblSequence = New System.Windows.Forms.Label()
        Me.lbSequence = New System.Windows.Forms.ListBox()
        Me.btRemoveSequence = New System.Windows.Forms.Button()
        Me.btAddSequence = New System.Windows.Forms.Button()
        Me.lblLabels = New System.Windows.Forms.Label()
        Me.lbLabels = New System.Windows.Forms.ListBox()
        Me.btRemoveLabels = New System.Windows.Forms.Button()
        Me.btAddLabels = New System.Windows.Forms.Button()
        Me.lblExposure = New System.Windows.Forms.Label()
        Me.lbExposure = New System.Windows.Forms.ListBox()
        Me.btRemoveExposure = New System.Windows.Forms.Button()
        Me.btAddExposure = New System.Windows.Forms.Button()
        Me.lblSampleSize = New System.Windows.Forms.Label()
        Me.lbSampleSize = New System.Windows.Forms.ListBox()
        Me.btRemoveSampleSize = New System.Windows.Forms.Button()
        Me.btAddSampleSize = New System.Windows.Forms.Button()
        Me.lblCount = New System.Windows.Forms.Label()
        Me.lbCounts = New System.Windows.Forms.ListBox()
        Me.btRemoveCounts = New System.Windows.Forms.Button()
        Me.btAddCounts = New System.Windows.Forms.Button()
        Me.lblSubgroupID = New System.Windows.Forms.Label()
        Me.lbSubgroupID = New System.Windows.Forms.ListBox()
        Me.btRemoveSubgroupID = New System.Windows.Forms.Button()
        Me.btAddSubgroupID = New System.Windows.Forms.Button()
        Me.lblVariables = New System.Windows.Forms.Label()
        Me.lbVariables = New System.Windows.Forms.ListBox()
        Me.btRemoveVariables = New System.Windows.Forms.Button()
        Me.btAddVariables = New System.Windows.Forms.Button()
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.btReload = New System.Windows.Forms.Button()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.cbSheetsList = New System.Windows.Forms.ComboBox()
        Me.lblSheetsList = New System.Windows.Forms.Label()
        Me.grpChartSelection = New System.Windows.Forms.GroupBox()
        Me.lblDataRequirements = New System.Windows.Forms.Label()
        Me.cbObservationStructure = New System.Windows.Forms.ComboBox()
        Me.lblObservationStructure = New System.Windows.Forms.Label()
        Me.lblChartDescription = New System.Windows.Forms.Label()
        Me.cbChartType = New System.Windows.Forms.ComboBox()
        Me.lblChartType = New System.Windows.Forms.Label()
        Me.TabPage2_ModelLimits = New System.Windows.Forms.TabPage()
        Me.grpHistoricalModel = New System.Windows.Forms.GroupBox()
        Me.btImportHistoricalCovariance = New System.Windows.Forms.Button()
        Me.btClearHistoricalModel = New System.Windows.Forms.Button()
        Me.btImportHistoricalMean = New System.Windows.Forms.Button()
        Me.btRefreshHistoricalVariables = New System.Windows.Forms.Button()
        Me.splitHistoricalModel = New System.Windows.Forms.SplitContainer()
        Me.lblHistoricalMeanGrid = New System.Windows.Forms.Label()
        Me.dgvHistoricalMean = New System.Windows.Forms.DataGridView()
        Me.colHistoricalMeanVariable = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colHistoricalMeanValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lblHistoricalCovarianceGrid = New System.Windows.Forms.Label()
        Me.dgvHistoricalCovariance = New System.Windows.Forms.DataGridView()
        Me.grpModelOptions = New System.Windows.Forms.GroupBox()
        Me.chkUseLowerHotellingLimit = New System.Windows.Forms.CheckBox()
        Me.chkAllowPseudoInverse = New System.Windows.Forms.CheckBox()
        Me.lblModelNote = New System.Windows.Forms.Label()
        Me.spinCovarianceRegularization = New System.Windows.Forms.NumericUpDown()
        Me.lblCovarianceRegularization = New System.Windows.Forms.Label()
        Me.spinControlLimitAlpha = New System.Windows.Forms.NumericUpDown()
        Me.lblControlLimitAlpha = New System.Windows.Forms.Label()
        Me.cbModelSource = New System.Windows.Forms.ComboBox()
        Me.lblModelSource = New System.Windows.Forms.Label()
        Me.cbMissingValuePolicy = New System.Windows.Forms.ComboBox()
        Me.lblMissingValuePolicy = New System.Windows.Forms.Label()
        Me.TabPage3_PhasesExclusions = New System.Windows.Forms.TabPage()
        Me.grpStages = New System.Windows.Forms.GroupBox()
        Me.dgvStages = New System.Windows.Forms.DataGridView()
        Me.colStageID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStageFirstPoint = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStageLastPoint = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStagePhase = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.btClearStages = New System.Windows.Forms.Button()
        Me.btRemoveStage = New System.Windows.Forms.Button()
        Me.btAddStage = New System.Windows.Forms.Button()
        Me.grpExclusions = New System.Windows.Forms.GroupBox()
        Me.btClearExclusions = New System.Windows.Forms.Button()
        Me.btRemoveExclusion = New System.Windows.Forms.Button()
        Me.btAddExclusion = New System.Windows.Forms.Button()
        Me.dgvExclusions = New System.Windows.Forms.DataGridView()
        Me.colExclusionPoint = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colExclusionScope = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colExclusionReason = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.grpQuickPhaseSetup = New System.Windows.Forms.GroupBox()
        Me.rbSinglePhaseII = New System.Windows.Forms.RadioButton()
        Me.btApplyQuickPhaseSetup = New System.Windows.Forms.Button()
        Me.spinLastPhaseIPoint = New System.Windows.Forms.NumericUpDown()
        Me.lblLastPhaseIPoint = New System.Windows.Forms.Label()
        Me.rbPhaseIThenPhaseII = New System.Windows.Forms.RadioButton()
        Me.rbSinglePhaseI = New System.Windows.Forms.RadioButton()
        Me.grpPhaseColumns = New System.Windows.Forms.GroupBox()
        Me.btImportExclusions = New System.Windows.Forms.Button()
        Me.btImportStages = New System.Windows.Forms.Button()
        Me.cbImportedExclusionScope = New System.Windows.Forms.ComboBox()
        Me.lblImportedExclusionScope = New System.Windows.Forms.Label()
        Me.cbExclusionReasonColumn = New System.Windows.Forms.ComboBox()
        Me.lblExclusionReasonColumn = New System.Windows.Forms.Label()
        Me.cbExclusionColumn = New System.Windows.Forms.ComboBox()
        Me.lblExclusionColumn = New System.Windows.Forms.Label()
        Me.cbPhaseColumn = New System.Windows.Forms.ComboBox()
        Me.lblPhaseColumn = New System.Windows.Forms.Label()
        Me.cbStageColumn = New System.Windows.Forms.ComboBox()
        Me.lblStageColumn = New System.Windows.Forms.Label()
        Me.TabPage4_MethodOptions = New System.Windows.Forms.TabPage()
        Me.flpMethodOptions = New System.Windows.Forms.FlowLayoutPanel()
        Me.grpPcaOptions = New System.Windows.Forms.GroupBox()
        Me.lblPcaNote = New System.Windows.Forms.Label()
        Me.spinPcaComponentCount = New System.Windows.Forms.NumericUpDown()
        Me.rbPcaFixedComponents = New System.Windows.Forms.RadioButton()
        Me.spinPcaCumulativeVariance = New System.Windows.Forms.NumericUpDown()
        Me.rbPcaVarianceSelection = New System.Windows.Forms.RadioButton()
        Me.cbPcaMatrix = New System.Windows.Forms.ComboBox()
        Me.lblPcaMatrix = New System.Windows.Forms.Label()
        Me.grpGeneralizedVarianceOptions = New System.Windows.Forms.GroupBox()
        Me.chkSpecifyGvSigmaMultiplier = New System.Windows.Forms.CheckBox()
        Me.lblGvNote = New System.Windows.Forms.Label()
        Me.spinGvSigmaMultiplier = New System.Windows.Forms.NumericUpDown()
        Me.grpMewmaOptions = New System.Windows.Forms.GroupBox()
        Me.chkSpecifyMewmaControlLimit = New System.Windows.Forms.CheckBox()
        Me.lblMewmaNote = New System.Windows.Forms.Label()
        Me.spinMewmaControlLimit = New System.Windows.Forms.NumericUpDown()
        Me.lblMewmaLambda = New System.Windows.Forms.Label()
        Me.spinMewmaLambda = New System.Windows.Forms.NumericUpDown()
        Me.grpMcusumOptions = New System.Windows.Forms.GroupBox()
        Me.lblMcusumDecisionInterval = New System.Windows.Forms.Label()
        Me.lblMcusumNote = New System.Windows.Forms.Label()
        Me.spinMcusumDecisionInterval = New System.Windows.Forms.NumericUpDown()
        Me.lblMcusumReferenceValue = New System.Windows.Forms.Label()
        Me.spinMcusumReferenceValue = New System.Windows.Forms.NumericUpDown()
        Me.grpSequentialResetOptions = New System.Windows.Forms.GroupBox()
        Me.cbSequenceGapBehavior = New System.Windows.Forms.ComboBox()
        Me.chkResetAfterSignal = New System.Windows.Forms.CheckBox()
        Me.chkResetAtPhaseBoundary = New System.Windows.Forms.CheckBox()
        Me.chkResetAtStageBoundary = New System.Windows.Forms.CheckBox()
        Me.lblSequenceGapBehavior = New System.Windows.Forms.Label()
        Me.lblMethodDescription = New System.Windows.Forms.Label()
        Me.TabPage5_OutputAppearance = New System.Windows.Forms.TabPage()
        Me.grpChartDimensions = New System.Windows.Forms.GroupBox()
        Me.spinChartHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblChartHeight = New System.Windows.Forms.Label()
        Me.btResetAppearance = New System.Windows.Forms.Button()
        Me.spinChartWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblChartWidth = New System.Windows.Forms.Label()
        Me.grpChartDisplay = New System.Windows.Forms.GroupBox()
        Me.chkShowStageBoundaries = New System.Windows.Forms.CheckBox()
        Me.chkShowSignalLabels = New System.Windows.Forms.CheckBox()
        Me.chkShowExclusionLabels = New System.Windows.Forms.CheckBox()
        Me.chkShowExcludedPoints = New System.Windows.Forms.CheckBox()
        Me.chkShowLimitLabels = New System.Windows.Forms.CheckBox()
        Me.chkShowLegend = New System.Windows.Forms.CheckBox()
        Me.chkShowPointLabels = New System.Windows.Forms.CheckBox()
        Me.chkShowMajorGridlines = New System.Windows.Forms.CheckBox()
        Me.grpOutputs = New System.Windows.Forms.GroupBox()
        Me.cbDiagnosticsScope = New System.Windows.Forms.ComboBox()
        Me.lblDiagnosticsScope = New System.Windows.Forms.Label()
        Me.chkWriteDiagnostics = New System.Windows.Forms.CheckBox()
        Me.chkWriteModelDetails = New System.Windows.Forms.CheckBox()
        Me.chkWriteSettingsAudit = New System.Windows.Forms.CheckBox()
        Me.chkWriteSummary = New System.Windows.Forms.CheckBox()
        Me.chkCreateControlChart = New System.Windows.Forms.CheckBox()
        Me.chkWriteSignals = New System.Windows.Forms.CheckBox()
        Me.chkWriteChartData = New System.Windows.Forms.CheckBox()
        Me.grpTitleAxes = New System.Windows.Forms.GroupBox()
        Me.tbValueNumberFormat = New System.Windows.Forms.TextBox()
        Me.lblValueNumberFormat = New System.Windows.Forms.Label()
        Me.cbHorizontalTickOrientation = New System.Windows.Forms.ComboBox()
        Me.lblHorizontalTickOrientation = New System.Windows.Forms.Label()
        Me.chkUseSequenceValuesForHorizontalAxis = New System.Windows.Forms.CheckBox()
        Me.lblHorizontalAxisTitle = New System.Windows.Forms.Label()
        Me.tbHorizontalAxisTitle = New System.Windows.Forms.TextBox()
        Me.lblValueAxisTitle = New System.Windows.Forms.Label()
        Me.tbValueAxisTitle = New System.Windows.Forms.TextBox()
        Me.lblChartTitle = New System.Windows.Forms.Label()
        Me.tbChartTitle = New System.Windows.Forms.TextBox()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1_ChartData.SuspendLayout()
        Me.grpWorksheet.SuspendLayout()
        Me.grpChartSelection.SuspendLayout()
        Me.TabPage2_ModelLimits.SuspendLayout()
        Me.grpHistoricalModel.SuspendLayout()
        CType(Me.splitHistoricalModel, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitHistoricalModel.Panel1.SuspendLayout()
        Me.splitHistoricalModel.Panel2.SuspendLayout()
        Me.splitHistoricalModel.SuspendLayout()
        CType(Me.dgvHistoricalMean, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvHistoricalCovariance, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpModelOptions.SuspendLayout()
        CType(Me.spinCovarianceRegularization, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinControlLimitAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage3_PhasesExclusions.SuspendLayout()
        Me.grpStages.SuspendLayout()
        CType(Me.dgvStages, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpExclusions.SuspendLayout()
        CType(Me.dgvExclusions, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpQuickPhaseSetup.SuspendLayout()
        CType(Me.spinLastPhaseIPoint, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpPhaseColumns.SuspendLayout()
        Me.TabPage4_MethodOptions.SuspendLayout()
        Me.flpMethodOptions.SuspendLayout()
        Me.grpPcaOptions.SuspendLayout()
        CType(Me.spinPcaComponentCount, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinPcaCumulativeVariance, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpGeneralizedVarianceOptions.SuspendLayout()
        CType(Me.spinGvSigmaMultiplier, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpMewmaOptions.SuspendLayout()
        CType(Me.spinMewmaControlLimit, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinMewmaLambda, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpMcusumOptions.SuspendLayout()
        CType(Me.spinMcusumDecisionInterval, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinMcusumReferenceValue, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpSequentialResetOptions.SuspendLayout()
        Me.TabPage5_OutputAppearance.SuspendLayout()
        Me.grpChartDimensions.SuspendLayout()
        CType(Me.spinChartHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinChartWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpChartDisplay.SuspendLayout()
        Me.grpOutputs.SuspendLayout()
        Me.grpTitleAxes.SuspendLayout()
        Me.SuspendLayout()
        '
        'ProgressBar
        '
        Me.ProgressBar.Location = New System.Drawing.Point(9, 780)
        Me.ProgressBar.Name = "ProgressBar"
        Me.ProgressBar.Size = New System.Drawing.Size(620, 23)
        Me.ProgressBar.TabIndex = 13
        '
        'btnInterrupt
        '
        Me.btnInterrupt.Enabled = False
        Me.btnInterrupt.Location = New System.Drawing.Point(716, 780)
        Me.btnInterrupt.Name = "btnInterrupt"
        Me.btnInterrupt.Size = New System.Drawing.Size(75, 23)
        Me.btnInterrupt.TabIndex = 12
        Me.btnInterrupt.Text = "Interrupt"
        Me.btnInterrupt.UseVisualStyleBackColor = True
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1_ChartData)
        Me.TabControl1.Controls.Add(Me.TabPage2_ModelLimits)
        Me.TabControl1.Controls.Add(Me.TabPage3_PhasesExclusions)
        Me.TabControl1.Controls.Add(Me.TabPage4_MethodOptions)
        Me.TabControl1.Controls.Add(Me.TabPage5_OutputAppearance)
        Me.TabControl1.Location = New System.Drawing.Point(3, 4)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(879, 770)
        Me.TabControl1.TabIndex = 11
        '
        'TabPage1_ChartData
        '
        Me.TabPage1_ChartData.AutoScroll = True
        Me.TabPage1_ChartData.Controls.Add(Me.grpWorksheet)
        Me.TabPage1_ChartData.Controls.Add(Me.grpChartSelection)
        Me.TabPage1_ChartData.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1_ChartData.Name = "TabPage1_ChartData"
        Me.TabPage1_ChartData.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1_ChartData.Size = New System.Drawing.Size(871, 741)
        Me.TabPage1_ChartData.TabIndex = 0
        Me.TabPage1_ChartData.Text = "Chart and Data"
        Me.TabPage1_ChartData.UseVisualStyleBackColor = True
        '
        'grpWorksheet
        '
        Me.grpWorksheet.Controls.Add(Me.btClearDataRoles)
        Me.grpWorksheet.Controls.Add(Me.lblSequence)
        Me.grpWorksheet.Controls.Add(Me.lbSequence)
        Me.grpWorksheet.Controls.Add(Me.btRemoveSequence)
        Me.grpWorksheet.Controls.Add(Me.btAddSequence)
        Me.grpWorksheet.Controls.Add(Me.lblLabels)
        Me.grpWorksheet.Controls.Add(Me.lbLabels)
        Me.grpWorksheet.Controls.Add(Me.btRemoveLabels)
        Me.grpWorksheet.Controls.Add(Me.btAddLabels)
        Me.grpWorksheet.Controls.Add(Me.lblExposure)
        Me.grpWorksheet.Controls.Add(Me.lbExposure)
        Me.grpWorksheet.Controls.Add(Me.btRemoveExposure)
        Me.grpWorksheet.Controls.Add(Me.btAddExposure)
        Me.grpWorksheet.Controls.Add(Me.lblSampleSize)
        Me.grpWorksheet.Controls.Add(Me.lbSampleSize)
        Me.grpWorksheet.Controls.Add(Me.btRemoveSampleSize)
        Me.grpWorksheet.Controls.Add(Me.btAddSampleSize)
        Me.grpWorksheet.Controls.Add(Me.lblCount)
        Me.grpWorksheet.Controls.Add(Me.lbCounts)
        Me.grpWorksheet.Controls.Add(Me.btRemoveCounts)
        Me.grpWorksheet.Controls.Add(Me.btAddCounts)
        Me.grpWorksheet.Controls.Add(Me.lblSubgroupID)
        Me.grpWorksheet.Controls.Add(Me.lbSubgroupID)
        Me.grpWorksheet.Controls.Add(Me.btRemoveSubgroupID)
        Me.grpWorksheet.Controls.Add(Me.btAddSubgroupID)
        Me.grpWorksheet.Controls.Add(Me.lblVariables)
        Me.grpWorksheet.Controls.Add(Me.lbVariables)
        Me.grpWorksheet.Controls.Add(Me.btRemoveVariables)
        Me.grpWorksheet.Controls.Add(Me.btAddVariables)
        Me.grpWorksheet.Controls.Add(Me.lbAllColumns)
        Me.grpWorksheet.Controls.Add(Me.btReload)
        Me.grpWorksheet.Controls.Add(Me.lblAllColumns)
        Me.grpWorksheet.Controls.Add(Me.cbSheetsList)
        Me.grpWorksheet.Controls.Add(Me.lblSheetsList)
        Me.grpWorksheet.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpWorksheet.Location = New System.Drawing.Point(6, 110)
        Me.grpWorksheet.Name = "grpWorksheet"
        Me.grpWorksheet.Size = New System.Drawing.Size(859, 514)
        Me.grpWorksheet.TabIndex = 2
        Me.grpWorksheet.TabStop = False
        Me.grpWorksheet.Text = "Worksheet variables"
        '
        'btClearDataRoles
        '
        Me.btClearDataRoles.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btClearDataRoles.Location = New System.Drawing.Point(715, 29)
        Me.btClearDataRoles.Name = "btClearDataRoles"
        Me.btClearDataRoles.Size = New System.Drawing.Size(138, 23)
        Me.btClearDataRoles.TabIndex = 50
        Me.btClearDataRoles.Text = "Clear assignments"
        Me.btClearDataRoles.UseVisualStyleBackColor = True
        '
        'lblSequence
        '
        Me.lblSequence.AutoSize = True
        Me.lblSequence.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSequence.Location = New System.Drawing.Point(496, 452)
        Me.lblSequence.Name = "lblSequence"
        Me.lblSequence.Size = New System.Drawing.Size(129, 16)
        Me.lblSequence.TabIndex = 49
        Me.lblSequence.Text = "Sequence/date/time"
        '
        'lbSequence
        '
        Me.lbSequence.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSequence.FormattingEnabled = True
        Me.lbSequence.ItemHeight = 16
        Me.lbSequence.Location = New System.Drawing.Point(496, 474)
        Me.lbSequence.Name = "lbSequence"
        Me.lbSequence.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSequence.Size = New System.Drawing.Size(357, 20)
        Me.lbSequence.TabIndex = 48
        '
        'btRemoveSequence
        '
        Me.btRemoveSequence.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveSequence.Location = New System.Drawing.Point(451, 471)
        Me.btRemoveSequence.Name = "btRemoveSequence"
        Me.btRemoveSequence.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveSequence.TabIndex = 47
        Me.btRemoveSequence.Text = "<<"
        Me.btRemoveSequence.UseVisualStyleBackColor = True
        '
        'btAddSequence
        '
        Me.btAddSequence.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddSequence.Location = New System.Drawing.Point(406, 471)
        Me.btAddSequence.Name = "btAddSequence"
        Me.btAddSequence.Size = New System.Drawing.Size(39, 23)
        Me.btAddSequence.TabIndex = 46
        Me.btAddSequence.Text = ">>"
        Me.btAddSequence.UseVisualStyleBackColor = True
        '
        'lblLabels
        '
        Me.lblLabels.AutoSize = True
        Me.lblLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLabels.Location = New System.Drawing.Point(496, 406)
        Me.lblLabels.Name = "lblLabels"
        Me.lblLabels.Size = New System.Drawing.Size(87, 16)
        Me.lblLabels.TabIndex = 45
        Me.lblLabels.Text = "Sample label"
        '
        'lbLabels
        '
        Me.lbLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbLabels.FormattingEnabled = True
        Me.lbLabels.ItemHeight = 16
        Me.lbLabels.Location = New System.Drawing.Point(496, 428)
        Me.lbLabels.Name = "lbLabels"
        Me.lbLabels.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbLabels.Size = New System.Drawing.Size(357, 20)
        Me.lbLabels.TabIndex = 44
        '
        'btRemoveLabels
        '
        Me.btRemoveLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveLabels.Location = New System.Drawing.Point(451, 425)
        Me.btRemoveLabels.Name = "btRemoveLabels"
        Me.btRemoveLabels.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveLabels.TabIndex = 43
        Me.btRemoveLabels.Text = "<<"
        Me.btRemoveLabels.UseVisualStyleBackColor = True
        '
        'btAddLabels
        '
        Me.btAddLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddLabels.Location = New System.Drawing.Point(406, 425)
        Me.btAddLabels.Name = "btAddLabels"
        Me.btAddLabels.Size = New System.Drawing.Size(39, 23)
        Me.btAddLabels.TabIndex = 42
        Me.btAddLabels.Text = ">>"
        Me.btAddLabels.UseVisualStyleBackColor = True
        '
        'lblExposure
        '
        Me.lblExposure.AutoSize = True
        Me.lblExposure.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblExposure.Location = New System.Drawing.Point(496, 362)
        Me.lblExposure.Name = "lblExposure"
        Me.lblExposure.Size = New System.Drawing.Size(145, 16)
        Me.lblExposure.TabIndex = 41
        Me.lblExposure.Text = "Exposure/opportunities"
        '
        'lbExposure
        '
        Me.lbExposure.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbExposure.FormattingEnabled = True
        Me.lbExposure.ItemHeight = 16
        Me.lbExposure.Location = New System.Drawing.Point(496, 384)
        Me.lbExposure.Name = "lbExposure"
        Me.lbExposure.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbExposure.Size = New System.Drawing.Size(357, 20)
        Me.lbExposure.TabIndex = 40
        '
        'btRemoveExposure
        '
        Me.btRemoveExposure.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveExposure.Location = New System.Drawing.Point(451, 381)
        Me.btRemoveExposure.Name = "btRemoveExposure"
        Me.btRemoveExposure.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveExposure.TabIndex = 39
        Me.btRemoveExposure.Text = "<<"
        Me.btRemoveExposure.UseVisualStyleBackColor = True
        '
        'btAddExposure
        '
        Me.btAddExposure.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddExposure.Location = New System.Drawing.Point(406, 381)
        Me.btAddExposure.Name = "btAddExposure"
        Me.btAddExposure.Size = New System.Drawing.Size(39, 23)
        Me.btAddExposure.TabIndex = 38
        Me.btAddExposure.Text = ">>"
        Me.btAddExposure.UseVisualStyleBackColor = True
        '
        'lblSampleSize
        '
        Me.lblSampleSize.AutoSize = True
        Me.lblSampleSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSampleSize.Location = New System.Drawing.Point(496, 316)
        Me.lblSampleSize.Name = "lblSampleSize"
        Me.lblSampleSize.Size = New System.Drawing.Size(81, 16)
        Me.lblSampleSize.TabIndex = 37
        Me.lblSampleSize.Text = "Sample size"
        '
        'lbSampleSize
        '
        Me.lbSampleSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSampleSize.FormattingEnabled = True
        Me.lbSampleSize.ItemHeight = 16
        Me.lbSampleSize.Location = New System.Drawing.Point(496, 338)
        Me.lbSampleSize.Name = "lbSampleSize"
        Me.lbSampleSize.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSampleSize.Size = New System.Drawing.Size(357, 20)
        Me.lbSampleSize.TabIndex = 36
        '
        'btRemoveSampleSize
        '
        Me.btRemoveSampleSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveSampleSize.Location = New System.Drawing.Point(451, 335)
        Me.btRemoveSampleSize.Name = "btRemoveSampleSize"
        Me.btRemoveSampleSize.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveSampleSize.TabIndex = 35
        Me.btRemoveSampleSize.Text = "<<"
        Me.btRemoveSampleSize.UseVisualStyleBackColor = True
        '
        'btAddSampleSize
        '
        Me.btAddSampleSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddSampleSize.Location = New System.Drawing.Point(406, 335)
        Me.btAddSampleSize.Name = "btAddSampleSize"
        Me.btAddSampleSize.Size = New System.Drawing.Size(39, 23)
        Me.btAddSampleSize.TabIndex = 34
        Me.btAddSampleSize.Text = ">>"
        Me.btAddSampleSize.UseVisualStyleBackColor = True
        '
        'lblCount
        '
        Me.lblCount.AutoSize = True
        Me.lblCount.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCount.Location = New System.Drawing.Point(496, 270)
        Me.lblCount.Name = "lblCount"
        Me.lblCount.Size = New System.Drawing.Size(41, 16)
        Me.lblCount.TabIndex = 29
        Me.lblCount.Text = "Count"
        '
        'lbCounts
        '
        Me.lbCounts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbCounts.FormattingEnabled = True
        Me.lbCounts.ItemHeight = 16
        Me.lbCounts.Location = New System.Drawing.Point(496, 292)
        Me.lbCounts.Name = "lbCounts"
        Me.lbCounts.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbCounts.Size = New System.Drawing.Size(357, 20)
        Me.lbCounts.TabIndex = 28
        '
        'btRemoveCounts
        '
        Me.btRemoveCounts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveCounts.Location = New System.Drawing.Point(451, 289)
        Me.btRemoveCounts.Name = "btRemoveCounts"
        Me.btRemoveCounts.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveCounts.TabIndex = 27
        Me.btRemoveCounts.Text = "<<"
        Me.btRemoveCounts.UseVisualStyleBackColor = True
        '
        'btAddCounts
        '
        Me.btAddCounts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddCounts.Location = New System.Drawing.Point(406, 289)
        Me.btAddCounts.Name = "btAddCounts"
        Me.btAddCounts.Size = New System.Drawing.Size(39, 23)
        Me.btAddCounts.TabIndex = 26
        Me.btAddCounts.Text = ">>"
        Me.btAddCounts.UseVisualStyleBackColor = True
        '
        'lblSubgroupID
        '
        Me.lblSubgroupID.AutoSize = True
        Me.lblSubgroupID.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSubgroupID.Location = New System.Drawing.Point(496, 222)
        Me.lblSubgroupID.Name = "lblSubgroupID"
        Me.lblSubgroupID.Size = New System.Drawing.Size(82, 16)
        Me.lblSubgroupID.TabIndex = 25
        Me.lblSubgroupID.Text = "Subgroup ID"
        '
        'lbSubgroupID
        '
        Me.lbSubgroupID.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbSubgroupID.FormattingEnabled = True
        Me.lbSubgroupID.ItemHeight = 16
        Me.lbSubgroupID.Location = New System.Drawing.Point(496, 246)
        Me.lbSubgroupID.Name = "lbSubgroupID"
        Me.lbSubgroupID.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSubgroupID.Size = New System.Drawing.Size(357, 20)
        Me.lbSubgroupID.TabIndex = 24
        '
        'btRemoveSubgroupID
        '
        Me.btRemoveSubgroupID.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveSubgroupID.Location = New System.Drawing.Point(451, 243)
        Me.btRemoveSubgroupID.Name = "btRemoveSubgroupID"
        Me.btRemoveSubgroupID.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveSubgroupID.TabIndex = 23
        Me.btRemoveSubgroupID.Text = "<<"
        Me.btRemoveSubgroupID.UseVisualStyleBackColor = True
        '
        'btAddSubgroupID
        '
        Me.btAddSubgroupID.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddSubgroupID.Location = New System.Drawing.Point(406, 243)
        Me.btAddSubgroupID.Name = "btAddSubgroupID"
        Me.btAddSubgroupID.Size = New System.Drawing.Size(39, 23)
        Me.btAddSubgroupID.TabIndex = 22
        Me.btAddSubgroupID.Text = ">>"
        Me.btAddSubgroupID.UseVisualStyleBackColor = True
        '
        'lblVariables
        '
        Me.lblVariables.AutoSize = True
        Me.lblVariables.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVariables.Location = New System.Drawing.Point(496, 80)
        Me.lblVariables.Name = "lblVariables"
        Me.lblVariables.Size = New System.Drawing.Size(215, 16)
        Me.lblVariables.TabIndex = 21
        Me.lblVariables.Text = "Measurement variables (2 or more)"
        '
        'lbVariables
        '
        Me.lbVariables.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbVariables.FormattingEnabled = True
        Me.lbVariables.ItemHeight = 16
        Me.lbVariables.Location = New System.Drawing.Point(496, 100)
        Me.lbVariables.Name = "lbVariables"
        Me.lbVariables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbVariables.Size = New System.Drawing.Size(357, 116)
        Me.lbVariables.TabIndex = 20
        '
        'btRemoveVariables
        '
        Me.btRemoveVariables.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveVariables.Location = New System.Drawing.Point(451, 97)
        Me.btRemoveVariables.Name = "btRemoveVariables"
        Me.btRemoveVariables.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveVariables.TabIndex = 19
        Me.btRemoveVariables.Text = "<<"
        Me.btRemoveVariables.UseVisualStyleBackColor = True
        '
        'btAddVariables
        '
        Me.btAddVariables.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddVariables.Location = New System.Drawing.Point(406, 97)
        Me.btAddVariables.Name = "btAddVariables"
        Me.btAddVariables.Size = New System.Drawing.Size(39, 23)
        Me.btAddVariables.TabIndex = 18
        Me.btAddVariables.Text = ">>"
        Me.btAddVariables.UseVisualStyleBackColor = True
        '
        'lbAllColumns
        '
        Me.lbAllColumns.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbAllColumns.FormattingEnabled = True
        Me.lbAllColumns.ItemHeight = 16
        Me.lbAllColumns.Location = New System.Drawing.Point(154, 81)
        Me.lbAllColumns.Name = "lbAllColumns"
        Me.lbAllColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbAllColumns.Size = New System.Drawing.Size(239, 420)
        Me.lbAllColumns.TabIndex = 17
        '
        'btReload
        '
        Me.btReload.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btReload.Location = New System.Drawing.Point(398, 27)
        Me.btReload.Name = "btReload"
        Me.btReload.Size = New System.Drawing.Size(75, 23)
        Me.btReload.TabIndex = 9
        Me.btReload.Text = "Reload"
        Me.btReload.UseVisualStyleBackColor = True
        '
        'lblAllColumns
        '
        Me.lblAllColumns.AutoSize = True
        Me.lblAllColumns.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAllColumns.Location = New System.Drawing.Point(8, 81)
        Me.lblAllColumns.Name = "lblAllColumns"
        Me.lblAllColumns.Size = New System.Drawing.Size(117, 16)
        Me.lblAllColumns.TabIndex = 16
        Me.lblAllColumns.Text = "Available columns"
        '
        'cbSheetsList
        '
        Me.cbSheetsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbSheetsList.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbSheetsList.FormattingEnabled = True
        Me.cbSheetsList.Location = New System.Drawing.Point(153, 26)
        Me.cbSheetsList.Name = "cbSheetsList"
        Me.cbSheetsList.Size = New System.Drawing.Size(239, 24)
        Me.cbSheetsList.TabIndex = 13
        '
        'lblSheetsList
        '
        Me.lblSheetsList.AutoSize = True
        Me.lblSheetsList.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSheetsList.Location = New System.Drawing.Point(6, 29)
        Me.lblSheetsList.Name = "lblSheetsList"
        Me.lblSheetsList.Size = New System.Drawing.Size(72, 16)
        Me.lblSheetsList.TabIndex = 12
        Me.lblSheetsList.Text = "Worksheet"
        '
        'grpChartSelection
        '
        Me.grpChartSelection.Controls.Add(Me.lblDataRequirements)
        Me.grpChartSelection.Controls.Add(Me.cbObservationStructure)
        Me.grpChartSelection.Controls.Add(Me.lblObservationStructure)
        Me.grpChartSelection.Controls.Add(Me.lblChartDescription)
        Me.grpChartSelection.Controls.Add(Me.cbChartType)
        Me.grpChartSelection.Controls.Add(Me.lblChartType)
        Me.grpChartSelection.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpChartSelection.Location = New System.Drawing.Point(6, 6)
        Me.grpChartSelection.Name = "grpChartSelection"
        Me.grpChartSelection.Size = New System.Drawing.Size(859, 98)
        Me.grpChartSelection.TabIndex = 1
        Me.grpChartSelection.TabStop = False
        Me.grpChartSelection.Text = "Multivariate control chart"
        '
        'lblDataRequirements
        '
        Me.lblDataRequirements.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDataRequirements.Location = New System.Drawing.Point(395, 56)
        Me.lblDataRequirements.Name = "lblDataRequirements"
        Me.lblDataRequirements.Size = New System.Drawing.Size(458, 31)
        Me.lblDataRequirements.TabIndex = 52
        Me.lblDataRequirements.Text = "Multiline chart-specific requirements"
        '
        'cbObservationStructure
        '
        Me.cbObservationStructure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbObservationStructure.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbObservationStructure.FormattingEnabled = True
        Me.cbObservationStructure.Location = New System.Drawing.Point(153, 56)
        Me.cbObservationStructure.Name = "cbObservationStructure"
        Me.cbObservationStructure.Size = New System.Drawing.Size(239, 24)
        Me.cbObservationStructure.TabIndex = 20
        '
        'lblObservationStructure
        '
        Me.lblObservationStructure.AutoSize = True
        Me.lblObservationStructure.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblObservationStructure.Location = New System.Drawing.Point(6, 59)
        Me.lblObservationStructure.Name = "lblObservationStructure"
        Me.lblObservationStructure.Size = New System.Drawing.Size(133, 16)
        Me.lblObservationStructure.TabIndex = 19
        Me.lblObservationStructure.Text = "Observation structure"
        '
        'lblChartDescription
        '
        Me.lblChartDescription.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChartDescription.Location = New System.Drawing.Point(395, 18)
        Me.lblChartDescription.Name = "lblChartDescription"
        Me.lblChartDescription.Size = New System.Drawing.Size(458, 34)
        Me.lblChartDescription.TabIndex = 18
        Me.lblChartDescription.Text = "Description of the selected chart"
        '
        'cbChartType
        '
        Me.cbChartType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbChartType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbChartType.FormattingEnabled = True
        Me.cbChartType.Location = New System.Drawing.Point(153, 26)
        Me.cbChartType.Name = "cbChartType"
        Me.cbChartType.Size = New System.Drawing.Size(239, 24)
        Me.cbChartType.TabIndex = 15
        '
        'lblChartType
        '
        Me.lblChartType.AutoSize = True
        Me.lblChartType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChartType.Location = New System.Drawing.Point(6, 29)
        Me.lblChartType.Name = "lblChartType"
        Me.lblChartType.Size = New System.Drawing.Size(67, 16)
        Me.lblChartType.TabIndex = 14
        Me.lblChartType.Text = "Chart type"
        '
        'TabPage2_ModelLimits
        '
        Me.TabPage2_ModelLimits.AutoScroll = True
        Me.TabPage2_ModelLimits.Controls.Add(Me.grpHistoricalModel)
        Me.TabPage2_ModelLimits.Controls.Add(Me.grpModelOptions)
        Me.TabPage2_ModelLimits.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2_ModelLimits.Name = "TabPage2_ModelLimits"
        Me.TabPage2_ModelLimits.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2_ModelLimits.Size = New System.Drawing.Size(871, 741)
        Me.TabPage2_ModelLimits.TabIndex = 1
        Me.TabPage2_ModelLimits.Text = "Model and Limits"
        Me.TabPage2_ModelLimits.UseVisualStyleBackColor = True
        '
        'grpHistoricalModel
        '
        Me.grpHistoricalModel.Controls.Add(Me.btImportHistoricalCovariance)
        Me.grpHistoricalModel.Controls.Add(Me.btClearHistoricalModel)
        Me.grpHistoricalModel.Controls.Add(Me.btImportHistoricalMean)
        Me.grpHistoricalModel.Controls.Add(Me.btRefreshHistoricalVariables)
        Me.grpHistoricalModel.Controls.Add(Me.splitHistoricalModel)
        Me.grpHistoricalModel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHistoricalModel.Location = New System.Drawing.Point(6, 221)
        Me.grpHistoricalModel.Name = "grpHistoricalModel"
        Me.grpHistoricalModel.Size = New System.Drawing.Size(858, 368)
        Me.grpHistoricalModel.TabIndex = 29
        Me.grpHistoricalModel.TabStop = False
        Me.grpHistoricalModel.Text = "Historical model"
        '
        'btImportHistoricalCovariance
        '
        Me.btImportHistoricalCovariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btImportHistoricalCovariance.Location = New System.Drawing.Point(398, 21)
        Me.btImportHistoricalCovariance.Name = "btImportHistoricalCovariance"
        Me.btImportHistoricalCovariance.Size = New System.Drawing.Size(287, 23)
        Me.btImportHistoricalCovariance.TabIndex = 4
        Me.btImportHistoricalCovariance.Text = "Import covariance from selected Excel range"
        Me.btImportHistoricalCovariance.UseVisualStyleBackColor = True
        '
        'btClearHistoricalModel
        '
        Me.btClearHistoricalModel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btClearHistoricalModel.Location = New System.Drawing.Point(691, 21)
        Me.btClearHistoricalModel.Name = "btClearHistoricalModel"
        Me.btClearHistoricalModel.Size = New System.Drawing.Size(149, 23)
        Me.btClearHistoricalModel.TabIndex = 3
        Me.btClearHistoricalModel.Text = "Clear historical model"
        Me.btClearHistoricalModel.UseVisualStyleBackColor = True
        '
        'btImportHistoricalMean
        '
        Me.btImportHistoricalMean.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btImportHistoricalMean.Location = New System.Drawing.Point(140, 21)
        Me.btImportHistoricalMean.Name = "btImportHistoricalMean"
        Me.btImportHistoricalMean.Size = New System.Drawing.Size(252, 23)
        Me.btImportHistoricalMean.TabIndex = 2
        Me.btImportHistoricalMean.Text = "Import mean from selected Excel range"
        Me.btImportHistoricalMean.UseVisualStyleBackColor = True
        '
        'btRefreshHistoricalVariables
        '
        Me.btRefreshHistoricalVariables.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRefreshHistoricalVariables.Location = New System.Drawing.Point(9, 21)
        Me.btRefreshHistoricalVariables.Name = "btRefreshHistoricalVariables"
        Me.btRefreshHistoricalVariables.Size = New System.Drawing.Size(125, 23)
        Me.btRefreshHistoricalVariables.TabIndex = 1
        Me.btRefreshHistoricalVariables.Text = "Refresh variables"
        Me.btRefreshHistoricalVariables.UseVisualStyleBackColor = True
        '
        'splitHistoricalModel
        '
        Me.splitHistoricalModel.Location = New System.Drawing.Point(6, 58)
        Me.splitHistoricalModel.Name = "splitHistoricalModel"
        Me.splitHistoricalModel.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'splitHistoricalModel.Panel1
        '
        Me.splitHistoricalModel.Panel1.Controls.Add(Me.lblHistoricalMeanGrid)
        Me.splitHistoricalModel.Panel1.Controls.Add(Me.dgvHistoricalMean)
        '
        'splitHistoricalModel.Panel2
        '
        Me.splitHistoricalModel.Panel2.Controls.Add(Me.lblHistoricalCovarianceGrid)
        Me.splitHistoricalModel.Panel2.Controls.Add(Me.dgvHistoricalCovariance)
        Me.splitHistoricalModel.Size = New System.Drawing.Size(846, 300)
        Me.splitHistoricalModel.SplitterDistance = 250
        Me.splitHistoricalModel.TabIndex = 0
        '
        'lblHistoricalMeanGrid
        '
        Me.lblHistoricalMeanGrid.AutoSize = True
        Me.lblHistoricalMeanGrid.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHistoricalMeanGrid.Location = New System.Drawing.Point(3, 5)
        Me.lblHistoricalMeanGrid.Name = "lblHistoricalMeanGrid"
        Me.lblHistoricalMeanGrid.Size = New System.Drawing.Size(140, 16)
        Me.lblHistoricalMeanGrid.TabIndex = 1
        Me.lblHistoricalMeanGrid.Text = "Historical mean vector"
        '
        'dgvHistoricalMean
        '
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvHistoricalMean.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        Me.dgvHistoricalMean.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvHistoricalMean.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colHistoricalMeanVariable, Me.colHistoricalMeanValue})
        Me.dgvHistoricalMean.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvHistoricalMean.Location = New System.Drawing.Point(0, 0)
        Me.dgvHistoricalMean.Name = "dgvHistoricalMean"
        Me.dgvHistoricalMean.RowHeadersWidth = 51
        Me.dgvHistoricalMean.RowTemplate.Height = 24
        Me.dgvHistoricalMean.Size = New System.Drawing.Size(846, 250)
        Me.dgvHistoricalMean.TabIndex = 0
        '
        'colHistoricalMeanVariable
        '
        Me.colHistoricalMeanVariable.HeaderText = "Variable name"
        Me.colHistoricalMeanVariable.MinimumWidth = 6
        Me.colHistoricalMeanVariable.Name = "colHistoricalMeanVariable"
        Me.colHistoricalMeanVariable.ReadOnly = True
        Me.colHistoricalMeanVariable.Width = 125
        '
        'colHistoricalMeanValue
        '
        Me.colHistoricalMeanValue.HeaderText = "Numeric mean"
        Me.colHistoricalMeanValue.MinimumWidth = 6
        Me.colHistoricalMeanValue.Name = "colHistoricalMeanValue"
        Me.colHistoricalMeanValue.Width = 125
        '
        'lblHistoricalCovarianceGrid
        '
        Me.lblHistoricalCovarianceGrid.AutoSize = True
        Me.lblHistoricalCovarianceGrid.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHistoricalCovarianceGrid.Location = New System.Drawing.Point(3, 2)
        Me.lblHistoricalCovarianceGrid.Name = "lblHistoricalCovarianceGrid"
        Me.lblHistoricalCovarianceGrid.Size = New System.Drawing.Size(171, 16)
        Me.lblHistoricalCovarianceGrid.TabIndex = 2
        Me.lblHistoricalCovarianceGrid.Text = "Historical covariance matrix"
        '
        'dgvHistoricalCovariance
        '
        Me.dgvHistoricalCovariance.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvHistoricalCovariance.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvHistoricalCovariance.Location = New System.Drawing.Point(0, 0)
        Me.dgvHistoricalCovariance.Name = "dgvHistoricalCovariance"
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvHistoricalCovariance.RowHeadersDefaultCellStyle = DataGridViewCellStyle2
        Me.dgvHistoricalCovariance.RowHeadersWidth = 51
        Me.dgvHistoricalCovariance.RowTemplate.Height = 24
        Me.dgvHistoricalCovariance.Size = New System.Drawing.Size(846, 46)
        Me.dgvHistoricalCovariance.TabIndex = 0
        '
        'grpModelOptions
        '
        Me.grpModelOptions.Controls.Add(Me.chkUseLowerHotellingLimit)
        Me.grpModelOptions.Controls.Add(Me.chkAllowPseudoInverse)
        Me.grpModelOptions.Controls.Add(Me.lblModelNote)
        Me.grpModelOptions.Controls.Add(Me.spinCovarianceRegularization)
        Me.grpModelOptions.Controls.Add(Me.lblCovarianceRegularization)
        Me.grpModelOptions.Controls.Add(Me.spinControlLimitAlpha)
        Me.grpModelOptions.Controls.Add(Me.lblControlLimitAlpha)
        Me.grpModelOptions.Controls.Add(Me.cbModelSource)
        Me.grpModelOptions.Controls.Add(Me.lblModelSource)
        Me.grpModelOptions.Controls.Add(Me.cbMissingValuePolicy)
        Me.grpModelOptions.Controls.Add(Me.lblMissingValuePolicy)
        Me.grpModelOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpModelOptions.Location = New System.Drawing.Point(7, 6)
        Me.grpModelOptions.Name = "grpModelOptions"
        Me.grpModelOptions.Size = New System.Drawing.Size(858, 209)
        Me.grpModelOptions.TabIndex = 5
        Me.grpModelOptions.TabStop = False
        Me.grpModelOptions.Text = "Model options"
        '
        'chkUseLowerHotellingLimit
        '
        Me.chkUseLowerHotellingLimit.AutoSize = True
        Me.chkUseLowerHotellingLimit.Checked = True
        Me.chkUseLowerHotellingLimit.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUseLowerHotellingLimit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkUseLowerHotellingLimit.Location = New System.Drawing.Point(510, 56)
        Me.chkUseLowerHotellingLimit.Name = "chkUseLowerHotellingLimit"
        Me.chkUseLowerHotellingLimit.Size = New System.Drawing.Size(174, 20)
        Me.chkUseLowerHotellingLimit.TabIndex = 30
        Me.chkUseLowerHotellingLimit.Text = "Use lower T² control limit"
        Me.chkUseLowerHotellingLimit.UseVisualStyleBackColor = True
        '
        'chkAllowPseudoInverse
        '
        Me.chkAllowPseudoInverse.AutoSize = True
        Me.chkAllowPseudoInverse.Checked = True
        Me.chkAllowPseudoInverse.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAllowPseudoInverse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkAllowPseudoInverse.Location = New System.Drawing.Point(510, 26)
        Me.chkAllowPseudoInverse.Name = "chkAllowPseudoInverse"
        Me.chkAllowPseudoInverse.Size = New System.Drawing.Size(325, 20)
        Me.chkAllowPseudoInverse.TabIndex = 29
        Me.chkAllowPseudoInverse.Text = "Allow pseudoinverse for rank-deficient covariance"
        Me.chkAllowPseudoInverse.UseVisualStyleBackColor = True
        '
        'lblModelNote
        '
        Me.lblModelNote.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblModelNote.Location = New System.Drawing.Point(6, 147)
        Me.lblModelNote.Name = "lblModelNote"
        Me.lblModelNote.Size = New System.Drawing.Size(851, 49)
        Me.lblModelNote.TabIndex = 27
        Me.lblModelNote.Text = "Dynamic explanation of estimation requirements"
        '
        'spinCovarianceRegularization
        '
        Me.spinCovarianceRegularization.DecimalPlaces = 6
        Me.spinCovarianceRegularization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinCovarianceRegularization.Increment = New Decimal(New Integer() {1, 0, 0, 262144})
        Me.spinCovarianceRegularization.Location = New System.Drawing.Point(207, 113)
        Me.spinCovarianceRegularization.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
        Me.spinCovarianceRegularization.Name = "spinCovarianceRegularization"
        Me.spinCovarianceRegularization.Size = New System.Drawing.Size(88, 22)
        Me.spinCovarianceRegularization.TabIndex = 25
        '
        'lblCovarianceRegularization
        '
        Me.lblCovarianceRegularization.AutoSize = True
        Me.lblCovarianceRegularization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCovarianceRegularization.Location = New System.Drawing.Point(6, 115)
        Me.lblCovarianceRegularization.Name = "lblCovarianceRegularization"
        Me.lblCovarianceRegularization.Size = New System.Drawing.Size(132, 16)
        Me.lblCovarianceRegularization.TabIndex = 24
        Me.lblCovarianceRegularization.Text = "Diagonal ridge factor"
        '
        'spinControlLimitAlpha
        '
        Me.spinControlLimitAlpha.DecimalPlaces = 4
        Me.spinControlLimitAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinControlLimitAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 262144})
        Me.spinControlLimitAlpha.Location = New System.Drawing.Point(207, 85)
        Me.spinControlLimitAlpha.Maximum = New Decimal(New Integer() {9999, 0, 0, 262144})
        Me.spinControlLimitAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 262144})
        Me.spinControlLimitAlpha.Name = "spinControlLimitAlpha"
        Me.spinControlLimitAlpha.Size = New System.Drawing.Size(88, 22)
        Me.spinControlLimitAlpha.TabIndex = 19
        Me.spinControlLimitAlpha.Value = New Decimal(New Integer() {27, 0, 0, 262144})
        '
        'lblControlLimitAlpha
        '
        Me.lblControlLimitAlpha.AutoSize = True
        Me.lblControlLimitAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblControlLimitAlpha.Location = New System.Drawing.Point(6, 87)
        Me.lblControlLimitAlpha.Name = "lblControlLimitAlpha"
        Me.lblControlLimitAlpha.Size = New System.Drawing.Size(112, 16)
        Me.lblControlLimitAlpha.TabIndex = 18
        Me.lblControlLimitAlpha.Text = "Control limit alpha"
        '
        'cbModelSource
        '
        Me.cbModelSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbModelSource.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbModelSource.FormattingEnabled = True
        Me.cbModelSource.Location = New System.Drawing.Point(207, 54)
        Me.cbModelSource.Name = "cbModelSource"
        Me.cbModelSource.Size = New System.Drawing.Size(239, 24)
        Me.cbModelSource.TabIndex = 17
        '
        'lblModelSource
        '
        Me.lblModelSource.AutoSize = True
        Me.lblModelSource.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblModelSource.Location = New System.Drawing.Point(6, 57)
        Me.lblModelSource.Name = "lblModelSource"
        Me.lblModelSource.Size = New System.Drawing.Size(89, 16)
        Me.lblModelSource.TabIndex = 16
        Me.lblModelSource.Text = "Model source"
        '
        'cbMissingValuePolicy
        '
        Me.cbMissingValuePolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbMissingValuePolicy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMissingValuePolicy.FormattingEnabled = True
        Me.cbMissingValuePolicy.Location = New System.Drawing.Point(207, 24)
        Me.cbMissingValuePolicy.Name = "cbMissingValuePolicy"
        Me.cbMissingValuePolicy.Size = New System.Drawing.Size(239, 24)
        Me.cbMissingValuePolicy.TabIndex = 15
        '
        'lblMissingValuePolicy
        '
        Me.lblMissingValuePolicy.AutoSize = True
        Me.lblMissingValuePolicy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMissingValuePolicy.Location = New System.Drawing.Point(6, 27)
        Me.lblMissingValuePolicy.Name = "lblMissingValuePolicy"
        Me.lblMissingValuePolicy.Size = New System.Drawing.Size(128, 16)
        Me.lblMissingValuePolicy.TabIndex = 14
        Me.lblMissingValuePolicy.Text = "Missing value policy"
        '
        'TabPage3_PhasesExclusions
        '
        Me.TabPage3_PhasesExclusions.AutoScroll = True
        Me.TabPage3_PhasesExclusions.Controls.Add(Me.grpStages)
        Me.TabPage3_PhasesExclusions.Controls.Add(Me.grpExclusions)
        Me.TabPage3_PhasesExclusions.Controls.Add(Me.grpQuickPhaseSetup)
        Me.TabPage3_PhasesExclusions.Controls.Add(Me.grpPhaseColumns)
        Me.TabPage3_PhasesExclusions.Location = New System.Drawing.Point(4, 25)
        Me.TabPage3_PhasesExclusions.Name = "TabPage3_PhasesExclusions"
        Me.TabPage3_PhasesExclusions.Size = New System.Drawing.Size(871, 741)
        Me.TabPage3_PhasesExclusions.TabIndex = 3
        Me.TabPage3_PhasesExclusions.Text = "Phases and Exclusions"
        Me.TabPage3_PhasesExclusions.UseVisualStyleBackColor = True
        '
        'grpStages
        '
        Me.grpStages.Controls.Add(Me.dgvStages)
        Me.grpStages.Controls.Add(Me.btClearStages)
        Me.grpStages.Controls.Add(Me.btRemoveStage)
        Me.grpStages.Controls.Add(Me.btAddStage)
        Me.grpStages.Location = New System.Drawing.Point(7, 270)
        Me.grpStages.Name = "grpStages"
        Me.grpStages.Size = New System.Drawing.Size(861, 148)
        Me.grpStages.TabIndex = 7
        Me.grpStages.TabStop = False
        Me.grpStages.Text = "Stages"
        '
        'dgvStages
        '
        Me.dgvStages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvStages.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colStageID, Me.colStageFirstPoint, Me.colStageLastPoint, Me.colStagePhase})
        Me.dgvStages.Location = New System.Drawing.Point(0, 48)
        Me.dgvStages.Name = "dgvStages"
        Me.dgvStages.RowHeadersWidth = 51
        Me.dgvStages.RowTemplate.Height = 24
        Me.dgvStages.Size = New System.Drawing.Size(852, 100)
        Me.dgvStages.TabIndex = 26
        '
        'colStageID
        '
        Me.colStageID.HeaderText = "Stage ID"
        Me.colStageID.MinimumWidth = 6
        Me.colStageID.Name = "colStageID"
        Me.colStageID.Width = 125
        '
        'colStageFirstPoint
        '
        Me.colStageFirstPoint.HeaderText = "First point"
        Me.colStageFirstPoint.MinimumWidth = 6
        Me.colStageFirstPoint.Name = "colStageFirstPoint"
        Me.colStageFirstPoint.Width = 125
        '
        'colStageLastPoint
        '
        Me.colStageLastPoint.HeaderText = "Last point"
        Me.colStageLastPoint.MinimumWidth = 6
        Me.colStageLastPoint.Name = "colStageLastPoint"
        Me.colStageLastPoint.Width = 125
        '
        'colStagePhase
        '
        Me.colStagePhase.HeaderText = "Phase"
        Me.colStagePhase.MinimumWidth = 6
        Me.colStagePhase.Name = "colStagePhase"
        Me.colStagePhase.Width = 125
        '
        'btClearStages
        '
        Me.btClearStages.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btClearStages.Location = New System.Drawing.Point(168, 19)
        Me.btClearStages.Name = "btClearStages"
        Me.btClearStages.Size = New System.Drawing.Size(75, 23)
        Me.btClearStages.TabIndex = 25
        Me.btClearStages.Text = "Clear"
        Me.btClearStages.UseVisualStyleBackColor = True
        '
        'btRemoveStage
        '
        Me.btRemoveStage.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveStage.Location = New System.Drawing.Point(87, 19)
        Me.btRemoveStage.Name = "btRemoveStage"
        Me.btRemoveStage.Size = New System.Drawing.Size(75, 23)
        Me.btRemoveStage.TabIndex = 24
        Me.btRemoveStage.Text = "Remove"
        Me.btRemoveStage.UseVisualStyleBackColor = True
        '
        'btAddStage
        '
        Me.btAddStage.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddStage.Location = New System.Drawing.Point(6, 19)
        Me.btAddStage.Name = "btAddStage"
        Me.btAddStage.Size = New System.Drawing.Size(75, 23)
        Me.btAddStage.TabIndex = 23
        Me.btAddStage.Text = "Add"
        Me.btAddStage.UseVisualStyleBackColor = True
        '
        'grpExclusions
        '
        Me.grpExclusions.Controls.Add(Me.btClearExclusions)
        Me.grpExclusions.Controls.Add(Me.btRemoveExclusion)
        Me.grpExclusions.Controls.Add(Me.btAddExclusion)
        Me.grpExclusions.Controls.Add(Me.dgvExclusions)
        Me.grpExclusions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpExclusions.Location = New System.Drawing.Point(7, 424)
        Me.grpExclusions.Name = "grpExclusions"
        Me.grpExclusions.Size = New System.Drawing.Size(858, 210)
        Me.grpExclusions.TabIndex = 6
        Me.grpExclusions.TabStop = False
        Me.grpExclusions.Text = "Explicit exclusions"
        '
        'btClearExclusions
        '
        Me.btClearExclusions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btClearExclusions.Location = New System.Drawing.Point(171, 21)
        Me.btClearExclusions.Name = "btClearExclusions"
        Me.btClearExclusions.Size = New System.Drawing.Size(75, 23)
        Me.btClearExclusions.TabIndex = 22
        Me.btClearExclusions.Text = "Clear"
        Me.btClearExclusions.UseVisualStyleBackColor = True
        '
        'btRemoveExclusion
        '
        Me.btRemoveExclusion.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveExclusion.Location = New System.Drawing.Point(90, 21)
        Me.btRemoveExclusion.Name = "btRemoveExclusion"
        Me.btRemoveExclusion.Size = New System.Drawing.Size(75, 23)
        Me.btRemoveExclusion.TabIndex = 21
        Me.btRemoveExclusion.Text = "Remove"
        Me.btRemoveExclusion.UseVisualStyleBackColor = True
        '
        'btAddExclusion
        '
        Me.btAddExclusion.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddExclusion.Location = New System.Drawing.Point(9, 21)
        Me.btAddExclusion.Name = "btAddExclusion"
        Me.btAddExclusion.Size = New System.Drawing.Size(75, 23)
        Me.btAddExclusion.TabIndex = 20
        Me.btAddExclusion.Text = "Add"
        Me.btAddExclusion.UseVisualStyleBackColor = True
        '
        'dgvExclusions
        '
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvExclusions.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle3
        Me.dgvExclusions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvExclusions.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colExclusionPoint, Me.colExclusionScope, Me.colExclusionReason})
        Me.dgvExclusions.Location = New System.Drawing.Point(6, 50)
        Me.dgvExclusions.Name = "dgvExclusions"
        Me.dgvExclusions.RowHeadersWidth = 51
        Me.dgvExclusions.RowTemplate.Height = 24
        Me.dgvExclusions.Size = New System.Drawing.Size(846, 150)
        Me.dgvExclusions.TabIndex = 0
        '
        'colExclusionPoint
        '
        Me.colExclusionPoint.HeaderText = "Point"
        Me.colExclusionPoint.MinimumWidth = 6
        Me.colExclusionPoint.Name = "colExclusionPoint"
        Me.colExclusionPoint.Width = 125
        '
        'colExclusionScope
        '
        Me.colExclusionScope.HeaderText = "Scope"
        Me.colExclusionScope.MinimumWidth = 6
        Me.colExclusionScope.Name = "colExclusionScope"
        Me.colExclusionScope.Width = 125
        '
        'colExclusionReason
        '
        Me.colExclusionReason.HeaderText = "Reason"
        Me.colExclusionReason.MinimumWidth = 6
        Me.colExclusionReason.Name = "colExclusionReason"
        Me.colExclusionReason.Width = 125
        '
        'grpQuickPhaseSetup
        '
        Me.grpQuickPhaseSetup.Controls.Add(Me.rbSinglePhaseII)
        Me.grpQuickPhaseSetup.Controls.Add(Me.btApplyQuickPhaseSetup)
        Me.grpQuickPhaseSetup.Controls.Add(Me.spinLastPhaseIPoint)
        Me.grpQuickPhaseSetup.Controls.Add(Me.lblLastPhaseIPoint)
        Me.grpQuickPhaseSetup.Controls.Add(Me.rbPhaseIThenPhaseII)
        Me.grpQuickPhaseSetup.Controls.Add(Me.rbSinglePhaseI)
        Me.grpQuickPhaseSetup.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpQuickPhaseSetup.Location = New System.Drawing.Point(7, 179)
        Me.grpQuickPhaseSetup.Name = "grpQuickPhaseSetup"
        Me.grpQuickPhaseSetup.Size = New System.Drawing.Size(858, 83)
        Me.grpQuickPhaseSetup.TabIndex = 4
        Me.grpQuickPhaseSetup.TabStop = False
        Me.grpQuickPhaseSetup.Text = "Quick phase setup"
        '
        'rbSinglePhaseII
        '
        Me.rbSinglePhaseII.AutoSize = True
        Me.rbSinglePhaseII.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbSinglePhaseII.Location = New System.Drawing.Point(210, 21)
        Me.rbSinglePhaseII.Name = "rbSinglePhaseII"
        Me.rbSinglePhaseII.Size = New System.Drawing.Size(198, 20)
        Me.rbSinglePhaseII.TabIndex = 29
        Me.rbSinglePhaseII.Text = "All observations are Phase II"
        Me.rbSinglePhaseII.UseVisualStyleBackColor = True
        '
        'btApplyQuickPhaseSetup
        '
        Me.btApplyQuickPhaseSetup.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btApplyQuickPhaseSetup.Location = New System.Drawing.Point(561, 54)
        Me.btApplyQuickPhaseSetup.Name = "btApplyQuickPhaseSetup"
        Me.btApplyQuickPhaseSetup.Size = New System.Drawing.Size(109, 23)
        Me.btApplyQuickPhaseSetup.TabIndex = 28
        Me.btApplyQuickPhaseSetup.Text = "Apply"
        Me.btApplyQuickPhaseSetup.UseVisualStyleBackColor = True
        '
        'spinLastPhaseIPoint
        '
        Me.spinLastPhaseIPoint.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinLastPhaseIPoint.Location = New System.Drawing.Point(606, 21)
        Me.spinLastPhaseIPoint.Maximum = New Decimal(New Integer() {1000000, 0, 0, 0})
        Me.spinLastPhaseIPoint.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinLastPhaseIPoint.Name = "spinLastPhaseIPoint"
        Me.spinLastPhaseIPoint.Size = New System.Drawing.Size(64, 22)
        Me.spinLastPhaseIPoint.TabIndex = 27
        Me.spinLastPhaseIPoint.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblLastPhaseIPoint
        '
        Me.lblLastPhaseIPoint.AutoSize = True
        Me.lblLastPhaseIPoint.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLastPhaseIPoint.Location = New System.Drawing.Point(452, 24)
        Me.lblLastPhaseIPoint.Name = "lblLastPhaseIPoint"
        Me.lblLastPhaseIPoint.Size = New System.Drawing.Size(112, 16)
        Me.lblLastPhaseIPoint.TabIndex = 26
        Me.lblLastPhaseIPoint.Text = "Last Phase I point"
        '
        'rbPhaseIThenPhaseII
        '
        Me.rbPhaseIThenPhaseII.AutoSize = True
        Me.rbPhaseIThenPhaseII.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbPhaseIThenPhaseII.Location = New System.Drawing.Point(9, 48)
        Me.rbPhaseIThenPhaseII.Name = "rbPhaseIThenPhaseII"
        Me.rbPhaseIThenPhaseII.Size = New System.Drawing.Size(195, 20)
        Me.rbPhaseIThenPhaseII.TabIndex = 1
        Me.rbPhaseIThenPhaseII.Text = "Phase I followed by Phase II"
        Me.rbPhaseIThenPhaseII.UseVisualStyleBackColor = True
        '
        'rbSinglePhaseI
        '
        Me.rbSinglePhaseI.AutoSize = True
        Me.rbSinglePhaseI.Checked = True
        Me.rbSinglePhaseI.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbSinglePhaseI.Location = New System.Drawing.Point(9, 22)
        Me.rbSinglePhaseI.Name = "rbSinglePhaseI"
        Me.rbSinglePhaseI.Size = New System.Drawing.Size(195, 20)
        Me.rbSinglePhaseI.TabIndex = 0
        Me.rbSinglePhaseI.TabStop = True
        Me.rbSinglePhaseI.Text = "All observations are Phase I"
        Me.rbSinglePhaseI.UseVisualStyleBackColor = True
        '
        'grpPhaseColumns
        '
        Me.grpPhaseColumns.Controls.Add(Me.btImportExclusions)
        Me.grpPhaseColumns.Controls.Add(Me.btImportStages)
        Me.grpPhaseColumns.Controls.Add(Me.cbImportedExclusionScope)
        Me.grpPhaseColumns.Controls.Add(Me.lblImportedExclusionScope)
        Me.grpPhaseColumns.Controls.Add(Me.cbExclusionReasonColumn)
        Me.grpPhaseColumns.Controls.Add(Me.lblExclusionReasonColumn)
        Me.grpPhaseColumns.Controls.Add(Me.cbExclusionColumn)
        Me.grpPhaseColumns.Controls.Add(Me.lblExclusionColumn)
        Me.grpPhaseColumns.Controls.Add(Me.cbPhaseColumn)
        Me.grpPhaseColumns.Controls.Add(Me.lblPhaseColumn)
        Me.grpPhaseColumns.Controls.Add(Me.cbStageColumn)
        Me.grpPhaseColumns.Controls.Add(Me.lblStageColumn)
        Me.grpPhaseColumns.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpPhaseColumns.Location = New System.Drawing.Point(7, 3)
        Me.grpPhaseColumns.Name = "grpPhaseColumns"
        Me.grpPhaseColumns.Size = New System.Drawing.Size(858, 170)
        Me.grpPhaseColumns.TabIndex = 3
        Me.grpPhaseColumns.TabStop = False
        Me.grpPhaseColumns.Text = "Optional source columns"
        '
        'btImportExclusions
        '
        Me.btImportExclusions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btImportExclusions.Location = New System.Drawing.Point(476, 45)
        Me.btImportExclusions.Name = "btImportExclusions"
        Me.btImportExclusions.Size = New System.Drawing.Size(208, 23)
        Me.btImportExclusions.TabIndex = 27
        Me.btImportExclusions.Text = "Build exclusions from columns"
        Me.btImportExclusions.UseVisualStyleBackColor = True
        '
        'btImportStages
        '
        Me.btImportStages.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btImportStages.Location = New System.Drawing.Point(476, 16)
        Me.btImportStages.Name = "btImportStages"
        Me.btImportStages.Size = New System.Drawing.Size(206, 23)
        Me.btImportStages.TabIndex = 26
        Me.btImportStages.Text = "Build stages from columns"
        Me.btImportStages.UseVisualStyleBackColor = True
        '
        'cbImportedExclusionScope
        '
        Me.cbImportedExclusionScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbImportedExclusionScope.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbImportedExclusionScope.FormattingEnabled = True
        Me.cbImportedExclusionScope.Location = New System.Drawing.Point(207, 135)
        Me.cbImportedExclusionScope.Name = "cbImportedExclusionScope"
        Me.cbImportedExclusionScope.Size = New System.Drawing.Size(239, 24)
        Me.cbImportedExclusionScope.TabIndex = 25
        '
        'lblImportedExclusionScope
        '
        Me.lblImportedExclusionScope.AutoSize = True
        Me.lblImportedExclusionScope.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblImportedExclusionScope.Location = New System.Drawing.Point(6, 138)
        Me.lblImportedExclusionScope.Name = "lblImportedExclusionScope"
        Me.lblImportedExclusionScope.Size = New System.Drawing.Size(160, 16)
        Me.lblImportedExclusionScope.TabIndex = 24
        Me.lblImportedExclusionScope.Text = "Imported exclusion scope"
        '
        'cbExclusionReasonColumn
        '
        Me.cbExclusionReasonColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbExclusionReasonColumn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbExclusionReasonColumn.FormattingEnabled = True
        Me.cbExclusionReasonColumn.Location = New System.Drawing.Point(207, 105)
        Me.cbExclusionReasonColumn.Name = "cbExclusionReasonColumn"
        Me.cbExclusionReasonColumn.Size = New System.Drawing.Size(239, 24)
        Me.cbExclusionReasonColumn.TabIndex = 23
        '
        'lblExclusionReasonColumn
        '
        Me.lblExclusionReasonColumn.AutoSize = True
        Me.lblExclusionReasonColumn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblExclusionReasonColumn.Location = New System.Drawing.Point(6, 108)
        Me.lblExclusionReasonColumn.Name = "lblExclusionReasonColumn"
        Me.lblExclusionReasonColumn.Size = New System.Drawing.Size(155, 16)
        Me.lblExclusionReasonColumn.TabIndex = 22
        Me.lblExclusionReasonColumn.Text = "Exclusion reason column"
        '
        'cbExclusionColumn
        '
        Me.cbExclusionColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbExclusionColumn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbExclusionColumn.FormattingEnabled = True
        Me.cbExclusionColumn.Location = New System.Drawing.Point(207, 75)
        Me.cbExclusionColumn.Name = "cbExclusionColumn"
        Me.cbExclusionColumn.Size = New System.Drawing.Size(239, 24)
        Me.cbExclusionColumn.TabIndex = 21
        '
        'lblExclusionColumn
        '
        Me.lblExclusionColumn.AutoSize = True
        Me.lblExclusionColumn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblExclusionColumn.Location = New System.Drawing.Point(6, 78)
        Me.lblExclusionColumn.Name = "lblExclusionColumn"
        Me.lblExclusionColumn.Size = New System.Drawing.Size(164, 16)
        Me.lblExclusionColumn.TabIndex = 20
        Me.lblExclusionColumn.Text = "Exclusion indicator column"
        '
        'cbPhaseColumn
        '
        Me.cbPhaseColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbPhaseColumn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbPhaseColumn.FormattingEnabled = True
        Me.cbPhaseColumn.Location = New System.Drawing.Point(207, 45)
        Me.cbPhaseColumn.Name = "cbPhaseColumn"
        Me.cbPhaseColumn.Size = New System.Drawing.Size(239, 24)
        Me.cbPhaseColumn.TabIndex = 19
        '
        'lblPhaseColumn
        '
        Me.lblPhaseColumn.AutoSize = True
        Me.lblPhaseColumn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPhaseColumn.Location = New System.Drawing.Point(6, 48)
        Me.lblPhaseColumn.Name = "lblPhaseColumn"
        Me.lblPhaseColumn.Size = New System.Drawing.Size(150, 16)
        Me.lblPhaseColumn.TabIndex = 18
        Me.lblPhaseColumn.Text = "Phase I/Phase II column"
        '
        'cbStageColumn
        '
        Me.cbStageColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbStageColumn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbStageColumn.FormattingEnabled = True
        Me.cbStageColumn.Location = New System.Drawing.Point(207, 15)
        Me.cbStageColumn.Name = "cbStageColumn"
        Me.cbStageColumn.Size = New System.Drawing.Size(239, 24)
        Me.cbStageColumn.TabIndex = 17
        '
        'lblStageColumn
        '
        Me.lblStageColumn.AutoSize = True
        Me.lblStageColumn.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStageColumn.Location = New System.Drawing.Point(6, 18)
        Me.lblStageColumn.Name = "lblStageColumn"
        Me.lblStageColumn.Size = New System.Drawing.Size(142, 16)
        Me.lblStageColumn.TabIndex = 16
        Me.lblStageColumn.Text = "Stage identifier column"
        '
        'TabPage4_MethodOptions
        '
        Me.TabPage4_MethodOptions.AutoScroll = True
        Me.TabPage4_MethodOptions.Controls.Add(Me.flpMethodOptions)
        Me.TabPage4_MethodOptions.Controls.Add(Me.lblMethodDescription)
        Me.TabPage4_MethodOptions.Location = New System.Drawing.Point(4, 25)
        Me.TabPage4_MethodOptions.Name = "TabPage4_MethodOptions"
        Me.TabPage4_MethodOptions.Size = New System.Drawing.Size(871, 741)
        Me.TabPage4_MethodOptions.TabIndex = 4
        Me.TabPage4_MethodOptions.Text = "Method Options"
        Me.TabPage4_MethodOptions.UseVisualStyleBackColor = True
        '
        'flpMethodOptions
        '
        Me.flpMethodOptions.AutoScroll = True
        Me.flpMethodOptions.Controls.Add(Me.grpPcaOptions)
        Me.flpMethodOptions.Controls.Add(Me.grpGeneralizedVarianceOptions)
        Me.flpMethodOptions.Controls.Add(Me.grpMewmaOptions)
        Me.flpMethodOptions.Controls.Add(Me.grpMcusumOptions)
        Me.flpMethodOptions.Controls.Add(Me.grpSequentialResetOptions)
        Me.flpMethodOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.flpMethodOptions.Location = New System.Drawing.Point(3, 53)
        Me.flpMethodOptions.Name = "flpMethodOptions"
        Me.flpMethodOptions.Size = New System.Drawing.Size(865, 500)
        Me.flpMethodOptions.TabIndex = 1
        Me.flpMethodOptions.WrapContents = False
        '
        'grpPcaOptions
        '
        Me.grpPcaOptions.Controls.Add(Me.lblPcaNote)
        Me.grpPcaOptions.Controls.Add(Me.spinPcaComponentCount)
        Me.grpPcaOptions.Controls.Add(Me.rbPcaFixedComponents)
        Me.grpPcaOptions.Controls.Add(Me.spinPcaCumulativeVariance)
        Me.grpPcaOptions.Controls.Add(Me.rbPcaVarianceSelection)
        Me.grpPcaOptions.Controls.Add(Me.cbPcaMatrix)
        Me.grpPcaOptions.Controls.Add(Me.lblPcaMatrix)
        Me.grpPcaOptions.Location = New System.Drawing.Point(3, 3)
        Me.grpPcaOptions.Name = "grpPcaOptions"
        Me.grpPcaOptions.Size = New System.Drawing.Size(849, 114)
        Me.grpPcaOptions.TabIndex = 0
        Me.grpPcaOptions.TabStop = False
        Me.grpPcaOptions.Text = "PCA options"
        '
        'lblPcaNote
        '
        Me.lblPcaNote.Location = New System.Drawing.Point(336, 18)
        Me.lblPcaNote.Name = "lblPcaNote"
        Me.lblPcaNote.Size = New System.Drawing.Size(507, 84)
        Me.lblPcaNote.TabIndex = 6
        Me.lblPcaNote.Text = "Dynamic PCA/Q explanation"
        '
        'spinPcaComponentCount
        '
        Me.spinPcaComponentCount.Location = New System.Drawing.Point(230, 80)
        Me.spinPcaComponentCount.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.spinPcaComponentCount.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinPcaComponentCount.Name = "spinPcaComponentCount"
        Me.spinPcaComponentCount.Size = New System.Drawing.Size(80, 22)
        Me.spinPcaComponentCount.TabIndex = 5
        Me.spinPcaComponentCount.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'rbPcaFixedComponents
        '
        Me.rbPcaFixedComponents.AutoSize = True
        Me.rbPcaFixedComponents.Location = New System.Drawing.Point(9, 82)
        Me.rbPcaFixedComponents.Name = "rbPcaFixedComponents"
        Me.rbPcaFixedComponents.Size = New System.Drawing.Size(178, 20)
        Me.rbPcaFixedComponents.TabIndex = 4
        Me.rbPcaFixedComponents.TabStop = True
        Me.rbPcaFixedComponents.Text = "Specify component count"
        Me.rbPcaFixedComponents.UseVisualStyleBackColor = True
        '
        'spinPcaCumulativeVariance
        '
        Me.spinPcaCumulativeVariance.DecimalPlaces = 1
        Me.spinPcaCumulativeVariance.Location = New System.Drawing.Point(230, 54)
        Me.spinPcaCumulativeVariance.Name = "spinPcaCumulativeVariance"
        Me.spinPcaCumulativeVariance.Size = New System.Drawing.Size(80, 22)
        Me.spinPcaCumulativeVariance.TabIndex = 3
        Me.spinPcaCumulativeVariance.Value = New Decimal(New Integer() {90, 0, 0, 0})
        '
        'rbPcaVarianceSelection
        '
        Me.rbPcaVarianceSelection.AutoSize = True
        Me.rbPcaVarianceSelection.Location = New System.Drawing.Point(9, 56)
        Me.rbPcaVarianceSelection.Name = "rbPcaVarianceSelection"
        Me.rbPcaVarianceSelection.Size = New System.Drawing.Size(206, 20)
        Me.rbPcaVarianceSelection.TabIndex = 2
        Me.rbPcaVarianceSelection.TabStop = True
        Me.rbPcaVarianceSelection.Text = "Select by cumulative variance"
        Me.rbPcaVarianceSelection.UseVisualStyleBackColor = True
        '
        'cbPcaMatrix
        '
        Me.cbPcaMatrix.FormattingEnabled = True
        Me.cbPcaMatrix.Location = New System.Drawing.Point(97, 15)
        Me.cbPcaMatrix.Name = "cbPcaMatrix"
        Me.cbPcaMatrix.Size = New System.Drawing.Size(213, 24)
        Me.cbPcaMatrix.TabIndex = 1
        '
        'lblPcaMatrix
        '
        Me.lblPcaMatrix.AutoSize = True
        Me.lblPcaMatrix.Location = New System.Drawing.Point(6, 18)
        Me.lblPcaMatrix.Name = "lblPcaMatrix"
        Me.lblPcaMatrix.Size = New System.Drawing.Size(72, 16)
        Me.lblPcaMatrix.TabIndex = 0
        Me.lblPcaMatrix.Text = "PCA matrix"
        '
        'grpGeneralizedVarianceOptions
        '
        Me.grpGeneralizedVarianceOptions.Controls.Add(Me.chkSpecifyGvSigmaMultiplier)
        Me.grpGeneralizedVarianceOptions.Controls.Add(Me.lblGvNote)
        Me.grpGeneralizedVarianceOptions.Controls.Add(Me.spinGvSigmaMultiplier)
        Me.grpGeneralizedVarianceOptions.Location = New System.Drawing.Point(3, 123)
        Me.grpGeneralizedVarianceOptions.Name = "grpGeneralizedVarianceOptions"
        Me.grpGeneralizedVarianceOptions.Size = New System.Drawing.Size(849, 67)
        Me.grpGeneralizedVarianceOptions.TabIndex = 7
        Me.grpGeneralizedVarianceOptions.TabStop = False
        Me.grpGeneralizedVarianceOptions.Text = "Generalized-variance options"
        '
        'chkSpecifyGvSigmaMultiplier
        '
        Me.chkSpecifyGvSigmaMultiplier.AutoSize = True
        Me.chkSpecifyGvSigmaMultiplier.Location = New System.Drawing.Point(9, 27)
        Me.chkSpecifyGvSigmaMultiplier.Name = "chkSpecifyGvSigmaMultiplier"
        Me.chkSpecifyGvSigmaMultiplier.Size = New System.Drawing.Size(170, 20)
        Me.chkSpecifyGvSigmaMultiplier.TabIndex = 7
        Me.chkSpecifyGvSigmaMultiplier.Text = "Specify sigma multiplier"
        Me.chkSpecifyGvSigmaMultiplier.UseVisualStyleBackColor = True
        '
        'lblGvNote
        '
        Me.lblGvNote.AutoSize = True
        Me.lblGvNote.Location = New System.Drawing.Point(321, 28)
        Me.lblGvNote.Name = "lblGvNote"
        Me.lblGvNote.Size = New System.Drawing.Size(264, 16)
        Me.lblGvNote.TabIndex = 6
        Me.lblGvNote.Text = "Alpha is used when no multiplier is supplied"
        '
        'spinGvSigmaMultiplier
        '
        Me.spinGvSigmaMultiplier.DecimalPlaces = 2
        Me.spinGvSigmaMultiplier.Enabled = False
        Me.spinGvSigmaMultiplier.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.spinGvSigmaMultiplier.Location = New System.Drawing.Point(210, 26)
        Me.spinGvSigmaMultiplier.Minimum = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.spinGvSigmaMultiplier.Name = "spinGvSigmaMultiplier"
        Me.spinGvSigmaMultiplier.Size = New System.Drawing.Size(80, 22)
        Me.spinGvSigmaMultiplier.TabIndex = 3
        Me.spinGvSigmaMultiplier.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        'grpMewmaOptions
        '
        Me.grpMewmaOptions.Controls.Add(Me.chkSpecifyMewmaControlLimit)
        Me.grpMewmaOptions.Controls.Add(Me.lblMewmaNote)
        Me.grpMewmaOptions.Controls.Add(Me.spinMewmaControlLimit)
        Me.grpMewmaOptions.Controls.Add(Me.lblMewmaLambda)
        Me.grpMewmaOptions.Controls.Add(Me.spinMewmaLambda)
        Me.grpMewmaOptions.Location = New System.Drawing.Point(3, 196)
        Me.grpMewmaOptions.Name = "grpMewmaOptions"
        Me.grpMewmaOptions.Size = New System.Drawing.Size(849, 90)
        Me.grpMewmaOptions.TabIndex = 8
        Me.grpMewmaOptions.TabStop = False
        Me.grpMewmaOptions.Text = "MEWMA options"
        '
        'chkSpecifyMewmaControlLimit
        '
        Me.chkSpecifyMewmaControlLimit.AutoSize = True
        Me.chkSpecifyMewmaControlLimit.Location = New System.Drawing.Point(6, 58)
        Me.chkSpecifyMewmaControlLimit.Name = "chkSpecifyMewmaControlLimit"
        Me.chkSpecifyMewmaControlLimit.Size = New System.Drawing.Size(196, 20)
        Me.chkSpecifyMewmaControlLimit.TabIndex = 10
        Me.chkSpecifyMewmaControlLimit.Text = "Specify ARL-calibrated UCL"
        Me.chkSpecifyMewmaControlLimit.UseVisualStyleBackColor = True
        '
        'lblMewmaNote
        '
        Me.lblMewmaNote.Location = New System.Drawing.Point(294, 30)
        Me.lblMewmaNote.Name = "lblMewmaNote"
        Me.lblMewmaNote.Size = New System.Drawing.Size(549, 50)
        Me.lblMewmaNote.TabIndex = 9
        Me.lblMewmaNote.Text = "Chi-square approximation note"
        '
        'spinMewmaControlLimit
        '
        Me.spinMewmaControlLimit.DecimalPlaces = 4
        Me.spinMewmaControlLimit.Enabled = False
        Me.spinMewmaControlLimit.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.spinMewmaControlLimit.Location = New System.Drawing.Point(208, 57)
        Me.spinMewmaControlLimit.Maximum = New Decimal(New Integer() {1000000, 0, 0, 0})
        Me.spinMewmaControlLimit.Minimum = New Decimal(New Integer() {1, 0, 0, 262144})
        Me.spinMewmaControlLimit.Name = "spinMewmaControlLimit"
        Me.spinMewmaControlLimit.Size = New System.Drawing.Size(80, 22)
        Me.spinMewmaControlLimit.TabIndex = 7
        Me.spinMewmaControlLimit.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblMewmaLambda
        '
        Me.lblMewmaLambda.AutoSize = True
        Me.lblMewmaLambda.Location = New System.Drawing.Point(145, 30)
        Me.lblMewmaLambda.Name = "lblMewmaLambda"
        Me.lblMewmaLambda.Size = New System.Drawing.Size(57, 16)
        Me.lblMewmaLambda.TabIndex = 6
        Me.lblMewmaLambda.Text = "Lambda"
        '
        'spinMewmaLambda
        '
        Me.spinMewmaLambda.DecimalPlaces = 2
        Me.spinMewmaLambda.Enabled = False
        Me.spinMewmaLambda.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        Me.spinMewmaLambda.Location = New System.Drawing.Point(208, 28)
        Me.spinMewmaLambda.Maximum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinMewmaLambda.Minimum = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.spinMewmaLambda.Name = "spinMewmaLambda"
        Me.spinMewmaLambda.Size = New System.Drawing.Size(80, 22)
        Me.spinMewmaLambda.TabIndex = 3
        Me.spinMewmaLambda.Value = New Decimal(New Integer() {2, 0, 0, 65536})
        '
        'grpMcusumOptions
        '
        Me.grpMcusumOptions.Controls.Add(Me.lblMcusumDecisionInterval)
        Me.grpMcusumOptions.Controls.Add(Me.lblMcusumNote)
        Me.grpMcusumOptions.Controls.Add(Me.spinMcusumDecisionInterval)
        Me.grpMcusumOptions.Controls.Add(Me.lblMcusumReferenceValue)
        Me.grpMcusumOptions.Controls.Add(Me.spinMcusumReferenceValue)
        Me.grpMcusumOptions.Location = New System.Drawing.Point(3, 292)
        Me.grpMcusumOptions.Name = "grpMcusumOptions"
        Me.grpMcusumOptions.Size = New System.Drawing.Size(849, 90)
        Me.grpMcusumOptions.TabIndex = 11
        Me.grpMcusumOptions.TabStop = False
        Me.grpMcusumOptions.Text = "MCUSUM options"
        '
        'lblMcusumDecisionInterval
        '
        Me.lblMcusumDecisionInterval.AutoSize = True
        Me.lblMcusumDecisionInterval.Location = New System.Drawing.Point(6, 59)
        Me.lblMcusumDecisionInterval.Name = "lblMcusumDecisionInterval"
        Me.lblMcusumDecisionInterval.Size = New System.Drawing.Size(124, 16)
        Me.lblMcusumDecisionInterval.TabIndex = 10
        Me.lblMcusumDecisionInterval.Text = "Decision interval (h)"
        '
        'lblMcusumNote
        '
        Me.lblMcusumNote.Location = New System.Drawing.Point(294, 30)
        Me.lblMcusumNote.Name = "lblMcusumNote"
        Me.lblMcusumNote.Size = New System.Drawing.Size(549, 50)
        Me.lblMcusumNote.TabIndex = 9
        Me.lblMcusumNote.Text = "ARL-design explanation"
        '
        'spinMcusumDecisionInterval
        '
        Me.spinMcusumDecisionInterval.DecimalPlaces = 3
        Me.spinMcusumDecisionInterval.Enabled = False
        Me.spinMcusumDecisionInterval.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.spinMcusumDecisionInterval.Location = New System.Drawing.Point(208, 57)
        Me.spinMcusumDecisionInterval.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.spinMcusumDecisionInterval.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinMcusumDecisionInterval.Name = "spinMcusumDecisionInterval"
        Me.spinMcusumDecisionInterval.Size = New System.Drawing.Size(80, 22)
        Me.spinMcusumDecisionInterval.TabIndex = 7
        Me.spinMcusumDecisionInterval.Value = New Decimal(New Integer() {55, 0, 0, 65536})
        '
        'lblMcusumReferenceValue
        '
        Me.lblMcusumReferenceValue.AutoSize = True
        Me.lblMcusumReferenceValue.Location = New System.Drawing.Point(6, 30)
        Me.lblMcusumReferenceValue.Name = "lblMcusumReferenceValue"
        Me.lblMcusumReferenceValue.Size = New System.Drawing.Size(124, 16)
        Me.lblMcusumReferenceValue.TabIndex = 6
        Me.lblMcusumReferenceValue.Text = "Reference value (k)"
        '
        'spinMcusumReferenceValue
        '
        Me.spinMcusumReferenceValue.DecimalPlaces = 3
        Me.spinMcusumReferenceValue.Enabled = False
        Me.spinMcusumReferenceValue.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.spinMcusumReferenceValue.Location = New System.Drawing.Point(208, 28)
        Me.spinMcusumReferenceValue.Name = "spinMcusumReferenceValue"
        Me.spinMcusumReferenceValue.Size = New System.Drawing.Size(80, 22)
        Me.spinMcusumReferenceValue.TabIndex = 3
        Me.spinMcusumReferenceValue.Value = New Decimal(New Integer() {5, 0, 0, 65536})
        '
        'grpSequentialResetOptions
        '
        Me.grpSequentialResetOptions.Controls.Add(Me.cbSequenceGapBehavior)
        Me.grpSequentialResetOptions.Controls.Add(Me.chkResetAfterSignal)
        Me.grpSequentialResetOptions.Controls.Add(Me.chkResetAtPhaseBoundary)
        Me.grpSequentialResetOptions.Controls.Add(Me.chkResetAtStageBoundary)
        Me.grpSequentialResetOptions.Controls.Add(Me.lblSequenceGapBehavior)
        Me.grpSequentialResetOptions.Location = New System.Drawing.Point(3, 388)
        Me.grpSequentialResetOptions.Name = "grpSequentialResetOptions"
        Me.grpSequentialResetOptions.Size = New System.Drawing.Size(849, 90)
        Me.grpSequentialResetOptions.TabIndex = 12
        Me.grpSequentialResetOptions.TabStop = False
        Me.grpSequentialResetOptions.Text = "Sequential-state options"
        '
        'cbSequenceGapBehavior
        '
        Me.cbSequenceGapBehavior.FormattingEnabled = True
        Me.cbSequenceGapBehavior.Location = New System.Drawing.Point(212, 56)
        Me.cbSequenceGapBehavior.Name = "cbSequenceGapBehavior"
        Me.cbSequenceGapBehavior.Size = New System.Drawing.Size(213, 24)
        Me.cbSequenceGapBehavior.TabIndex = 14
        '
        'chkResetAfterSignal
        '
        Me.chkResetAfterSignal.AutoSize = True
        Me.chkResetAfterSignal.Checked = True
        Me.chkResetAfterSignal.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkResetAfterSignal.Location = New System.Drawing.Point(409, 28)
        Me.chkResetAfterSignal.Name = "chkResetAfterSignal"
        Me.chkResetAfterSignal.Size = New System.Drawing.Size(144, 20)
        Me.chkResetAfterSignal.TabIndex = 13
        Me.chkResetAfterSignal.Text = "Reset after a signal"
        Me.chkResetAfterSignal.UseVisualStyleBackColor = True
        '
        'chkResetAtPhaseBoundary
        '
        Me.chkResetAtPhaseBoundary.AutoSize = True
        Me.chkResetAtPhaseBoundary.Checked = True
        Me.chkResetAtPhaseBoundary.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkResetAtPhaseBoundary.Location = New System.Drawing.Point(208, 28)
        Me.chkResetAtPhaseBoundary.Name = "chkResetAtPhaseBoundary"
        Me.chkResetAtPhaseBoundary.Size = New System.Drawing.Size(180, 20)
        Me.chkResetAtPhaseBoundary.TabIndex = 12
        Me.chkResetAtPhaseBoundary.Text = "Reset at phase boundary"
        Me.chkResetAtPhaseBoundary.UseVisualStyleBackColor = True
        '
        'chkResetAtStageBoundary
        '
        Me.chkResetAtStageBoundary.AutoSize = True
        Me.chkResetAtStageBoundary.Checked = True
        Me.chkResetAtStageBoundary.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkResetAtStageBoundary.Location = New System.Drawing.Point(6, 28)
        Me.chkResetAtStageBoundary.Name = "chkResetAtStageBoundary"
        Me.chkResetAtStageBoundary.Size = New System.Drawing.Size(176, 20)
        Me.chkResetAtStageBoundary.TabIndex = 11
        Me.chkResetAtStageBoundary.Text = "Reset at stage boundary"
        Me.chkResetAtStageBoundary.UseVisualStyleBackColor = True
        '
        'lblSequenceGapBehavior
        '
        Me.lblSequenceGapBehavior.AutoSize = True
        Me.lblSequenceGapBehavior.Location = New System.Drawing.Point(6, 59)
        Me.lblSequenceGapBehavior.Name = "lblSequenceGapBehavior"
        Me.lblSequenceGapBehavior.Size = New System.Drawing.Size(200, 16)
        Me.lblSequenceGapBehavior.TabIndex = 10
        Me.lblSequenceGapBehavior.Text = "Missing/excluded point behavior"
        '
        'lblMethodDescription
        '
        Me.lblMethodDescription.Location = New System.Drawing.Point(5, 10)
        Me.lblMethodDescription.Name = "lblMethodDescription"
        Me.lblMethodDescription.Size = New System.Drawing.Size(850, 40)
        Me.lblMethodDescription.TabIndex = 0
        Me.lblMethodDescription.Text = "Label1"
        '
        'TabPage5_OutputAppearance
        '
        Me.TabPage5_OutputAppearance.AutoScroll = True
        Me.TabPage5_OutputAppearance.Controls.Add(Me.grpChartDimensions)
        Me.TabPage5_OutputAppearance.Controls.Add(Me.grpChartDisplay)
        Me.TabPage5_OutputAppearance.Controls.Add(Me.grpOutputs)
        Me.TabPage5_OutputAppearance.Controls.Add(Me.grpTitleAxes)
        Me.TabPage5_OutputAppearance.Location = New System.Drawing.Point(4, 25)
        Me.TabPage5_OutputAppearance.Name = "TabPage5_OutputAppearance"
        Me.TabPage5_OutputAppearance.Size = New System.Drawing.Size(871, 741)
        Me.TabPage5_OutputAppearance.TabIndex = 9
        Me.TabPage5_OutputAppearance.Text = "Output and Appearance"
        Me.TabPage5_OutputAppearance.UseVisualStyleBackColor = True
        '
        'grpChartDimensions
        '
        Me.grpChartDimensions.Controls.Add(Me.spinChartHeight)
        Me.grpChartDimensions.Controls.Add(Me.lblChartHeight)
        Me.grpChartDimensions.Controls.Add(Me.btResetAppearance)
        Me.grpChartDimensions.Controls.Add(Me.spinChartWidth)
        Me.grpChartDimensions.Controls.Add(Me.lblChartWidth)
        Me.grpChartDimensions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpChartDimensions.Location = New System.Drawing.Point(0, 428)
        Me.grpChartDimensions.Name = "grpChartDimensions"
        Me.grpChartDimensions.Size = New System.Drawing.Size(862, 77)
        Me.grpChartDimensions.TabIndex = 46
        Me.grpChartDimensions.TabStop = False
        Me.grpChartDimensions.Text = "Chart dimensions"
        '
        'spinChartHeight
        '
        Me.spinChartHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinChartHeight.Location = New System.Drawing.Point(132, 44)
        Me.spinChartHeight.Maximum = New Decimal(New Integer() {5000, 0, 0, 0})
        Me.spinChartHeight.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.spinChartHeight.Name = "spinChartHeight"
        Me.spinChartHeight.Size = New System.Drawing.Size(64, 22)
        Me.spinChartHeight.TabIndex = 30
        Me.spinChartHeight.Value = New Decimal(New Integer() {360, 0, 0, 0})
        '
        'lblChartHeight
        '
        Me.lblChartHeight.AutoSize = True
        Me.lblChartHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChartHeight.Location = New System.Drawing.Point(9, 46)
        Me.lblChartHeight.Name = "lblChartHeight"
        Me.lblChartHeight.Size = New System.Drawing.Size(77, 16)
        Me.lblChartHeight.TabIndex = 29
        Me.lblChartHeight.Text = "Chart height"
        '
        'btResetAppearance
        '
        Me.btResetAppearance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btResetAppearance.Location = New System.Drawing.Point(216, 15)
        Me.btResetAppearance.Name = "btResetAppearance"
        Me.btResetAppearance.Size = New System.Drawing.Size(122, 23)
        Me.btResetAppearance.TabIndex = 28
        Me.btResetAppearance.Text = "Restore defaults"
        Me.btResetAppearance.UseVisualStyleBackColor = True
        '
        'spinChartWidth
        '
        Me.spinChartWidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinChartWidth.Location = New System.Drawing.Point(132, 16)
        Me.spinChartWidth.Maximum = New Decimal(New Integer() {5000, 0, 0, 0})
        Me.spinChartWidth.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.spinChartWidth.Name = "spinChartWidth"
        Me.spinChartWidth.Size = New System.Drawing.Size(64, 22)
        Me.spinChartWidth.TabIndex = 27
        Me.spinChartWidth.Value = New Decimal(New Integer() {760, 0, 0, 0})
        '
        'lblChartWidth
        '
        Me.lblChartWidth.AutoSize = True
        Me.lblChartWidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChartWidth.Location = New System.Drawing.Point(9, 18)
        Me.lblChartWidth.Name = "lblChartWidth"
        Me.lblChartWidth.Size = New System.Drawing.Size(71, 16)
        Me.lblChartWidth.TabIndex = 26
        Me.lblChartWidth.Text = "Chart width"
        '
        'grpChartDisplay
        '
        Me.grpChartDisplay.Controls.Add(Me.chkShowStageBoundaries)
        Me.grpChartDisplay.Controls.Add(Me.chkShowSignalLabels)
        Me.grpChartDisplay.Controls.Add(Me.chkShowExclusionLabels)
        Me.grpChartDisplay.Controls.Add(Me.chkShowExcludedPoints)
        Me.grpChartDisplay.Controls.Add(Me.chkShowLimitLabels)
        Me.grpChartDisplay.Controls.Add(Me.chkShowLegend)
        Me.grpChartDisplay.Controls.Add(Me.chkShowPointLabels)
        Me.grpChartDisplay.Controls.Add(Me.chkShowMajorGridlines)
        Me.grpChartDisplay.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpChartDisplay.Location = New System.Drawing.Point(2, 281)
        Me.grpChartDisplay.Name = "grpChartDisplay"
        Me.grpChartDisplay.Size = New System.Drawing.Size(862, 141)
        Me.grpChartDisplay.TabIndex = 37
        Me.grpChartDisplay.TabStop = False
        Me.grpChartDisplay.Text = "Chart display"
        '
        'chkShowStageBoundaries
        '
        Me.chkShowStageBoundaries.AutoSize = True
        Me.chkShowStageBoundaries.Checked = True
        Me.chkShowStageBoundaries.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowStageBoundaries.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowStageBoundaries.Location = New System.Drawing.Point(156, 99)
        Me.chkShowStageBoundaries.Name = "chkShowStageBoundaries"
        Me.chkShowStageBoundaries.Size = New System.Drawing.Size(136, 20)
        Me.chkShowStageBoundaries.TabIndex = 38
        Me.chkShowStageBoundaries.Text = "Stage boundaries"
        Me.chkShowStageBoundaries.UseVisualStyleBackColor = True
        '
        'chkShowSignalLabels
        '
        Me.chkShowSignalLabels.AutoSize = True
        Me.chkShowSignalLabels.Checked = True
        Me.chkShowSignalLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowSignalLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowSignalLabels.Location = New System.Drawing.Point(9, 99)
        Me.chkShowSignalLabels.Name = "chkShowSignalLabels"
        Me.chkShowSignalLabels.Size = New System.Drawing.Size(107, 20)
        Me.chkShowSignalLabels.TabIndex = 37
        Me.chkShowSignalLabels.Text = "Signal labels"
        Me.chkShowSignalLabels.UseVisualStyleBackColor = True
        '
        'chkShowExclusionLabels
        '
        Me.chkShowExclusionLabels.AutoSize = True
        Me.chkShowExclusionLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowExclusionLabels.Location = New System.Drawing.Point(156, 21)
        Me.chkShowExclusionLabels.Name = "chkShowExclusionLabels"
        Me.chkShowExclusionLabels.Size = New System.Drawing.Size(126, 20)
        Me.chkShowExclusionLabels.TabIndex = 36
        Me.chkShowExclusionLabels.Text = "Exclusion labels"
        Me.chkShowExclusionLabels.UseVisualStyleBackColor = True
        '
        'chkShowExcludedPoints
        '
        Me.chkShowExcludedPoints.AutoSize = True
        Me.chkShowExcludedPoints.Checked = True
        Me.chkShowExcludedPoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowExcludedPoints.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowExcludedPoints.Location = New System.Drawing.Point(156, 73)
        Me.chkShowExcludedPoints.Name = "chkShowExcludedPoints"
        Me.chkShowExcludedPoints.Size = New System.Drawing.Size(124, 20)
        Me.chkShowExcludedPoints.TabIndex = 35
        Me.chkShowExcludedPoints.Text = "Excluded points"
        Me.chkShowExcludedPoints.UseVisualStyleBackColor = True
        '
        'chkShowLimitLabels
        '
        Me.chkShowLimitLabels.AutoSize = True
        Me.chkShowLimitLabels.Checked = True
        Me.chkShowLimitLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowLimitLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowLimitLabels.Location = New System.Drawing.Point(156, 47)
        Me.chkShowLimitLabels.Name = "chkShowLimitLabels"
        Me.chkShowLimitLabels.Size = New System.Drawing.Size(96, 20)
        Me.chkShowLimitLabels.TabIndex = 34
        Me.chkShowLimitLabels.Text = "Limit labels"
        Me.chkShowLimitLabels.UseVisualStyleBackColor = True
        '
        'chkShowLegend
        '
        Me.chkShowLegend.AutoSize = True
        Me.chkShowLegend.Checked = True
        Me.chkShowLegend.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowLegend.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowLegend.Location = New System.Drawing.Point(9, 21)
        Me.chkShowLegend.Name = "chkShowLegend"
        Me.chkShowLegend.Size = New System.Drawing.Size(111, 20)
        Me.chkShowLegend.TabIndex = 33
        Me.chkShowLegend.Text = "Show Legend"
        Me.chkShowLegend.UseVisualStyleBackColor = True
        '
        'chkShowPointLabels
        '
        Me.chkShowPointLabels.AutoSize = True
        Me.chkShowPointLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowPointLabels.Location = New System.Drawing.Point(9, 73)
        Me.chkShowPointLabels.Name = "chkShowPointLabels"
        Me.chkShowPointLabels.Size = New System.Drawing.Size(99, 20)
        Me.chkShowPointLabels.TabIndex = 32
        Me.chkShowPointLabels.Text = "Point labels"
        Me.chkShowPointLabels.UseVisualStyleBackColor = True
        '
        'chkShowMajorGridlines
        '
        Me.chkShowMajorGridlines.AutoSize = True
        Me.chkShowMajorGridlines.Checked = True
        Me.chkShowMajorGridlines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowMajorGridlines.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowMajorGridlines.Location = New System.Drawing.Point(9, 47)
        Me.chkShowMajorGridlines.Name = "chkShowMajorGridlines"
        Me.chkShowMajorGridlines.Size = New System.Drawing.Size(117, 20)
        Me.chkShowMajorGridlines.TabIndex = 31
        Me.chkShowMajorGridlines.Text = "Major gridlines"
        Me.chkShowMajorGridlines.UseVisualStyleBackColor = True
        '
        'grpOutputs
        '
        Me.grpOutputs.Controls.Add(Me.cbDiagnosticsScope)
        Me.grpOutputs.Controls.Add(Me.lblDiagnosticsScope)
        Me.grpOutputs.Controls.Add(Me.chkWriteDiagnostics)
        Me.grpOutputs.Controls.Add(Me.chkWriteModelDetails)
        Me.grpOutputs.Controls.Add(Me.chkWriteSettingsAudit)
        Me.grpOutputs.Controls.Add(Me.chkWriteSummary)
        Me.grpOutputs.Controls.Add(Me.chkCreateControlChart)
        Me.grpOutputs.Controls.Add(Me.chkWriteSignals)
        Me.grpOutputs.Controls.Add(Me.chkWriteChartData)
        Me.grpOutputs.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpOutputs.Location = New System.Drawing.Point(3, 3)
        Me.grpOutputs.Name = "grpOutputs"
        Me.grpOutputs.Size = New System.Drawing.Size(862, 125)
        Me.grpOutputs.TabIndex = 33
        Me.grpOutputs.TabStop = False
        Me.grpOutputs.Text = "Outputs"
        '
        'cbDiagnosticsScope
        '
        Me.cbDiagnosticsScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDiagnosticsScope.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDiagnosticsScope.FormattingEnabled = True
        Me.cbDiagnosticsScope.Location = New System.Drawing.Point(131, 78)
        Me.cbDiagnosticsScope.Name = "cbDiagnosticsScope"
        Me.cbDiagnosticsScope.Size = New System.Drawing.Size(216, 24)
        Me.cbDiagnosticsScope.TabIndex = 35
        '
        'lblDiagnosticsScope
        '
        Me.lblDiagnosticsScope.AutoSize = True
        Me.lblDiagnosticsScope.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDiagnosticsScope.Location = New System.Drawing.Point(6, 81)
        Me.lblDiagnosticsScope.Name = "lblDiagnosticsScope"
        Me.lblDiagnosticsScope.Size = New System.Drawing.Size(119, 16)
        Me.lblDiagnosticsScope.TabIndex = 34
        Me.lblDiagnosticsScope.Text = "Diagnostics scope"
        '
        'chkWriteDiagnostics
        '
        Me.chkWriteDiagnostics.AutoSize = True
        Me.chkWriteDiagnostics.Checked = True
        Me.chkWriteDiagnostics.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWriteDiagnostics.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWriteDiagnostics.Location = New System.Drawing.Point(454, 47)
        Me.chkWriteDiagnostics.Name = "chkWriteDiagnostics"
        Me.chkWriteDiagnostics.Size = New System.Drawing.Size(206, 20)
        Me.chkWriteDiagnostics.TabIndex = 33
        Me.chkWriteDiagnostics.Text = "Diagnostics and Contributions"
        Me.chkWriteDiagnostics.UseVisualStyleBackColor = True
        '
        'chkWriteModelDetails
        '
        Me.chkWriteModelDetails.AutoSize = True
        Me.chkWriteModelDetails.Checked = True
        Me.chkWriteModelDetails.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWriteModelDetails.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWriteModelDetails.Location = New System.Drawing.Point(300, 47)
        Me.chkWriteModelDetails.Name = "chkWriteModelDetails"
        Me.chkWriteModelDetails.Size = New System.Drawing.Size(112, 20)
        Me.chkWriteModelDetails.TabIndex = 32
        Me.chkWriteModelDetails.Text = "Model Details"
        Me.chkWriteModelDetails.UseVisualStyleBackColor = True
        '
        'chkWriteSettingsAudit
        '
        Me.chkWriteSettingsAudit.AutoSize = True
        Me.chkWriteSettingsAudit.Checked = True
        Me.chkWriteSettingsAudit.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWriteSettingsAudit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWriteSettingsAudit.Location = New System.Drawing.Point(300, 21)
        Me.chkWriteSettingsAudit.Name = "chkWriteSettingsAudit"
        Me.chkWriteSettingsAudit.Size = New System.Drawing.Size(136, 20)
        Me.chkWriteSettingsAudit.TabIndex = 31
        Me.chkWriteSettingsAudit.Text = "Settings and Audit"
        Me.chkWriteSettingsAudit.UseVisualStyleBackColor = True
        '
        'chkWriteSummary
        '
        Me.chkWriteSummary.AutoSize = True
        Me.chkWriteSummary.Checked = True
        Me.chkWriteSummary.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWriteSummary.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWriteSummary.Location = New System.Drawing.Point(5, 21)
        Me.chkWriteSummary.Name = "chkWriteSummary"
        Me.chkWriteSummary.Size = New System.Drawing.Size(157, 20)
        Me.chkWriteSummary.TabIndex = 27
        Me.chkWriteSummary.Text = "Multivariate Summary"
        Me.chkWriteSummary.UseVisualStyleBackColor = True
        '
        'chkCreateControlChart
        '
        Me.chkCreateControlChart.AutoSize = True
        Me.chkCreateControlChart.Checked = True
        Me.chkCreateControlChart.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCreateControlChart.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkCreateControlChart.Location = New System.Drawing.Point(5, 47)
        Me.chkCreateControlChart.Name = "chkCreateControlChart"
        Me.chkCreateControlChart.Size = New System.Drawing.Size(144, 20)
        Me.chkCreateControlChart.TabIndex = 28
        Me.chkCreateControlChart.Text = "Create control chart"
        Me.chkCreateControlChart.UseVisualStyleBackColor = True
        '
        'chkWriteSignals
        '
        Me.chkWriteSignals.AutoSize = True
        Me.chkWriteSignals.Checked = True
        Me.chkWriteSignals.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWriteSignals.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWriteSignals.Location = New System.Drawing.Point(167, 47)
        Me.chkWriteSignals.Name = "chkWriteSignals"
        Me.chkWriteSignals.Size = New System.Drawing.Size(74, 20)
        Me.chkWriteSignals.TabIndex = 30
        Me.chkWriteSignals.Text = "Signals"
        Me.chkWriteSignals.UseVisualStyleBackColor = True
        '
        'chkWriteChartData
        '
        Me.chkWriteChartData.AutoSize = True
        Me.chkWriteChartData.Checked = True
        Me.chkWriteChartData.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWriteChartData.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWriteChartData.Location = New System.Drawing.Point(167, 21)
        Me.chkWriteChartData.Name = "chkWriteChartData"
        Me.chkWriteChartData.Size = New System.Drawing.Size(92, 20)
        Me.chkWriteChartData.TabIndex = 29
        Me.chkWriteChartData.Text = "Chart Data"
        Me.chkWriteChartData.UseVisualStyleBackColor = True
        '
        'grpTitleAxes
        '
        Me.grpTitleAxes.Controls.Add(Me.tbValueNumberFormat)
        Me.grpTitleAxes.Controls.Add(Me.lblValueNumberFormat)
        Me.grpTitleAxes.Controls.Add(Me.cbHorizontalTickOrientation)
        Me.grpTitleAxes.Controls.Add(Me.lblHorizontalTickOrientation)
        Me.grpTitleAxes.Controls.Add(Me.chkUseSequenceValuesForHorizontalAxis)
        Me.grpTitleAxes.Controls.Add(Me.lblHorizontalAxisTitle)
        Me.grpTitleAxes.Controls.Add(Me.tbHorizontalAxisTitle)
        Me.grpTitleAxes.Controls.Add(Me.lblValueAxisTitle)
        Me.grpTitleAxes.Controls.Add(Me.tbValueAxisTitle)
        Me.grpTitleAxes.Controls.Add(Me.lblChartTitle)
        Me.grpTitleAxes.Controls.Add(Me.tbChartTitle)
        Me.grpTitleAxes.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpTitleAxes.Location = New System.Drawing.Point(3, 134)
        Me.grpTitleAxes.Name = "grpTitleAxes"
        Me.grpTitleAxes.Size = New System.Drawing.Size(862, 141)
        Me.grpTitleAxes.TabIndex = 32
        Me.grpTitleAxes.TabStop = False
        Me.grpTitleAxes.Text = "Titles and axes"
        '
        'tbValueNumberFormat
        '
        Me.tbValueNumberFormat.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbValueNumberFormat.Location = New System.Drawing.Point(494, 101)
        Me.tbValueNumberFormat.Name = "tbValueNumberFormat"
        Me.tbValueNumberFormat.Size = New System.Drawing.Size(240, 22)
        Me.tbValueNumberFormat.TabIndex = 36
        Me.tbValueNumberFormat.Text = "0.####"
        '
        'lblValueNumberFormat
        '
        Me.lblValueNumberFormat.AutoSize = True
        Me.lblValueNumberFormat.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblValueNumberFormat.Location = New System.Drawing.Point(370, 104)
        Me.lblValueNumberFormat.Name = "lblValueNumberFormat"
        Me.lblValueNumberFormat.Size = New System.Drawing.Size(95, 16)
        Me.lblValueNumberFormat.TabIndex = 35
        Me.lblValueNumberFormat.Text = "Number format"
        '
        'cbHorizontalTickOrientation
        '
        Me.cbHorizontalTickOrientation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHorizontalTickOrientation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHorizontalTickOrientation.FormattingEnabled = True
        Me.cbHorizontalTickOrientation.Location = New System.Drawing.Point(144, 101)
        Me.cbHorizontalTickOrientation.Name = "cbHorizontalTickOrientation"
        Me.cbHorizontalTickOrientation.Size = New System.Drawing.Size(97, 24)
        Me.cbHorizontalTickOrientation.TabIndex = 34
        '
        'lblHorizontalTickOrientation
        '
        Me.lblHorizontalTickOrientation.AutoSize = True
        Me.lblHorizontalTickOrientation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHorizontalTickOrientation.Location = New System.Drawing.Point(6, 104)
        Me.lblHorizontalTickOrientation.Name = "lblHorizontalTickOrientation"
        Me.lblHorizontalTickOrientation.Size = New System.Drawing.Size(132, 16)
        Me.lblHorizontalTickOrientation.TabIndex = 33
        Me.lblHorizontalTickOrientation.Text = "Tick-label orientation"
        '
        'chkUseSequenceValuesForHorizontalAxis
        '
        Me.chkUseSequenceValuesForHorizontalAxis.AutoSize = True
        Me.chkUseSequenceValuesForHorizontalAxis.Checked = True
        Me.chkUseSequenceValuesForHorizontalAxis.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUseSequenceValuesForHorizontalAxis.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkUseSequenceValuesForHorizontalAxis.Location = New System.Drawing.Point(9, 55)
        Me.chkUseSequenceValuesForHorizontalAxis.Name = "chkUseSequenceValuesForHorizontalAxis"
        Me.chkUseSequenceValuesForHorizontalAxis.Size = New System.Drawing.Size(296, 20)
        Me.chkUseSequenceValuesForHorizontalAxis.TabIndex = 31
        Me.chkUseSequenceValuesForHorizontalAxis.Text = "Use sequence/date values on horizontal axis"
        Me.chkUseSequenceValuesForHorizontalAxis.UseVisualStyleBackColor = True
        '
        'lblHorizontalAxisTitle
        '
        Me.lblHorizontalAxisTitle.AutoSize = True
        Me.lblHorizontalAxisTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHorizontalAxisTitle.Location = New System.Drawing.Point(370, 55)
        Me.lblHorizontalAxisTitle.Name = "lblHorizontalAxisTitle"
        Me.lblHorizontalAxisTitle.Size = New System.Drawing.Size(118, 16)
        Me.lblHorizontalAxisTitle.TabIndex = 5
        Me.lblHorizontalAxisTitle.Text = "Horizontal-axis title"
        '
        'tbHorizontalAxisTitle
        '
        Me.tbHorizontalAxisTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbHorizontalAxisTitle.Location = New System.Drawing.Point(494, 52)
        Me.tbHorizontalAxisTitle.Name = "tbHorizontalAxisTitle"
        Me.tbHorizontalAxisTitle.Size = New System.Drawing.Size(240, 22)
        Me.tbHorizontalAxisTitle.TabIndex = 4
        Me.tbHorizontalAxisTitle.Text = "Sample"
        '
        'lblValueAxisTitle
        '
        Me.lblValueAxisTitle.AutoSize = True
        Me.lblValueAxisTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblValueAxisTitle.Location = New System.Drawing.Point(370, 27)
        Me.lblValueAxisTitle.Name = "lblValueAxisTitle"
        Me.lblValueAxisTitle.Size = New System.Drawing.Size(93, 16)
        Me.lblValueAxisTitle.TabIndex = 3
        Me.lblValueAxisTitle.Text = "Value-axis title"
        '
        'tbValueAxisTitle
        '
        Me.tbValueAxisTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbValueAxisTitle.Location = New System.Drawing.Point(494, 24)
        Me.tbValueAxisTitle.Name = "tbValueAxisTitle"
        Me.tbValueAxisTitle.Size = New System.Drawing.Size(240, 22)
        Me.tbValueAxisTitle.TabIndex = 2
        '
        'lblChartTitle
        '
        Me.lblChartTitle.AutoSize = True
        Me.lblChartTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChartTitle.Location = New System.Drawing.Point(6, 27)
        Me.lblChartTitle.Name = "lblChartTitle"
        Me.lblChartTitle.Size = New System.Drawing.Size(61, 16)
        Me.lblChartTitle.TabIndex = 1
        Me.lblChartTitle.Text = "Chart title"
        '
        'tbChartTitle
        '
        Me.tbChartTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbChartTitle.Location = New System.Drawing.Point(73, 24)
        Me.tbChartTitle.Name = "tbChartTitle"
        Me.tbChartTitle.Size = New System.Drawing.Size(240, 22)
        Me.tbChartTitle.TabIndex = 0
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(635, 780)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 10
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(797, 780)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 9
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'Ui22MultivariateControlCharts
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(883, 807)
        Me.Controls.Add(Me.ProgressBar)
        Me.Controls.Add(Me.btnInterrupt)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Name = "Ui22MultivariateControlCharts"
        Me.ShowIcon = False
        Me.Text = "Ui22MultivariateControlCharts"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1_ChartData.ResumeLayout(False)
        Me.grpWorksheet.ResumeLayout(False)
        Me.grpWorksheet.PerformLayout()
        Me.grpChartSelection.ResumeLayout(False)
        Me.grpChartSelection.PerformLayout()
        Me.TabPage2_ModelLimits.ResumeLayout(False)
        Me.grpHistoricalModel.ResumeLayout(False)
        Me.splitHistoricalModel.Panel1.ResumeLayout(False)
        Me.splitHistoricalModel.Panel1.PerformLayout()
        Me.splitHistoricalModel.Panel2.ResumeLayout(False)
        Me.splitHistoricalModel.Panel2.PerformLayout()
        CType(Me.splitHistoricalModel, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitHistoricalModel.ResumeLayout(False)
        CType(Me.dgvHistoricalMean, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvHistoricalCovariance, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpModelOptions.ResumeLayout(False)
        Me.grpModelOptions.PerformLayout()
        CType(Me.spinCovarianceRegularization, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinControlLimitAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage3_PhasesExclusions.ResumeLayout(False)
        Me.grpStages.ResumeLayout(False)
        CType(Me.dgvStages, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpExclusions.ResumeLayout(False)
        CType(Me.dgvExclusions, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpQuickPhaseSetup.ResumeLayout(False)
        Me.grpQuickPhaseSetup.PerformLayout()
        CType(Me.spinLastPhaseIPoint, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpPhaseColumns.ResumeLayout(False)
        Me.grpPhaseColumns.PerformLayout()
        Me.TabPage4_MethodOptions.ResumeLayout(False)
        Me.flpMethodOptions.ResumeLayout(False)
        Me.grpPcaOptions.ResumeLayout(False)
        Me.grpPcaOptions.PerformLayout()
        CType(Me.spinPcaComponentCount, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinPcaCumulativeVariance, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpGeneralizedVarianceOptions.ResumeLayout(False)
        Me.grpGeneralizedVarianceOptions.PerformLayout()
        CType(Me.spinGvSigmaMultiplier, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpMewmaOptions.ResumeLayout(False)
        Me.grpMewmaOptions.PerformLayout()
        CType(Me.spinMewmaControlLimit, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinMewmaLambda, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpMcusumOptions.ResumeLayout(False)
        Me.grpMcusumOptions.PerformLayout()
        CType(Me.spinMcusumDecisionInterval, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinMcusumReferenceValue, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpSequentialResetOptions.ResumeLayout(False)
        Me.grpSequentialResetOptions.PerformLayout()
        Me.TabPage5_OutputAppearance.ResumeLayout(False)
        Me.grpChartDimensions.ResumeLayout(False)
        Me.grpChartDimensions.PerformLayout()
        CType(Me.spinChartHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinChartWidth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpChartDisplay.ResumeLayout(False)
        Me.grpChartDisplay.PerformLayout()
        Me.grpOutputs.ResumeLayout(False)
        Me.grpOutputs.PerformLayout()
        Me.grpTitleAxes.ResumeLayout(False)
        Me.grpTitleAxes.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ProgressBar As Windows.Forms.ProgressBar
    Friend WithEvents btnInterrupt As Windows.Forms.Button
    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents TabPage1_ChartData As Windows.Forms.TabPage
    Friend WithEvents grpWorksheet As Windows.Forms.GroupBox
    Friend WithEvents btClearDataRoles As Windows.Forms.Button
    Friend WithEvents lblSequence As Windows.Forms.Label
    Friend WithEvents lbSequence As Windows.Forms.ListBox
    Friend WithEvents btRemoveSequence As Windows.Forms.Button
    Friend WithEvents btAddSequence As Windows.Forms.Button
    Friend WithEvents lblLabels As Windows.Forms.Label
    Friend WithEvents lbLabels As Windows.Forms.ListBox
    Friend WithEvents btRemoveLabels As Windows.Forms.Button
    Friend WithEvents btAddLabels As Windows.Forms.Button
    Friend WithEvents lblExposure As Windows.Forms.Label
    Friend WithEvents lbExposure As Windows.Forms.ListBox
    Friend WithEvents btRemoveExposure As Windows.Forms.Button
    Friend WithEvents btAddExposure As Windows.Forms.Button
    Friend WithEvents lblSampleSize As Windows.Forms.Label
    Friend WithEvents lbSampleSize As Windows.Forms.ListBox
    Friend WithEvents btRemoveSampleSize As Windows.Forms.Button
    Friend WithEvents btAddSampleSize As Windows.Forms.Button
    Friend WithEvents lblCount As Windows.Forms.Label
    Friend WithEvents lbCounts As Windows.Forms.ListBox
    Friend WithEvents btRemoveCounts As Windows.Forms.Button
    Friend WithEvents btAddCounts As Windows.Forms.Button
    Friend WithEvents lblSubgroupID As Windows.Forms.Label
    Friend WithEvents lbSubgroupID As Windows.Forms.ListBox
    Friend WithEvents btRemoveSubgroupID As Windows.Forms.Button
    Friend WithEvents btAddSubgroupID As Windows.Forms.Button
    Friend WithEvents lblVariables As Windows.Forms.Label
    Friend WithEvents lbVariables As Windows.Forms.ListBox
    Friend WithEvents btRemoveVariables As Windows.Forms.Button
    Friend WithEvents btAddVariables As Windows.Forms.Button
    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents lblSheetsList As Windows.Forms.Label
    Friend WithEvents grpChartSelection As Windows.Forms.GroupBox
    Friend WithEvents lblChartDescription As Windows.Forms.Label
    Friend WithEvents cbChartType As Windows.Forms.ComboBox
    Friend WithEvents lblChartType As Windows.Forms.Label
    Friend WithEvents TabPage2_ModelLimits As Windows.Forms.TabPage
    Friend WithEvents grpHistoricalModel As Windows.Forms.GroupBox
    Friend WithEvents grpModelOptions As Windows.Forms.GroupBox
    Friend WithEvents lblModelNote As Windows.Forms.Label
    Friend WithEvents spinCovarianceRegularization As Windows.Forms.NumericUpDown
    Friend WithEvents lblCovarianceRegularization As Windows.Forms.Label
    Friend WithEvents spinControlLimitAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents lblControlLimitAlpha As Windows.Forms.Label
    Friend WithEvents cbModelSource As Windows.Forms.ComboBox
    Friend WithEvents lblModelSource As Windows.Forms.Label
    Friend WithEvents cbMissingValuePolicy As Windows.Forms.ComboBox
    Friend WithEvents lblMissingValuePolicy As Windows.Forms.Label
    Friend WithEvents TabPage3_PhasesExclusions As Windows.Forms.TabPage
    Friend WithEvents grpExclusions As Windows.Forms.GroupBox
    Friend WithEvents btClearExclusions As Windows.Forms.Button
    Friend WithEvents btRemoveExclusion As Windows.Forms.Button
    Friend WithEvents btAddExclusion As Windows.Forms.Button
    Friend WithEvents dgvExclusions As Windows.Forms.DataGridView
    Friend WithEvents colExclusionPoint As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colExclusionScope As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colExclusionReason As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents grpQuickPhaseSetup As Windows.Forms.GroupBox
    Friend WithEvents btApplyQuickPhaseSetup As Windows.Forms.Button
    Friend WithEvents spinLastPhaseIPoint As Windows.Forms.NumericUpDown
    Friend WithEvents lblLastPhaseIPoint As Windows.Forms.Label
    Friend WithEvents rbPhaseIThenPhaseII As Windows.Forms.RadioButton
    Friend WithEvents rbSinglePhaseI As Windows.Forms.RadioButton
    Friend WithEvents grpPhaseColumns As Windows.Forms.GroupBox
    Friend WithEvents btImportExclusions As Windows.Forms.Button
    Friend WithEvents btImportStages As Windows.Forms.Button
    Friend WithEvents cbImportedExclusionScope As Windows.Forms.ComboBox
    Friend WithEvents lblImportedExclusionScope As Windows.Forms.Label
    Friend WithEvents cbExclusionReasonColumn As Windows.Forms.ComboBox
    Friend WithEvents lblExclusionReasonColumn As Windows.Forms.Label
    Friend WithEvents cbExclusionColumn As Windows.Forms.ComboBox
    Friend WithEvents lblExclusionColumn As Windows.Forms.Label
    Friend WithEvents cbPhaseColumn As Windows.Forms.ComboBox
    Friend WithEvents lblPhaseColumn As Windows.Forms.Label
    Friend WithEvents cbStageColumn As Windows.Forms.ComboBox
    Friend WithEvents lblStageColumn As Windows.Forms.Label
    Friend WithEvents TabPage4_MethodOptions As Windows.Forms.TabPage
    Friend WithEvents TabPage5_OutputAppearance As Windows.Forms.TabPage
    Friend WithEvents grpChartDimensions As Windows.Forms.GroupBox
    Friend WithEvents spinChartHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartHeight As Windows.Forms.Label
    Friend WithEvents btResetAppearance As Windows.Forms.Button
    Friend WithEvents spinChartWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartWidth As Windows.Forms.Label
    Friend WithEvents grpChartDisplay As Windows.Forms.GroupBox
    Friend WithEvents chkShowStageBoundaries As Windows.Forms.CheckBox
    Friend WithEvents chkShowSignalLabels As Windows.Forms.CheckBox
    Friend WithEvents chkShowExclusionLabels As Windows.Forms.CheckBox
    Friend WithEvents chkShowExcludedPoints As Windows.Forms.CheckBox
    Friend WithEvents chkShowLimitLabels As Windows.Forms.CheckBox
    Friend WithEvents chkShowLegend As Windows.Forms.CheckBox
    Friend WithEvents chkShowPointLabels As Windows.Forms.CheckBox
    Friend WithEvents chkShowMajorGridlines As Windows.Forms.CheckBox
    Friend WithEvents grpOutputs As Windows.Forms.GroupBox
    Friend WithEvents chkWriteSettingsAudit As Windows.Forms.CheckBox
    Friend WithEvents chkWriteSummary As Windows.Forms.CheckBox
    Friend WithEvents chkCreateControlChart As Windows.Forms.CheckBox
    Friend WithEvents chkWriteSignals As Windows.Forms.CheckBox
    Friend WithEvents chkWriteChartData As Windows.Forms.CheckBox
    Friend WithEvents grpTitleAxes As Windows.Forms.GroupBox
    Friend WithEvents tbValueNumberFormat As Windows.Forms.TextBox
    Friend WithEvents lblValueNumberFormat As Windows.Forms.Label
    Friend WithEvents cbHorizontalTickOrientation As Windows.Forms.ComboBox
    Friend WithEvents lblHorizontalTickOrientation As Windows.Forms.Label
    Friend WithEvents chkUseSequenceValuesForHorizontalAxis As Windows.Forms.CheckBox
    Friend WithEvents lblHorizontalAxisTitle As Windows.Forms.Label
    Friend WithEvents tbHorizontalAxisTitle As Windows.Forms.TextBox
    Friend WithEvents lblValueAxisTitle As Windows.Forms.Label
    Friend WithEvents tbValueAxisTitle As Windows.Forms.TextBox
    Friend WithEvents lblChartTitle As Windows.Forms.Label
    Friend WithEvents tbChartTitle As Windows.Forms.TextBox
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents lblDataRequirements As Windows.Forms.Label
    Friend WithEvents cbObservationStructure As Windows.Forms.ComboBox
    Friend WithEvents lblObservationStructure As Windows.Forms.Label
    Friend WithEvents chkUseLowerHotellingLimit As Windows.Forms.CheckBox
    Friend WithEvents chkAllowPseudoInverse As Windows.Forms.CheckBox
    Friend WithEvents splitHistoricalModel As Windows.Forms.SplitContainer
    Friend WithEvents dgvHistoricalMean As Windows.Forms.DataGridView
    Friend WithEvents colHistoricalMeanVariable As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colHistoricalMeanValue As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btRefreshHistoricalVariables As Windows.Forms.Button
    Friend WithEvents dgvHistoricalCovariance As Windows.Forms.DataGridView
    Friend WithEvents btImportHistoricalCovariance As Windows.Forms.Button
    Friend WithEvents btClearHistoricalModel As Windows.Forms.Button
    Friend WithEvents btImportHistoricalMean As Windows.Forms.Button
    Friend WithEvents lblHistoricalMeanGrid As Windows.Forms.Label
    Friend WithEvents lblHistoricalCovarianceGrid As Windows.Forms.Label
    Friend WithEvents grpStages As Windows.Forms.GroupBox
    Friend WithEvents dgvStages As Windows.Forms.DataGridView
    Friend WithEvents colStageID As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStageFirstPoint As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStageLastPoint As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStagePhase As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents btClearStages As Windows.Forms.Button
    Friend WithEvents btRemoveStage As Windows.Forms.Button
    Friend WithEvents btAddStage As Windows.Forms.Button
    Friend WithEvents rbSinglePhaseII As Windows.Forms.RadioButton
    Friend WithEvents flpMethodOptions As Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblMethodDescription As Windows.Forms.Label
    Friend WithEvents cbDiagnosticsScope As Windows.Forms.ComboBox
    Friend WithEvents lblDiagnosticsScope As Windows.Forms.Label
    Friend WithEvents chkWriteDiagnostics As Windows.Forms.CheckBox
    Friend WithEvents chkWriteModelDetails As Windows.Forms.CheckBox
    Friend WithEvents grpPcaOptions As Windows.Forms.GroupBox
    Friend WithEvents lblPcaNote As Windows.Forms.Label
    Friend WithEvents spinPcaComponentCount As Windows.Forms.NumericUpDown
    Friend WithEvents rbPcaFixedComponents As Windows.Forms.RadioButton
    Friend WithEvents spinPcaCumulativeVariance As Windows.Forms.NumericUpDown
    Friend WithEvents rbPcaVarianceSelection As Windows.Forms.RadioButton
    Friend WithEvents cbPcaMatrix As Windows.Forms.ComboBox
    Friend WithEvents lblPcaMatrix As Windows.Forms.Label
    Friend WithEvents grpGeneralizedVarianceOptions As Windows.Forms.GroupBox
    Friend WithEvents chkSpecifyGvSigmaMultiplier As Windows.Forms.CheckBox
    Friend WithEvents lblGvNote As Windows.Forms.Label
    Friend WithEvents spinGvSigmaMultiplier As Windows.Forms.NumericUpDown
    Friend WithEvents grpMewmaOptions As Windows.Forms.GroupBox
    Friend WithEvents lblMewmaNote As Windows.Forms.Label
    Friend WithEvents spinMewmaControlLimit As Windows.Forms.NumericUpDown
    Friend WithEvents lblMewmaLambda As Windows.Forms.Label
    Friend WithEvents spinMewmaLambda As Windows.Forms.NumericUpDown
    Friend WithEvents chkSpecifyMewmaControlLimit As Windows.Forms.CheckBox
    Friend WithEvents grpMcusumOptions As Windows.Forms.GroupBox
    Friend WithEvents lblMcusumDecisionInterval As Windows.Forms.Label
    Friend WithEvents lblMcusumNote As Windows.Forms.Label
    Friend WithEvents spinMcusumDecisionInterval As Windows.Forms.NumericUpDown
    Friend WithEvents lblMcusumReferenceValue As Windows.Forms.Label
    Friend WithEvents spinMcusumReferenceValue As Windows.Forms.NumericUpDown
    Friend WithEvents grpSequentialResetOptions As Windows.Forms.GroupBox
    Friend WithEvents chkResetAtStageBoundary As Windows.Forms.CheckBox
    Friend WithEvents lblSequenceGapBehavior As Windows.Forms.Label
    Friend WithEvents chkResetAfterSignal As Windows.Forms.CheckBox
    Friend WithEvents chkResetAtPhaseBoundary As Windows.Forms.CheckBox
    Friend WithEvents cbSequenceGapBehavior As Windows.Forms.ComboBox
End Class
