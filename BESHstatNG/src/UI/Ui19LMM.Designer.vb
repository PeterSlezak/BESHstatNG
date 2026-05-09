<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui19LMM
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
        Me.btInterrupt = New System.Windows.Forms.Button()
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
        Me.btAddRandomEffectCategoricalFactor = New System.Windows.Forms.Button()
        Me.btnRandomCustomInteraction = New System.Windows.Forms.Button()
        Me.btnRandom2Interactions = New System.Windows.Forms.Button()
        Me.spinBtnRandomPoly = New System.Windows.Forms.NumericUpDown()
        Me.btnRandomPoly = New System.Windows.Forms.Button()
        Me.cbRandomIntercept = New System.Windows.Forms.CheckBox()
        Me.btAddRandomEffect = New System.Windows.Forms.Button()
        Me.lbSelectedRandomEffectsList = New System.Windows.Forms.ListBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btAddFixedEffectCategoricalFactor = New System.Windows.Forms.Button()
        Me.btnFixedCustomInteraction = New System.Windows.Forms.Button()
        Me.btnFixed2Interactions = New System.Windows.Forms.Button()
        Me.spinBtnFixedPoly = New System.Windows.Forms.NumericUpDown()
        Me.btnFixedPoly = New System.Windows.Forms.Button()
        Me.cbFixedIntercept = New System.Windows.Forms.CheckBox()
        Me.btAddFixedEffect = New System.Windows.Forms.Button()
        Me.btClearAllSelectedFixedEffects = New System.Windows.Forms.Button()
        Me.tbRemoveSelectedFixedEffects = New System.Windows.Forms.Button()
        Me.lbSelectedFixedEffectsList = New System.Windows.Forms.ListBox()
        Me.lbSelectedVariables = New System.Windows.Forms.ListBox()
        Me.lblSelectedEffectsList = New System.Windows.Forms.Label()
        Me.lblSelectedVariables = New System.Windows.Forms.Label()
        Me.TabPageOptions = New System.Windows.Forms.TabPage()
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
        Me.cbLMMCovOptimizerMode = New System.Windows.Forms.ComboBox()
        Me.lblLMMCovGradientMode = New System.Windows.Forms.Label()
        Me.cbLMMCovGradientMode = New System.Windows.Forms.ComboBox()
        Me.lblLMMCovOptimizerMode = New System.Windows.Forms.Label()
        Me.cbDiagnostic = New System.Windows.Forms.CheckBox()
        Me.ckTrace = New System.Windows.Forms.CheckBox()
        Me.ckIterationsDetails = New System.Windows.Forms.CheckBox()
        Me.tbMaxIter = New System.Windows.Forms.TextBox()
        Me.lblMaxIter = New System.Windows.Forms.Label()
        Me.lblEps = New System.Windows.Forms.Label()
        Me.tbEps = New System.Windows.Forms.TextBox()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCalculate = New System.Windows.Forms.Button()
        Me.cbRandomCovarStruct = New System.Windows.Forms.ComboBox()
        Me.lblRandomCovarStruct = New System.Windows.Forms.Label()
        Me.btClearAllSelectedRandomEffects = New System.Windows.Forms.Button()
        Me.tbRemoveSelectedRandomEffects = New System.Windows.Forms.Button()
        Me.grpLMMOutputs = New System.Windows.Forms.GroupBox()
        Me.ckLMMGCovarianceMatrix = New System.Windows.Forms.CheckBox()
        Me.ckLMMRCovarianceMatrix = New System.Windows.Forms.CheckBox()
        Me.ckLMMRandomEffects = New System.Windows.Forms.CheckBox()
        Me.ckLMMClassInfo = New System.Windows.Forms.CheckBox()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPageBuildModel.SuspendLayout()
        CType(Me.spinBtnRandomPoly, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnFixedPoly, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageOptions.SuspendLayout()
        Me.grpModelSpecification.SuspendLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpIterOptions.SuspendLayout()
        Me.grpLMMOutputs.SuspendLayout()
        Me.SuspendLayout()
        '
        'btInterrupt
        '
        Me.btInterrupt.Location = New System.Drawing.Point(603, 530)
        Me.btInterrupt.Name = "btInterrupt"
        Me.btInterrupt.Size = New System.Drawing.Size(75, 23)
        Me.btInterrupt.TabIndex = 23
        Me.btInterrupt.Text = "Interrupt"
        Me.btInterrupt.UseVisualStyleBackColor = True
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPageBuildModel)
        Me.TabControl1.Controls.Add(Me.TabPageOptions)
        Me.TabControl1.Location = New System.Drawing.Point(3, 1)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(844, 494)
        Me.TabControl1.TabIndex = 22
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
        Me.lblTime.Size = New System.Drawing.Size(233, 16)
        Me.lblTime.TabIndex = 27
        Me.lblTime.Text = "Visit / Time / Ordering Variable**"
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
        Me.lblClusterID.Size = New System.Drawing.Size(78, 16)
        Me.lblClusterID.TabIndex = 25
        Me.lblClusterID.Text = "Subject ID"
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
        Me.lblNote.Location = New System.Drawing.Point(561, 412)
        Me.lblNote.Name = "lblNote"
        Me.lblNote.Size = New System.Drawing.Size(269, 46)
        Me.lblNote.TabIndex = 10
        Me.lblNote.Text = "* indicate mandatory fields" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "** conditionally required for visit-indexed " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "    re" &
    "sidual structures"
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
        Me.lblX.Size = New System.Drawing.Size(190, 16)
        Me.lblX.TabIndex = 18
        Me.lblX.Text = "Model Source Variable(s)*"
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
        Me.TabPageBuildModel.Controls.Add(Me.btClearAllSelectedRandomEffects)
        Me.TabPageBuildModel.Controls.Add(Me.tbRemoveSelectedRandomEffects)
        Me.TabPageBuildModel.Controls.Add(Me.btAddRandomEffectCategoricalFactor)
        Me.TabPageBuildModel.Controls.Add(Me.btnRandomCustomInteraction)
        Me.TabPageBuildModel.Controls.Add(Me.btnRandom2Interactions)
        Me.TabPageBuildModel.Controls.Add(Me.spinBtnRandomPoly)
        Me.TabPageBuildModel.Controls.Add(Me.btnRandomPoly)
        Me.TabPageBuildModel.Controls.Add(Me.cbRandomIntercept)
        Me.TabPageBuildModel.Controls.Add(Me.btAddRandomEffect)
        Me.TabPageBuildModel.Controls.Add(Me.lbSelectedRandomEffectsList)
        Me.TabPageBuildModel.Controls.Add(Me.Label1)
        Me.TabPageBuildModel.Controls.Add(Me.btAddFixedEffectCategoricalFactor)
        Me.TabPageBuildModel.Controls.Add(Me.btnFixedCustomInteraction)
        Me.TabPageBuildModel.Controls.Add(Me.btnFixed2Interactions)
        Me.TabPageBuildModel.Controls.Add(Me.spinBtnFixedPoly)
        Me.TabPageBuildModel.Controls.Add(Me.btnFixedPoly)
        Me.TabPageBuildModel.Controls.Add(Me.cbFixedIntercept)
        Me.TabPageBuildModel.Controls.Add(Me.btAddFixedEffect)
        Me.TabPageBuildModel.Controls.Add(Me.btClearAllSelectedFixedEffects)
        Me.TabPageBuildModel.Controls.Add(Me.tbRemoveSelectedFixedEffects)
        Me.TabPageBuildModel.Controls.Add(Me.lbSelectedFixedEffectsList)
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
        'btAddRandomEffectCategoricalFactor
        '
        Me.btAddRandomEffectCategoricalFactor.Location = New System.Drawing.Point(332, 285)
        Me.btAddRandomEffectCategoricalFactor.Name = "btAddRandomEffectCategoricalFactor"
        Me.btAddRandomEffectCategoricalFactor.Size = New System.Drawing.Size(191, 23)
        Me.btAddRandomEffectCategoricalFactor.TabIndex = 36
        Me.btAddRandomEffectCategoricalFactor.Text = "Add as Categorical Factor >>"
        Me.btAddRandomEffectCategoricalFactor.UseVisualStyleBackColor = True
        '
        'btnRandomCustomInteraction
        '
        Me.btnRandomCustomInteraction.Location = New System.Drawing.Point(332, 372)
        Me.btnRandomCustomInteraction.Name = "btnRandomCustomInteraction"
        Me.btnRandomCustomInteraction.Size = New System.Drawing.Size(191, 23)
        Me.btnRandomCustomInteraction.TabIndex = 35
        Me.btnRandomCustomInteraction.Text = "Custom Interaction >>"
        Me.btnRandomCustomInteraction.UseVisualStyleBackColor = True
        '
        'btnRandom2Interactions
        '
        Me.btnRandom2Interactions.Location = New System.Drawing.Point(332, 343)
        Me.btnRandom2Interactions.Name = "btnRandom2Interactions"
        Me.btnRandom2Interactions.Size = New System.Drawing.Size(191, 23)
        Me.btnRandom2Interactions.TabIndex = 34
        Me.btnRandom2Interactions.Text = "2-way Interactions >>"
        Me.btnRandom2Interactions.UseVisualStyleBackColor = True
        '
        'spinBtnRandomPoly
        '
        Me.spinBtnRandomPoly.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnRandomPoly.Location = New System.Drawing.Point(479, 314)
        Me.spinBtnRandomPoly.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnRandomPoly.Name = "spinBtnRandomPoly"
        Me.spinBtnRandomPoly.Size = New System.Drawing.Size(44, 22)
        Me.spinBtnRandomPoly.TabIndex = 33
        Me.spinBtnRandomPoly.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'btnRandomPoly
        '
        Me.btnRandomPoly.Location = New System.Drawing.Point(332, 314)
        Me.btnRandomPoly.Name = "btnRandomPoly"
        Me.btnRandomPoly.Size = New System.Drawing.Size(131, 23)
        Me.btnRandomPoly.TabIndex = 32
        Me.btnRandomPoly.Text = "Poly >>"
        Me.btnRandomPoly.UseVisualStyleBackColor = True
        '
        'cbRandomIntercept
        '
        Me.cbRandomIntercept.AutoSize = True
        Me.cbRandomIntercept.Checked = True
        Me.cbRandomIntercept.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbRandomIntercept.Location = New System.Drawing.Point(386, 401)
        Me.cbRandomIntercept.Name = "cbRandomIntercept"
        Me.cbRandomIntercept.Size = New System.Drawing.Size(80, 20)
        Me.cbRandomIntercept.TabIndex = 31
        Me.cbRandomIntercept.Text = "Intercept"
        Me.cbRandomIntercept.UseVisualStyleBackColor = True
        '
        'btAddRandomEffect
        '
        Me.btAddRandomEffect.Location = New System.Drawing.Point(386, 256)
        Me.btAddRandomEffect.Name = "btAddRandomEffect"
        Me.btAddRandomEffect.Size = New System.Drawing.Size(75, 23)
        Me.btAddRandomEffect.TabIndex = 30
        Me.btAddRandomEffect.Text = "Add >>"
        Me.btAddRandomEffect.UseVisualStyleBackColor = True
        '
        'lbSelectedRandomEffectsList
        '
        Me.lbSelectedRandomEffectsList.FormattingEnabled = True
        Me.lbSelectedRandomEffectsList.ItemHeight = 16
        Me.lbSelectedRandomEffectsList.Location = New System.Drawing.Point(548, 256)
        Me.lbSelectedRandomEffectsList.Name = "lbSelectedRandomEffectsList"
        Me.lbSelectedRandomEffectsList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedRandomEffectsList.Size = New System.Drawing.Size(282, 164)
        Me.lbSelectedRandomEffectsList.TabIndex = 29
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(559, 237)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(183, 16)
        Me.Label1.TabIndex = 28
        Me.Label1.Text = "Selected Random-Effects"
        '
        'btAddFixedEffectCategoricalFactor
        '
        Me.btAddFixedEffectCategoricalFactor.Location = New System.Drawing.Point(332, 60)
        Me.btAddFixedEffectCategoricalFactor.Name = "btAddFixedEffectCategoricalFactor"
        Me.btAddFixedEffectCategoricalFactor.Size = New System.Drawing.Size(191, 23)
        Me.btAddFixedEffectCategoricalFactor.TabIndex = 26
        Me.btAddFixedEffectCategoricalFactor.Text = "Add as Categorical Factor >>"
        Me.btAddFixedEffectCategoricalFactor.UseVisualStyleBackColor = True
        '
        'btnFixedCustomInteraction
        '
        Me.btnFixedCustomInteraction.Location = New System.Drawing.Point(332, 147)
        Me.btnFixedCustomInteraction.Name = "btnFixedCustomInteraction"
        Me.btnFixedCustomInteraction.Size = New System.Drawing.Size(191, 23)
        Me.btnFixedCustomInteraction.TabIndex = 25
        Me.btnFixedCustomInteraction.Text = "Custom Interaction >>"
        Me.btnFixedCustomInteraction.UseVisualStyleBackColor = True
        '
        'btnFixed2Interactions
        '
        Me.btnFixed2Interactions.Location = New System.Drawing.Point(332, 118)
        Me.btnFixed2Interactions.Name = "btnFixed2Interactions"
        Me.btnFixed2Interactions.Size = New System.Drawing.Size(191, 23)
        Me.btnFixed2Interactions.TabIndex = 24
        Me.btnFixed2Interactions.Text = "2-way Interactions >>"
        Me.btnFixed2Interactions.UseVisualStyleBackColor = True
        '
        'spinBtnFixedPoly
        '
        Me.spinBtnFixedPoly.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnFixedPoly.Location = New System.Drawing.Point(479, 89)
        Me.spinBtnFixedPoly.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnFixedPoly.Name = "spinBtnFixedPoly"
        Me.spinBtnFixedPoly.Size = New System.Drawing.Size(44, 22)
        Me.spinBtnFixedPoly.TabIndex = 23
        Me.spinBtnFixedPoly.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'btnFixedPoly
        '
        Me.btnFixedPoly.Location = New System.Drawing.Point(332, 89)
        Me.btnFixedPoly.Name = "btnFixedPoly"
        Me.btnFixedPoly.Size = New System.Drawing.Size(131, 23)
        Me.btnFixedPoly.TabIndex = 22
        Me.btnFixedPoly.Text = "Poly >>"
        Me.btnFixedPoly.UseVisualStyleBackColor = True
        '
        'cbFixedIntercept
        '
        Me.cbFixedIntercept.AutoSize = True
        Me.cbFixedIntercept.Checked = True
        Me.cbFixedIntercept.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbFixedIntercept.Location = New System.Drawing.Point(386, 176)
        Me.cbFixedIntercept.Name = "cbFixedIntercept"
        Me.cbFixedIntercept.Size = New System.Drawing.Size(80, 20)
        Me.cbFixedIntercept.TabIndex = 11
        Me.cbFixedIntercept.Text = "Intercept"
        Me.cbFixedIntercept.UseVisualStyleBackColor = True
        '
        'btAddFixedEffect
        '
        Me.btAddFixedEffect.Location = New System.Drawing.Point(386, 31)
        Me.btAddFixedEffect.Name = "btAddFixedEffect"
        Me.btAddFixedEffect.Size = New System.Drawing.Size(75, 23)
        Me.btAddFixedEffect.TabIndex = 10
        Me.btAddFixedEffect.Text = "Add >>"
        Me.btAddFixedEffect.UseVisualStyleBackColor = True
        '
        'btClearAllSelectedFixedEffects
        '
        Me.btClearAllSelectedFixedEffects.AutoEllipsis = True
        Me.btClearAllSelectedFixedEffects.Location = New System.Drawing.Point(726, 201)
        Me.btClearAllSelectedFixedEffects.Name = "btClearAllSelectedFixedEffects"
        Me.btClearAllSelectedFixedEffects.Size = New System.Drawing.Size(94, 23)
        Me.btClearAllSelectedFixedEffects.TabIndex = 9
        Me.btClearAllSelectedFixedEffects.Text = "Clear All"
        Me.btClearAllSelectedFixedEffects.UseVisualStyleBackColor = True
        '
        'tbRemoveSelectedFixedEffects
        '
        Me.tbRemoveSelectedFixedEffects.AutoEllipsis = True
        Me.tbRemoveSelectedFixedEffects.Location = New System.Drawing.Point(562, 202)
        Me.tbRemoveSelectedFixedEffects.Name = "tbRemoveSelectedFixedEffects"
        Me.tbRemoveSelectedFixedEffects.Size = New System.Drawing.Size(91, 23)
        Me.tbRemoveSelectedFixedEffects.TabIndex = 8
        Me.tbRemoveSelectedFixedEffects.Text = "Remove"
        Me.tbRemoveSelectedFixedEffects.UseVisualStyleBackColor = True
        '
        'lbSelectedFixedEffectsList
        '
        Me.lbSelectedFixedEffectsList.FormattingEnabled = True
        Me.lbSelectedFixedEffectsList.ItemHeight = 16
        Me.lbSelectedFixedEffectsList.Location = New System.Drawing.Point(548, 31)
        Me.lbSelectedFixedEffectsList.Name = "lbSelectedFixedEffectsList"
        Me.lbSelectedFixedEffectsList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedFixedEffectsList.Size = New System.Drawing.Size(282, 164)
        Me.lbSelectedFixedEffectsList.TabIndex = 4
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
        Me.TabPageOptions.Controls.Add(Me.grpLMMOutputs)
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
        'grpModelSpecification
        '
        Me.grpModelSpecification.Controls.Add(Me.cbInferenceMethod)
        Me.grpModelSpecification.Controls.Add(Me.lblRandomCovarStruct)
        Me.grpModelSpecification.Controls.Add(Me.cbRandomCovarStruct)
        Me.grpModelSpecification.Controls.Add(Me.lblInferenceMethod)
        Me.grpModelSpecification.Controls.Add(Me.cbFitMethod)
        Me.grpModelSpecification.Controls.Add(Me.cbCovarStruct)
        Me.grpModelSpecification.Controls.Add(Me.lblCovarStruct)
        Me.grpModelSpecification.Controls.Add(Me.lblFitMethod)
        Me.grpModelSpecification.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpModelSpecification.Location = New System.Drawing.Point(389, 16)
        Me.grpModelSpecification.Name = "grpModelSpecification"
        Me.grpModelSpecification.Size = New System.Drawing.Size(441, 171)
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
        Me.lblCovarStruct.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCovarStruct.Location = New System.Drawing.Point(9, 57)
        Me.lblCovarStruct.Name = "lblCovarStruct"
        Me.lblCovarStruct.Size = New System.Drawing.Size(188, 32)
        Me.lblCovarStruct.TabIndex = 15
        Me.lblCovarStruct.Text = "R-side Residual Covariance Structure"
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
        Me.lblAlpha.Location = New System.Drawing.Point(393, 223)
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
        Me.spinBtnAlpha.Location = New System.Drawing.Point(441, 221)
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
        Me.ckResiduals.Location = New System.Drawing.Point(396, 195)
        Me.ckResiduals.Name = "ckResiduals"
        Me.ckResiduals.Size = New System.Drawing.Size(147, 20)
        Me.ckResiduals.TabIndex = 3
        Me.ckResiduals.Text = "Compute Residuals"
        Me.ckResiduals.UseVisualStyleBackColor = True
        '
        'grpIterOptions
        '
        Me.grpIterOptions.Controls.Add(Me.cbLMMCovOptimizerMode)
        Me.grpIterOptions.Controls.Add(Me.lblLMMCovGradientMode)
        Me.grpIterOptions.Controls.Add(Me.cbLMMCovGradientMode)
        Me.grpIterOptions.Controls.Add(Me.lblLMMCovOptimizerMode)
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
        'cbLMMCovOptimizerMode
        '
        Me.cbLMMCovOptimizerMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbLMMCovOptimizerMode.FormattingEnabled = True
        Me.cbLMMCovOptimizerMode.Location = New System.Drawing.Point(108, 103)
        Me.cbLMMCovOptimizerMode.Name = "cbLMMCovOptimizerMode"
        Me.cbLMMCovOptimizerMode.Size = New System.Drawing.Size(256, 24)
        Me.cbLMMCovOptimizerMode.TabIndex = 20
        '
        'lblLMMCovGradientMode
        '
        Me.lblLMMCovGradientMode.AutoSize = True
        Me.lblLMMCovGradientMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLMMCovGradientMode.Location = New System.Drawing.Point(1, 135)
        Me.lblLMMCovGradientMode.Name = "lblLMMCovGradientMode"
        Me.lblLMMCovGradientMode.Size = New System.Drawing.Size(96, 16)
        Me.lblLMMCovGradientMode.TabIndex = 21
        Me.lblLMMCovGradientMode.Text = "Gradient mode"
        '
        'cbLMMCovGradientMode
        '
        Me.cbLMMCovGradientMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbLMMCovGradientMode.FormattingEnabled = True
        Me.cbLMMCovGradientMode.Location = New System.Drawing.Point(108, 132)
        Me.cbLMMCovGradientMode.Name = "cbLMMCovGradientMode"
        Me.cbLMMCovGradientMode.Size = New System.Drawing.Size(256, 24)
        Me.cbLMMCovGradientMode.TabIndex = 18
        '
        'lblLMMCovOptimizerMode
        '
        Me.lblLMMCovOptimizerMode.AutoSize = True
        Me.lblLMMCovOptimizerMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLMMCovOptimizerMode.Location = New System.Drawing.Point(1, 106)
        Me.lblLMMCovOptimizerMode.Name = "lblLMMCovOptimizerMode"
        Me.lblLMMCovOptimizerMode.Size = New System.Drawing.Size(101, 16)
        Me.lblLMMCovOptimizerMode.TabIndex = 19
        Me.lblLMMCovOptimizerMode.Text = "Optimizer mode"
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
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(3, 501)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(837, 23)
        Me.ProgressBar1.TabIndex = 21
        '
        'lblProgress
        '
        Me.lblProgress.Location = New System.Drawing.Point(1, 527)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(596, 32)
        Me.lblProgress.TabIndex = 20
        Me.lblProgress.Text = "Elapsed Time: "
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(684, 530)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 19
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCalculate
        '
        Me.btCalculate.Location = New System.Drawing.Point(765, 530)
        Me.btCalculate.Name = "btCalculate"
        Me.btCalculate.Size = New System.Drawing.Size(75, 23)
        Me.btCalculate.TabIndex = 18
        Me.btCalculate.Text = "Fit"
        Me.btCalculate.UseVisualStyleBackColor = True
        '
        'cbRandomCovarStruct
        '
        Me.cbRandomCovarStruct.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbRandomCovarStruct.FormattingEnabled = True
        Me.cbRandomCovarStruct.Location = New System.Drawing.Point(203, 119)
        Me.cbRandomCovarStruct.Name = "cbRandomCovarStruct"
        Me.cbRandomCovarStruct.Size = New System.Drawing.Size(232, 24)
        Me.cbRandomCovarStruct.TabIndex = 22
        '
        'lblRandomCovarStruct
        '
        Me.lblRandomCovarStruct.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRandomCovarStruct.Location = New System.Drawing.Point(12, 122)
        Me.lblRandomCovarStruct.Name = "lblRandomCovarStruct"
        Me.lblRandomCovarStruct.Size = New System.Drawing.Size(189, 37)
        Me.lblRandomCovarStruct.TabIndex = 21
        Me.lblRandomCovarStruct.Text = "G-side Random-Effects Covariance Structure"
        '
        'btClearAllSelectedRandomEffects
        '
        Me.btClearAllSelectedRandomEffects.AutoEllipsis = True
        Me.btClearAllSelectedRandomEffects.Location = New System.Drawing.Point(726, 432)
        Me.btClearAllSelectedRandomEffects.Name = "btClearAllSelectedRandomEffects"
        Me.btClearAllSelectedRandomEffects.Size = New System.Drawing.Size(94, 23)
        Me.btClearAllSelectedRandomEffects.TabIndex = 38
        Me.btClearAllSelectedRandomEffects.Text = "Clear All"
        Me.btClearAllSelectedRandomEffects.UseVisualStyleBackColor = True
        '
        'tbRemoveSelectedRandomEffects
        '
        Me.tbRemoveSelectedRandomEffects.AutoEllipsis = True
        Me.tbRemoveSelectedRandomEffects.Location = New System.Drawing.Point(562, 433)
        Me.tbRemoveSelectedRandomEffects.Name = "tbRemoveSelectedRandomEffects"
        Me.tbRemoveSelectedRandomEffects.Size = New System.Drawing.Size(91, 23)
        Me.tbRemoveSelectedRandomEffects.TabIndex = 37
        Me.tbRemoveSelectedRandomEffects.Text = "Remove"
        Me.tbRemoveSelectedRandomEffects.UseVisualStyleBackColor = True
        '
        'grpLMMOutputs
        '
        Me.grpLMMOutputs.Controls.Add(Me.ckLMMClassInfo)
        Me.grpLMMOutputs.Controls.Add(Me.ckLMMRandomEffects)
        Me.grpLMMOutputs.Controls.Add(Me.ckLMMRCovarianceMatrix)
        Me.grpLMMOutputs.Controls.Add(Me.ckLMMGCovarianceMatrix)
        Me.grpLMMOutputs.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpLMMOutputs.Location = New System.Drawing.Point(6, 195)
        Me.grpLMMOutputs.Name = "grpLMMOutputs"
        Me.grpLMMOutputs.Size = New System.Drawing.Size(368, 124)
        Me.grpLMMOutputs.TabIndex = 18
        Me.grpLMMOutputs.TabStop = False
        Me.grpLMMOutputs.Text = "LMM outputs"
        '
        'ckLMMGCovarianceMatrix
        '
        Me.ckLMMGCovarianceMatrix.AutoSize = True
        Me.ckLMMGCovarianceMatrix.Checked = True
        Me.ckLMMGCovarianceMatrix.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckLMMGCovarianceMatrix.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckLMMGCovarianceMatrix.Location = New System.Drawing.Point(6, 21)
        Me.ckLMMGCovarianceMatrix.Name = "ckLMMGCovarianceMatrix"
        Me.ckLMMGCovarianceMatrix.Size = New System.Drawing.Size(217, 20)
        Me.ckLMMGCovarianceMatrix.TabIndex = 6
        Me.ckLMMGCovarianceMatrix.Text = "Output G covariance/correlation"
        Me.ckLMMGCovarianceMatrix.UseVisualStyleBackColor = True
        '
        'ckLMMRCovarianceMatrix
        '
        Me.ckLMMRCovarianceMatrix.AutoSize = True
        Me.ckLMMRCovarianceMatrix.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckLMMRCovarianceMatrix.Location = New System.Drawing.Point(6, 47)
        Me.ckLMMRCovarianceMatrix.Name = "ckLMMRCovarianceMatrix"
        Me.ckLMMRCovarianceMatrix.Size = New System.Drawing.Size(217, 20)
        Me.ckLMMRCovarianceMatrix.TabIndex = 7
        Me.ckLMMRCovarianceMatrix.Text = "Output R covariance/correlation"
        Me.ckLMMRCovarianceMatrix.UseVisualStyleBackColor = True
        '
        'ckLMMRandomEffects
        '
        Me.ckLMMRandomEffects.AutoSize = True
        Me.ckLMMRandomEffects.Checked = True
        Me.ckLMMRandomEffects.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckLMMRandomEffects.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckLMMRandomEffects.Location = New System.Drawing.Point(6, 73)
        Me.ckLMMRandomEffects.Name = "ckLMMRandomEffects"
        Me.ckLMMRandomEffects.Size = New System.Drawing.Size(256, 20)
        Me.ckLMMRandomEffects.TabIndex = 8
        Me.ckLMMRandomEffects.Text = "Output BLUPs / subject random effects"
        Me.ckLMMRandomEffects.UseVisualStyleBackColor = True
        '
        'ckLMMClassInfo
        '
        Me.ckLMMClassInfo.AutoSize = True
        Me.ckLMMClassInfo.Checked = True
        Me.ckLMMClassInfo.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckLMMClassInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckLMMClassInfo.Location = New System.Drawing.Point(6, 99)
        Me.ckLMMClassInfo.Name = "ckLMMClassInfo"
        Me.ckLMMClassInfo.Size = New System.Drawing.Size(163, 20)
        Me.ckLMMClassInfo.TabIndex = 9
        Me.ckLMMClassInfo.Text = "Class level information"
        Me.ckLMMClassInfo.UseVisualStyleBackColor = True
        '
        'Ui19LMM
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(849, 558)
        Me.Controls.Add(Me.btInterrupt)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCalculate)
        Me.MaximumSize = New System.Drawing.Size(867, 605)
        Me.Name = "Ui19LMM"
        Me.ShowIcon = False
        Me.Text = "Ui19LMM"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPageBuildModel.ResumeLayout(False)
        Me.TabPageBuildModel.PerformLayout()
        CType(Me.spinBtnRandomPoly, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnFixedPoly, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageOptions.ResumeLayout(False)
        Me.TabPageOptions.PerformLayout()
        Me.grpModelSpecification.ResumeLayout(False)
        Me.grpModelSpecification.PerformLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpIterOptions.ResumeLayout(False)
        Me.grpIterOptions.PerformLayout()
        Me.grpLMMOutputs.ResumeLayout(False)
        Me.grpLMMOutputs.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btInterrupt As Windows.Forms.Button
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
    Friend WithEvents lbSelectedRandomEffectsList As Windows.Forms.ListBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents btAddFixedEffectCategoricalFactor As Windows.Forms.Button
    Friend WithEvents btnFixedCustomInteraction As Windows.Forms.Button
    Friend WithEvents btnFixed2Interactions As Windows.Forms.Button
    Friend WithEvents spinBtnFixedPoly As Windows.Forms.NumericUpDown
    Friend WithEvents btnFixedPoly As Windows.Forms.Button
    Friend WithEvents cbFixedIntercept As Windows.Forms.CheckBox
    Friend WithEvents btAddFixedEffect As Windows.Forms.Button
    Friend WithEvents btClearAllSelectedFixedEffects As Windows.Forms.Button
    Friend WithEvents tbRemoveSelectedFixedEffects As Windows.Forms.Button
    Friend WithEvents lbSelectedFixedEffectsList As Windows.Forms.ListBox
    Friend WithEvents lbSelectedVariables As Windows.Forms.ListBox
    Friend WithEvents lblSelectedEffectsList As Windows.Forms.Label
    Friend WithEvents lblSelectedVariables As Windows.Forms.Label
    Friend WithEvents TabPageOptions As Windows.Forms.TabPage
    Friend WithEvents grpModelSpecification As Windows.Forms.GroupBox
    Friend WithEvents cbInferenceMethod As Windows.Forms.ComboBox
    Friend WithEvents lblInferenceMethod As Windows.Forms.Label
    Friend WithEvents cbFitMethod As Windows.Forms.ComboBox
    Friend WithEvents cbCovarStruct As Windows.Forms.ComboBox
    Friend WithEvents lblCovarStruct As Windows.Forms.Label
    Friend WithEvents lblFitMethod As Windows.Forms.Label
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents ckResiduals As Windows.Forms.CheckBox
    Friend WithEvents grpIterOptions As Windows.Forms.GroupBox
    Friend WithEvents cbLMMCovOptimizerMode As Windows.Forms.ComboBox
    Friend WithEvents lblLMMCovGradientMode As Windows.Forms.Label
    Friend WithEvents cbLMMCovGradientMode As Windows.Forms.ComboBox
    Friend WithEvents lblLMMCovOptimizerMode As Windows.Forms.Label
    Friend WithEvents cbDiagnostic As Windows.Forms.CheckBox
    Friend WithEvents ckTrace As Windows.Forms.CheckBox
    Friend WithEvents ckIterationsDetails As Windows.Forms.CheckBox
    Friend WithEvents tbMaxIter As Windows.Forms.TextBox
    Friend WithEvents lblMaxIter As Windows.Forms.Label
    Friend WithEvents lblEps As Windows.Forms.Label
    Friend WithEvents tbEps As Windows.Forms.TextBox
    Friend WithEvents ProgressBar1 As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCalculate As Windows.Forms.Button
    Friend WithEvents btAddRandomEffectCategoricalFactor As Windows.Forms.Button
    Friend WithEvents btnRandomCustomInteraction As Windows.Forms.Button
    Friend WithEvents btnRandom2Interactions As Windows.Forms.Button
    Friend WithEvents spinBtnRandomPoly As Windows.Forms.NumericUpDown
    Friend WithEvents btnRandomPoly As Windows.Forms.Button
    Friend WithEvents cbRandomIntercept As Windows.Forms.CheckBox
    Friend WithEvents btAddRandomEffect As Windows.Forms.Button
    Friend WithEvents cbRandomCovarStruct As Windows.Forms.ComboBox
    Friend WithEvents lblRandomCovarStruct As Windows.Forms.Label
    Friend WithEvents btClearAllSelectedRandomEffects As Windows.Forms.Button
    Friend WithEvents tbRemoveSelectedRandomEffects As Windows.Forms.Button
    Friend WithEvents grpLMMOutputs As Windows.Forms.GroupBox
    Friend WithEvents ckLMMClassInfo As Windows.Forms.CheckBox
    Friend WithEvents ckLMMRandomEffects As Windows.Forms.CheckBox
    Friend WithEvents ckLMMRCovarianceMatrix As Windows.Forms.CheckBox
    Friend WithEvents ckLMMGCovarianceMatrix As Windows.Forms.CheckBox
End Class
