<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui21ControlCharts
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
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabPage5_OutputAppearance = New System.Windows.Forms.TabPage()
        Me.grpChartDimensions = New System.Windows.Forms.GroupBox()
        Me.spinPanelSpacing = New System.Windows.Forms.NumericUpDown()
        Me.lblPanelSpacing = New System.Windows.Forms.Label()
        Me.spinPanelHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblPanelHeight = New System.Windows.Forms.Label()
        Me.btResetAppearance = New System.Windows.Forms.Button()
        Me.spinChartWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblChartWidth = New System.Windows.Forms.Label()
        Me.grpSpecifications = New System.Windows.Forms.GroupBox()
        Me.tbUpperSpecificationLimit = New System.Windows.Forms.TextBox()
        Me.lblUpperSpecificationLimit = New System.Windows.Forms.Label()
        Me.tbTarget = New System.Windows.Forms.TextBox()
        Me.lblTarget = New System.Windows.Forms.Label()
        Me.tbLowerSpecificationLimit = New System.Windows.Forms.TextBox()
        Me.lblLowerSpecificationLimit = New System.Windows.Forms.Label()
        Me.chkShowTargetLine = New System.Windows.Forms.CheckBox()
        Me.chkShowSpecificationLimits = New System.Windows.Forms.CheckBox()
        Me.grpChartDisplay = New System.Windows.Forms.GroupBox()
        Me.cbZoneDisplay = New System.Windows.Forms.ComboBox()
        Me.lblZoneDisplay = New System.Windows.Forms.Label()
        Me.chkShowZoneSeriesInLegend = New System.Windows.Forms.CheckBox()
        Me.chkShowStageBoundaries = New System.Windows.Forms.CheckBox()
        Me.chkShowSignalLabels = New System.Windows.Forms.CheckBox()
        Me.chkShowExclusionLabels = New System.Windows.Forms.CheckBox()
        Me.chkShowExcludedPoints = New System.Windows.Forms.CheckBox()
        Me.chkShowLimitLabels = New System.Windows.Forms.CheckBox()
        Me.chkShowLegend = New System.Windows.Forms.CheckBox()
        Me.chkShowPointLabels = New System.Windows.Forms.CheckBox()
        Me.chkShowMajorGridlines = New System.Windows.Forms.CheckBox()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.chkWriteSettingsAudit = New System.Windows.Forms.CheckBox()
        Me.chkWriteSummary = New System.Windows.Forms.CheckBox()
        Me.chkCreateControlCharts = New System.Windows.Forms.CheckBox()
        Me.chkWriteSignals = New System.Windows.Forms.CheckBox()
        Me.chkWriteChartData = New System.Windows.Forms.CheckBox()
        Me.grpTitleAxes = New System.Windows.Forms.GroupBox()
        Me.tbValueNumberFormat = New System.Windows.Forms.TextBox()
        Me.lblValueNumberFormat = New System.Windows.Forms.Label()
        Me.cbHorizontalTickOrientation = New System.Windows.Forms.ComboBox()
        Me.lblHorizontalTickOrientation = New System.Windows.Forms.Label()
        Me.chkShowHorizontalAxisOnEveryPanel = New System.Windows.Forms.CheckBox()
        Me.chkUseSequenceValuesForHorizontalAxis = New System.Windows.Forms.CheckBox()
        Me.lblHorizontalAxisTitle = New System.Windows.Forms.Label()
        Me.tbHorizontalAxisTitle = New System.Windows.Forms.TextBox()
        Me.lblValueAxisTitle = New System.Windows.Forms.Label()
        Me.tbValueAxisTitle = New System.Windows.Forms.TextBox()
        Me.lblChartTitle = New System.Windows.Forms.Label()
        Me.tbChartTitle = New System.Windows.Forms.TextBox()
        Me.TabPage4_SignalRules = New System.Windows.Forms.TabPage()
        Me.grpSequenceOptions = New System.Windows.Forms.GroupBox()
        Me.cbSignalMarkingMode = New System.Windows.Forms.ComboBox()
        Me.lblSignalMarkingMode = New System.Windows.Forms.Label()
        Me.cbSequenceGapBehavior = New System.Windows.Forms.ComboBox()
        Me.lblSequenceGapBehavior = New System.Windows.Forms.Label()
        Me.lblRuleApplicability = New System.Windows.Forms.Label()
        Me.cbRulePhaseScope = New System.Windows.Forms.ComboBox()
        Me.lblRulePhaseScope = New System.Windows.Forms.Label()
        Me.tbRuleDescription = New System.Windows.Forms.TextBox()
        Me.btResetCustomRules = New System.Windows.Forms.Button()
        Me.btRemoveRule = New System.Windows.Forms.Button()
        Me.btAddRule = New System.Windows.Forms.Button()
        Me.dgvRules = New System.Windows.Forms.DataGridView()
        Me.colRuleEnabled = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colRuleNumber = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colRuleCode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colRuleName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colRuleKind = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colRuleWindow = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colRuleMinimumPoints = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colRuleSigma = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colRuleSide = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colRuleScope = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.grpRulePreset = New System.Windows.Forms.GroupBox()
        Me.lblRulePresetDescription = New System.Windows.Forms.Label()
        Me.btCopyPresetToCustom = New System.Windows.Forms.Button()
        Me.btLoadRulePreset = New System.Windows.Forms.Button()
        Me.cbRulePreset = New System.Windows.Forms.ComboBox()
        Me.lblRulePreset = New System.Windows.Forms.Label()
        Me.TabPage3_PhasesExclusions = New System.Windows.Forms.TabPage()
        Me.grpExclusions = New System.Windows.Forms.GroupBox()
        Me.btClearExclusions = New System.Windows.Forms.Button()
        Me.btRemoveExclusion = New System.Windows.Forms.Button()
        Me.btAddExclusion = New System.Windows.Forms.Button()
        Me.dgvExclusions = New System.Windows.Forms.DataGridView()
        Me.colExclusionPoint = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colExclusionScope = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colExclusionReason = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvStages = New System.Windows.Forms.DataGridView()
        Me.colStageID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStageDisplayName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStageFirstPoint = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStageLastPoint = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStagePhase = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colStageLimitMode = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colStageReferenceID = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.grpQuickPhaseSetup = New System.Windows.Forms.GroupBox()
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
        Me.TabPage2_ParametersLimits = New System.Windows.Forms.TabPage()
        Me.grpTimeWeightedParameters = New System.Windows.Forms.GroupBox()
        Me.chkUseSteadyStateLimits = New System.Windows.Forms.CheckBox()
        Me.lblTimeWeightedNote = New System.Windows.Forms.Label()
        Me.spinCusumDecisionInterval = New System.Windows.Forms.NumericUpDown()
        Me.lblCusumDecisionInterval = New System.Windows.Forms.Label()
        Me.spinHeadStart = New System.Windows.Forms.NumericUpDown()
        Me.lblHeadStart = New System.Windows.Forms.Label()
        Me.spinMovingAverageSpan = New System.Windows.Forms.NumericUpDown()
        Me.lblMovingAverageSpan = New System.Windows.Forms.Label()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.spinCusumReferenceValue = New System.Windows.Forms.NumericUpDown()
        Me.lblCusumReferenceValue = New System.Windows.Forms.Label()
        Me.spinEwmaLambda = New System.Windows.Forms.NumericUpDown()
        Me.lblEwmaLambda = New System.Windows.Forms.Label()
        Me.grpHistoricalParameters = New System.Windows.Forms.GroupBox()
        Me.btClearHistoricalParameters = New System.Windows.Forms.Button()
        Me.btRemoveHistoricalParameter = New System.Windows.Forms.Button()
        Me.btAddHistoricalParameter = New System.Windows.Forms.Button()
        Me.dgvHistoricalParameters = New System.Windows.Forms.DataGridView()
        Me.colHistoryStageID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colHistoryMean = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colHistorySigma = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colHistoryProportion = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colHistoryMeanCount = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colHistoryMeanRate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.grpGeneralOptions = New System.Windows.Forms.GroupBox()
        Me.cbNaturalLimitPolicy = New System.Windows.Forms.ComboBox()
        Me.lblNaturalLimitPolicy = New System.Windows.Forms.Label()
        Me.chkUseBiasCorrection = New System.Windows.Forms.CheckBox()
        Me.spinMovingRangeLength = New System.Windows.Forms.NumericUpDown()
        Me.lblMovingRangeLength = New System.Windows.Forms.Label()
        Me.cbWithinSigmaEstimator = New System.Windows.Forms.ComboBox()
        Me.lblWithinSigmaEstimator = New System.Windows.Forms.Label()
        Me.cbControlLimitMethod = New System.Windows.Forms.ComboBox()
        Me.lblControlLimitMethod = New System.Windows.Forms.Label()
        Me.spinSigmaMultiplier = New System.Windows.Forms.NumericUpDown()
        Me.lblSigmaMultiplier = New System.Windows.Forms.Label()
        Me.cbParameterSource = New System.Windows.Forms.ComboBox()
        Me.lblParameterSource = New System.Windows.Forms.Label()
        Me.cbMissingValuePolicy = New System.Windows.Forms.ComboBox()
        Me.lblMissingValuePolicy = New System.Windows.Forms.Label()
        Me.TabPage1_ChartData = New System.Windows.Forms.TabPage()
        Me.grpWorksheet = New System.Windows.Forms.GroupBox()
        Me.lblDataRequirements = New System.Windows.Forms.Label()
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
        Me.lblValues = New System.Windows.Forms.Label()
        Me.lbValues = New System.Windows.Forms.ListBox()
        Me.btRemoveValues = New System.Windows.Forms.Button()
        Me.btAddValues = New System.Windows.Forms.Button()
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.btReload = New System.Windows.Forms.Button()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.cbSheetsList = New System.Windows.Forms.ComboBox()
        Me.lblSheetsList = New System.Windows.Forms.Label()
        Me.grpChartSelection = New System.Windows.Forms.GroupBox()
        Me.lblChartDescription = New System.Windows.Forms.Label()
        Me.cbDataLayout = New System.Windows.Forms.ComboBox()
        Me.lblDataLayout = New System.Windows.Forms.Label()
        Me.cbChartType = New System.Windows.Forms.ComboBox()
        Me.lblChartType = New System.Windows.Forms.Label()
        Me.cbChartFamily = New System.Windows.Forms.ComboBox()
        Me.lblChartFamily = New System.Windows.Forms.Label()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.btnInterrupt = New System.Windows.Forms.Button()
        Me.ProgressBar = New System.Windows.Forms.ProgressBar()
        Me.TabPage5_OutputAppearance.SuspendLayout()
        Me.grpChartDimensions.SuspendLayout()
        CType(Me.spinPanelSpacing, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinPanelHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinChartWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpSpecifications.SuspendLayout()
        Me.grpChartDisplay.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpTitleAxes.SuspendLayout()
        Me.TabPage4_SignalRules.SuspendLayout()
        Me.grpSequenceOptions.SuspendLayout()
        CType(Me.dgvRules, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpRulePreset.SuspendLayout()
        Me.TabPage3_PhasesExclusions.SuspendLayout()
        Me.grpExclusions.SuspendLayout()
        CType(Me.dgvExclusions, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvStages, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpQuickPhaseSetup.SuspendLayout()
        CType(Me.spinLastPhaseIPoint, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpPhaseColumns.SuspendLayout()
        Me.TabPage2_ParametersLimits.SuspendLayout()
        Me.grpTimeWeightedParameters.SuspendLayout()
        CType(Me.spinCusumDecisionInterval, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinHeadStart, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinMovingAverageSpan, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinCusumReferenceValue, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinEwmaLambda, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpHistoricalParameters.SuspendLayout()
        CType(Me.dgvHistoricalParameters, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpGeneralOptions.SuspendLayout()
        CType(Me.spinMovingRangeLength, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinSigmaMultiplier, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage1_ChartData.SuspendLayout()
        Me.grpWorksheet.SuspendLayout()
        Me.grpChartSelection.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(633, 778)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 5
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(795, 778)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 4
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'TabPage5_OutputAppearance
        '
        Me.TabPage5_OutputAppearance.AutoScroll = True
        Me.TabPage5_OutputAppearance.Controls.Add(Me.grpChartDimensions)
        Me.TabPage5_OutputAppearance.Controls.Add(Me.grpSpecifications)
        Me.TabPage5_OutputAppearance.Controls.Add(Me.grpChartDisplay)
        Me.TabPage5_OutputAppearance.Controls.Add(Me.grpOutput)
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
        Me.grpChartDimensions.Controls.Add(Me.spinPanelSpacing)
        Me.grpChartDimensions.Controls.Add(Me.lblPanelSpacing)
        Me.grpChartDimensions.Controls.Add(Me.spinPanelHeight)
        Me.grpChartDimensions.Controls.Add(Me.lblPanelHeight)
        Me.grpChartDimensions.Controls.Add(Me.btResetAppearance)
        Me.grpChartDimensions.Controls.Add(Me.spinChartWidth)
        Me.grpChartDimensions.Controls.Add(Me.lblChartWidth)
        Me.grpChartDimensions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpChartDimensions.Location = New System.Drawing.Point(3, 493)
        Me.grpChartDimensions.Name = "grpChartDimensions"
        Me.grpChartDimensions.Size = New System.Drawing.Size(862, 109)
        Me.grpChartDimensions.TabIndex = 46
        Me.grpChartDimensions.TabStop = False
        Me.grpChartDimensions.Text = "Chart dimensions"
        '
        'spinPanelSpacing
        '
        Me.spinPanelSpacing.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinPanelSpacing.Location = New System.Drawing.Point(132, 72)
        Me.spinPanelSpacing.Name = "spinPanelSpacing"
        Me.spinPanelSpacing.Size = New System.Drawing.Size(64, 22)
        Me.spinPanelSpacing.TabIndex = 32
        Me.spinPanelSpacing.Value = New Decimal(New Integer() {18, 0, 0, 0})
        '
        'lblPanelSpacing
        '
        Me.lblPanelSpacing.AutoSize = True
        Me.lblPanelSpacing.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPanelSpacing.Location = New System.Drawing.Point(9, 74)
        Me.lblPanelSpacing.Name = "lblPanelSpacing"
        Me.lblPanelSpacing.Size = New System.Drawing.Size(93, 16)
        Me.lblPanelSpacing.TabIndex = 31
        Me.lblPanelSpacing.Text = "Panel spacing"
        '
        'spinPanelHeight
        '
        Me.spinPanelHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinPanelHeight.Location = New System.Drawing.Point(132, 44)
        Me.spinPanelHeight.Maximum = New Decimal(New Integer() {5000, 0, 0, 0})
        Me.spinPanelHeight.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.spinPanelHeight.Name = "spinPanelHeight"
        Me.spinPanelHeight.Size = New System.Drawing.Size(64, 22)
        Me.spinPanelHeight.TabIndex = 30
        Me.spinPanelHeight.Value = New Decimal(New Integer() {300, 0, 0, 0})
        '
        'lblPanelHeight
        '
        Me.lblPanelHeight.AutoSize = True
        Me.lblPanelHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPanelHeight.Location = New System.Drawing.Point(9, 46)
        Me.lblPanelHeight.Name = "lblPanelHeight"
        Me.lblPanelHeight.Size = New System.Drawing.Size(77, 16)
        Me.lblPanelHeight.TabIndex = 29
        Me.lblPanelHeight.Text = "Chart height"
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
        'grpSpecifications
        '
        Me.grpSpecifications.Controls.Add(Me.tbUpperSpecificationLimit)
        Me.grpSpecifications.Controls.Add(Me.lblUpperSpecificationLimit)
        Me.grpSpecifications.Controls.Add(Me.tbTarget)
        Me.grpSpecifications.Controls.Add(Me.lblTarget)
        Me.grpSpecifications.Controls.Add(Me.tbLowerSpecificationLimit)
        Me.grpSpecifications.Controls.Add(Me.lblLowerSpecificationLimit)
        Me.grpSpecifications.Controls.Add(Me.chkShowTargetLine)
        Me.grpSpecifications.Controls.Add(Me.chkShowSpecificationLimits)
        Me.grpSpecifications.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpSpecifications.Location = New System.Drawing.Point(3, 378)
        Me.grpSpecifications.Name = "grpSpecifications"
        Me.grpSpecifications.Size = New System.Drawing.Size(862, 109)
        Me.grpSpecifications.TabIndex = 42
        Me.grpSpecifications.TabStop = False
        Me.grpSpecifications.Text = "Specifications"
        '
        'tbUpperSpecificationLimit
        '
        Me.tbUpperSpecificationLimit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbUpperSpecificationLimit.Location = New System.Drawing.Point(399, 75)
        Me.tbUpperSpecificationLimit.Name = "tbUpperSpecificationLimit"
        Me.tbUpperSpecificationLimit.Size = New System.Drawing.Size(240, 22)
        Me.tbUpperSpecificationLimit.TabIndex = 45
        Me.tbUpperSpecificationLimit.Text = "USL"
        '
        'lblUpperSpecificationLimit
        '
        Me.lblUpperSpecificationLimit.AutoSize = True
        Me.lblUpperSpecificationLimit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUpperSpecificationLimit.Location = New System.Drawing.Point(213, 78)
        Me.lblUpperSpecificationLimit.Name = "lblUpperSpecificationLimit"
        Me.lblUpperSpecificationLimit.Size = New System.Drawing.Size(182, 16)
        Me.lblUpperSpecificationLimit.TabIndex = 44
        Me.lblUpperSpecificationLimit.Text = "Upper specification limit label"
        '
        'tbTarget
        '
        Me.tbTarget.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbTarget.Location = New System.Drawing.Point(399, 47)
        Me.tbTarget.Name = "tbTarget"
        Me.tbTarget.Size = New System.Drawing.Size(240, 22)
        Me.tbTarget.TabIndex = 43
        Me.tbTarget.Text = "Target"
        '
        'lblTarget
        '
        Me.lblTarget.AutoSize = True
        Me.lblTarget.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTarget.Location = New System.Drawing.Point(213, 50)
        Me.lblTarget.Name = "lblTarget"
        Me.lblTarget.Size = New System.Drawing.Size(80, 16)
        Me.lblTarget.TabIndex = 42
        Me.lblTarget.Text = "Target label"
        '
        'tbLowerSpecificationLimit
        '
        Me.tbLowerSpecificationLimit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbLowerSpecificationLimit.Location = New System.Drawing.Point(399, 19)
        Me.tbLowerSpecificationLimit.Name = "tbLowerSpecificationLimit"
        Me.tbLowerSpecificationLimit.Size = New System.Drawing.Size(240, 22)
        Me.tbLowerSpecificationLimit.TabIndex = 41
        Me.tbLowerSpecificationLimit.Text = "LSL"
        '
        'lblLowerSpecificationLimit
        '
        Me.lblLowerSpecificationLimit.AutoSize = True
        Me.lblLowerSpecificationLimit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLowerSpecificationLimit.Location = New System.Drawing.Point(213, 22)
        Me.lblLowerSpecificationLimit.Name = "lblLowerSpecificationLimit"
        Me.lblLowerSpecificationLimit.Size = New System.Drawing.Size(180, 16)
        Me.lblLowerSpecificationLimit.TabIndex = 40
        Me.lblLowerSpecificationLimit.Text = "Lower specification limit label"
        '
        'chkShowTargetLine
        '
        Me.chkShowTargetLine.AutoSize = True
        Me.chkShowTargetLine.Checked = True
        Me.chkShowTargetLine.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowTargetLine.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowTargetLine.Location = New System.Drawing.Point(9, 47)
        Me.chkShowTargetLine.Name = "chkShowTargetLine"
        Me.chkShowTargetLine.Size = New System.Drawing.Size(123, 20)
        Me.chkShowTargetLine.TabIndex = 36
        Me.chkShowTargetLine.Text = "Show target line"
        Me.chkShowTargetLine.UseVisualStyleBackColor = True
        '
        'chkShowSpecificationLimits
        '
        Me.chkShowSpecificationLimits.AutoSize = True
        Me.chkShowSpecificationLimits.Checked = True
        Me.chkShowSpecificationLimits.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowSpecificationLimits.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowSpecificationLimits.Location = New System.Drawing.Point(9, 21)
        Me.chkShowSpecificationLimits.Name = "chkShowSpecificationLimits"
        Me.chkShowSpecificationLimits.Size = New System.Drawing.Size(173, 20)
        Me.chkShowSpecificationLimits.TabIndex = 33
        Me.chkShowSpecificationLimits.Text = "Show specification limits"
        Me.chkShowSpecificationLimits.UseVisualStyleBackColor = True
        '
        'grpChartDisplay
        '
        Me.grpChartDisplay.Controls.Add(Me.cbZoneDisplay)
        Me.grpChartDisplay.Controls.Add(Me.lblZoneDisplay)
        Me.grpChartDisplay.Controls.Add(Me.chkShowZoneSeriesInLegend)
        Me.grpChartDisplay.Controls.Add(Me.chkShowStageBoundaries)
        Me.grpChartDisplay.Controls.Add(Me.chkShowSignalLabels)
        Me.grpChartDisplay.Controls.Add(Me.chkShowExclusionLabels)
        Me.grpChartDisplay.Controls.Add(Me.chkShowExcludedPoints)
        Me.grpChartDisplay.Controls.Add(Me.chkShowLimitLabels)
        Me.grpChartDisplay.Controls.Add(Me.chkShowLegend)
        Me.grpChartDisplay.Controls.Add(Me.chkShowPointLabels)
        Me.grpChartDisplay.Controls.Add(Me.chkShowMajorGridlines)
        Me.grpChartDisplay.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpChartDisplay.Location = New System.Drawing.Point(3, 231)
        Me.grpChartDisplay.Name = "grpChartDisplay"
        Me.grpChartDisplay.Size = New System.Drawing.Size(862, 141)
        Me.grpChartDisplay.TabIndex = 37
        Me.grpChartDisplay.TabStop = False
        Me.grpChartDisplay.Text = "Chart display"
        '
        'cbZoneDisplay
        '
        Me.cbZoneDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbZoneDisplay.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbZoneDisplay.FormattingEnabled = True
        Me.cbZoneDisplay.Location = New System.Drawing.Point(428, 48)
        Me.cbZoneDisplay.Name = "cbZoneDisplay"
        Me.cbZoneDisplay.Size = New System.Drawing.Size(177, 24)
        Me.cbZoneDisplay.TabIndex = 41
        '
        'lblZoneDisplay
        '
        Me.lblZoneDisplay.AutoSize = True
        Me.lblZoneDisplay.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblZoneDisplay.Location = New System.Drawing.Point(337, 51)
        Me.lblZoneDisplay.Name = "lblZoneDisplay"
        Me.lblZoneDisplay.Size = New System.Drawing.Size(85, 16)
        Me.lblZoneDisplay.TabIndex = 40
        Me.lblZoneDisplay.Text = "Zone display"
        '
        'chkShowZoneSeriesInLegend
        '
        Me.chkShowZoneSeriesInLegend.AutoSize = True
        Me.chkShowZoneSeriesInLegend.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowZoneSeriesInLegend.Location = New System.Drawing.Point(340, 21)
        Me.chkShowZoneSeriesInLegend.Name = "chkShowZoneSeriesInLegend"
        Me.chkShowZoneSeriesInLegend.Size = New System.Drawing.Size(192, 20)
        Me.chkShowZoneSeriesInLegend.TabIndex = 39
        Me.chkShowZoneSeriesInLegend.Text = "Show zone series in legend"
        Me.chkShowZoneSeriesInLegend.UseVisualStyleBackColor = True
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
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.chkWriteSettingsAudit)
        Me.grpOutput.Controls.Add(Me.chkWriteSummary)
        Me.grpOutput.Controls.Add(Me.chkCreateControlCharts)
        Me.grpOutput.Controls.Add(Me.chkWriteSignals)
        Me.grpOutput.Controls.Add(Me.chkWriteChartData)
        Me.grpOutput.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpOutput.Location = New System.Drawing.Point(3, 3)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(862, 75)
        Me.grpOutput.TabIndex = 33
        Me.grpOutput.TabStop = False
        Me.grpOutput.Text = "Outputs"
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
        Me.chkWriteSummary.Size = New System.Drawing.Size(116, 20)
        Me.chkWriteSummary.TabIndex = 27
        Me.chkWriteSummary.Text = "SPC Summary"
        Me.chkWriteSummary.UseVisualStyleBackColor = True
        '
        'chkCreateControlCharts
        '
        Me.chkCreateControlCharts.AutoSize = True
        Me.chkCreateControlCharts.Checked = True
        Me.chkCreateControlCharts.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCreateControlCharts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkCreateControlCharts.Location = New System.Drawing.Point(5, 47)
        Me.chkCreateControlCharts.Name = "chkCreateControlCharts"
        Me.chkCreateControlCharts.Size = New System.Drawing.Size(112, 20)
        Me.chkCreateControlCharts.TabIndex = 28
        Me.chkCreateControlCharts.Text = "Control Charts"
        Me.chkCreateControlCharts.UseVisualStyleBackColor = True
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
        Me.grpTitleAxes.Controls.Add(Me.chkShowHorizontalAxisOnEveryPanel)
        Me.grpTitleAxes.Controls.Add(Me.chkUseSequenceValuesForHorizontalAxis)
        Me.grpTitleAxes.Controls.Add(Me.lblHorizontalAxisTitle)
        Me.grpTitleAxes.Controls.Add(Me.tbHorizontalAxisTitle)
        Me.grpTitleAxes.Controls.Add(Me.lblValueAxisTitle)
        Me.grpTitleAxes.Controls.Add(Me.tbValueAxisTitle)
        Me.grpTitleAxes.Controls.Add(Me.lblChartTitle)
        Me.grpTitleAxes.Controls.Add(Me.tbChartTitle)
        Me.grpTitleAxes.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpTitleAxes.Location = New System.Drawing.Point(3, 84)
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
        'chkShowHorizontalAxisOnEveryPanel
        '
        Me.chkShowHorizontalAxisOnEveryPanel.AutoSize = True
        Me.chkShowHorizontalAxisOnEveryPanel.Checked = True
        Me.chkShowHorizontalAxisOnEveryPanel.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowHorizontalAxisOnEveryPanel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowHorizontalAxisOnEveryPanel.Location = New System.Drawing.Point(9, 81)
        Me.chkShowHorizontalAxisOnEveryPanel.Name = "chkShowHorizontalAxisOnEveryPanel"
        Me.chkShowHorizontalAxisOnEveryPanel.Size = New System.Drawing.Size(241, 20)
        Me.chkShowHorizontalAxisOnEveryPanel.TabIndex = 32
        Me.chkShowHorizontalAxisOnEveryPanel.Text = "Show horizontal axis on every panel"
        Me.chkShowHorizontalAxisOnEveryPanel.UseVisualStyleBackColor = True
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
        'TabPage4_SignalRules
        '
        Me.TabPage4_SignalRules.AutoScroll = True
        Me.TabPage4_SignalRules.Controls.Add(Me.grpSequenceOptions)
        Me.TabPage4_SignalRules.Controls.Add(Me.tbRuleDescription)
        Me.TabPage4_SignalRules.Controls.Add(Me.btResetCustomRules)
        Me.TabPage4_SignalRules.Controls.Add(Me.btRemoveRule)
        Me.TabPage4_SignalRules.Controls.Add(Me.btAddRule)
        Me.TabPage4_SignalRules.Controls.Add(Me.dgvRules)
        Me.TabPage4_SignalRules.Controls.Add(Me.grpRulePreset)
        Me.TabPage4_SignalRules.Location = New System.Drawing.Point(4, 25)
        Me.TabPage4_SignalRules.Name = "TabPage4_SignalRules"
        Me.TabPage4_SignalRules.Size = New System.Drawing.Size(871, 741)
        Me.TabPage4_SignalRules.TabIndex = 4
        Me.TabPage4_SignalRules.Text = "Signal Rules"
        Me.TabPage4_SignalRules.UseVisualStyleBackColor = True
        '
        'grpSequenceOptions
        '
        Me.grpSequenceOptions.Controls.Add(Me.cbSignalMarkingMode)
        Me.grpSequenceOptions.Controls.Add(Me.lblSignalMarkingMode)
        Me.grpSequenceOptions.Controls.Add(Me.cbSequenceGapBehavior)
        Me.grpSequenceOptions.Controls.Add(Me.lblSequenceGapBehavior)
        Me.grpSequenceOptions.Controls.Add(Me.lblRuleApplicability)
        Me.grpSequenceOptions.Controls.Add(Me.cbRulePhaseScope)
        Me.grpSequenceOptions.Controls.Add(Me.lblRulePhaseScope)
        Me.grpSequenceOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpSequenceOptions.Location = New System.Drawing.Point(7, 509)
        Me.grpSequenceOptions.Name = "grpSequenceOptions"
        Me.grpSequenceOptions.Size = New System.Drawing.Size(858, 116)
        Me.grpSequenceOptions.TabIndex = 27
        Me.grpSequenceOptions.TabStop = False
        Me.grpSequenceOptions.Text = "Sequence options"
        '
        'cbSignalMarkingMode
        '
        Me.cbSignalMarkingMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbSignalMarkingMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbSignalMarkingMode.FormattingEnabled = True
        Me.cbSignalMarkingMode.Location = New System.Drawing.Point(214, 81)
        Me.cbSignalMarkingMode.Name = "cbSignalMarkingMode"
        Me.cbSignalMarkingMode.Size = New System.Drawing.Size(272, 24)
        Me.cbSignalMarkingMode.TabIndex = 35
        '
        'lblSignalMarkingMode
        '
        Me.lblSignalMarkingMode.AutoSize = True
        Me.lblSignalMarkingMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSignalMarkingMode.Location = New System.Drawing.Point(8, 84)
        Me.lblSignalMarkingMode.Name = "lblSignalMarkingMode"
        Me.lblSignalMarkingMode.Size = New System.Drawing.Size(91, 16)
        Me.lblSignalMarkingMode.TabIndex = 34
        Me.lblSignalMarkingMode.Text = "Points to mark"
        '
        'cbSequenceGapBehavior
        '
        Me.cbSequenceGapBehavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbSequenceGapBehavior.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbSequenceGapBehavior.FormattingEnabled = True
        Me.cbSequenceGapBehavior.Location = New System.Drawing.Point(214, 51)
        Me.cbSequenceGapBehavior.Name = "cbSequenceGapBehavior"
        Me.cbSequenceGapBehavior.Size = New System.Drawing.Size(272, 24)
        Me.cbSequenceGapBehavior.TabIndex = 33
        '
        'lblSequenceGapBehavior
        '
        Me.lblSequenceGapBehavior.AutoSize = True
        Me.lblSequenceGapBehavior.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSequenceGapBehavior.Location = New System.Drawing.Point(8, 54)
        Me.lblSequenceGapBehavior.Name = "lblSequenceGapBehavior"
        Me.lblSequenceGapBehavior.Size = New System.Drawing.Size(200, 16)
        Me.lblSequenceGapBehavior.TabIndex = 32
        Me.lblSequenceGapBehavior.Text = "Missing/excluded point behavior"
        '
        'lblRuleApplicability
        '
        Me.lblRuleApplicability.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRuleApplicability.Location = New System.Drawing.Point(490, 24)
        Me.lblRuleApplicability.Name = "lblRuleApplicability"
        Me.lblRuleApplicability.Size = New System.Drawing.Size(362, 81)
        Me.lblRuleApplicability.TabIndex = 31
        Me.lblRuleApplicability.Text = "Explain that dispersion and time-weighted charts use restricted rules"
        '
        'cbRulePhaseScope
        '
        Me.cbRulePhaseScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbRulePhaseScope.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbRulePhaseScope.FormattingEnabled = True
        Me.cbRulePhaseScope.Location = New System.Drawing.Point(214, 24)
        Me.cbRulePhaseScope.Name = "cbRulePhaseScope"
        Me.cbRulePhaseScope.Size = New System.Drawing.Size(272, 24)
        Me.cbRulePhaseScope.TabIndex = 19
        '
        'lblRulePhaseScope
        '
        Me.lblRulePhaseScope.AutoSize = True
        Me.lblRulePhaseScope.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRulePhaseScope.Location = New System.Drawing.Point(8, 24)
        Me.lblRulePhaseScope.Name = "lblRulePhaseScope"
        Me.lblRulePhaseScope.Size = New System.Drawing.Size(121, 16)
        Me.lblRulePhaseScope.TabIndex = 18
        Me.lblRulePhaseScope.Text = "Evaluate in phases"
        '
        'tbRuleDescription
        '
        Me.tbRuleDescription.Location = New System.Drawing.Point(169, 110)
        Me.tbRuleDescription.Multiline = True
        Me.tbRuleDescription.Name = "tbRuleDescription"
        Me.tbRuleDescription.Size = New System.Drawing.Size(696, 62)
        Me.tbRuleDescription.TabIndex = 26
        '
        'btResetCustomRules
        '
        Me.btResetCustomRules.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btResetCustomRules.Location = New System.Drawing.Point(88, 109)
        Me.btResetCustomRules.Name = "btResetCustomRules"
        Me.btResetCustomRules.Size = New System.Drawing.Size(75, 23)
        Me.btResetCustomRules.TabIndex = 25
        Me.btResetCustomRules.Text = "Reset"
        Me.btResetCustomRules.UseVisualStyleBackColor = True
        '
        'btRemoveRule
        '
        Me.btRemoveRule.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveRule.Location = New System.Drawing.Point(7, 138)
        Me.btRemoveRule.Name = "btRemoveRule"
        Me.btRemoveRule.Size = New System.Drawing.Size(75, 23)
        Me.btRemoveRule.TabIndex = 24
        Me.btRemoveRule.Text = "Remove"
        Me.btRemoveRule.UseVisualStyleBackColor = True
        '
        'btAddRule
        '
        Me.btAddRule.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddRule.Location = New System.Drawing.Point(7, 109)
        Me.btAddRule.Name = "btAddRule"
        Me.btAddRule.Size = New System.Drawing.Size(75, 23)
        Me.btAddRule.TabIndex = 23
        Me.btAddRule.Text = "Add"
        Me.btAddRule.UseVisualStyleBackColor = True
        '
        'dgvRules
        '
        Me.dgvRules.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvRules.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colRuleEnabled, Me.colRuleNumber, Me.colRuleCode, Me.colRuleName, Me.colRuleKind, Me.colRuleWindow, Me.colRuleMinimumPoints, Me.colRuleSigma, Me.colRuleSide, Me.colRuleScope})
        Me.dgvRules.Location = New System.Drawing.Point(7, 178)
        Me.dgvRules.Name = "dgvRules"
        Me.dgvRules.RowHeadersWidth = 51
        Me.dgvRules.RowTemplate.Height = 24
        Me.dgvRules.Size = New System.Drawing.Size(858, 325)
        Me.dgvRules.TabIndex = 1
        '
        'colRuleEnabled
        '
        Me.colRuleEnabled.HeaderText = "Use"
        Me.colRuleEnabled.MinimumWidth = 6
        Me.colRuleEnabled.Name = "colRuleEnabled"
        Me.colRuleEnabled.Width = 125
        '
        'colRuleNumber
        '
        Me.colRuleNumber.HeaderText = "No."
        Me.colRuleNumber.MinimumWidth = 6
        Me.colRuleNumber.Name = "colRuleNumber"
        Me.colRuleNumber.Width = 125
        '
        'colRuleCode
        '
        Me.colRuleCode.HeaderText = "Code"
        Me.colRuleCode.MinimumWidth = 6
        Me.colRuleCode.Name = "colRuleCode"
        Me.colRuleCode.Width = 125
        '
        'colRuleName
        '
        Me.colRuleName.HeaderText = "Name"
        Me.colRuleName.MinimumWidth = 6
        Me.colRuleName.Name = "colRuleName"
        Me.colRuleName.ReadOnly = True
        Me.colRuleName.Width = 125
        '
        'colRuleKind
        '
        Me.colRuleKind.HeaderText = "Pattern"
        Me.colRuleKind.MinimumWidth = 6
        Me.colRuleKind.Name = "colRuleKind"
        Me.colRuleKind.Width = 125
        '
        'colRuleWindow
        '
        Me.colRuleWindow.HeaderText = "Window"
        Me.colRuleWindow.MinimumWidth = 6
        Me.colRuleWindow.Name = "colRuleWindow"
        Me.colRuleWindow.Width = 125
        '
        'colRuleMinimumPoints
        '
        Me.colRuleMinimumPoints.HeaderText = "Required"
        Me.colRuleMinimumPoints.MinimumWidth = 6
        Me.colRuleMinimumPoints.Name = "colRuleMinimumPoints"
        Me.colRuleMinimumPoints.Width = 125
        '
        'colRuleSigma
        '
        Me.colRuleSigma.HeaderText = "Sigma"
        Me.colRuleSigma.MinimumWidth = 6
        Me.colRuleSigma.Name = "colRuleSigma"
        Me.colRuleSigma.Width = 125
        '
        'colRuleSide
        '
        Me.colRuleSide.HeaderText = "Side"
        Me.colRuleSide.MinimumWidth = 6
        Me.colRuleSide.Name = "colRuleSide"
        Me.colRuleSide.Width = 125
        '
        'colRuleScope
        '
        Me.colRuleScope.HeaderText = "Applies to"
        Me.colRuleScope.MinimumWidth = 6
        Me.colRuleScope.Name = "colRuleScope"
        Me.colRuleScope.Width = 125
        '
        'grpRulePreset
        '
        Me.grpRulePreset.Controls.Add(Me.lblRulePresetDescription)
        Me.grpRulePreset.Controls.Add(Me.btCopyPresetToCustom)
        Me.grpRulePreset.Controls.Add(Me.btLoadRulePreset)
        Me.grpRulePreset.Controls.Add(Me.cbRulePreset)
        Me.grpRulePreset.Controls.Add(Me.lblRulePreset)
        Me.grpRulePreset.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpRulePreset.Location = New System.Drawing.Point(7, 3)
        Me.grpRulePreset.Name = "grpRulePreset"
        Me.grpRulePreset.Size = New System.Drawing.Size(858, 100)
        Me.grpRulePreset.TabIndex = 0
        Me.grpRulePreset.TabStop = False
        Me.grpRulePreset.Text = "Rule preset"
        '
        'lblRulePresetDescription
        '
        Me.lblRulePresetDescription.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRulePresetDescription.Location = New System.Drawing.Point(8, 57)
        Me.lblRulePresetDescription.Name = "lblRulePresetDescription"
        Me.lblRulePresetDescription.Size = New System.Drawing.Size(844, 40)
        Me.lblRulePresetDescription.TabIndex = 31
        Me.lblRulePresetDescription.Text = "preset description"
        '
        'btCopyPresetToCustom
        '
        Me.btCopyPresetToCustom.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btCopyPresetToCustom.Location = New System.Drawing.Point(594, 21)
        Me.btCopyPresetToCustom.Name = "btCopyPresetToCustom"
        Me.btCopyPresetToCustom.Size = New System.Drawing.Size(120, 23)
        Me.btCopyPresetToCustom.TabIndex = 30
        Me.btCopyPresetToCustom.Text = "Copy to custom"
        Me.btCopyPresetToCustom.UseVisualStyleBackColor = True
        '
        'btLoadRulePreset
        '
        Me.btLoadRulePreset.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btLoadRulePreset.Location = New System.Drawing.Point(469, 21)
        Me.btLoadRulePreset.Name = "btLoadRulePreset"
        Me.btLoadRulePreset.Size = New System.Drawing.Size(109, 23)
        Me.btLoadRulePreset.TabIndex = 29
        Me.btLoadRulePreset.Text = "Reload preset"
        Me.btLoadRulePreset.UseVisualStyleBackColor = True
        '
        'cbRulePreset
        '
        Me.cbRulePreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbRulePreset.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbRulePreset.FormattingEnabled = True
        Me.cbRulePreset.Location = New System.Drawing.Point(121, 21)
        Me.cbRulePreset.Name = "cbRulePreset"
        Me.cbRulePreset.Size = New System.Drawing.Size(272, 24)
        Me.cbRulePreset.TabIndex = 19
        '
        'lblRulePreset
        '
        Me.lblRulePreset.AutoSize = True
        Me.lblRulePreset.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRulePreset.Location = New System.Drawing.Point(8, 24)
        Me.lblRulePreset.Name = "lblRulePreset"
        Me.lblRulePreset.Size = New System.Drawing.Size(76, 16)
        Me.lblRulePreset.TabIndex = 18
        Me.lblRulePreset.Text = "Rule preset"
        '
        'TabPage3_PhasesExclusions
        '
        Me.TabPage3_PhasesExclusions.AutoScroll = True
        Me.TabPage3_PhasesExclusions.Controls.Add(Me.grpExclusions)
        Me.TabPage3_PhasesExclusions.Controls.Add(Me.dgvStages)
        Me.TabPage3_PhasesExclusions.Controls.Add(Me.grpQuickPhaseSetup)
        Me.TabPage3_PhasesExclusions.Controls.Add(Me.grpPhaseColumns)
        Me.TabPage3_PhasesExclusions.Location = New System.Drawing.Point(4, 25)
        Me.TabPage3_PhasesExclusions.Name = "TabPage3_PhasesExclusions"
        Me.TabPage3_PhasesExclusions.Size = New System.Drawing.Size(871, 741)
        Me.TabPage3_PhasesExclusions.TabIndex = 3
        Me.TabPage3_PhasesExclusions.Text = "Phases and Exclusions"
        Me.TabPage3_PhasesExclusions.UseVisualStyleBackColor = True
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
        'dgvStages
        '
        Me.dgvStages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvStages.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colStageID, Me.colStageDisplayName, Me.colStageFirstPoint, Me.colStageLastPoint, Me.colStagePhase, Me.colStageLimitMode, Me.colStageReferenceID})
        Me.dgvStages.Location = New System.Drawing.Point(7, 268)
        Me.dgvStages.Name = "dgvStages"
        Me.dgvStages.RowHeadersWidth = 51
        Me.dgvStages.RowTemplate.Height = 24
        Me.dgvStages.Size = New System.Drawing.Size(858, 150)
        Me.dgvStages.TabIndex = 5
        '
        'colStageID
        '
        Me.colStageID.HeaderText = "Stage ID"
        Me.colStageID.MinimumWidth = 6
        Me.colStageID.Name = "colStageID"
        Me.colStageID.Width = 125
        '
        'colStageDisplayName
        '
        Me.colStageDisplayName.HeaderText = "Display name"
        Me.colStageDisplayName.MinimumWidth = 6
        Me.colStageDisplayName.Name = "colStageDisplayName"
        Me.colStageDisplayName.Width = 125
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
        'colStageLimitMode
        '
        Me.colStageLimitMode.HeaderText = "Limits"
        Me.colStageLimitMode.MinimumWidth = 6
        Me.colStageLimitMode.Name = "colStageLimitMode"
        Me.colStageLimitMode.Width = 125
        '
        'colStageReferenceID
        '
        Me.colStageReferenceID.HeaderText = "Reference stage"
        Me.colStageReferenceID.MinimumWidth = 6
        Me.colStageReferenceID.Name = "colStageReferenceID"
        Me.colStageReferenceID.Width = 125
        '
        'grpQuickPhaseSetup
        '
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
        'btApplyQuickPhaseSetup
        '
        Me.btApplyQuickPhaseSetup.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btApplyQuickPhaseSetup.Location = New System.Drawing.Point(337, 54)
        Me.btApplyQuickPhaseSetup.Name = "btApplyQuickPhaseSetup"
        Me.btApplyQuickPhaseSetup.Size = New System.Drawing.Size(109, 23)
        Me.btApplyQuickPhaseSetup.TabIndex = 28
        Me.btApplyQuickPhaseSetup.Text = "Apply"
        Me.btApplyQuickPhaseSetup.UseVisualStyleBackColor = True
        '
        'spinLastPhaseIPoint
        '
        Me.spinLastPhaseIPoint.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinLastPhaseIPoint.Location = New System.Drawing.Point(382, 21)
        Me.spinLastPhaseIPoint.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
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
        Me.lblLastPhaseIPoint.Location = New System.Drawing.Point(228, 24)
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
        'TabPage2_ParametersLimits
        '
        Me.TabPage2_ParametersLimits.AutoScroll = True
        Me.TabPage2_ParametersLimits.Controls.Add(Me.grpTimeWeightedParameters)
        Me.TabPage2_ParametersLimits.Controls.Add(Me.grpHistoricalParameters)
        Me.TabPage2_ParametersLimits.Controls.Add(Me.grpGeneralOptions)
        Me.TabPage2_ParametersLimits.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2_ParametersLimits.Name = "TabPage2_ParametersLimits"
        Me.TabPage2_ParametersLimits.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2_ParametersLimits.Size = New System.Drawing.Size(871, 741)
        Me.TabPage2_ParametersLimits.TabIndex = 1
        Me.TabPage2_ParametersLimits.Text = "Parameters and Limits"
        Me.TabPage2_ParametersLimits.UseVisualStyleBackColor = True
        '
        'grpTimeWeightedParameters
        '
        Me.grpTimeWeightedParameters.Controls.Add(Me.chkUseSteadyStateLimits)
        Me.grpTimeWeightedParameters.Controls.Add(Me.lblTimeWeightedNote)
        Me.grpTimeWeightedParameters.Controls.Add(Me.spinCusumDecisionInterval)
        Me.grpTimeWeightedParameters.Controls.Add(Me.lblCusumDecisionInterval)
        Me.grpTimeWeightedParameters.Controls.Add(Me.spinHeadStart)
        Me.grpTimeWeightedParameters.Controls.Add(Me.lblHeadStart)
        Me.grpTimeWeightedParameters.Controls.Add(Me.spinMovingAverageSpan)
        Me.grpTimeWeightedParameters.Controls.Add(Me.lblMovingAverageSpan)
        Me.grpTimeWeightedParameters.Controls.Add(Me.ComboBox1)
        Me.grpTimeWeightedParameters.Controls.Add(Me.Label1)
        Me.grpTimeWeightedParameters.Controls.Add(Me.spinCusumReferenceValue)
        Me.grpTimeWeightedParameters.Controls.Add(Me.lblCusumReferenceValue)
        Me.grpTimeWeightedParameters.Controls.Add(Me.spinEwmaLambda)
        Me.grpTimeWeightedParameters.Controls.Add(Me.lblEwmaLambda)
        Me.grpTimeWeightedParameters.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpTimeWeightedParameters.Location = New System.Drawing.Point(7, 526)
        Me.grpTimeWeightedParameters.Name = "grpTimeWeightedParameters"
        Me.grpTimeWeightedParameters.Size = New System.Drawing.Size(858, 201)
        Me.grpTimeWeightedParameters.TabIndex = 30
        Me.grpTimeWeightedParameters.TabStop = False
        Me.grpTimeWeightedParameters.Text = "Time-weighted parameters"
        Me.grpTimeWeightedParameters.Visible = False
        '
        'chkUseSteadyStateLimits
        '
        Me.chkUseSteadyStateLimits.AutoSize = True
        Me.chkUseSteadyStateLimits.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkUseSteadyStateLimits.Location = New System.Drawing.Point(277, 135)
        Me.chkUseSteadyStateLimits.Name = "chkUseSteadyStateLimits"
        Me.chkUseSteadyStateLimits.Size = New System.Drawing.Size(164, 20)
        Me.chkUseSteadyStateLimits.TabIndex = 36
        Me.chkUseSteadyStateLimits.Text = "Use steady-state limits"
        Me.chkUseSteadyStateLimits.UseVisualStyleBackColor = True
        '
        'lblTimeWeightedNote
        '
        Me.lblTimeWeightedNote.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTimeWeightedNote.Location = New System.Drawing.Point(277, 25)
        Me.lblTimeWeightedNote.Name = "lblTimeWeightedNote"
        Me.lblTimeWeightedNote.Size = New System.Drawing.Size(324, 102)
        Me.lblTimeWeightedNote.TabIndex = 35
        Me.lblTimeWeightedNote.Text = "CUSUM k, h and head start are expressed in process-sigma units."
        '
        'spinCusumDecisionInterval
        '
        Me.spinCusumDecisionInterval.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinCusumDecisionInterval.Location = New System.Drawing.Point(207, 81)
        Me.spinCusumDecisionInterval.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
        Me.spinCusumDecisionInterval.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinCusumDecisionInterval.Name = "spinCusumDecisionInterval"
        Me.spinCusumDecisionInterval.Size = New System.Drawing.Size(64, 22)
        Me.spinCusumDecisionInterval.TabIndex = 34
        Me.spinCusumDecisionInterval.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'lblCusumDecisionInterval
        '
        Me.lblCusumDecisionInterval.AutoSize = True
        Me.lblCusumDecisionInterval.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCusumDecisionInterval.Location = New System.Drawing.Point(6, 83)
        Me.lblCusumDecisionInterval.Name = "lblCusumDecisionInterval"
        Me.lblCusumDecisionInterval.Size = New System.Drawing.Size(166, 16)
        Me.lblCusumDecisionInterval.TabIndex = 33
        Me.lblCusumDecisionInterval.Text = "CUSUM decision interval h"
        '
        'spinHeadStart
        '
        Me.spinHeadStart.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinHeadStart.Location = New System.Drawing.Point(207, 109)
        Me.spinHeadStart.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
        Me.spinHeadStart.Name = "spinHeadStart"
        Me.spinHeadStart.Size = New System.Drawing.Size(64, 22)
        Me.spinHeadStart.TabIndex = 32
        '
        'lblHeadStart
        '
        Me.lblHeadStart.AutoSize = True
        Me.lblHeadStart.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHeadStart.Location = New System.Drawing.Point(6, 111)
        Me.lblHeadStart.Name = "lblHeadStart"
        Me.lblHeadStart.Size = New System.Drawing.Size(69, 16)
        Me.lblHeadStart.TabIndex = 31
        Me.lblHeadStart.Text = "Head start"
        '
        'spinMovingAverageSpan
        '
        Me.spinMovingAverageSpan.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinMovingAverageSpan.Location = New System.Drawing.Point(207, 137)
        Me.spinMovingAverageSpan.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
        Me.spinMovingAverageSpan.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinMovingAverageSpan.Name = "spinMovingAverageSpan"
        Me.spinMovingAverageSpan.Size = New System.Drawing.Size(64, 22)
        Me.spinMovingAverageSpan.TabIndex = 30
        Me.spinMovingAverageSpan.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        'lblMovingAverageSpan
        '
        Me.lblMovingAverageSpan.AutoSize = True
        Me.lblMovingAverageSpan.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMovingAverageSpan.Location = New System.Drawing.Point(6, 139)
        Me.lblMovingAverageSpan.Name = "lblMovingAverageSpan"
        Me.lblMovingAverageSpan.Size = New System.Drawing.Size(139, 16)
        Me.lblMovingAverageSpan.TabIndex = 29
        Me.lblMovingAverageSpan.Text = "Moving-average span"
        '
        'ComboBox1
        '
        Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Location = New System.Drawing.Point(207, 165)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(239, 24)
        Me.ComboBox1.TabIndex = 28
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(6, 168)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(131, 16)
        Me.Label1.TabIndex = 27
        Me.Label1.Text = "Natural-limit handling"
        '
        'spinCusumReferenceValue
        '
        Me.spinCusumReferenceValue.DecimalPlaces = 1
        Me.spinCusumReferenceValue.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinCusumReferenceValue.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.spinCusumReferenceValue.Location = New System.Drawing.Point(207, 53)
        Me.spinCusumReferenceValue.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
        Me.spinCusumReferenceValue.Name = "spinCusumReferenceValue"
        Me.spinCusumReferenceValue.Size = New System.Drawing.Size(64, 22)
        Me.spinCusumReferenceValue.TabIndex = 25
        Me.spinCusumReferenceValue.Value = New Decimal(New Integer() {5, 0, 0, 65536})
        '
        'lblCusumReferenceValue
        '
        Me.lblCusumReferenceValue.AutoSize = True
        Me.lblCusumReferenceValue.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCusumReferenceValue.Location = New System.Drawing.Point(6, 55)
        Me.lblCusumReferenceValue.Name = "lblCusumReferenceValue"
        Me.lblCusumReferenceValue.Size = New System.Drawing.Size(162, 16)
        Me.lblCusumReferenceValue.TabIndex = 24
        Me.lblCusumReferenceValue.Text = "CUSUM reference value k"
        '
        'spinEwmaLambda
        '
        Me.spinEwmaLambda.DecimalPlaces = 2
        Me.spinEwmaLambda.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinEwmaLambda.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.spinEwmaLambda.Location = New System.Drawing.Point(207, 25)
        Me.spinEwmaLambda.Name = "spinEwmaLambda"
        Me.spinEwmaLambda.Size = New System.Drawing.Size(64, 22)
        Me.spinEwmaLambda.TabIndex = 19
        Me.spinEwmaLambda.Value = New Decimal(New Integer() {2, 0, 0, 65536})
        '
        'lblEwmaLambda
        '
        Me.lblEwmaLambda.AutoSize = True
        Me.lblEwmaLambda.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEwmaLambda.Location = New System.Drawing.Point(6, 27)
        Me.lblEwmaLambda.Name = "lblEwmaLambda"
        Me.lblEwmaLambda.Size = New System.Drawing.Size(98, 16)
        Me.lblEwmaLambda.TabIndex = 18
        Me.lblEwmaLambda.Text = "EWMA lambda"
        '
        'grpHistoricalParameters
        '
        Me.grpHistoricalParameters.Controls.Add(Me.btClearHistoricalParameters)
        Me.grpHistoricalParameters.Controls.Add(Me.btRemoveHistoricalParameter)
        Me.grpHistoricalParameters.Controls.Add(Me.btAddHistoricalParameter)
        Me.grpHistoricalParameters.Controls.Add(Me.dgvHistoricalParameters)
        Me.grpHistoricalParameters.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHistoricalParameters.Location = New System.Drawing.Point(7, 269)
        Me.grpHistoricalParameters.Name = "grpHistoricalParameters"
        Me.grpHistoricalParameters.Size = New System.Drawing.Size(858, 251)
        Me.grpHistoricalParameters.TabIndex = 29
        Me.grpHistoricalParameters.TabStop = False
        Me.grpHistoricalParameters.Text = "Historical parameters"
        Me.grpHistoricalParameters.Visible = False
        '
        'btClearHistoricalParameters
        '
        Me.btClearHistoricalParameters.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btClearHistoricalParameters.Location = New System.Drawing.Point(171, 21)
        Me.btClearHistoricalParameters.Name = "btClearHistoricalParameters"
        Me.btClearHistoricalParameters.Size = New System.Drawing.Size(75, 23)
        Me.btClearHistoricalParameters.TabIndex = 19
        Me.btClearHistoricalParameters.Text = "Clear"
        Me.btClearHistoricalParameters.UseVisualStyleBackColor = True
        '
        'btRemoveHistoricalParameter
        '
        Me.btRemoveHistoricalParameter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveHistoricalParameter.Location = New System.Drawing.Point(90, 21)
        Me.btRemoveHistoricalParameter.Name = "btRemoveHistoricalParameter"
        Me.btRemoveHistoricalParameter.Size = New System.Drawing.Size(75, 23)
        Me.btRemoveHistoricalParameter.TabIndex = 18
        Me.btRemoveHistoricalParameter.Text = "Remove"
        Me.btRemoveHistoricalParameter.UseVisualStyleBackColor = True
        '
        'btAddHistoricalParameter
        '
        Me.btAddHistoricalParameter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddHistoricalParameter.Location = New System.Drawing.Point(9, 21)
        Me.btAddHistoricalParameter.Name = "btAddHistoricalParameter"
        Me.btAddHistoricalParameter.Size = New System.Drawing.Size(75, 23)
        Me.btAddHistoricalParameter.TabIndex = 17
        Me.btAddHistoricalParameter.Text = "Add"
        Me.btAddHistoricalParameter.UseVisualStyleBackColor = True
        '
        'dgvHistoricalParameters
        '
        Me.dgvHistoricalParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvHistoricalParameters.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colHistoryStageID, Me.colHistoryMean, Me.colHistorySigma, Me.colHistoryProportion, Me.colHistoryMeanCount, Me.colHistoryMeanRate})
        Me.dgvHistoricalParameters.Location = New System.Drawing.Point(6, 50)
        Me.dgvHistoricalParameters.Name = "dgvHistoricalParameters"
        Me.dgvHistoricalParameters.RowHeadersWidth = 51
        Me.dgvHistoricalParameters.RowTemplate.Height = 24
        Me.dgvHistoricalParameters.Size = New System.Drawing.Size(846, 195)
        Me.dgvHistoricalParameters.TabIndex = 16
        '
        'colHistoryStageID
        '
        Me.colHistoryStageID.HeaderText = "Stage ID (blank = default)"
        Me.colHistoryStageID.MinimumWidth = 6
        Me.colHistoryStageID.Name = "colHistoryStageID"
        Me.colHistoryStageID.Width = 125
        '
        'colHistoryMean
        '
        Me.colHistoryMean.HeaderText = "Process mean"
        Me.colHistoryMean.MinimumWidth = 6
        Me.colHistoryMean.Name = "colHistoryMean"
        Me.colHistoryMean.Width = 125
        '
        'colHistorySigma
        '
        Me.colHistorySigma.HeaderText = "Process SD"
        Me.colHistorySigma.MinimumWidth = 6
        Me.colHistorySigma.Name = "colHistorySigma"
        Me.colHistorySigma.Width = 125
        '
        'colHistoryProportion
        '
        Me.colHistoryProportion.HeaderText = "Proportion"
        Me.colHistoryProportion.MinimumWidth = 6
        Me.colHistoryProportion.Name = "colHistoryProportion"
        Me.colHistoryProportion.Width = 125
        '
        'colHistoryMeanCount
        '
        Me.colHistoryMeanCount.HeaderText = "Mean count"
        Me.colHistoryMeanCount.MinimumWidth = 6
        Me.colHistoryMeanCount.Name = "colHistoryMeanCount"
        Me.colHistoryMeanCount.Width = 125
        '
        'colHistoryMeanRate
        '
        Me.colHistoryMeanRate.HeaderText = "Mean rate"
        Me.colHistoryMeanRate.MinimumWidth = 6
        Me.colHistoryMeanRate.Name = "colHistoryMeanRate"
        Me.colHistoryMeanRate.Width = 125
        '
        'grpGeneralOptions
        '
        Me.grpGeneralOptions.Controls.Add(Me.cbNaturalLimitPolicy)
        Me.grpGeneralOptions.Controls.Add(Me.lblNaturalLimitPolicy)
        Me.grpGeneralOptions.Controls.Add(Me.chkUseBiasCorrection)
        Me.grpGeneralOptions.Controls.Add(Me.spinMovingRangeLength)
        Me.grpGeneralOptions.Controls.Add(Me.lblMovingRangeLength)
        Me.grpGeneralOptions.Controls.Add(Me.cbWithinSigmaEstimator)
        Me.grpGeneralOptions.Controls.Add(Me.lblWithinSigmaEstimator)
        Me.grpGeneralOptions.Controls.Add(Me.cbControlLimitMethod)
        Me.grpGeneralOptions.Controls.Add(Me.lblControlLimitMethod)
        Me.grpGeneralOptions.Controls.Add(Me.spinSigmaMultiplier)
        Me.grpGeneralOptions.Controls.Add(Me.lblSigmaMultiplier)
        Me.grpGeneralOptions.Controls.Add(Me.cbParameterSource)
        Me.grpGeneralOptions.Controls.Add(Me.lblParameterSource)
        Me.grpGeneralOptions.Controls.Add(Me.cbMissingValuePolicy)
        Me.grpGeneralOptions.Controls.Add(Me.lblMissingValuePolicy)
        Me.grpGeneralOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpGeneralOptions.Location = New System.Drawing.Point(7, 6)
        Me.grpGeneralOptions.Name = "grpGeneralOptions"
        Me.grpGeneralOptions.Size = New System.Drawing.Size(858, 257)
        Me.grpGeneralOptions.TabIndex = 5
        Me.grpGeneralOptions.TabStop = False
        Me.grpGeneralOptions.Text = "General options"
        '
        'cbNaturalLimitPolicy
        '
        Me.cbNaturalLimitPolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbNaturalLimitPolicy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbNaturalLimitPolicy.FormattingEnabled = True
        Me.cbNaturalLimitPolicy.Location = New System.Drawing.Point(207, 226)
        Me.cbNaturalLimitPolicy.Name = "cbNaturalLimitPolicy"
        Me.cbNaturalLimitPolicy.Size = New System.Drawing.Size(239, 24)
        Me.cbNaturalLimitPolicy.TabIndex = 28
        '
        'lblNaturalLimitPolicy
        '
        Me.lblNaturalLimitPolicy.AutoSize = True
        Me.lblNaturalLimitPolicy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNaturalLimitPolicy.Location = New System.Drawing.Point(6, 229)
        Me.lblNaturalLimitPolicy.Name = "lblNaturalLimitPolicy"
        Me.lblNaturalLimitPolicy.Size = New System.Drawing.Size(131, 16)
        Me.lblNaturalLimitPolicy.TabIndex = 27
        Me.lblNaturalLimitPolicy.Text = "Natural-limit handling"
        '
        'chkUseBiasCorrection
        '
        Me.chkUseBiasCorrection.AutoSize = True
        Me.chkUseBiasCorrection.Checked = True
        Me.chkUseBiasCorrection.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUseBiasCorrection.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkUseBiasCorrection.Location = New System.Drawing.Point(207, 200)
        Me.chkUseBiasCorrection.Name = "chkUseBiasCorrection"
        Me.chkUseBiasCorrection.Size = New System.Drawing.Size(145, 20)
        Me.chkUseBiasCorrection.TabIndex = 26
        Me.chkUseBiasCorrection.Text = "Use bias correction"
        Me.chkUseBiasCorrection.UseVisualStyleBackColor = True
        '
        'spinMovingRangeLength
        '
        Me.spinMovingRangeLength.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinMovingRangeLength.Location = New System.Drawing.Point(207, 172)
        Me.spinMovingRangeLength.Maximum = New Decimal(New Integer() {25, 0, 0, 0})
        Me.spinMovingRangeLength.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinMovingRangeLength.Name = "spinMovingRangeLength"
        Me.spinMovingRangeLength.Size = New System.Drawing.Size(64, 22)
        Me.spinMovingRangeLength.TabIndex = 25
        Me.spinMovingRangeLength.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'lblMovingRangeLength
        '
        Me.lblMovingRangeLength.AutoSize = True
        Me.lblMovingRangeLength.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMovingRangeLength.Location = New System.Drawing.Point(6, 174)
        Me.lblMovingRangeLength.Name = "lblMovingRangeLength"
        Me.lblMovingRangeLength.Size = New System.Drawing.Size(129, 16)
        Me.lblMovingRangeLength.TabIndex = 24
        Me.lblMovingRangeLength.Text = "Moving-range length"
        '
        'cbWithinSigmaEstimator
        '
        Me.cbWithinSigmaEstimator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbWithinSigmaEstimator.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbWithinSigmaEstimator.FormattingEnabled = True
        Me.cbWithinSigmaEstimator.Location = New System.Drawing.Point(207, 142)
        Me.cbWithinSigmaEstimator.Name = "cbWithinSigmaEstimator"
        Me.cbWithinSigmaEstimator.Size = New System.Drawing.Size(239, 24)
        Me.cbWithinSigmaEstimator.TabIndex = 23
        '
        'lblWithinSigmaEstimator
        '
        Me.lblWithinSigmaEstimator.AutoSize = True
        Me.lblWithinSigmaEstimator.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblWithinSigmaEstimator.Location = New System.Drawing.Point(6, 145)
        Me.lblWithinSigmaEstimator.Name = "lblWithinSigmaEstimator"
        Me.lblWithinSigmaEstimator.Size = New System.Drawing.Size(194, 16)
        Me.lblWithinSigmaEstimator.TabIndex = 22
        Me.lblWithinSigmaEstimator.Text = "Within-process sigma estimator"
        '
        'cbControlLimitMethod
        '
        Me.cbControlLimitMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbControlLimitMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbControlLimitMethod.FormattingEnabled = True
        Me.cbControlLimitMethod.Location = New System.Drawing.Point(207, 84)
        Me.cbControlLimitMethod.Name = "cbControlLimitMethod"
        Me.cbControlLimitMethod.Size = New System.Drawing.Size(239, 24)
        Me.cbControlLimitMethod.TabIndex = 21
        '
        'lblControlLimitMethod
        '
        Me.lblControlLimitMethod.AutoSize = True
        Me.lblControlLimitMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblControlLimitMethod.Location = New System.Drawing.Point(6, 87)
        Me.lblControlLimitMethod.Name = "lblControlLimitMethod"
        Me.lblControlLimitMethod.Size = New System.Drawing.Size(124, 16)
        Me.lblControlLimitMethod.TabIndex = 20
        Me.lblControlLimitMethod.Text = "Control-limit method"
        '
        'spinSigmaMultiplier
        '
        Me.spinSigmaMultiplier.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinSigmaMultiplier.Location = New System.Drawing.Point(207, 114)
        Me.spinSigmaMultiplier.Name = "spinSigmaMultiplier"
        Me.spinSigmaMultiplier.Size = New System.Drawing.Size(64, 22)
        Me.spinSigmaMultiplier.TabIndex = 19
        Me.spinSigmaMultiplier.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        'lblSigmaMultiplier
        '
        Me.lblSigmaMultiplier.AutoSize = True
        Me.lblSigmaMultiplier.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSigmaMultiplier.Location = New System.Drawing.Point(6, 116)
        Me.lblSigmaMultiplier.Name = "lblSigmaMultiplier"
        Me.lblSigmaMultiplier.Size = New System.Drawing.Size(102, 16)
        Me.lblSigmaMultiplier.TabIndex = 18
        Me.lblSigmaMultiplier.Text = "Sigma multiplier"
        '
        'cbParameterSource
        '
        Me.cbParameterSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbParameterSource.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbParameterSource.FormattingEnabled = True
        Me.cbParameterSource.Location = New System.Drawing.Point(207, 54)
        Me.cbParameterSource.Name = "cbParameterSource"
        Me.cbParameterSource.Size = New System.Drawing.Size(239, 24)
        Me.cbParameterSource.TabIndex = 17
        '
        'lblParameterSource
        '
        Me.lblParameterSource.AutoSize = True
        Me.lblParameterSource.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblParameterSource.Location = New System.Drawing.Point(6, 57)
        Me.lblParameterSource.Name = "lblParameterSource"
        Me.lblParameterSource.Size = New System.Drawing.Size(114, 16)
        Me.lblParameterSource.TabIndex = 16
        Me.lblParameterSource.Text = "Parameter source"
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
        Me.grpWorksheet.Controls.Add(Me.lblDataRequirements)
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
        Me.grpWorksheet.Controls.Add(Me.lblValues)
        Me.grpWorksheet.Controls.Add(Me.lbValues)
        Me.grpWorksheet.Controls.Add(Me.btRemoveValues)
        Me.grpWorksheet.Controls.Add(Me.btAddValues)
        Me.grpWorksheet.Controls.Add(Me.lbAllColumns)
        Me.grpWorksheet.Controls.Add(Me.btReload)
        Me.grpWorksheet.Controls.Add(Me.lblAllColumns)
        Me.grpWorksheet.Controls.Add(Me.cbSheetsList)
        Me.grpWorksheet.Controls.Add(Me.lblSheetsList)
        Me.grpWorksheet.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpWorksheet.Location = New System.Drawing.Point(6, 133)
        Me.grpWorksheet.Name = "grpWorksheet"
        Me.grpWorksheet.Size = New System.Drawing.Size(859, 602)
        Me.grpWorksheet.TabIndex = 2
        Me.grpWorksheet.TabStop = False
        Me.grpWorksheet.Text = "Worksheet variables"
        '
        'lblDataRequirements
        '
        Me.lblDataRequirements.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDataRequirements.Location = New System.Drawing.Point(157, 515)
        Me.lblDataRequirements.Name = "lblDataRequirements"
        Me.lblDataRequirements.Size = New System.Drawing.Size(676, 54)
        Me.lblDataRequirements.TabIndex = 51
        Me.lblDataRequirements.Text = "Multiline chart-specific requirements"
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
        'lblValues
        '
        Me.lblValues.AutoSize = True
        Me.lblValues.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblValues.Location = New System.Drawing.Point(496, 80)
        Me.lblValues.Name = "lblValues"
        Me.lblValues.Size = New System.Drawing.Size(140, 16)
        Me.lblValues.TabIndex = 21
        Me.lblValues.Text = "Measurements/values"
        '
        'lbValues
        '
        Me.lbValues.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbValues.FormattingEnabled = True
        Me.lbValues.ItemHeight = 16
        Me.lbValues.Location = New System.Drawing.Point(496, 100)
        Me.lbValues.Name = "lbValues"
        Me.lbValues.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbValues.Size = New System.Drawing.Size(357, 116)
        Me.lbValues.TabIndex = 20
        '
        'btRemoveValues
        '
        Me.btRemoveValues.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btRemoveValues.Location = New System.Drawing.Point(451, 97)
        Me.btRemoveValues.Name = "btRemoveValues"
        Me.btRemoveValues.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveValues.TabIndex = 19
        Me.btRemoveValues.Text = "<<"
        Me.btRemoveValues.UseVisualStyleBackColor = True
        '
        'btAddValues
        '
        Me.btAddValues.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btAddValues.Location = New System.Drawing.Point(406, 97)
        Me.btAddValues.Name = "btAddValues"
        Me.btAddValues.Size = New System.Drawing.Size(39, 23)
        Me.btAddValues.TabIndex = 18
        Me.btAddValues.Text = ">>"
        Me.btAddValues.UseVisualStyleBackColor = True
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
        Me.grpChartSelection.Controls.Add(Me.lblChartDescription)
        Me.grpChartSelection.Controls.Add(Me.cbDataLayout)
        Me.grpChartSelection.Controls.Add(Me.lblDataLayout)
        Me.grpChartSelection.Controls.Add(Me.cbChartType)
        Me.grpChartSelection.Controls.Add(Me.lblChartType)
        Me.grpChartSelection.Controls.Add(Me.cbChartFamily)
        Me.grpChartSelection.Controls.Add(Me.lblChartFamily)
        Me.grpChartSelection.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpChartSelection.Location = New System.Drawing.Point(6, 6)
        Me.grpChartSelection.Name = "grpChartSelection"
        Me.grpChartSelection.Size = New System.Drawing.Size(859, 121)
        Me.grpChartSelection.TabIndex = 1
        Me.grpChartSelection.TabStop = False
        Me.grpChartSelection.Text = "Control chart"
        '
        'lblChartDescription
        '
        Me.lblChartDescription.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChartDescription.Location = New System.Drawing.Point(395, 26)
        Me.lblChartDescription.Name = "lblChartDescription"
        Me.lblChartDescription.Size = New System.Drawing.Size(363, 84)
        Me.lblChartDescription.TabIndex = 18
        Me.lblChartDescription.Text = "Description of the selected chart"
        '
        'cbDataLayout
        '
        Me.cbDataLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDataLayout.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDataLayout.FormattingEnabled = True
        Me.cbDataLayout.Location = New System.Drawing.Point(153, 86)
        Me.cbDataLayout.Name = "cbDataLayout"
        Me.cbDataLayout.Size = New System.Drawing.Size(239, 24)
        Me.cbDataLayout.TabIndex = 17
        '
        'lblDataLayout
        '
        Me.lblDataLayout.AutoSize = True
        Me.lblDataLayout.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDataLayout.Location = New System.Drawing.Point(6, 89)
        Me.lblDataLayout.Name = "lblDataLayout"
        Me.lblDataLayout.Size = New System.Drawing.Size(75, 16)
        Me.lblDataLayout.TabIndex = 16
        Me.lblDataLayout.Text = "Data layout"
        '
        'cbChartType
        '
        Me.cbChartType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbChartType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbChartType.FormattingEnabled = True
        Me.cbChartType.Location = New System.Drawing.Point(153, 56)
        Me.cbChartType.Name = "cbChartType"
        Me.cbChartType.Size = New System.Drawing.Size(239, 24)
        Me.cbChartType.TabIndex = 15
        '
        'lblChartType
        '
        Me.lblChartType.AutoSize = True
        Me.lblChartType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChartType.Location = New System.Drawing.Point(6, 59)
        Me.lblChartType.Name = "lblChartType"
        Me.lblChartType.Size = New System.Drawing.Size(67, 16)
        Me.lblChartType.TabIndex = 14
        Me.lblChartType.Text = "Chart type"
        '
        'cbChartFamily
        '
        Me.cbChartFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbChartFamily.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbChartFamily.FormattingEnabled = True
        Me.cbChartFamily.Location = New System.Drawing.Point(153, 26)
        Me.cbChartFamily.Name = "cbChartFamily"
        Me.cbChartFamily.Size = New System.Drawing.Size(239, 24)
        Me.cbChartFamily.TabIndex = 13
        '
        'lblChartFamily
        '
        Me.lblChartFamily.AutoSize = True
        Me.lblChartFamily.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblChartFamily.Location = New System.Drawing.Point(6, 29)
        Me.lblChartFamily.Name = "lblChartFamily"
        Me.lblChartFamily.Size = New System.Drawing.Size(76, 16)
        Me.lblChartFamily.TabIndex = 12
        Me.lblChartFamily.Text = "Chart family"
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1_ChartData)
        Me.TabControl1.Controls.Add(Me.TabPage2_ParametersLimits)
        Me.TabControl1.Controls.Add(Me.TabPage3_PhasesExclusions)
        Me.TabControl1.Controls.Add(Me.TabPage4_SignalRules)
        Me.TabControl1.Controls.Add(Me.TabPage5_OutputAppearance)
        Me.TabControl1.Location = New System.Drawing.Point(1, 2)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(879, 770)
        Me.TabControl1.TabIndex = 6
        '
        'btnInterrupt
        '
        Me.btnInterrupt.Location = New System.Drawing.Point(714, 778)
        Me.btnInterrupt.Name = "btnInterrupt"
        Me.btnInterrupt.Size = New System.Drawing.Size(75, 23)
        Me.btnInterrupt.TabIndex = 7
        Me.btnInterrupt.Text = "Interrupt"
        Me.btnInterrupt.UseVisualStyleBackColor = True
        '
        'ProgressBar
        '
        Me.ProgressBar.Location = New System.Drawing.Point(7, 778)
        Me.ProgressBar.Name = "ProgressBar"
        Me.ProgressBar.Size = New System.Drawing.Size(620, 23)
        Me.ProgressBar.TabIndex = 8
        '
        'Ui21ControlCharts
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(882, 809)
        Me.Controls.Add(Me.ProgressBar)
        Me.Controls.Add(Me.btnInterrupt)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Name = "Ui21ControlCharts"
        Me.ShowIcon = False
        Me.Text = "Ui21ControlCharts"
        Me.TabPage5_OutputAppearance.ResumeLayout(False)
        Me.grpChartDimensions.ResumeLayout(False)
        Me.grpChartDimensions.PerformLayout()
        CType(Me.spinPanelSpacing, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinPanelHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinChartWidth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpSpecifications.ResumeLayout(False)
        Me.grpSpecifications.PerformLayout()
        Me.grpChartDisplay.ResumeLayout(False)
        Me.grpChartDisplay.PerformLayout()
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpTitleAxes.ResumeLayout(False)
        Me.grpTitleAxes.PerformLayout()
        Me.TabPage4_SignalRules.ResumeLayout(False)
        Me.TabPage4_SignalRules.PerformLayout()
        Me.grpSequenceOptions.ResumeLayout(False)
        Me.grpSequenceOptions.PerformLayout()
        CType(Me.dgvRules, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpRulePreset.ResumeLayout(False)
        Me.grpRulePreset.PerformLayout()
        Me.TabPage3_PhasesExclusions.ResumeLayout(False)
        Me.grpExclusions.ResumeLayout(False)
        CType(Me.dgvExclusions, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvStages, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpQuickPhaseSetup.ResumeLayout(False)
        Me.grpQuickPhaseSetup.PerformLayout()
        CType(Me.spinLastPhaseIPoint, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpPhaseColumns.ResumeLayout(False)
        Me.grpPhaseColumns.PerformLayout()
        Me.TabPage2_ParametersLimits.ResumeLayout(False)
        Me.grpTimeWeightedParameters.ResumeLayout(False)
        Me.grpTimeWeightedParameters.PerformLayout()
        CType(Me.spinCusumDecisionInterval, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinHeadStart, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinMovingAverageSpan, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinCusumReferenceValue, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinEwmaLambda, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpHistoricalParameters.ResumeLayout(False)
        CType(Me.dgvHistoricalParameters, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpGeneralOptions.ResumeLayout(False)
        Me.grpGeneralOptions.PerformLayout()
        CType(Me.spinMovingRangeLength, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinSigmaMultiplier, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage1_ChartData.ResumeLayout(False)
        Me.grpWorksheet.ResumeLayout(False)
        Me.grpWorksheet.PerformLayout()
        Me.grpChartSelection.ResumeLayout(False)
        Me.grpChartSelection.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents TabPage5_OutputAppearance As Windows.Forms.TabPage
    Friend WithEvents TabPage4_SignalRules As Windows.Forms.TabPage
    Friend WithEvents grpRulePreset As Windows.Forms.GroupBox
    Friend WithEvents TabPage3_PhasesExclusions As Windows.Forms.TabPage
    Friend WithEvents grpPhaseColumns As Windows.Forms.GroupBox
    Friend WithEvents TabPage2_ParametersLimits As Windows.Forms.TabPage
    Friend WithEvents grpGeneralOptions As Windows.Forms.GroupBox
    Friend WithEvents TabPage1_ChartData As Windows.Forms.TabPage
    Friend WithEvents grpChartSelection As Windows.Forms.GroupBox
    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents btnInterrupt As Windows.Forms.Button
    Friend WithEvents ProgressBar As Windows.Forms.ProgressBar
    Friend WithEvents cbDataLayout As Windows.Forms.ComboBox
    Friend WithEvents lblDataLayout As Windows.Forms.Label
    Friend WithEvents cbChartType As Windows.Forms.ComboBox
    Friend WithEvents lblChartType As Windows.Forms.Label
    Friend WithEvents cbChartFamily As Windows.Forms.ComboBox
    Friend WithEvents lblChartFamily As Windows.Forms.Label
    Friend WithEvents lblChartDescription As Windows.Forms.Label
    Friend WithEvents grpWorksheet As Windows.Forms.GroupBox
    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents lblSheetsList As Windows.Forms.Label
    Friend WithEvents lblSubgroupID As Windows.Forms.Label
    Friend WithEvents lbSubgroupID As Windows.Forms.ListBox
    Friend WithEvents btRemoveSubgroupID As Windows.Forms.Button
    Friend WithEvents btAddSubgroupID As Windows.Forms.Button
    Friend WithEvents lblValues As Windows.Forms.Label
    Friend WithEvents lbValues As Windows.Forms.ListBox
    Friend WithEvents btRemoveValues As Windows.Forms.Button
    Friend WithEvents btAddValues As Windows.Forms.Button
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
    Friend WithEvents btClearDataRoles As Windows.Forms.Button
    Friend WithEvents lblDataRequirements As Windows.Forms.Label
    Friend WithEvents spinSigmaMultiplier As Windows.Forms.NumericUpDown
    Friend WithEvents lblSigmaMultiplier As Windows.Forms.Label
    Friend WithEvents cbParameterSource As Windows.Forms.ComboBox
    Friend WithEvents lblParameterSource As Windows.Forms.Label
    Friend WithEvents cbMissingValuePolicy As Windows.Forms.ComboBox
    Friend WithEvents lblMissingValuePolicy As Windows.Forms.Label
    Friend WithEvents cbWithinSigmaEstimator As Windows.Forms.ComboBox
    Friend WithEvents lblWithinSigmaEstimator As Windows.Forms.Label
    Friend WithEvents cbControlLimitMethod As Windows.Forms.ComboBox
    Friend WithEvents lblControlLimitMethod As Windows.Forms.Label
    Friend WithEvents cbNaturalLimitPolicy As Windows.Forms.ComboBox
    Friend WithEvents lblNaturalLimitPolicy As Windows.Forms.Label
    Friend WithEvents chkUseBiasCorrection As Windows.Forms.CheckBox
    Friend WithEvents spinMovingRangeLength As Windows.Forms.NumericUpDown
    Friend WithEvents lblMovingRangeLength As Windows.Forms.Label
    Friend WithEvents grpHistoricalParameters As Windows.Forms.GroupBox
    Friend WithEvents grpTimeWeightedParameters As Windows.Forms.GroupBox
    Friend WithEvents ComboBox1 As Windows.Forms.ComboBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents spinCusumReferenceValue As Windows.Forms.NumericUpDown
    Friend WithEvents lblCusumReferenceValue As Windows.Forms.Label
    Friend WithEvents spinEwmaLambda As Windows.Forms.NumericUpDown
    Friend WithEvents lblEwmaLambda As Windows.Forms.Label
    Friend WithEvents btClearHistoricalParameters As Windows.Forms.Button
    Friend WithEvents btRemoveHistoricalParameter As Windows.Forms.Button
    Friend WithEvents btAddHistoricalParameter As Windows.Forms.Button
    Friend WithEvents dgvHistoricalParameters As Windows.Forms.DataGridView
    Friend WithEvents spinCusumDecisionInterval As Windows.Forms.NumericUpDown
    Friend WithEvents lblCusumDecisionInterval As Windows.Forms.Label
    Friend WithEvents spinHeadStart As Windows.Forms.NumericUpDown
    Friend WithEvents lblHeadStart As Windows.Forms.Label
    Friend WithEvents spinMovingAverageSpan As Windows.Forms.NumericUpDown
    Friend WithEvents lblMovingAverageSpan As Windows.Forms.Label
    Friend WithEvents lblTimeWeightedNote As Windows.Forms.Label
    Friend WithEvents chkUseSteadyStateLimits As Windows.Forms.CheckBox
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
    Friend WithEvents btImportExclusions As Windows.Forms.Button
    Friend WithEvents btImportStages As Windows.Forms.Button
    Friend WithEvents grpQuickPhaseSetup As Windows.Forms.GroupBox
    Friend WithEvents btApplyQuickPhaseSetup As Windows.Forms.Button
    Friend WithEvents spinLastPhaseIPoint As Windows.Forms.NumericUpDown
    Friend WithEvents lblLastPhaseIPoint As Windows.Forms.Label
    Friend WithEvents rbPhaseIThenPhaseII As Windows.Forms.RadioButton
    Friend WithEvents rbSinglePhaseI As Windows.Forms.RadioButton
    Friend WithEvents dgvStages As Windows.Forms.DataGridView
    Friend WithEvents colStageID As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStageDisplayName As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStageFirstPoint As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStageLastPoint As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStagePhase As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colStageLimitMode As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colStageReferenceID As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colHistoryStageID As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colHistoryMean As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colHistorySigma As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colHistoryProportion As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colHistoryMeanCount As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colHistoryMeanRate As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents grpExclusions As Windows.Forms.GroupBox
    Friend WithEvents btClearExclusions As Windows.Forms.Button
    Friend WithEvents btRemoveExclusion As Windows.Forms.Button
    Friend WithEvents btAddExclusion As Windows.Forms.Button
    Friend WithEvents dgvExclusions As Windows.Forms.DataGridView
    Friend WithEvents colExclusionPoint As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colExclusionScope As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colExclusionReason As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lblRulePresetDescription As Windows.Forms.Label
    Friend WithEvents btCopyPresetToCustom As Windows.Forms.Button
    Friend WithEvents btLoadRulePreset As Windows.Forms.Button
    Friend WithEvents cbRulePreset As Windows.Forms.ComboBox
    Friend WithEvents lblRulePreset As Windows.Forms.Label
    Friend WithEvents dgvRules As Windows.Forms.DataGridView
    Friend WithEvents tbRuleDescription As Windows.Forms.TextBox
    Friend WithEvents btResetCustomRules As Windows.Forms.Button
    Friend WithEvents btRemoveRule As Windows.Forms.Button
    Friend WithEvents btAddRule As Windows.Forms.Button
    Friend WithEvents colRuleEnabled As Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents colRuleNumber As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colRuleCode As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colRuleName As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colRuleKind As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colRuleWindow As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colRuleMinimumPoints As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colRuleSigma As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colRuleSide As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colRuleScope As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents grpSequenceOptions As Windows.Forms.GroupBox
    Friend WithEvents cbSignalMarkingMode As Windows.Forms.ComboBox
    Friend WithEvents lblSignalMarkingMode As Windows.Forms.Label
    Friend WithEvents cbSequenceGapBehavior As Windows.Forms.ComboBox
    Friend WithEvents lblSequenceGapBehavior As Windows.Forms.Label
    Friend WithEvents lblRuleApplicability As Windows.Forms.Label
    Friend WithEvents cbRulePhaseScope As Windows.Forms.ComboBox
    Friend WithEvents lblRulePhaseScope As Windows.Forms.Label
    Friend WithEvents chkWriteSummary As Windows.Forms.CheckBox
    Friend WithEvents chkWriteSettingsAudit As Windows.Forms.CheckBox
    Friend WithEvents chkWriteSignals As Windows.Forms.CheckBox
    Friend WithEvents chkWriteChartData As Windows.Forms.CheckBox
    Friend WithEvents chkCreateControlCharts As Windows.Forms.CheckBox
    Friend WithEvents grpTitleAxes As Windows.Forms.GroupBox
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents lblHorizontalAxisTitle As Windows.Forms.Label
    Friend WithEvents tbHorizontalAxisTitle As Windows.Forms.TextBox
    Friend WithEvents lblValueAxisTitle As Windows.Forms.Label
    Friend WithEvents tbValueAxisTitle As Windows.Forms.TextBox
    Friend WithEvents lblChartTitle As Windows.Forms.Label
    Friend WithEvents tbChartTitle As Windows.Forms.TextBox
    Friend WithEvents cbHorizontalTickOrientation As Windows.Forms.ComboBox
    Friend WithEvents lblHorizontalTickOrientation As Windows.Forms.Label
    Friend WithEvents chkShowHorizontalAxisOnEveryPanel As Windows.Forms.CheckBox
    Friend WithEvents chkUseSequenceValuesForHorizontalAxis As Windows.Forms.CheckBox
    Friend WithEvents tbValueNumberFormat As Windows.Forms.TextBox
    Friend WithEvents lblValueNumberFormat As Windows.Forms.Label
    Friend WithEvents grpChartDisplay As Windows.Forms.GroupBox
    Friend WithEvents chkShowPointLabels As Windows.Forms.CheckBox
    Friend WithEvents chkShowMajorGridlines As Windows.Forms.CheckBox
    Friend WithEvents chkShowStageBoundaries As Windows.Forms.CheckBox
    Friend WithEvents chkShowSignalLabels As Windows.Forms.CheckBox
    Friend WithEvents chkShowExclusionLabels As Windows.Forms.CheckBox
    Friend WithEvents chkShowExcludedPoints As Windows.Forms.CheckBox
    Friend WithEvents chkShowLimitLabels As Windows.Forms.CheckBox
    Friend WithEvents chkShowLegend As Windows.Forms.CheckBox
    Friend WithEvents cbZoneDisplay As Windows.Forms.ComboBox
    Friend WithEvents lblZoneDisplay As Windows.Forms.Label
    Friend WithEvents chkShowZoneSeriesInLegend As Windows.Forms.CheckBox
    Friend WithEvents grpSpecifications As Windows.Forms.GroupBox
    Friend WithEvents tbUpperSpecificationLimit As Windows.Forms.TextBox
    Friend WithEvents lblUpperSpecificationLimit As Windows.Forms.Label
    Friend WithEvents tbTarget As Windows.Forms.TextBox
    Friend WithEvents lblTarget As Windows.Forms.Label
    Friend WithEvents tbLowerSpecificationLimit As Windows.Forms.TextBox
    Friend WithEvents lblLowerSpecificationLimit As Windows.Forms.Label
    Friend WithEvents chkShowTargetLine As Windows.Forms.CheckBox
    Friend WithEvents chkShowSpecificationLimits As Windows.Forms.CheckBox
    Friend WithEvents grpChartDimensions As Windows.Forms.GroupBox
    Friend WithEvents spinPanelSpacing As Windows.Forms.NumericUpDown
    Friend WithEvents lblPanelSpacing As Windows.Forms.Label
    Friend WithEvents spinPanelHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblPanelHeight As Windows.Forms.Label
    Friend WithEvents btResetAppearance As Windows.Forms.Button
    Friend WithEvents spinChartWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartWidth As Windows.Forms.Label
End Class
