<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UiGLM
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
        Me.components = New System.ComponentModel.Container()
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.lbXs = New System.Windows.Forms.ListBox()
        Me.lbWeights = New System.Windows.Forms.ListBox()
        Me.lbOffset = New System.Windows.Forms.ListBox()
        Me.lbY = New System.Windows.Forms.ListBox()
        Me.cbSheetsList = New System.Windows.Forms.ComboBox()
        Me.btReload = New System.Windows.Forms.Button()
        Me.btRemoveX = New System.Windows.Forms.Button()
        Me.btAddX = New System.Windows.Forms.Button()
        Me.btRemoveWeights = New System.Windows.Forms.Button()
        Me.btAddWeights = New System.Windows.Forms.Button()
        Me.lblNote = New System.Windows.Forms.Label()
        Me.lblOffset = New System.Windows.Forms.Label()
        Me.btRemoveOffset = New System.Windows.Forms.Button()
        Me.btAddOffset = New System.Windows.Forms.Button()
        Me.lblY = New System.Windows.Forms.Label()
        Me.btRemoveY = New System.Windows.Forms.Button()
        Me.btAddY = New System.Windows.Forms.Button()
        Me.lblWeights = New System.Windows.Forms.Label()
        Me.lblX = New System.Windows.Forms.Label()
        Me.lblSelectedSheet = New System.Windows.Forms.Label()
        Me.TabPageBuildModel = New System.Windows.Forms.TabPage()
        Me.btAddEffectCategoricalFactor = New System.Windows.Forms.Button()
        Me.btnCustomInteraction = New System.Windows.Forms.Button()
        Me.btn2Interactions = New System.Windows.Forms.Button()
        Me.spinBtnPoly = New System.Windows.Forms.NumericUpDown()
        Me.btnPoly = New System.Windows.Forms.Button()
        Me.ckIntercept = New System.Windows.Forms.CheckBox()
        Me.btAddEffect = New System.Windows.Forms.Button()
        Me.btClearAllSelectedEffects = New System.Windows.Forms.Button()
        Me.tbRemoveSelectedEffects = New System.Windows.Forms.Button()
        Me.tbInitValues = New System.Windows.Forms.TextBox()
        Me.lblInitValues = New System.Windows.Forms.Label()
        Me.lbSelectedEffectsList = New System.Windows.Forms.ListBox()
        Me.lbSelectedVariables = New System.Windows.Forms.ListBox()
        Me.lblSelectedEffectsList = New System.Windows.Forms.Label()
        Me.lblSelectedVariables = New System.Windows.Forms.Label()
        Me.TabPageLogisticModel = New System.Windows.Forms.TabPage()
        Me.ckInterceptLogistic = New System.Windows.Forms.CheckBox()
        Me.btAddEffectLogistic = New System.Windows.Forms.Button()
        Me.btClearAllSelectedEffectsLogistic = New System.Windows.Forms.Button()
        Me.tbRemoveSelectedEffectsLogistic = New System.Windows.Forms.Button()
        Me.tbInitValuesLogistic = New System.Windows.Forms.TextBox()
        Me.lblInitValuesLogistic = New System.Windows.Forms.Label()
        Me.lbSelectedEffectsListLogistic = New System.Windows.Forms.ListBox()
        Me.lbSelectedVariablesLogistic = New System.Windows.Forms.ListBox()
        Me.lblSelectedEffectsListLogistic = New System.Windows.Forms.Label()
        Me.lblSelectedVariablesLogistic = New System.Windows.Forms.Label()
        Me.TabPageOptions = New System.Windows.Forms.TabPage()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.ckCovarMatrix = New System.Windows.Forms.CheckBox()
        Me.ckResiduals = New System.Windows.Forms.CheckBox()
        Me.grpReference = New System.Windows.Forms.GroupBox()
        Me.optLast = New System.Windows.Forms.RadioButton()
        Me.optFirst = New System.Windows.Forms.RadioButton()
        Me.grpModelSpecification = New System.Windows.Forms.GroupBox()
        Me.tbDispersionParameterNB2 = New System.Windows.Forms.TextBox()
        Me.lblDisperisionParameter = New System.Windows.Forms.Label()
        Me.tbPower = New System.Windows.Forms.TextBox()
        Me.lblPower = New System.Windows.Forms.Label()
        Me.cbLink = New System.Windows.Forms.ComboBox()
        Me.lblLink = New System.Windows.Forms.Label()
        Me.cbFamily = New System.Windows.Forms.ComboBox()
        Me.lblFamily = New System.Windows.Forms.Label()
        Me.grpIterOptions = New System.Windows.Forms.GroupBox()
        Me.tbEMiterations = New System.Windows.Forms.TextBox()
        Me.lblEMiterations = New System.Windows.Forms.Label()
        Me.ckIterationsDetails = New System.Windows.Forms.CheckBox()
        Me.tbMaxIter = New System.Windows.Forms.TextBox()
        Me.lblMaxIter = New System.Windows.Forms.Label()
        Me.lblEps = New System.Windows.Forms.Label()
        Me.tbEps = New System.Windows.Forms.TextBox()
        Me.TabPageOptions_LinearModel = New System.Windows.Forms.TabPage()
        Me.ckCovarMatrixLM = New System.Windows.Forms.CheckBox()
        Me.ckResidualsLM = New System.Windows.Forms.CheckBox()
        Me.grpSumOfSquares = New System.Windows.Forms.GroupBox()
        Me.optTypeIIISS = New System.Windows.Forms.RadioButton()
        Me.optTypeISS = New System.Windows.Forms.RadioButton()
        Me.btCalculate = New System.Windows.Forms.Button()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.btAddEffectCategoricalFactorLogistic = New System.Windows.Forms.Button()
        Me.btnCustomInteractionLogistic = New System.Windows.Forms.Button()
        Me.btn2InteractionsLogistic = New System.Windows.Forms.Button()
        Me.spinBtnPolyLogistic = New System.Windows.Forms.NumericUpDown()
        Me.btnPolyLogistic = New System.Windows.Forms.Button()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPageBuildModel.SuspendLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageLogisticModel.SuspendLayout()
        Me.TabPageOptions.SuspendLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpReference.SuspendLayout()
        Me.grpModelSpecification.SuspendLayout()
        Me.grpIterOptions.SuspendLayout()
        Me.TabPageOptions_LinearModel.SuspendLayout()
        Me.grpSumOfSquares.SuspendLayout()
        CType(Me.spinBtnPolyLogistic, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lbAllColumns
        '
        Me.lbAllColumns.FormattingEnabled = True
        Me.lbAllColumns.ItemHeight = 16
        Me.lbAllColumns.Location = New System.Drawing.Point(17, 22)
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
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPageBuildModel)
        Me.TabControl1.Controls.Add(Me.TabPageLogisticModel)
        Me.TabControl1.Controls.Add(Me.TabPageOptions)
        Me.TabControl1.Controls.Add(Me.TabPageOptions_LinearModel)
        Me.TabControl1.Location = New System.Drawing.Point(4, -2)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(844, 494)
        Me.TabControl1.TabIndex = 2
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.lbXs)
        Me.TabPage1.Controls.Add(Me.lbWeights)
        Me.TabPage1.Controls.Add(Me.lbOffset)
        Me.TabPage1.Controls.Add(Me.lbY)
        Me.TabPage1.Controls.Add(Me.cbSheetsList)
        Me.TabPage1.Controls.Add(Me.btReload)
        Me.TabPage1.Controls.Add(Me.btRemoveX)
        Me.TabPage1.Controls.Add(Me.btAddX)
        Me.TabPage1.Controls.Add(Me.btRemoveWeights)
        Me.TabPage1.Controls.Add(Me.btAddWeights)
        Me.TabPage1.Controls.Add(Me.lblNote)
        Me.TabPage1.Controls.Add(Me.lblOffset)
        Me.TabPage1.Controls.Add(Me.btRemoveOffset)
        Me.TabPage1.Controls.Add(Me.btAddOffset)
        Me.TabPage1.Controls.Add(Me.lblY)
        Me.TabPage1.Controls.Add(Me.btRemoveY)
        Me.TabPage1.Controls.Add(Me.btAddY)
        Me.TabPage1.Controls.Add(Me.lbAllColumns)
        Me.TabPage1.Controls.Add(Me.lblAllColumns)
        Me.TabPage1.Controls.Add(Me.lblWeights)
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
        'lbXs
        '
        Me.lbXs.FormattingEnabled = True
        Me.lbXs.ItemHeight = 16
        Me.lbXs.Location = New System.Drawing.Point(334, 151)
        Me.lbXs.Name = "lbXs"
        Me.lbXs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbXs.Size = New System.Drawing.Size(221, 292)
        Me.lbXs.TabIndex = 17
        '
        'lbWeights
        '
        Me.lbWeights.FormattingEnabled = True
        Me.lbWeights.ItemHeight = 16
        Me.lbWeights.Location = New System.Drawing.Point(334, 109)
        Me.lbWeights.Name = "lbWeights"
        Me.lbWeights.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbWeights.Size = New System.Drawing.Size(221, 20)
        Me.lbWeights.TabIndex = 13
        '
        'lbOffset
        '
        Me.lbOffset.FormattingEnabled = True
        Me.lbOffset.ItemHeight = 16
        Me.lbOffset.Location = New System.Drawing.Point(334, 67)
        Me.lbOffset.Name = "lbOffset"
        Me.lbOffset.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbOffset.Size = New System.Drawing.Size(221, 20)
        Me.lbOffset.TabIndex = 8
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
        Me.btRemoveX.Location = New System.Drawing.Point(289, 148)
        Me.btRemoveX.Name = "btRemoveX"
        Me.btRemoveX.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveX.TabIndex = 16
        Me.btRemoveX.Text = "<<"
        Me.btRemoveX.UseVisualStyleBackColor = True
        '
        'btAddX
        '
        Me.btAddX.Location = New System.Drawing.Point(244, 148)
        Me.btAddX.Name = "btAddX"
        Me.btAddX.Size = New System.Drawing.Size(39, 23)
        Me.btAddX.TabIndex = 15
        Me.btAddX.Text = ">>"
        Me.btAddX.UseVisualStyleBackColor = True
        '
        'btRemoveWeights
        '
        Me.btRemoveWeights.Location = New System.Drawing.Point(289, 106)
        Me.btRemoveWeights.Name = "btRemoveWeights"
        Me.btRemoveWeights.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveWeights.TabIndex = 12
        Me.btRemoveWeights.Text = "<<"
        Me.btRemoveWeights.UseVisualStyleBackColor = True
        '
        'btAddWeights
        '
        Me.btAddWeights.Location = New System.Drawing.Point(244, 106)
        Me.btAddWeights.Name = "btAddWeights"
        Me.btAddWeights.Size = New System.Drawing.Size(39, 23)
        Me.btAddWeights.TabIndex = 11
        Me.btAddWeights.Text = ">>"
        Me.btAddWeights.UseVisualStyleBackColor = True
        '
        'lblNote
        '
        Me.lblNote.AutoSize = True
        Me.lblNote.Location = New System.Drawing.Point(561, 427)
        Me.lblNote.Name = "lblNote"
        Me.lblNote.Size = New System.Drawing.Size(164, 16)
        Me.lblNote.TabIndex = 10
        Me.lblNote.Text = "* indicate mandatory fields"
        '
        'lblOffset
        '
        Me.lblOffset.AutoSize = True
        Me.lblOffset.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOffset.Location = New System.Drawing.Point(334, 48)
        Me.lblOffset.Name = "lblOffset"
        Me.lblOffset.Size = New System.Drawing.Size(47, 16)
        Me.lblOffset.TabIndex = 9
        Me.lblOffset.Text = "Offset"
        '
        'btRemoveOffset
        '
        Me.btRemoveOffset.Location = New System.Drawing.Point(289, 64)
        Me.btRemoveOffset.Name = "btRemoveOffset"
        Me.btRemoveOffset.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveOffset.TabIndex = 7
        Me.btRemoveOffset.Text = "<<"
        Me.btRemoveOffset.UseVisualStyleBackColor = True
        '
        'btAddOffset
        '
        Me.btAddOffset.Location = New System.Drawing.Point(244, 64)
        Me.btAddOffset.Name = "btAddOffset"
        Me.btAddOffset.Size = New System.Drawing.Size(39, 23)
        Me.btAddOffset.TabIndex = 6
        Me.btAddOffset.Text = ">>"
        Me.btAddOffset.UseVisualStyleBackColor = True
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
        'lblWeights
        '
        Me.lblWeights.AutoSize = True
        Me.lblWeights.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblWeights.Location = New System.Drawing.Point(334, 90)
        Me.lblWeights.Name = "lblWeights"
        Me.lblWeights.Size = New System.Drawing.Size(63, 16)
        Me.lblWeights.TabIndex = 14
        Me.lblWeights.Text = "Weights"
        '
        'lblX
        '
        Me.lblX.AutoSize = True
        Me.lblX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblX.Location = New System.Drawing.Point(334, 132)
        Me.lblX.Name = "lblX"
        Me.lblX.Size = New System.Drawing.Size(157, 16)
        Me.lblX.TabIndex = 18
        Me.lblX.Text = "Predictor Variable(s)*"
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
        Me.TabPageBuildModel.Controls.Add(Me.ckIntercept)
        Me.TabPageBuildModel.Controls.Add(Me.btAddEffect)
        Me.TabPageBuildModel.Controls.Add(Me.btClearAllSelectedEffects)
        Me.TabPageBuildModel.Controls.Add(Me.tbRemoveSelectedEffects)
        Me.TabPageBuildModel.Controls.Add(Me.tbInitValues)
        Me.TabPageBuildModel.Controls.Add(Me.lblInitValues)
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
        Me.btAddEffectCategoricalFactor.Location = New System.Drawing.Point(333, 89)
        Me.btAddEffectCategoricalFactor.Name = "btAddEffectCategoricalFactor"
        Me.btAddEffectCategoricalFactor.Size = New System.Drawing.Size(191, 23)
        Me.btAddEffectCategoricalFactor.TabIndex = 16
        Me.btAddEffectCategoricalFactor.Text = "Add as Categorical Factor >>"
        Me.btAddEffectCategoricalFactor.UseVisualStyleBackColor = True
        '
        'btnCustomInteraction
        '
        Me.btnCustomInteraction.Location = New System.Drawing.Point(333, 176)
        Me.btnCustomInteraction.Name = "btnCustomInteraction"
        Me.btnCustomInteraction.Size = New System.Drawing.Size(191, 23)
        Me.btnCustomInteraction.TabIndex = 15
        Me.btnCustomInteraction.Text = "Custom Interaction >>"
        Me.btnCustomInteraction.UseVisualStyleBackColor = True
        '
        'btn2Interactions
        '
        Me.btn2Interactions.Location = New System.Drawing.Point(333, 147)
        Me.btn2Interactions.Name = "btn2Interactions"
        Me.btn2Interactions.Size = New System.Drawing.Size(191, 23)
        Me.btn2Interactions.TabIndex = 14
        Me.btn2Interactions.Text = "2-way Interactions >>"
        Me.btn2Interactions.UseVisualStyleBackColor = True
        '
        'spinBtnPoly
        '
        Me.spinBtnPoly.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnPoly.Location = New System.Drawing.Point(480, 118)
        Me.spinBtnPoly.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnPoly.Name = "spinBtnPoly"
        Me.spinBtnPoly.Size = New System.Drawing.Size(44, 22)
        Me.spinBtnPoly.TabIndex = 13
        Me.spinBtnPoly.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'btnPoly
        '
        Me.btnPoly.Location = New System.Drawing.Point(333, 118)
        Me.btnPoly.Name = "btnPoly"
        Me.btnPoly.Size = New System.Drawing.Size(131, 23)
        Me.btnPoly.TabIndex = 12
        Me.btnPoly.Text = "Poly >>"
        Me.btnPoly.UseVisualStyleBackColor = True
        '
        'ckIntercept
        '
        Me.ckIntercept.AutoSize = True
        Me.ckIntercept.Checked = True
        Me.ckIntercept.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckIntercept.Location = New System.Drawing.Point(384, 277)
        Me.ckIntercept.Name = "ckIntercept"
        Me.ckIntercept.Size = New System.Drawing.Size(80, 20)
        Me.ckIntercept.TabIndex = 11
        Me.ckIntercept.Text = "Intercept"
        Me.ckIntercept.UseVisualStyleBackColor = True
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
        'tbInitValues
        '
        Me.tbInitValues.Location = New System.Drawing.Point(302, 348)
        Me.tbInitValues.Multiline = True
        Me.tbInitValues.Name = "tbInitValues"
        Me.tbInitValues.Size = New System.Drawing.Size(240, 103)
        Me.tbInitValues.TabIndex = 6
        '
        'lblInitValues
        '
        Me.lblInitValues.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblInitValues.Location = New System.Drawing.Point(302, 312)
        Me.lblInitValues.Name = "lblInitValues"
        Me.lblInitValues.Size = New System.Drawing.Size(240, 33)
        Me.lblInitValues.TabIndex = 7
        Me.lblInitValues.Text = "Initial parameter values (space separated list of numbers) - optional:"
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
        Me.lblSelectedEffectsList.Size = New System.Drawing.Size(120, 16)
        Me.lblSelectedEffectsList.TabIndex = 5
        Me.lblSelectedEffectsList.Text = "Selected Effects"
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
        'TabPageLogisticModel
        '
        Me.TabPageLogisticModel.Controls.Add(Me.btAddEffectCategoricalFactorLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.btnCustomInteractionLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.btn2InteractionsLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.spinBtnPolyLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.btnPolyLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.ckInterceptLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.btAddEffectLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.btClearAllSelectedEffectsLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.tbRemoveSelectedEffectsLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.tbInitValuesLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.lblInitValuesLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.lbSelectedEffectsListLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.lbSelectedVariablesLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.lblSelectedEffectsListLogistic)
        Me.TabPageLogisticModel.Controls.Add(Me.lblSelectedVariablesLogistic)
        Me.TabPageLogisticModel.Location = New System.Drawing.Point(4, 25)
        Me.TabPageLogisticModel.Name = "TabPageLogisticModel"
        Me.TabPageLogisticModel.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageLogisticModel.Size = New System.Drawing.Size(836, 465)
        Me.TabPageLogisticModel.TabIndex = 3
        Me.TabPageLogisticModel.Text = "Build Model - Logistic"
        Me.TabPageLogisticModel.UseVisualStyleBackColor = True
        '
        'ckInterceptLogistic
        '
        Me.ckInterceptLogistic.AutoSize = True
        Me.ckInterceptLogistic.Checked = True
        Me.ckInterceptLogistic.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckInterceptLogistic.Location = New System.Drawing.Point(385, 277)
        Me.ckInterceptLogistic.Name = "ckInterceptLogistic"
        Me.ckInterceptLogistic.Size = New System.Drawing.Size(80, 20)
        Me.ckInterceptLogistic.TabIndex = 21
        Me.ckInterceptLogistic.Text = "Intercept"
        Me.ckInterceptLogistic.UseVisualStyleBackColor = True
        '
        'btAddEffectLogistic
        '
        Me.btAddEffectLogistic.Location = New System.Drawing.Point(385, 55)
        Me.btAddEffectLogistic.Name = "btAddEffectLogistic"
        Me.btAddEffectLogistic.Size = New System.Drawing.Size(75, 23)
        Me.btAddEffectLogistic.TabIndex = 20
        Me.btAddEffectLogistic.Text = "Add >>"
        Me.btAddEffectLogistic.UseVisualStyleBackColor = True
        '
        'btClearAllSelectedEffectsLogistic
        '
        Me.btClearAllSelectedEffectsLogistic.AutoEllipsis = True
        Me.btClearAllSelectedEffectsLogistic.Location = New System.Drawing.Point(727, 428)
        Me.btClearAllSelectedEffectsLogistic.Name = "btClearAllSelectedEffectsLogistic"
        Me.btClearAllSelectedEffectsLogistic.Size = New System.Drawing.Size(94, 23)
        Me.btClearAllSelectedEffectsLogistic.TabIndex = 19
        Me.btClearAllSelectedEffectsLogistic.Text = "Clear All"
        Me.btClearAllSelectedEffectsLogistic.UseVisualStyleBackColor = True
        '
        'tbRemoveSelectedEffectsLogistic
        '
        Me.tbRemoveSelectedEffectsLogistic.AutoEllipsis = True
        Me.tbRemoveSelectedEffectsLogistic.Location = New System.Drawing.Point(563, 429)
        Me.tbRemoveSelectedEffectsLogistic.Name = "tbRemoveSelectedEffectsLogistic"
        Me.tbRemoveSelectedEffectsLogistic.Size = New System.Drawing.Size(91, 23)
        Me.tbRemoveSelectedEffectsLogistic.TabIndex = 18
        Me.tbRemoveSelectedEffectsLogistic.Text = "Remove"
        Me.tbRemoveSelectedEffectsLogistic.UseVisualStyleBackColor = True
        '
        'tbInitValuesLogistic
        '
        Me.tbInitValuesLogistic.Location = New System.Drawing.Point(303, 348)
        Me.tbInitValuesLogistic.Multiline = True
        Me.tbInitValuesLogistic.Name = "tbInitValuesLogistic"
        Me.tbInitValuesLogistic.Size = New System.Drawing.Size(240, 103)
        Me.tbInitValuesLogistic.TabIndex = 16
        '
        'lblInitValuesLogistic
        '
        Me.lblInitValuesLogistic.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblInitValuesLogistic.Location = New System.Drawing.Point(303, 312)
        Me.lblInitValuesLogistic.Name = "lblInitValuesLogistic"
        Me.lblInitValuesLogistic.Size = New System.Drawing.Size(240, 33)
        Me.lblInitValuesLogistic.TabIndex = 17
        Me.lblInitValuesLogistic.Text = "Initial parameter values (space separated list of numbers) - optional:"
        '
        'lbSelectedEffectsListLogistic
        '
        Me.lbSelectedEffectsListLogistic.FormattingEnabled = True
        Me.lbSelectedEffectsListLogistic.ItemHeight = 16
        Me.lbSelectedEffectsListLogistic.Location = New System.Drawing.Point(549, 31)
        Me.lbSelectedEffectsListLogistic.Name = "lbSelectedEffectsListLogistic"
        Me.lbSelectedEffectsListLogistic.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedEffectsListLogistic.Size = New System.Drawing.Size(282, 388)
        Me.lbSelectedEffectsListLogistic.TabIndex = 14
        '
        'lbSelectedVariablesLogistic
        '
        Me.lbSelectedVariablesLogistic.FormattingEnabled = True
        Me.lbSelectedVariablesLogistic.ItemHeight = 16
        Me.lbSelectedVariablesLogistic.Location = New System.Drawing.Point(6, 31)
        Me.lbSelectedVariablesLogistic.Name = "lbSelectedVariablesLogistic"
        Me.lbSelectedVariablesLogistic.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedVariablesLogistic.Size = New System.Drawing.Size(291, 420)
        Me.lbSelectedVariablesLogistic.TabIndex = 12
        '
        'lblSelectedEffectsListLogistic
        '
        Me.lblSelectedEffectsListLogistic.AutoSize = True
        Me.lblSelectedEffectsListLogistic.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedEffectsListLogistic.Location = New System.Drawing.Point(560, 12)
        Me.lblSelectedEffectsListLogistic.Name = "lblSelectedEffectsListLogistic"
        Me.lblSelectedEffectsListLogistic.Size = New System.Drawing.Size(120, 16)
        Me.lblSelectedEffectsListLogistic.TabIndex = 15
        Me.lblSelectedEffectsListLogistic.Text = "Selected Effects"
        '
        'lblSelectedVariablesLogistic
        '
        Me.lblSelectedVariablesLogistic.AutoSize = True
        Me.lblSelectedVariablesLogistic.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedVariablesLogistic.Location = New System.Drawing.Point(16, 12)
        Me.lblSelectedVariablesLogistic.Name = "lblSelectedVariablesLogistic"
        Me.lblSelectedVariablesLogistic.Size = New System.Drawing.Size(140, 16)
        Me.lblSelectedVariablesLogistic.TabIndex = 13
        Me.lblSelectedVariablesLogistic.Text = "Selected Variables"
        '
        'TabPageOptions
        '
        Me.TabPageOptions.Controls.Add(Me.lblAlpha)
        Me.TabPageOptions.Controls.Add(Me.spinBtnAlpha)
        Me.TabPageOptions.Controls.Add(Me.ckCovarMatrix)
        Me.TabPageOptions.Controls.Add(Me.ckResiduals)
        Me.TabPageOptions.Controls.Add(Me.grpReference)
        Me.TabPageOptions.Controls.Add(Me.grpModelSpecification)
        Me.TabPageOptions.Controls.Add(Me.grpIterOptions)
        Me.TabPageOptions.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptions.Name = "TabPageOptions"
        Me.TabPageOptions.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptions.TabIndex = 2
        Me.TabPageOptions.Text = "Options"
        Me.TabPageOptions.UseVisualStyleBackColor = True
        '
        'lblAlpha
        '
        Me.lblAlpha.AutoSize = True
        Me.lblAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlpha.Location = New System.Drawing.Point(28, 209)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(41, 16)
        Me.lblAlpha.TabIndex = 11
        Me.lblAlpha.Text = "alpha"
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(76, 207)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 10
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'ckCovarMatrix
        '
        Me.ckCovarMatrix.AutoSize = True
        Me.ckCovarMatrix.Location = New System.Drawing.Point(349, 59)
        Me.ckCovarMatrix.Name = "ckCovarMatrix"
        Me.ckCovarMatrix.Size = New System.Drawing.Size(223, 20)
        Me.ckCovarMatrix.TabIndex = 4
        Me.ckCovarMatrix.Text = "Covariance Matrix of Parameters"
        Me.ckCovarMatrix.UseVisualStyleBackColor = True
        '
        'ckResiduals
        '
        Me.ckResiduals.AutoSize = True
        Me.ckResiduals.Location = New System.Drawing.Point(349, 31)
        Me.ckResiduals.Name = "ckResiduals"
        Me.ckResiduals.Size = New System.Drawing.Size(147, 20)
        Me.ckResiduals.TabIndex = 3
        Me.ckResiduals.Text = "Compute Residuals"
        Me.ckResiduals.UseVisualStyleBackColor = True
        '
        'grpReference
        '
        Me.grpReference.Controls.Add(Me.optLast)
        Me.grpReference.Controls.Add(Me.optFirst)
        Me.grpReference.Enabled = False
        Me.grpReference.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpReference.Location = New System.Drawing.Point(349, 87)
        Me.grpReference.Name = "grpReference"
        Me.grpReference.Size = New System.Drawing.Size(312, 106)
        Me.grpReference.TabIndex = 2
        Me.grpReference.TabStop = False
        Me.grpReference.Text = "Reference Category"
        '
        'optLast
        '
        Me.optLast.AutoSize = True
        Me.optLast.Checked = True
        Me.optLast.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optLast.Location = New System.Drawing.Point(18, 61)
        Me.optLast.Name = "optLast"
        Me.optLast.Size = New System.Drawing.Size(53, 20)
        Me.optLast.TabIndex = 4
        Me.optLast.TabStop = True
        Me.optLast.Text = "Last"
        Me.optLast.UseVisualStyleBackColor = True
        '
        'optFirst
        '
        Me.optFirst.AutoSize = True
        Me.optFirst.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optFirst.Location = New System.Drawing.Point(18, 35)
        Me.optFirst.Name = "optFirst"
        Me.optFirst.Size = New System.Drawing.Size(53, 20)
        Me.optFirst.TabIndex = 3
        Me.optFirst.Text = "First"
        Me.optFirst.UseVisualStyleBackColor = True
        '
        'grpModelSpecification
        '
        Me.grpModelSpecification.Controls.Add(Me.tbDispersionParameterNB2)
        Me.grpModelSpecification.Controls.Add(Me.lblDisperisionParameter)
        Me.grpModelSpecification.Controls.Add(Me.tbPower)
        Me.grpModelSpecification.Controls.Add(Me.lblPower)
        Me.grpModelSpecification.Controls.Add(Me.cbLink)
        Me.grpModelSpecification.Controls.Add(Me.lblLink)
        Me.grpModelSpecification.Controls.Add(Me.cbFamily)
        Me.grpModelSpecification.Controls.Add(Me.lblFamily)
        Me.grpModelSpecification.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpModelSpecification.Location = New System.Drawing.Point(23, 242)
        Me.grpModelSpecification.Name = "grpModelSpecification"
        Me.grpModelSpecification.Size = New System.Drawing.Size(312, 155)
        Me.grpModelSpecification.TabIndex = 1
        Me.grpModelSpecification.TabStop = False
        Me.grpModelSpecification.Text = "Model Specification"
        '
        'tbDispersionParameterNB2
        '
        Me.tbDispersionParameterNB2.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbDispersionParameterNB2.Location = New System.Drawing.Point(246, 113)
        Me.tbDispersionParameterNB2.Name = "tbDispersionParameterNB2"
        Me.tbDispersionParameterNB2.Size = New System.Drawing.Size(50, 22)
        Me.tbDispersionParameterNB2.TabIndex = 7
        '
        'lblDisperisionParameter
        '
        Me.lblDisperisionParameter.AutoSize = True
        Me.lblDisperisionParameter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDisperisionParameter.Location = New System.Drawing.Point(15, 119)
        Me.lblDisperisionParameter.Name = "lblDisperisionParameter"
        Me.lblDisperisionParameter.Size = New System.Drawing.Size(228, 16)
        Me.lblDisperisionParameter.TabIndex = 6
        Me.lblDisperisionParameter.Text = "Dispersion Parameter for NB2 Family"
        '
        'tbPower
        '
        Me.tbPower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbPower.Location = New System.Drawing.Point(246, 70)
        Me.tbPower.Name = "tbPower"
        Me.tbPower.Size = New System.Drawing.Size(50, 22)
        Me.tbPower.TabIndex = 2
        Me.tbPower.Text = "1"
        '
        'lblPower
        '
        Me.lblPower.AutoSize = True
        Me.lblPower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPower.Location = New System.Drawing.Point(195, 76)
        Me.lblPower.Name = "lblPower"
        Me.lblPower.Size = New System.Drawing.Size(45, 16)
        Me.lblPower.TabIndex = 5
        Me.lblPower.Text = "Power"
        '
        'cbLink
        '
        Me.cbLink.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbLink.FormattingEnabled = True
        Me.cbLink.Location = New System.Drawing.Point(68, 68)
        Me.cbLink.Name = "cbLink"
        Me.cbLink.Size = New System.Drawing.Size(121, 24)
        Me.cbLink.TabIndex = 4
        '
        'lblLink
        '
        Me.lblLink.AutoSize = True
        Me.lblLink.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLink.Location = New System.Drawing.Point(15, 68)
        Me.lblLink.Name = "lblLink"
        Me.lblLink.Size = New System.Drawing.Size(31, 16)
        Me.lblLink.TabIndex = 3
        Me.lblLink.Text = "Link"
        '
        'cbFamily
        '
        Me.cbFamily.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFamily.FormattingEnabled = True
        Me.cbFamily.Location = New System.Drawing.Point(68, 28)
        Me.cbFamily.Name = "cbFamily"
        Me.cbFamily.Size = New System.Drawing.Size(172, 24)
        Me.cbFamily.TabIndex = 2
        '
        'lblFamily
        '
        Me.lblFamily.AutoSize = True
        Me.lblFamily.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFamily.Location = New System.Drawing.Point(15, 28)
        Me.lblFamily.Name = "lblFamily"
        Me.lblFamily.Size = New System.Drawing.Size(47, 16)
        Me.lblFamily.TabIndex = 2
        Me.lblFamily.Text = "Family"
        '
        'grpIterOptions
        '
        Me.grpIterOptions.Controls.Add(Me.tbEMiterations)
        Me.grpIterOptions.Controls.Add(Me.lblEMiterations)
        Me.grpIterOptions.Controls.Add(Me.ckIterationsDetails)
        Me.grpIterOptions.Controls.Add(Me.tbMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblEps)
        Me.grpIterOptions.Controls.Add(Me.tbEps)
        Me.grpIterOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpIterOptions.Location = New System.Drawing.Point(23, 22)
        Me.grpIterOptions.Name = "grpIterOptions"
        Me.grpIterOptions.Size = New System.Drawing.Size(312, 171)
        Me.grpIterOptions.TabIndex = 0
        Me.grpIterOptions.TabStop = False
        Me.grpIterOptions.Text = "Convergence Options"
        '
        'tbEMiterations
        '
        Me.tbEMiterations.Enabled = False
        Me.tbEMiterations.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbEMiterations.Location = New System.Drawing.Point(171, 100)
        Me.tbEMiterations.Name = "tbEMiterations"
        Me.tbEMiterations.Size = New System.Drawing.Size(125, 22)
        Me.tbEMiterations.TabIndex = 6
        Me.tbEMiterations.Text = "500"
        '
        'lblEMiterations
        '
        Me.lblEMiterations.AutoSize = True
        Me.lblEMiterations.Enabled = False
        Me.lblEMiterations.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEMiterations.Location = New System.Drawing.Point(15, 106)
        Me.lblEMiterations.Name = "lblEMiterations"
        Me.lblEMiterations.Size = New System.Drawing.Size(115, 16)
        Me.lblEMiterations.TabIndex = 5
        Me.lblEMiterations.Text = "Max. EM Iterations"
        '
        'ckIterationsDetails
        '
        Me.ckIterationsDetails.AutoSize = True
        Me.ckIterationsDetails.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckIterationsDetails.Location = New System.Drawing.Point(18, 142)
        Me.ckIterationsDetails.Name = "ckIterationsDetails"
        Me.ckIterationsDetails.Size = New System.Drawing.Size(167, 20)
        Me.ckIterationsDetails.TabIndex = 4
        Me.ckIterationsDetails.Text = "Iterations Details Table"
        Me.ckIterationsDetails.UseVisualStyleBackColor = True
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
        Me.lblMaxIter.Size = New System.Drawing.Size(124, 16)
        Me.lblMaxIter.TabIndex = 2
        Me.lblMaxIter.Text = "Max. IRLS Iterations"
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
        'TabPageOptions_LinearModel
        '
        Me.TabPageOptions_LinearModel.Controls.Add(Me.ckCovarMatrixLM)
        Me.TabPageOptions_LinearModel.Controls.Add(Me.ckResidualsLM)
        Me.TabPageOptions_LinearModel.Controls.Add(Me.grpSumOfSquares)
        Me.TabPageOptions_LinearModel.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptions_LinearModel.Name = "TabPageOptions_LinearModel"
        Me.TabPageOptions_LinearModel.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptions_LinearModel.TabIndex = 4
        Me.TabPageOptions_LinearModel.Text = "Options"
        Me.TabPageOptions_LinearModel.UseVisualStyleBackColor = True
        '
        'ckCovarMatrixLM
        '
        Me.ckCovarMatrixLM.AutoSize = True
        Me.ckCovarMatrixLM.Location = New System.Drawing.Point(355, 49)
        Me.ckCovarMatrixLM.Name = "ckCovarMatrixLM"
        Me.ckCovarMatrixLM.Size = New System.Drawing.Size(223, 20)
        Me.ckCovarMatrixLM.TabIndex = 5
        Me.ckCovarMatrixLM.Text = "Covariance Matrix of Parameters"
        Me.ckCovarMatrixLM.UseVisualStyleBackColor = True
        '
        'ckResidualsLM
        '
        Me.ckResidualsLM.AutoSize = True
        Me.ckResidualsLM.Checked = True
        Me.ckResidualsLM.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckResidualsLM.Location = New System.Drawing.Point(355, 23)
        Me.ckResidualsLM.Name = "ckResidualsLM"
        Me.ckResidualsLM.Size = New System.Drawing.Size(147, 20)
        Me.ckResidualsLM.TabIndex = 4
        Me.ckResidualsLM.Text = "Compute Residuals"
        Me.ckResidualsLM.UseVisualStyleBackColor = True
        '
        'grpSumOfSquares
        '
        Me.grpSumOfSquares.Controls.Add(Me.optTypeIIISS)
        Me.grpSumOfSquares.Controls.Add(Me.optTypeISS)
        Me.grpSumOfSquares.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpSumOfSquares.Location = New System.Drawing.Point(14, 13)
        Me.grpSumOfSquares.Name = "grpSumOfSquares"
        Me.grpSumOfSquares.Size = New System.Drawing.Size(312, 106)
        Me.grpSumOfSquares.TabIndex = 3
        Me.grpSumOfSquares.TabStop = False
        Me.grpSumOfSquares.Text = "Sum-of-Squares Decomposition"
        '
        'optTypeIIISS
        '
        Me.optTypeIIISS.AutoSize = True
        Me.optTypeIIISS.Checked = True
        Me.optTypeIIISS.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optTypeIIISS.Location = New System.Drawing.Point(18, 61)
        Me.optTypeIIISS.Name = "optTypeIIISS"
        Me.optTypeIIISS.Size = New System.Drawing.Size(172, 20)
        Me.optTypeIIISS.TabIndex = 4
        Me.optTypeIIISS.TabStop = True
        Me.optTypeIIISS.Text = "Type III Sum-of-Squares"
        Me.optTypeIIISS.UseVisualStyleBackColor = True
        '
        'optTypeISS
        '
        Me.optTypeISS.AutoSize = True
        Me.optTypeISS.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optTypeISS.Location = New System.Drawing.Point(18, 35)
        Me.optTypeISS.Name = "optTypeISS"
        Me.optTypeISS.Size = New System.Drawing.Size(166, 20)
        Me.optTypeISS.TabIndex = 3
        Me.optTypeISS.Text = "Type I Sum-of-Squares"
        Me.optTypeISS.UseVisualStyleBackColor = True
        '
        'btCalculate
        '
        Me.btCalculate.Location = New System.Drawing.Point(766, 527)
        Me.btCalculate.Name = "btCalculate"
        Me.btCalculate.Size = New System.Drawing.Size(75, 23)
        Me.btCalculate.TabIndex = 3
        Me.btCalculate.Text = "Fit"
        Me.btCalculate.UseVisualStyleBackColor = True
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(685, 527)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 4
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'lblProgress
        '
        Me.lblProgress.Location = New System.Drawing.Point(2, 524)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(664, 23)
        Me.lblProgress.TabIndex = 5
        Me.lblProgress.Text = "Elapsed Time: "
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(4, 498)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(837, 23)
        Me.ProgressBar1.TabIndex = 10
        '
        'btAddEffectCategoricalFactorLogistic
        '
        Me.btAddEffectCategoricalFactorLogistic.Location = New System.Drawing.Point(328, 84)
        Me.btAddEffectCategoricalFactorLogistic.Name = "btAddEffectCategoricalFactorLogistic"
        Me.btAddEffectCategoricalFactorLogistic.Size = New System.Drawing.Size(191, 23)
        Me.btAddEffectCategoricalFactorLogistic.TabIndex = 26
        Me.btAddEffectCategoricalFactorLogistic.Text = "Add as Categorical Factor >>"
        Me.btAddEffectCategoricalFactorLogistic.UseVisualStyleBackColor = True
        '
        'btnCustomInteractionLogistic
        '
        Me.btnCustomInteractionLogistic.Location = New System.Drawing.Point(328, 171)
        Me.btnCustomInteractionLogistic.Name = "btnCustomInteractionLogistic"
        Me.btnCustomInteractionLogistic.Size = New System.Drawing.Size(191, 23)
        Me.btnCustomInteractionLogistic.TabIndex = 25
        Me.btnCustomInteractionLogistic.Text = "Custom Interaction >>"
        Me.btnCustomInteractionLogistic.UseVisualStyleBackColor = True
        '
        'btn2InteractionsLogistic
        '
        Me.btn2InteractionsLogistic.Location = New System.Drawing.Point(328, 142)
        Me.btn2InteractionsLogistic.Name = "btn2InteractionsLogistic"
        Me.btn2InteractionsLogistic.Size = New System.Drawing.Size(191, 23)
        Me.btn2InteractionsLogistic.TabIndex = 24
        Me.btn2InteractionsLogistic.Text = "2-way Interactions >>"
        Me.btn2InteractionsLogistic.UseVisualStyleBackColor = True
        '
        'spinBtnPolyLogistic
        '
        Me.spinBtnPolyLogistic.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnPolyLogistic.Location = New System.Drawing.Point(475, 113)
        Me.spinBtnPolyLogistic.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnPolyLogistic.Name = "spinBtnPolyLogistic"
        Me.spinBtnPolyLogistic.Size = New System.Drawing.Size(44, 22)
        Me.spinBtnPolyLogistic.TabIndex = 23
        Me.spinBtnPolyLogistic.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'btnPolyLogistic
        '
        Me.btnPolyLogistic.Location = New System.Drawing.Point(328, 113)
        Me.btnPolyLogistic.Name = "btnPolyLogistic"
        Me.btnPolyLogistic.Size = New System.Drawing.Size(131, 23)
        Me.btnPolyLogistic.TabIndex = 22
        Me.btnPolyLogistic.Text = "Poly >>"
        Me.btnPolyLogistic.UseVisualStyleBackColor = True
        '
        'UiGLM
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(850, 556)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCalculate)
        Me.Controls.Add(Me.TabControl1)
        Me.MinimumSize = New System.Drawing.Size(868, 603)
        Me.Name = "UiGLM"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Generalized Linear Models"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPageBuildModel.ResumeLayout(False)
        Me.TabPageBuildModel.PerformLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageLogisticModel.ResumeLayout(False)
        Me.TabPageLogisticModel.PerformLayout()
        Me.TabPageOptions.ResumeLayout(False)
        Me.TabPageOptions.PerformLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpReference.ResumeLayout(False)
        Me.grpReference.PerformLayout()
        Me.grpModelSpecification.ResumeLayout(False)
        Me.grpModelSpecification.PerformLayout()
        Me.grpIterOptions.ResumeLayout(False)
        Me.grpIterOptions.PerformLayout()
        Me.TabPageOptions_LinearModel.ResumeLayout(False)
        Me.TabPageOptions_LinearModel.PerformLayout()
        Me.grpSumOfSquares.ResumeLayout(False)
        Me.grpSumOfSquares.PerformLayout()
        CType(Me.spinBtnPolyLogistic, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents TabPageBuildModel As Windows.Forms.TabPage
    Friend WithEvents TabPageOptions As Windows.Forms.TabPage
    Friend WithEvents btAddY As Windows.Forms.Button
    Friend WithEvents lbY As Windows.Forms.ListBox
    Friend WithEvents btRemoveY As Windows.Forms.Button
    Friend WithEvents lblY As Windows.Forms.Label
    Friend WithEvents lblOffset As Windows.Forms.Label
    Friend WithEvents lbOffset As Windows.Forms.ListBox
    Friend WithEvents btRemoveOffset As Windows.Forms.Button
    Friend WithEvents btAddOffset As Windows.Forms.Button
    Friend WithEvents lblNote As Windows.Forms.Label
    Friend WithEvents lblX As Windows.Forms.Label
    Friend WithEvents lbXs As Windows.Forms.ListBox
    Friend WithEvents btRemoveX As Windows.Forms.Button
    Friend WithEvents btAddX As Windows.Forms.Button
    Friend WithEvents lblWeights As Windows.Forms.Label
    Friend WithEvents lbWeights As Windows.Forms.ListBox
    Friend WithEvents btRemoveWeights As Windows.Forms.Button
    Friend WithEvents btAddWeights As Windows.Forms.Button
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents btCalculate As Windows.Forms.Button
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents lbSelectedVariables As Windows.Forms.ListBox
    Friend WithEvents lblSelectedVariables As Windows.Forms.Label
    Friend WithEvents lblSelectedEffectsList As Windows.Forms.Label
    Friend WithEvents lbSelectedEffectsList As Windows.Forms.ListBox
    Friend WithEvents lblInitValues As Windows.Forms.Label
    Friend WithEvents tbInitValues As Windows.Forms.TextBox
    Friend WithEvents btClearAllSelectedEffects As Windows.Forms.Button
    Friend WithEvents tbRemoveSelectedEffects As Windows.Forms.Button
    Friend WithEvents btAddEffect As Windows.Forms.Button
    Friend WithEvents ckIntercept As Windows.Forms.CheckBox
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents grpIterOptions As Windows.Forms.GroupBox
    Friend WithEvents tbMaxIter As Windows.Forms.TextBox
    Friend WithEvents lblMaxIter As Windows.Forms.Label
    Friend WithEvents lblEps As Windows.Forms.Label
    Friend WithEvents tbEps As Windows.Forms.TextBox
    Friend WithEvents ckIterationsDetails As Windows.Forms.CheckBox
    Friend WithEvents grpModelSpecification As Windows.Forms.GroupBox
    Friend WithEvents lblLink As Windows.Forms.Label
    Friend WithEvents cbFamily As Windows.Forms.ComboBox
    Friend WithEvents lblFamily As Windows.Forms.Label
    Friend WithEvents tbPower As Windows.Forms.TextBox
    Friend WithEvents lblPower As Windows.Forms.Label
    Friend WithEvents cbLink As Windows.Forms.ComboBox
    Friend WithEvents grpReference As Windows.Forms.GroupBox
    Friend WithEvents optLast As Windows.Forms.RadioButton
    Friend WithEvents optFirst As Windows.Forms.RadioButton
    Friend WithEvents ToolTip1 As Windows.Forms.ToolTip
    Friend WithEvents lblSelectedSheet As Windows.Forms.Label
    Friend WithEvents ProgressBar1 As Windows.Forms.ProgressBar
    Friend WithEvents tbDispersionParameterNB2 As Windows.Forms.TextBox
    Friend WithEvents lblDisperisionParameter As Windows.Forms.Label
    Friend WithEvents ckResiduals As Windows.Forms.CheckBox
    Friend WithEvents ckCovarMatrix As Windows.Forms.CheckBox
    Friend WithEvents TabPageLogisticModel As Windows.Forms.TabPage
    Friend WithEvents ckInterceptLogistic As Windows.Forms.CheckBox
    Friend WithEvents btAddEffectLogistic As Windows.Forms.Button
    Friend WithEvents btClearAllSelectedEffectsLogistic As Windows.Forms.Button
    Friend WithEvents tbRemoveSelectedEffectsLogistic As Windows.Forms.Button
    Friend WithEvents tbInitValuesLogistic As Windows.Forms.TextBox
    Friend WithEvents lblInitValuesLogistic As Windows.Forms.Label
    Friend WithEvents lbSelectedEffectsListLogistic As Windows.Forms.ListBox
    Friend WithEvents lbSelectedVariablesLogistic As Windows.Forms.ListBox
    Friend WithEvents lblSelectedEffectsListLogistic As Windows.Forms.Label
    Friend WithEvents lblSelectedVariablesLogistic As Windows.Forms.Label
    Friend WithEvents tbEMiterations As Windows.Forms.TextBox
    Friend WithEvents lblEMiterations As Windows.Forms.Label
    Friend WithEvents TabPageOptions_LinearModel As Windows.Forms.TabPage
    Friend WithEvents grpSumOfSquares As Windows.Forms.GroupBox
    Friend WithEvents optTypeIIISS As Windows.Forms.RadioButton
    Friend WithEvents optTypeISS As Windows.Forms.RadioButton
    Friend WithEvents ckResidualsLM As Windows.Forms.CheckBox
    Friend WithEvents ckCovarMatrixLM As Windows.Forms.CheckBox
    Friend WithEvents btnPoly As Windows.Forms.Button
    Friend WithEvents spinBtnPoly As Windows.Forms.NumericUpDown
    Friend WithEvents btnCustomInteraction As Windows.Forms.Button
    Friend WithEvents btn2Interactions As Windows.Forms.Button
    Friend WithEvents btAddEffectCategoricalFactor As Windows.Forms.Button
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents btAddEffectCategoricalFactorLogistic As Windows.Forms.Button
    Friend WithEvents btnCustomInteractionLogistic As Windows.Forms.Button
    Friend WithEvents btn2InteractionsLogistic As Windows.Forms.Button
    Friend WithEvents spinBtnPolyLogistic As Windows.Forms.NumericUpDown
    Friend WithEvents btnPolyLogistic As Windows.Forms.Button
End Class
