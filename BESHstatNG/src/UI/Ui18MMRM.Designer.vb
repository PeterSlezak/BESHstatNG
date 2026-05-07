<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Ui18MMRM
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
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCalculate = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.btRemoveTime = New System.Windows.Forms.Button()
        Me.btAddTime = New System.Windows.Forms.Button()
        Me.btRemoveClusterID = New System.Windows.Forms.Button()
        Me.btAddClusterID = New System.Windows.Forms.Button()
        Me.lbTime = New System.Windows.Forms.ListBox()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.lbClusterID = New System.Windows.Forms.ListBox()
        Me.lblClusterID = New System.Windows.Forms.Label()
        Me.lbXs = New System.Windows.Forms.ListBox()
        Me.lbY = New System.Windows.Forms.ListBox()
        Me.cbSheetsList = New System.Windows.Forms.ComboBox()
        Me.btReload = New System.Windows.Forms.Button()
        Me.btRemoveX = New System.Windows.Forms.Button()
        Me.btAddX = New System.Windows.Forms.Button()
        Me.lblNote = New System.Windows.Forms.Label()
        Me.lblY = New System.Windows.Forms.Label()
        Me.btRemoveY = New System.Windows.Forms.Button()
        Me.btAddY = New System.Windows.Forms.Button()
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.lblX = New System.Windows.Forms.Label()
        Me.lblSelectedSheet = New System.Windows.Forms.Label()
        Me.TabPageBuildModel = New System.Windows.Forms.TabPage()
        Me.btAddEffectCategoricalFactor = New System.Windows.Forms.Button()
        Me.btnCustomInteraction = New System.Windows.Forms.Button()
        Me.btn2Interactions = New System.Windows.Forms.Button()
        Me.spinBtnPoly = New System.Windows.Forms.NumericUpDown()
        Me.btnPoly = New System.Windows.Forms.Button()
        Me.cbIntercept = New System.Windows.Forms.CheckBox()
        Me.btAddEffect = New System.Windows.Forms.Button()
        Me.btClearAllSelectedEffects = New System.Windows.Forms.Button()
        Me.tbRemoveSelectedEffects = New System.Windows.Forms.Button()
        Me.lbSelectedEffectsList = New System.Windows.Forms.ListBox()
        Me.lbSelectedVariables = New System.Windows.Forms.ListBox()
        Me.lblSelectedEffectsList = New System.Windows.Forms.Label()
        Me.lblSelectedVariables = New System.Windows.Forms.Label()
        Me.TabPageOptions = New System.Windows.Forms.TabPage()
        Me.grpMMRMRpostestOutputs = New System.Windows.Forms.GroupBox()
        Me.ckMMRMEstimatedMeans = New System.Windows.Forms.CheckBox()
        Me.ckMMRMClassInfo = New System.Windows.Forms.CheckBox()
        Me.grpMMRMReferenceGrid = New System.Windows.Forms.GroupBox()
        Me.lblMMRMRefGridWeighting = New System.Windows.Forms.Label()
        Me.cbMMRMRefGridCovariates = New System.Windows.Forms.ComboBox()
        Me.lblMMRMRefGridCovariates = New System.Windows.Forms.Label()
        Me.cbMMRMLSMeansMode = New System.Windows.Forms.ComboBox()
        Me.cbMMRMRefGridWeighting = New System.Windows.Forms.ComboBox()
        Me.lblMMRMLSMeansMode = New System.Windows.Forms.Label()
        Me.grpMMRMContrasts = New System.Windows.Forms.GroupBox()
        Me.cbMMRMComparisonLevel = New System.Windows.Forms.ComboBox()
        Me.lblMMRMMultiplicity = New System.Windows.Forms.Label()
        Me.lblMMRMComparisonLevel = New System.Windows.Forms.Label()
        Me.cbMMRMMultiplicity = New System.Windows.Forms.ComboBox()
        Me.ckMMRMDiffInChange = New System.Windows.Forms.CheckBox()
        Me.ckMMRMChangeFromBaseline = New System.Windows.Forms.CheckBox()
        Me.cbMMRMContrastDirection = New System.Windows.Forms.ComboBox()
        Me.lblMMRMContrastDirection = New System.Windows.Forms.Label()
        Me.cbMMRMControlLevel = New System.Windows.Forms.ComboBox()
        Me.lblMMRMControlLevel = New System.Windows.Forms.Label()
        Me.cbMMRMContrastMode = New System.Windows.Forms.ComboBox()
        Me.lblMMRMContrastMode = New System.Windows.Forms.Label()
        Me.cbMMRMBaselineVisit = New System.Windows.Forms.ComboBox()
        Me.lblMMRMBaselineVisit = New System.Windows.Forms.Label()
        Me.cbMMRMGroupingFactor = New System.Windows.Forms.ComboBox()
        Me.lblMMRMGroupingFactor = New System.Windows.Forms.Label()
        Me.grpModelSpecification = New System.Windows.Forms.GroupBox()
        Me.cbInferenceMethod = New System.Windows.Forms.ComboBox()
        Me.lblInferenceMethod = New System.Windows.Forms.Label()
        Me.cbFitMethod = New System.Windows.Forms.ComboBox()
        Me.cbCovarStruct = New System.Windows.Forms.ComboBox()
        Me.lblCovarStruct = New System.Windows.Forms.Label()
        Me.lblFitMethod = New System.Windows.Forms.Label()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.ckResiduals = New System.Windows.Forms.CheckBox()
        Me.grpIterOptions = New System.Windows.Forms.GroupBox()
        Me.cbDiagnostic = New System.Windows.Forms.CheckBox()
        Me.ckTrace = New System.Windows.Forms.CheckBox()
        Me.ckIterationsDetails = New System.Windows.Forms.CheckBox()
        Me.tbMaxIter = New System.Windows.Forms.TextBox()
        Me.lblMaxIter = New System.Windows.Forms.Label()
        Me.lblEps = New System.Windows.Forms.Label()
        Me.tbEps = New System.Windows.Forms.TextBox()
        Me.btInterrupt = New System.Windows.Forms.Button()
        Me.cbMMRMCovGradientMode = New System.Windows.Forms.ComboBox()
        Me.lblMMRMCovOptimizerMode = New System.Windows.Forms.Label()
        Me.cbMMRMCovOptimizerMode = New System.Windows.Forms.ComboBox()
        Me.lblMMRMCovGradientMode = New System.Windows.Forms.Label()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPageBuildModel.SuspendLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageOptions.SuspendLayout()
        Me.grpMMRMRpostestOutputs.SuspendLayout()
        Me.grpMMRMReferenceGrid.SuspendLayout()
        Me.grpMMRMContrasts.SuspendLayout()
        Me.grpModelSpecification.SuspendLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpIterOptions.SuspendLayout()
        Me.SuspendLayout()
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(2, 500)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(837, 23)
        Me.ProgressBar1.TabIndex = 15
        '
        'lblProgress
        '
        Me.lblProgress.Location = New System.Drawing.Point(0, 526)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(596, 32)
        Me.lblProgress.TabIndex = 14
        Me.lblProgress.Text = "Elapsed Time: "
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(683, 529)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 13
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCalculate
        '
        Me.btCalculate.Location = New System.Drawing.Point(764, 529)
        Me.btCalculate.Name = "btCalculate"
        Me.btCalculate.Size = New System.Drawing.Size(75, 23)
        Me.btCalculate.TabIndex = 12
        Me.btCalculate.Text = "Fit"
        Me.btCalculate.UseVisualStyleBackColor = True
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPageBuildModel)
        Me.TabControl1.Controls.Add(Me.TabPageOptions)
        Me.TabControl1.Location = New System.Drawing.Point(2, 0)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(844, 494)
        Me.TabControl1.TabIndex = 16
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.btRemoveTime)
        Me.TabPage1.Controls.Add(Me.btAddTime)
        Me.TabPage1.Controls.Add(Me.btRemoveClusterID)
        Me.TabPage1.Controls.Add(Me.btAddClusterID)
        Me.TabPage1.Controls.Add(Me.lbTime)
        Me.TabPage1.Controls.Add(Me.lblTime)
        Me.TabPage1.Controls.Add(Me.lbClusterID)
        Me.TabPage1.Controls.Add(Me.lblClusterID)
        Me.TabPage1.Controls.Add(Me.lbXs)
        Me.TabPage1.Controls.Add(Me.lbY)
        Me.TabPage1.Controls.Add(Me.cbSheetsList)
        Me.TabPage1.Controls.Add(Me.btReload)
        Me.TabPage1.Controls.Add(Me.btRemoveX)
        Me.TabPage1.Controls.Add(Me.btAddX)
        Me.TabPage1.Controls.Add(Me.lblNote)
        Me.TabPage1.Controls.Add(Me.lblY)
        Me.TabPage1.Controls.Add(Me.btRemoveY)
        Me.TabPage1.Controls.Add(Me.btAddY)
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
        'btRemoveTime
        '
        Me.btRemoveTime.Location = New System.Drawing.Point(289, 109)
        Me.btRemoveTime.Name = "btRemoveTime"
        Me.btRemoveTime.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveTime.TabIndex = 31
        Me.btRemoveTime.Text = "<<"
        Me.btRemoveTime.UseVisualStyleBackColor = True
        '
        'btAddTime
        '
        Me.btAddTime.Location = New System.Drawing.Point(244, 109)
        Me.btAddTime.Name = "btAddTime"
        Me.btAddTime.Size = New System.Drawing.Size(39, 23)
        Me.btAddTime.TabIndex = 30
        Me.btAddTime.Text = ">>"
        Me.btAddTime.UseVisualStyleBackColor = True
        '
        'btRemoveClusterID
        '
        Me.btRemoveClusterID.Location = New System.Drawing.Point(289, 67)
        Me.btRemoveClusterID.Name = "btRemoveClusterID"
        Me.btRemoveClusterID.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveClusterID.TabIndex = 29
        Me.btRemoveClusterID.Text = "<<"
        Me.btRemoveClusterID.UseVisualStyleBackColor = True
        '
        'btAddClusterID
        '
        Me.btAddClusterID.Location = New System.Drawing.Point(244, 67)
        Me.btAddClusterID.Name = "btAddClusterID"
        Me.btAddClusterID.Size = New System.Drawing.Size(39, 23)
        Me.btAddClusterID.TabIndex = 28
        Me.btAddClusterID.Text = ">>"
        Me.btAddClusterID.UseVisualStyleBackColor = True
        '
        'lbTime
        '
        Me.lbTime.FormattingEnabled = True
        Me.lbTime.ItemHeight = 16
        Me.lbTime.Location = New System.Drawing.Point(337, 109)
        Me.lbTime.Name = "lbTime"
        Me.lbTime.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbTime.Size = New System.Drawing.Size(221, 20)
        Me.lbTime.TabIndex = 26
        '
        'lblTime
        '
        Me.lblTime.AutoSize = True
        Me.lblTime.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTime.Location = New System.Drawing.Point(334, 90)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(101, 16)
        Me.lblTime.TabIndex = 27
        Me.lblTime.Text = "Visit / Time **"
        '
        'lbClusterID
        '
        Me.lbClusterID.FormattingEnabled = True
        Me.lbClusterID.ItemHeight = 16
        Me.lbClusterID.Location = New System.Drawing.Point(334, 67)
        Me.lbClusterID.Name = "lbClusterID"
        Me.lbClusterID.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbClusterID.Size = New System.Drawing.Size(221, 20)
        Me.lbClusterID.TabIndex = 24
        '
        'lblClusterID
        '
        Me.lblClusterID.AutoSize = True
        Me.lblClusterID.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblClusterID.Location = New System.Drawing.Point(334, 48)
        Me.lblClusterID.Name = "lblClusterID"
        Me.lblClusterID.Size = New System.Drawing.Size(84, 16)
        Me.lblClusterID.TabIndex = 25
        Me.lblClusterID.Text = "Subject ID*"
        '
        'lbXs
        '
        Me.lbXs.FormattingEnabled = True
        Me.lbXs.ItemHeight = 16
        Me.lbXs.Location = New System.Drawing.Point(334, 151)
        Me.lbXs.Name = "lbXs"
        Me.lbXs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbXs.Size = New System.Drawing.Size(221, 308)
        Me.lbXs.TabIndex = 17
        '
        'lbY
        '
        Me.lbY.FormattingEnabled = True
        Me.lbY.ItemHeight = 16
        Me.lbY.Location = New System.Drawing.Point(334, 25)
        Me.lbY.Name = "lbY"
        Me.lbY.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbY.Size = New System.Drawing.Size(221, 20)
        Me.lbY.TabIndex = 4
        '
        'cbSheetsList
        '
        Me.cbSheetsList.FormattingEnabled = True
        Me.cbSheetsList.Location = New System.Drawing.Point(590, 25)
        Me.cbSheetsList.Name = "cbSheetsList"
        Me.cbSheetsList.Size = New System.Drawing.Size(240, 24)
        Me.cbSheetsList.TabIndex = 21
        Me.cbSheetsList.Text = "Select Sheet"
        '
        'btReload
        '
        Me.btReload.Location = New System.Drawing.Point(589, 55)
        Me.btReload.Name = "btReload"
        Me.btReload.Size = New System.Drawing.Size(130, 23)
        Me.btReload.TabIndex = 20
        Me.btReload.Text = "Reload Sheet Data"
        Me.btReload.UseVisualStyleBackColor = True
        '
        'btRemoveX
        '
        Me.btRemoveX.Location = New System.Drawing.Point(289, 151)
        Me.btRemoveX.Name = "btRemoveX"
        Me.btRemoveX.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveX.TabIndex = 16
        Me.btRemoveX.Text = "<<"
        Me.btRemoveX.UseVisualStyleBackColor = True
        '
        'btAddX
        '
        Me.btAddX.Location = New System.Drawing.Point(244, 151)
        Me.btAddX.Name = "btAddX"
        Me.btAddX.Size = New System.Drawing.Size(39, 23)
        Me.btAddX.TabIndex = 15
        Me.btAddX.Text = ">>"
        Me.btAddX.UseVisualStyleBackColor = True
        '
        'lblNote
        '
        Me.lblNote.AutoSize = True
        Me.lblNote.Location = New System.Drawing.Point(561, 427)
        Me.lblNote.Name = "lblNote"
        Me.lblNote.Size = New System.Drawing.Size(233, 32)
        Me.lblNote.TabIndex = 10
        Me.lblNote.Text = "* indicate mandatory fields" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "** indicate conditionally required fields"
        '
        'lblY
        '
        Me.lblY.AutoSize = True
        Me.lblY.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblY.Location = New System.Drawing.Point(334, 6)
        Me.lblY.Name = "lblY"
        Me.lblY.Size = New System.Drawing.Size(227, 16)
        Me.lblY.TabIndex = 5
        Me.lblY.Text = "Dependent Variable (Outcome)*"
        '
        'btRemoveY
        '
        Me.btRemoveY.Location = New System.Drawing.Point(289, 22)
        Me.btRemoveY.Name = "btRemoveY"
        Me.btRemoveY.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveY.TabIndex = 3
        Me.btRemoveY.Text = "<<"
        Me.btRemoveY.UseVisualStyleBackColor = True
        '
        'btAddY
        '
        Me.btAddY.Location = New System.Drawing.Point(244, 22)
        Me.btAddY.Name = "btAddY"
        Me.btAddY.Size = New System.Drawing.Size(39, 23)
        Me.btAddY.TabIndex = 2
        Me.btAddY.Text = ">>"
        Me.btAddY.UseVisualStyleBackColor = True
        '
        'lbAllColumns
        '
        Me.lbAllColumns.FormattingEnabled = True
        Me.lbAllColumns.ItemHeight = 16
        Me.lbAllColumns.Location = New System.Drawing.Point(17, 22)
        Me.lbAllColumns.Name = "lbAllColumns"
        Me.lbAllColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbAllColumns.Size = New System.Drawing.Size(221, 436)
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
        Me.lblX.Location = New System.Drawing.Point(334, 132)
        Me.lblX.Name = "lblX"
        Me.lblX.Size = New System.Drawing.Size(229, 16)
        Me.lblX.TabIndex = 18
        Me.lblX.Text = "Fixed-Effect Source Variable(s)*"
        '
        'lblSelectedSheet
        '
        Me.lblSelectedSheet.AutoSize = True
        Me.lblSelectedSheet.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedSheet.Location = New System.Drawing.Point(587, 6)
        Me.lblSelectedSheet.Name = "lblSelectedSheet"
        Me.lblSelectedSheet.Size = New System.Drawing.Size(132, 16)
        Me.lblSelectedSheet.TabIndex = 23
        Me.lblSelectedSheet.Text = "Active Worksheet:"
        '
        'TabPageBuildModel
        '
        Me.TabPageBuildModel.Controls.Add(Me.btAddEffectCategoricalFactor)
        Me.TabPageBuildModel.Controls.Add(Me.btnCustomInteraction)
        Me.TabPageBuildModel.Controls.Add(Me.btn2Interactions)
        Me.TabPageBuildModel.Controls.Add(Me.spinBtnPoly)
        Me.TabPageBuildModel.Controls.Add(Me.btnPoly)
        Me.TabPageBuildModel.Controls.Add(Me.cbIntercept)
        Me.TabPageBuildModel.Controls.Add(Me.btAddEffect)
        Me.TabPageBuildModel.Controls.Add(Me.btClearAllSelectedEffects)
        Me.TabPageBuildModel.Controls.Add(Me.tbRemoveSelectedEffects)
        Me.TabPageBuildModel.Controls.Add(Me.lbSelectedEffectsList)
        Me.TabPageBuildModel.Controls.Add(Me.lbSelectedVariables)
        Me.TabPageBuildModel.Controls.Add(Me.lblSelectedEffectsList)
        Me.TabPageBuildModel.Controls.Add(Me.lblSelectedVariables)
        Me.TabPageBuildModel.Location = New System.Drawing.Point(4, 25)
        Me.TabPageBuildModel.Name = "TabPageBuildModel"
        Me.TabPageBuildModel.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageBuildModel.Size = New System.Drawing.Size(836, 465)
        Me.TabPageBuildModel.TabIndex = 1
        Me.TabPageBuildModel.Text = "Build Model"
        Me.TabPageBuildModel.UseVisualStyleBackColor = True
        '
        'btAddEffectCategoricalFactor
        '
        Me.btAddEffectCategoricalFactor.Location = New System.Drawing.Point(330, 84)
        Me.btAddEffectCategoricalFactor.Name = "btAddEffectCategoricalFactor"
        Me.btAddEffectCategoricalFactor.Size = New System.Drawing.Size(191, 23)
        Me.btAddEffectCategoricalFactor.TabIndex = 26
        Me.btAddEffectCategoricalFactor.Text = "Add as Categorical Factor >>"
        Me.btAddEffectCategoricalFactor.UseVisualStyleBackColor = True
        '
        'btnCustomInteraction
        '
        Me.btnCustomInteraction.Location = New System.Drawing.Point(330, 171)
        Me.btnCustomInteraction.Name = "btnCustomInteraction"
        Me.btnCustomInteraction.Size = New System.Drawing.Size(191, 23)
        Me.btnCustomInteraction.TabIndex = 25
        Me.btnCustomInteraction.Text = "Custom Interaction >>"
        Me.btnCustomInteraction.UseVisualStyleBackColor = True
        '
        'btn2Interactions
        '
        Me.btn2Interactions.Location = New System.Drawing.Point(330, 142)
        Me.btn2Interactions.Name = "btn2Interactions"
        Me.btn2Interactions.Size = New System.Drawing.Size(191, 23)
        Me.btn2Interactions.TabIndex = 24
        Me.btn2Interactions.Text = "2-way Interactions >>"
        Me.btn2Interactions.UseVisualStyleBackColor = True
        '
        'spinBtnPoly
        '
        Me.spinBtnPoly.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnPoly.Location = New System.Drawing.Point(477, 113)
        Me.spinBtnPoly.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnPoly.Name = "spinBtnPoly"
        Me.spinBtnPoly.Size = New System.Drawing.Size(44, 22)
        Me.spinBtnPoly.TabIndex = 23
        Me.spinBtnPoly.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'btnPoly
        '
        Me.btnPoly.Location = New System.Drawing.Point(330, 113)
        Me.btnPoly.Name = "btnPoly"
        Me.btnPoly.Size = New System.Drawing.Size(131, 23)
        Me.btnPoly.TabIndex = 22
        Me.btnPoly.Text = "Poly >>"
        Me.btnPoly.UseVisualStyleBackColor = True
        '
        'cbIntercept
        '
        Me.cbIntercept.AutoSize = True
        Me.cbIntercept.Checked = True
        Me.cbIntercept.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbIntercept.Location = New System.Drawing.Point(384, 277)
        Me.cbIntercept.Name = "cbIntercept"
        Me.cbIntercept.Size = New System.Drawing.Size(80, 20)
        Me.cbIntercept.TabIndex = 11
        Me.cbIntercept.Text = "Intercept"
        Me.cbIntercept.UseVisualStyleBackColor = True
        '
        'btAddEffect
        '
        Me.btAddEffect.Location = New System.Drawing.Point(384, 55)
        Me.btAddEffect.Name = "btAddEffect"
        Me.btAddEffect.Size = New System.Drawing.Size(75, 23)
        Me.btAddEffect.TabIndex = 10
        Me.btAddEffect.Text = "Add >>"
        Me.btAddEffect.UseVisualStyleBackColor = True
        '
        'btClearAllSelectedEffects
        '
        Me.btClearAllSelectedEffects.AutoEllipsis = True
        Me.btClearAllSelectedEffects.Location = New System.Drawing.Point(726, 428)
        Me.btClearAllSelectedEffects.Name = "btClearAllSelectedEffects"
        Me.btClearAllSelectedEffects.Size = New System.Drawing.Size(94, 23)
        Me.btClearAllSelectedEffects.TabIndex = 9
        Me.btClearAllSelectedEffects.Text = "Clear All"
        Me.btClearAllSelectedEffects.UseVisualStyleBackColor = True
        '
        'tbRemoveSelectedEffects
        '
        Me.tbRemoveSelectedEffects.AutoEllipsis = True
        Me.tbRemoveSelectedEffects.Location = New System.Drawing.Point(562, 429)
        Me.tbRemoveSelectedEffects.Name = "tbRemoveSelectedEffects"
        Me.tbRemoveSelectedEffects.Size = New System.Drawing.Size(91, 23)
        Me.tbRemoveSelectedEffects.TabIndex = 8
        Me.tbRemoveSelectedEffects.Text = "Remove"
        Me.tbRemoveSelectedEffects.UseVisualStyleBackColor = True
        '
        'lbSelectedEffectsList
        '
        Me.lbSelectedEffectsList.FormattingEnabled = True
        Me.lbSelectedEffectsList.ItemHeight = 16
        Me.lbSelectedEffectsList.Location = New System.Drawing.Point(548, 31)
        Me.lbSelectedEffectsList.Name = "lbSelectedEffectsList"
        Me.lbSelectedEffectsList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedEffectsList.Size = New System.Drawing.Size(282, 388)
        Me.lbSelectedEffectsList.TabIndex = 4
        '
        'lbSelectedVariables
        '
        Me.lbSelectedVariables.FormattingEnabled = True
        Me.lbSelectedVariables.ItemHeight = 16
        Me.lbSelectedVariables.Location = New System.Drawing.Point(5, 31)
        Me.lbSelectedVariables.Name = "lbSelectedVariables"
        Me.lbSelectedVariables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedVariables.Size = New System.Drawing.Size(291, 420)
        Me.lbSelectedVariables.TabIndex = 2
        '
        'lblSelectedEffectsList
        '
        Me.lblSelectedEffectsList.AutoSize = True
        Me.lblSelectedEffectsList.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedEffectsList.Location = New System.Drawing.Point(559, 12)
        Me.lblSelectedEffectsList.Name = "lblSelectedEffectsList"
        Me.lblSelectedEffectsList.Size = New System.Drawing.Size(163, 16)
        Me.lblSelectedEffectsList.TabIndex = 5
        Me.lblSelectedEffectsList.Text = "Selected Fixed-Effects"
        '
        'lblSelectedVariables
        '
        Me.lblSelectedVariables.AutoSize = True
        Me.lblSelectedVariables.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedVariables.Location = New System.Drawing.Point(15, 12)
        Me.lblSelectedVariables.Name = "lblSelectedVariables"
        Me.lblSelectedVariables.Size = New System.Drawing.Size(140, 16)
        Me.lblSelectedVariables.TabIndex = 3
        Me.lblSelectedVariables.Text = "Selected Variables"
        '
        'TabPageOptions
        '
        Me.TabPageOptions.Controls.Add(Me.grpMMRMRpostestOutputs)
        Me.TabPageOptions.Controls.Add(Me.grpMMRMReferenceGrid)
        Me.TabPageOptions.Controls.Add(Me.grpMMRMContrasts)
        Me.TabPageOptions.Controls.Add(Me.grpModelSpecification)
        Me.TabPageOptions.Controls.Add(Me.lblAlpha)
        Me.TabPageOptions.Controls.Add(Me.spinBtnAlpha)
        Me.TabPageOptions.Controls.Add(Me.ckResiduals)
        Me.TabPageOptions.Controls.Add(Me.grpIterOptions)
        Me.TabPageOptions.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptions.Name = "TabPageOptions"
        Me.TabPageOptions.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptions.TabIndex = 2
        Me.TabPageOptions.Text = "Options"
        Me.TabPageOptions.UseVisualStyleBackColor = True
        '
        'grpMMRMRpostestOutputs
        '
        Me.grpMMRMRpostestOutputs.Controls.Add(Me.ckMMRMEstimatedMeans)
        Me.grpMMRMRpostestOutputs.Controls.Add(Me.ckMMRMClassInfo)
        Me.grpMMRMRpostestOutputs.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpMMRMRpostestOutputs.Location = New System.Drawing.Point(387, 204)
        Me.grpMMRMRpostestOutputs.Name = "grpMMRMRpostestOutputs"
        Me.grpMMRMRpostestOutputs.Size = New System.Drawing.Size(441, 72)
        Me.grpMMRMRpostestOutputs.TabIndex = 21
        Me.grpMMRMRpostestOutputs.TabStop = False
        Me.grpMMRMRpostestOutputs.Text = "Post-estimation output"
        '
        'ckMMRMEstimatedMeans
        '
        Me.ckMMRMEstimatedMeans.AutoSize = True
        Me.ckMMRMEstimatedMeans.Checked = True
        Me.ckMMRMEstimatedMeans.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMMRMEstimatedMeans.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMMRMEstimatedMeans.Location = New System.Drawing.Point(13, 47)
        Me.ckMMRMEstimatedMeans.Name = "ckMMRMEstimatedMeans"
        Me.ckMMRMEstimatedMeans.Size = New System.Drawing.Size(294, 20)
        Me.ckMMRMEstimatedMeans.TabIndex = 21
        Me.ckMMRMEstimatedMeans.Text = "Show LS-means / estimated marginal means"
        Me.ckMMRMEstimatedMeans.UseVisualStyleBackColor = True
        '
        'ckMMRMClassInfo
        '
        Me.ckMMRMClassInfo.AutoSize = True
        Me.ckMMRMClassInfo.Checked = True
        Me.ckMMRMClassInfo.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMMRMClassInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMMRMClassInfo.Location = New System.Drawing.Point(13, 21)
        Me.ckMMRMClassInfo.Name = "ckMMRMClassInfo"
        Me.ckMMRMClassInfo.Size = New System.Drawing.Size(198, 20)
        Me.ckMMRMClassInfo.TabIndex = 20
        Me.ckMMRMClassInfo.Text = "Show class-level information"
        Me.ckMMRMClassInfo.UseVisualStyleBackColor = True
        '
        'grpMMRMReferenceGrid
        '
        Me.grpMMRMReferenceGrid.Controls.Add(Me.lblMMRMRefGridWeighting)
        Me.grpMMRMReferenceGrid.Controls.Add(Me.cbMMRMRefGridCovariates)
        Me.grpMMRMReferenceGrid.Controls.Add(Me.lblMMRMRefGridCovariates)
        Me.grpMMRMReferenceGrid.Controls.Add(Me.cbMMRMLSMeansMode)
        Me.grpMMRMReferenceGrid.Controls.Add(Me.cbMMRMRefGridWeighting)
        Me.grpMMRMReferenceGrid.Controls.Add(Me.lblMMRMLSMeansMode)
        Me.grpMMRMReferenceGrid.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpMMRMReferenceGrid.Location = New System.Drawing.Point(387, 285)
        Me.grpMMRMReferenceGrid.Name = "grpMMRMReferenceGrid"
        Me.grpMMRMReferenceGrid.Size = New System.Drawing.Size(441, 116)
        Me.grpMMRMReferenceGrid.TabIndex = 18
        Me.grpMMRMReferenceGrid.TabStop = False
        Me.grpMMRMReferenceGrid.Text = "How LS-means are averaged"
        '
        'lblMMRMRefGridWeighting
        '
        Me.lblMMRMRefGridWeighting.AutoSize = True
        Me.lblMMRMRefGridWeighting.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMRefGridWeighting.Location = New System.Drawing.Point(6, 60)
        Me.lblMMRMRefGridWeighting.Name = "lblMMRMRefGridWeighting"
        Me.lblMMRMRefGridWeighting.Size = New System.Drawing.Size(125, 16)
        Me.lblMMRMRefGridWeighting.TabIndex = 20
        Me.lblMMRMRefGridWeighting.Text = "Class-cell weighting"
        '
        'cbMMRMRefGridCovariates
        '
        Me.cbMMRMRefGridCovariates.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMRefGridCovariates.FormattingEnabled = True
        Me.cbMMRMRefGridCovariates.Location = New System.Drawing.Point(151, 85)
        Me.cbMMRMRefGridCovariates.Name = "cbMMRMRefGridCovariates"
        Me.cbMMRMRefGridCovariates.Size = New System.Drawing.Size(284, 24)
        Me.cbMMRMRefGridCovariates.TabIndex = 16
        '
        'lblMMRMRefGridCovariates
        '
        Me.lblMMRMRefGridCovariates.AutoSize = True
        Me.lblMMRMRefGridCovariates.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMRefGridCovariates.Location = New System.Drawing.Point(6, 88)
        Me.lblMMRMRefGridCovariates.Name = "lblMMRMRefGridCovariates"
        Me.lblMMRMRefGridCovariates.Size = New System.Drawing.Size(139, 16)
        Me.lblMMRMRefGridCovariates.TabIndex = 17
        Me.lblMMRMRefGridCovariates.Text = "Continuous covariates"
        '
        'cbMMRMLSMeansMode
        '
        Me.cbMMRMLSMeansMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMLSMeansMode.FormattingEnabled = True
        Me.cbMMRMLSMeansMode.Location = New System.Drawing.Point(151, 28)
        Me.cbMMRMLSMeansMode.Name = "cbMMRMLSMeansMode"
        Me.cbMMRMLSMeansMode.Size = New System.Drawing.Size(284, 24)
        Me.cbMMRMLSMeansMode.TabIndex = 2
        '
        'cbMMRMRefGridWeighting
        '
        Me.cbMMRMRefGridWeighting.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMRefGridWeighting.FormattingEnabled = True
        Me.cbMMRMRefGridWeighting.Location = New System.Drawing.Point(151, 57)
        Me.cbMMRMRefGridWeighting.Name = "cbMMRMRefGridWeighting"
        Me.cbMMRMRefGridWeighting.Size = New System.Drawing.Size(284, 24)
        Me.cbMMRMRefGridWeighting.TabIndex = 14
        '
        'lblMMRMLSMeansMode
        '
        Me.lblMMRMLSMeansMode.AutoSize = True
        Me.lblMMRMLSMeansMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMLSMeansMode.Location = New System.Drawing.Point(6, 31)
        Me.lblMMRMLSMeansMode.Name = "lblMMRMLSMeansMode"
        Me.lblMMRMLSMeansMode.Size = New System.Drawing.Size(135, 16)
        Me.lblMMRMLSMeansMode.TabIndex = 2
        Me.lblMMRMLSMeansMode.Text = "LS-means calculation"
        '
        'grpMMRMContrasts
        '
        Me.grpMMRMContrasts.Controls.Add(Me.cbMMRMComparisonLevel)
        Me.grpMMRMContrasts.Controls.Add(Me.lblMMRMMultiplicity)
        Me.grpMMRMContrasts.Controls.Add(Me.lblMMRMComparisonLevel)
        Me.grpMMRMContrasts.Controls.Add(Me.cbMMRMMultiplicity)
        Me.grpMMRMContrasts.Controls.Add(Me.ckMMRMDiffInChange)
        Me.grpMMRMContrasts.Controls.Add(Me.ckMMRMChangeFromBaseline)
        Me.grpMMRMContrasts.Controls.Add(Me.cbMMRMContrastDirection)
        Me.grpMMRMContrasts.Controls.Add(Me.lblMMRMContrastDirection)
        Me.grpMMRMContrasts.Controls.Add(Me.cbMMRMControlLevel)
        Me.grpMMRMContrasts.Controls.Add(Me.lblMMRMControlLevel)
        Me.grpMMRMContrasts.Controls.Add(Me.cbMMRMContrastMode)
        Me.grpMMRMContrasts.Controls.Add(Me.lblMMRMContrastMode)
        Me.grpMMRMContrasts.Controls.Add(Me.cbMMRMBaselineVisit)
        Me.grpMMRMContrasts.Controls.Add(Me.lblMMRMBaselineVisit)
        Me.grpMMRMContrasts.Controls.Add(Me.cbMMRMGroupingFactor)
        Me.grpMMRMContrasts.Controls.Add(Me.lblMMRMGroupingFactor)
        Me.grpMMRMContrasts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpMMRMContrasts.Location = New System.Drawing.Point(6, 184)
        Me.grpMMRMContrasts.Name = "grpMMRMContrasts"
        Me.grpMMRMContrasts.Size = New System.Drawing.Size(368, 278)
        Me.grpMMRMContrasts.TabIndex = 17
        Me.grpMMRMContrasts.TabStop = False
        Me.grpMMRMContrasts.Text = "Post-estimation group comparisons"
        '
        'cbMMRMComparisonLevel
        '
        Me.cbMMRMComparisonLevel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMComparisonLevel.FormattingEnabled = True
        Me.cbMMRMComparisonLevel.Location = New System.Drawing.Point(170, 98)
        Me.cbMMRMComparisonLevel.Name = "cbMMRMComparisonLevel"
        Me.cbMMRMComparisonLevel.Size = New System.Drawing.Size(194, 24)
        Me.cbMMRMComparisonLevel.TabIndex = 30
        '
        'lblMMRMMultiplicity
        '
        Me.lblMMRMMultiplicity.AutoSize = True
        Me.lblMMRMMultiplicity.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMMultiplicity.Location = New System.Drawing.Point(94, 153)
        Me.lblMMRMMultiplicity.Name = "lblMMRMMultiplicity"
        Me.lblMMRMMultiplicity.Size = New System.Drawing.Size(68, 16)
        Me.lblMMRMMultiplicity.TabIndex = 19
        Me.lblMMRMMultiplicity.Text = "Multiplicity"
        '
        'lblMMRMComparisonLevel
        '
        Me.lblMMRMComparisonLevel.AutoSize = True
        Me.lblMMRMComparisonLevel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMComparisonLevel.Location = New System.Drawing.Point(50, 101)
        Me.lblMMRMComparisonLevel.Name = "lblMMRMComparisonLevel"
        Me.lblMMRMComparisonLevel.Size = New System.Drawing.Size(112, 16)
        Me.lblMMRMComparisonLevel.TabIndex = 31
        Me.lblMMRMComparisonLevel.Text = "Comparison level"
        '
        'cbMMRMMultiplicity
        '
        Me.cbMMRMMultiplicity.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMMultiplicity.FormattingEnabled = True
        Me.cbMMRMMultiplicity.Location = New System.Drawing.Point(170, 150)
        Me.cbMMRMMultiplicity.Name = "cbMMRMMultiplicity"
        Me.cbMMRMMultiplicity.Size = New System.Drawing.Size(194, 24)
        Me.cbMMRMMultiplicity.TabIndex = 18
        '
        'ckMMRMDiffInChange
        '
        Me.ckMMRMDiffInChange.AutoSize = True
        Me.ckMMRMDiffInChange.Checked = True
        Me.ckMMRMDiffInChange.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMMRMDiffInChange.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMMRMDiffInChange.Location = New System.Drawing.Point(17, 248)
        Me.ckMMRMDiffInChange.Name = "ckMMRMDiffInChange"
        Me.ckMMRMDiffInChange.Size = New System.Drawing.Size(223, 20)
        Me.ckMMRMDiffInChange.TabIndex = 29
        Me.ckMMRMDiffInChange.Text = "Show group difference in change"
        Me.ckMMRMDiffInChange.UseVisualStyleBackColor = True
        '
        'ckMMRMChangeFromBaseline
        '
        Me.ckMMRMChangeFromBaseline.AutoSize = True
        Me.ckMMRMChangeFromBaseline.Checked = True
        Me.ckMMRMChangeFromBaseline.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMMRMChangeFromBaseline.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMMRMChangeFromBaseline.Location = New System.Drawing.Point(17, 222)
        Me.ckMMRMChangeFromBaseline.Name = "ckMMRMChangeFromBaseline"
        Me.ckMMRMChangeFromBaseline.Size = New System.Drawing.Size(194, 20)
        Me.ckMMRMChangeFromBaseline.TabIndex = 28
        Me.ckMMRMChangeFromBaseline.Text = "Show change from baseline"
        Me.ckMMRMChangeFromBaseline.UseVisualStyleBackColor = True
        '
        'cbMMRMContrastDirection
        '
        Me.cbMMRMContrastDirection.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMContrastDirection.FormattingEnabled = True
        Me.cbMMRMContrastDirection.Location = New System.Drawing.Point(170, 124)
        Me.cbMMRMContrastDirection.Name = "cbMMRMContrastDirection"
        Me.cbMMRMContrastDirection.Size = New System.Drawing.Size(194, 24)
        Me.cbMMRMContrastDirection.TabIndex = 26
        '
        'lblMMRMContrastDirection
        '
        Me.lblMMRMContrastDirection.AutoSize = True
        Me.lblMMRMContrastDirection.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMContrastDirection.Location = New System.Drawing.Point(42, 127)
        Me.lblMMRMContrastDirection.Name = "lblMMRMContrastDirection"
        Me.lblMMRMContrastDirection.Size = New System.Drawing.Size(122, 16)
        Me.lblMMRMContrastDirection.TabIndex = 27
        Me.lblMMRMContrastDirection.Text = "Difference direction"
        '
        'cbMMRMControlLevel
        '
        Me.cbMMRMControlLevel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMControlLevel.FormattingEnabled = True
        Me.cbMMRMControlLevel.Location = New System.Drawing.Point(170, 73)
        Me.cbMMRMControlLevel.Name = "cbMMRMControlLevel"
        Me.cbMMRMControlLevel.Size = New System.Drawing.Size(194, 24)
        Me.cbMMRMControlLevel.TabIndex = 24
        '
        'lblMMRMControlLevel
        '
        Me.lblMMRMControlLevel.AutoSize = True
        Me.lblMMRMControlLevel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMControlLevel.Location = New System.Drawing.Point(16, 76)
        Me.lblMMRMControlLevel.Name = "lblMMRMControlLevel"
        Me.lblMMRMControlLevel.Size = New System.Drawing.Size(146, 16)
        Me.lblMMRMControlLevel.TabIndex = 25
        Me.lblMMRMControlLevel.Text = "Reference/control level"
        '
        'cbMMRMContrastMode
        '
        Me.cbMMRMContrastMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMContrastMode.FormattingEnabled = True
        Me.cbMMRMContrastMode.Location = New System.Drawing.Point(170, 48)
        Me.cbMMRMContrastMode.Name = "cbMMRMContrastMode"
        Me.cbMMRMContrastMode.Size = New System.Drawing.Size(194, 24)
        Me.cbMMRMContrastMode.TabIndex = 22
        '
        'lblMMRMContrastMode
        '
        Me.lblMMRMContrastMode.AutoSize = True
        Me.lblMMRMContrastMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMContrastMode.Location = New System.Drawing.Point(53, 51)
        Me.lblMMRMContrastMode.Name = "lblMMRMContrastMode"
        Me.lblMMRMContrastMode.Size = New System.Drawing.Size(109, 16)
        Me.lblMMRMContrastMode.TabIndex = 23
        Me.lblMMRMContrastMode.Text = "Comparison type"
        '
        'cbMMRMBaselineVisit
        '
        Me.cbMMRMBaselineVisit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMBaselineVisit.FormattingEnabled = True
        Me.cbMMRMBaselineVisit.Location = New System.Drawing.Point(170, 192)
        Me.cbMMRMBaselineVisit.Name = "cbMMRMBaselineVisit"
        Me.cbMMRMBaselineVisit.Size = New System.Drawing.Size(194, 24)
        Me.cbMMRMBaselineVisit.TabIndex = 20
        '
        'lblMMRMBaselineVisit
        '
        Me.lblMMRMBaselineVisit.AutoSize = True
        Me.lblMMRMBaselineVisit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMBaselineVisit.Location = New System.Drawing.Point(8, 195)
        Me.lblMMRMBaselineVisit.Name = "lblMMRMBaselineVisit"
        Me.lblMMRMBaselineVisit.Size = New System.Drawing.Size(152, 16)
        Me.lblMMRMBaselineVisit.TabIndex = 21
        Me.lblMMRMBaselineVisit.Text = "Baseline visit for change"
        '
        'cbMMRMGroupingFactor
        '
        Me.cbMMRMGroupingFactor.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMGroupingFactor.FormattingEnabled = True
        Me.cbMMRMGroupingFactor.Location = New System.Drawing.Point(170, 23)
        Me.cbMMRMGroupingFactor.Name = "cbMMRMGroupingFactor"
        Me.cbMMRMGroupingFactor.Size = New System.Drawing.Size(194, 24)
        Me.cbMMRMGroupingFactor.TabIndex = 16
        '
        'lblMMRMGroupingFactor
        '
        Me.lblMMRMGroupingFactor.AutoSize = True
        Me.lblMMRMGroupingFactor.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMGroupingFactor.Location = New System.Drawing.Point(36, 26)
        Me.lblMMRMGroupingFactor.Name = "lblMMRMGroupingFactor"
        Me.lblMMRMGroupingFactor.Size = New System.Drawing.Size(126, 16)
        Me.lblMMRMGroupingFactor.TabIndex = 17
        Me.lblMMRMGroupingFactor.Text = "Compare groups by"
        '
        'grpModelSpecification
        '
        Me.grpModelSpecification.Controls.Add(Me.cbInferenceMethod)
        Me.grpModelSpecification.Controls.Add(Me.lblInferenceMethod)
        Me.grpModelSpecification.Controls.Add(Me.cbFitMethod)
        Me.grpModelSpecification.Controls.Add(Me.cbCovarStruct)
        Me.grpModelSpecification.Controls.Add(Me.lblCovarStruct)
        Me.grpModelSpecification.Controls.Add(Me.lblFitMethod)
        Me.grpModelSpecification.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpModelSpecification.Location = New System.Drawing.Point(387, 16)
        Me.grpModelSpecification.Name = "grpModelSpecification"
        Me.grpModelSpecification.Size = New System.Drawing.Size(441, 119)
        Me.grpModelSpecification.TabIndex = 16
        Me.grpModelSpecification.TabStop = False
        Me.grpModelSpecification.Text = "Model Specification"
        '
        'cbInferenceMethod
        '
        Me.cbInferenceMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbInferenceMethod.FormattingEnabled = True
        Me.cbInferenceMethod.Location = New System.Drawing.Point(203, 89)
        Me.cbInferenceMethod.Name = "cbInferenceMethod"
        Me.cbInferenceMethod.Size = New System.Drawing.Size(232, 24)
        Me.cbInferenceMethod.TabIndex = 16
        '
        'lblInferenceMethod
        '
        Me.lblInferenceMethod.AutoSize = True
        Me.lblInferenceMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblInferenceMethod.Location = New System.Drawing.Point(87, 92)
        Me.lblInferenceMethod.Name = "lblInferenceMethod"
        Me.lblInferenceMethod.Size = New System.Drawing.Size(110, 16)
        Me.lblInferenceMethod.TabIndex = 17
        Me.lblInferenceMethod.Text = "Inference Method"
        '
        'cbFitMethod
        '
        Me.cbFitMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFitMethod.FormattingEnabled = True
        Me.cbFitMethod.Location = New System.Drawing.Point(203, 25)
        Me.cbFitMethod.Name = "cbFitMethod"
        Me.cbFitMethod.Size = New System.Drawing.Size(232, 24)
        Me.cbFitMethod.TabIndex = 2
        '
        'cbCovarStruct
        '
        Me.cbCovarStruct.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbCovarStruct.FormattingEnabled = True
        Me.cbCovarStruct.Location = New System.Drawing.Point(203, 57)
        Me.cbCovarStruct.Name = "cbCovarStruct"
        Me.cbCovarStruct.Size = New System.Drawing.Size(232, 24)
        Me.cbCovarStruct.TabIndex = 14
        '
        'lblCovarStruct
        '
        Me.lblCovarStruct.AutoSize = True
        Me.lblCovarStruct.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCovarStruct.Location = New System.Drawing.Point(10, 60)
        Me.lblCovarStruct.Name = "lblCovarStruct"
        Me.lblCovarStruct.Size = New System.Drawing.Size(188, 16)
        Me.lblCovarStruct.TabIndex = 15
        Me.lblCovarStruct.Text = "Residual Covariance Structure"
        '
        'lblFitMethod
        '
        Me.lblFitMethod.AutoSize = True
        Me.lblFitMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFitMethod.Location = New System.Drawing.Point(128, 28)
        Me.lblFitMethod.Name = "lblFitMethod"
        Me.lblFitMethod.Size = New System.Drawing.Size(69, 16)
        Me.lblFitMethod.TabIndex = 2
        Me.lblFitMethod.Text = "Fit Method"
        '
        'lblAlpha
        '
        Me.lblAlpha.AutoSize = True
        Me.lblAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlpha.Location = New System.Drawing.Point(384, 169)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(41, 16)
        Me.lblAlpha.TabIndex = 13
        Me.lblAlpha.Text = "alpha"
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(432, 167)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 12
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'ckResiduals
        '
        Me.ckResiduals.AutoSize = True
        Me.ckResiduals.Location = New System.Drawing.Point(387, 141)
        Me.ckResiduals.Name = "ckResiduals"
        Me.ckResiduals.Size = New System.Drawing.Size(147, 20)
        Me.ckResiduals.TabIndex = 3
        Me.ckResiduals.Text = "Compute Residuals"
        Me.ckResiduals.UseVisualStyleBackColor = True
        '
        'grpIterOptions
        '
        Me.grpIterOptions.Controls.Add(Me.cbMMRMCovOptimizerMode)
        Me.grpIterOptions.Controls.Add(Me.lblMMRMCovGradientMode)
        Me.grpIterOptions.Controls.Add(Me.cbMMRMCovGradientMode)
        Me.grpIterOptions.Controls.Add(Me.lblMMRMCovOptimizerMode)
        Me.grpIterOptions.Controls.Add(Me.cbDiagnostic)
        Me.grpIterOptions.Controls.Add(Me.ckTrace)
        Me.grpIterOptions.Controls.Add(Me.ckIterationsDetails)
        Me.grpIterOptions.Controls.Add(Me.tbMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblEps)
        Me.grpIterOptions.Controls.Add(Me.tbEps)
        Me.grpIterOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpIterOptions.Location = New System.Drawing.Point(6, 16)
        Me.grpIterOptions.Name = "grpIterOptions"
        Me.grpIterOptions.Size = New System.Drawing.Size(368, 162)
        Me.grpIterOptions.TabIndex = 0
        Me.grpIterOptions.TabStop = False
        Me.grpIterOptions.Text = "Convergence Options"
        '
        'cbDiagnostic
        '
        Me.cbDiagnostic.AutoSize = True
        Me.cbDiagnostic.Checked = True
        Me.cbDiagnostic.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbDiagnostic.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDiagnostic.Location = New System.Drawing.Point(274, 76)
        Me.cbDiagnostic.Name = "cbDiagnostic"
        Me.cbDiagnostic.Size = New System.Drawing.Size(93, 20)
        Me.cbDiagnostic.TabIndex = 6
        Me.cbDiagnostic.Text = "Diagnostic"
        Me.cbDiagnostic.UseVisualStyleBackColor = True
        '
        'ckTrace
        '
        Me.ckTrace.AutoSize = True
        Me.ckTrace.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckTrace.Location = New System.Drawing.Point(145, 76)
        Me.ckTrace.Name = "ckTrace"
        Me.ckTrace.Size = New System.Drawing.Size(126, 20)
        Me.ckTrace.TabIndex = 5
        Me.ckTrace.Text = "Trace Execution"
        Me.ckTrace.UseVisualStyleBackColor = True
        '
        'ckIterationsDetails
        '
        Me.ckIterationsDetails.AutoSize = True
        Me.ckIterationsDetails.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckIterationsDetails.Location = New System.Drawing.Point(11, 76)
        Me.ckIterationsDetails.Name = "ckIterationsDetails"
        Me.ckIterationsDetails.Size = New System.Drawing.Size(128, 20)
        Me.ckIterationsDetails.TabIndex = 4
        Me.ckIterationsDetails.Text = "Iterations Details"
        Me.ckIterationsDetails.UseVisualStyleBackColor = True
        '
        'tbMaxIter
        '
        Me.tbMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMaxIter.Location = New System.Drawing.Point(170, 45)
        Me.tbMaxIter.Name = "tbMaxIter"
        Me.tbMaxIter.Size = New System.Drawing.Size(125, 22)
        Me.tbMaxIter.TabIndex = 3
        Me.tbMaxIter.Text = "500"
        '
        'lblMaxIter
        '
        Me.lblMaxIter.AutoSize = True
        Me.lblMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMaxIter.Location = New System.Drawing.Point(68, 48)
        Me.lblMaxIter.Name = "lblMaxIter"
        Me.lblMaxIter.Size = New System.Drawing.Size(92, 16)
        Me.lblMaxIter.TabIndex = 2
        Me.lblMaxIter.Text = "Max. Iterations"
        '
        'lblEps
        '
        Me.lblEps.AutoSize = True
        Me.lblEps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEps.Location = New System.Drawing.Point(20, 25)
        Me.lblEps.Name = "lblEps"
        Me.lblEps.Size = New System.Drawing.Size(140, 16)
        Me.lblEps.TabIndex = 1
        Me.lblEps.Text = "Convergence Criterion"
        '
        'tbEps
        '
        Me.tbEps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbEps.Location = New System.Drawing.Point(170, 19)
        Me.tbEps.Name = "tbEps"
        Me.tbEps.Size = New System.Drawing.Size(125, 22)
        Me.tbEps.TabIndex = 1
        Me.tbEps.Text = "0.000001"
        '
        'btInterrupt
        '
        Me.btInterrupt.Location = New System.Drawing.Point(602, 529)
        Me.btInterrupt.Name = "btInterrupt"
        Me.btInterrupt.Size = New System.Drawing.Size(75, 23)
        Me.btInterrupt.TabIndex = 17
        Me.btInterrupt.Text = "Interrupt"
        Me.btInterrupt.UseVisualStyleBackColor = True
        '
        'cbMMRMCovGradientMode
        '
        Me.cbMMRMCovGradientMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMCovGradientMode.FormattingEnabled = True
        Me.cbMMRMCovGradientMode.Location = New System.Drawing.Point(108, 132)
        Me.cbMMRMCovGradientMode.Name = "cbMMRMCovGradientMode"
        Me.cbMMRMCovGradientMode.Size = New System.Drawing.Size(256, 24)
        Me.cbMMRMCovGradientMode.TabIndex = 18
        '
        'lblMMRMCovOptimizerMode
        '
        Me.lblMMRMCovOptimizerMode.AutoSize = True
        Me.lblMMRMCovOptimizerMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMCovOptimizerMode.Location = New System.Drawing.Point(1, 106)
        Me.lblMMRMCovOptimizerMode.Name = "lblMMRMCovOptimizerMode"
        Me.lblMMRMCovOptimizerMode.Size = New System.Drawing.Size(101, 16)
        Me.lblMMRMCovOptimizerMode.TabIndex = 19
        Me.lblMMRMCovOptimizerMode.Text = "Optimizer mode"
        '
        'cbMMRMCovOptimizerMode
        '
        Me.cbMMRMCovOptimizerMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMMRMCovOptimizerMode.FormattingEnabled = True
        Me.cbMMRMCovOptimizerMode.Location = New System.Drawing.Point(108, 103)
        Me.cbMMRMCovOptimizerMode.Name = "cbMMRMCovOptimizerMode"
        Me.cbMMRMCovOptimizerMode.Size = New System.Drawing.Size(256, 24)
        Me.cbMMRMCovOptimizerMode.TabIndex = 20
        '
        'lblMMRMCovGradientMode
        '
        Me.lblMMRMCovGradientMode.AutoSize = True
        Me.lblMMRMCovGradientMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMMRMCovGradientMode.Location = New System.Drawing.Point(1, 135)
        Me.lblMMRMCovGradientMode.Name = "lblMMRMCovGradientMode"
        Me.lblMMRMCovGradientMode.Size = New System.Drawing.Size(96, 16)
        Me.lblMMRMCovGradientMode.TabIndex = 21
        Me.lblMMRMCovGradientMode.Text = "Gradient mode"
        '
        'Ui18MMRM
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(846, 562)
        Me.Controls.Add(Me.btInterrupt)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCalculate)
        Me.MinimumSize = New System.Drawing.Size(864, 608)
        Me.Name = "Ui18MMRM"
        Me.ShowIcon = False
        Me.Text = "Ui18MMRM"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPageBuildModel.ResumeLayout(False)
        Me.TabPageBuildModel.PerformLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageOptions.ResumeLayout(False)
        Me.TabPageOptions.PerformLayout()
        Me.grpMMRMRpostestOutputs.ResumeLayout(False)
        Me.grpMMRMRpostestOutputs.PerformLayout()
        Me.grpMMRMReferenceGrid.ResumeLayout(False)
        Me.grpMMRMReferenceGrid.PerformLayout()
        Me.grpMMRMContrasts.ResumeLayout(False)
        Me.grpMMRMContrasts.PerformLayout()
        Me.grpModelSpecification.ResumeLayout(False)
        Me.grpModelSpecification.PerformLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpIterOptions.ResumeLayout(False)
        Me.grpIterOptions.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ProgressBar1 As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCalculate As Windows.Forms.Button
    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents btRemoveTime As Windows.Forms.Button
    Friend WithEvents btAddTime As Windows.Forms.Button
    Friend WithEvents btRemoveClusterID As Windows.Forms.Button
    Friend WithEvents btAddClusterID As Windows.Forms.Button
    Friend WithEvents lbTime As Windows.Forms.ListBox
    Friend WithEvents lblTime As Windows.Forms.Label
    Friend WithEvents lbClusterID As Windows.Forms.ListBox
    Friend WithEvents lblClusterID As Windows.Forms.Label
    Friend WithEvents lbXs As Windows.Forms.ListBox
    Friend WithEvents lbY As Windows.Forms.ListBox
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents btRemoveX As Windows.Forms.Button
    Friend WithEvents btAddX As Windows.Forms.Button
    Friend WithEvents lblNote As Windows.Forms.Label
    Friend WithEvents lblY As Windows.Forms.Label
    Friend WithEvents btRemoveY As Windows.Forms.Button
    Friend WithEvents btAddY As Windows.Forms.Button
    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents lblX As Windows.Forms.Label
    Friend WithEvents lblSelectedSheet As Windows.Forms.Label
    Friend WithEvents TabPageBuildModel As Windows.Forms.TabPage
    Friend WithEvents btAddEffectCategoricalFactor As Windows.Forms.Button
    Friend WithEvents btnCustomInteraction As Windows.Forms.Button
    Friend WithEvents btn2Interactions As Windows.Forms.Button
    Friend WithEvents spinBtnPoly As Windows.Forms.NumericUpDown
    Friend WithEvents btnPoly As Windows.Forms.Button
    Friend WithEvents cbIntercept As Windows.Forms.CheckBox
    Friend WithEvents btAddEffect As Windows.Forms.Button
    Friend WithEvents btClearAllSelectedEffects As Windows.Forms.Button
    Friend WithEvents tbRemoveSelectedEffects As Windows.Forms.Button
    Friend WithEvents lbSelectedEffectsList As Windows.Forms.ListBox
    Friend WithEvents lbSelectedVariables As Windows.Forms.ListBox
    Friend WithEvents lblSelectedEffectsList As Windows.Forms.Label
    Friend WithEvents lblSelectedVariables As Windows.Forms.Label
    Friend WithEvents TabPageOptions As Windows.Forms.TabPage
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents ckResiduals As Windows.Forms.CheckBox
    Friend WithEvents grpIterOptions As Windows.Forms.GroupBox
    Friend WithEvents ckTrace As Windows.Forms.CheckBox
    Friend WithEvents ckIterationsDetails As Windows.Forms.CheckBox
    Friend WithEvents tbMaxIter As Windows.Forms.TextBox
    Friend WithEvents lblMaxIter As Windows.Forms.Label
    Friend WithEvents lblEps As Windows.Forms.Label
    Friend WithEvents tbEps As Windows.Forms.TextBox
    Friend WithEvents cbCovarStruct As Windows.Forms.ComboBox
    Friend WithEvents lblCovarStruct As Windows.Forms.Label
    Friend WithEvents grpModelSpecification As Windows.Forms.GroupBox
    Friend WithEvents cbFitMethod As Windows.Forms.ComboBox
    Friend WithEvents lblFitMethod As Windows.Forms.Label
    Friend WithEvents cbInferenceMethod As Windows.Forms.ComboBox
    Friend WithEvents lblInferenceMethod As Windows.Forms.Label
    Friend WithEvents grpMMRMContrasts As Windows.Forms.GroupBox
    Friend WithEvents cbMMRMBaselineVisit As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMBaselineVisit As Windows.Forms.Label
    Friend WithEvents cbMMRMGroupingFactor As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMGroupingFactor As Windows.Forms.Label
    Friend WithEvents ckMMRMChangeFromBaseline As Windows.Forms.CheckBox
    Friend WithEvents cbMMRMContrastDirection As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMContrastDirection As Windows.Forms.Label
    Friend WithEvents cbMMRMControlLevel As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMControlLevel As Windows.Forms.Label
    Friend WithEvents cbMMRMContrastMode As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMContrastMode As Windows.Forms.Label
    Friend WithEvents ckMMRMDiffInChange As Windows.Forms.CheckBox
    Friend WithEvents cbMMRMComparisonLevel As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMComparisonLevel As Windows.Forms.Label
    Friend WithEvents grpMMRMReferenceGrid As Windows.Forms.GroupBox
    Friend WithEvents cbMMRMRefGridCovariates As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMRefGridCovariates As Windows.Forms.Label
    Friend WithEvents cbMMRMLSMeansMode As Windows.Forms.ComboBox
    Friend WithEvents cbMMRMRefGridWeighting As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMLSMeansMode As Windows.Forms.Label
    Friend WithEvents lblMMRMRefGridWeighting As Windows.Forms.Label
    Friend WithEvents lblMMRMMultiplicity As Windows.Forms.Label
    Friend WithEvents cbMMRMMultiplicity As Windows.Forms.ComboBox
    Friend WithEvents cbDiagnostic As Windows.Forms.CheckBox
    Friend WithEvents btInterrupt As Windows.Forms.Button
    Friend WithEvents grpMMRMRpostestOutputs As Windows.Forms.GroupBox
    Friend WithEvents ckMMRMEstimatedMeans As Windows.Forms.CheckBox
    Friend WithEvents ckMMRMClassInfo As Windows.Forms.CheckBox
    Friend WithEvents cbMMRMCovOptimizerMode As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMCovGradientMode As Windows.Forms.Label
    Friend WithEvents cbMMRMCovGradientMode As Windows.Forms.ComboBox
    Friend WithEvents lblMMRMCovOptimizerMode As Windows.Forms.Label
End Class
