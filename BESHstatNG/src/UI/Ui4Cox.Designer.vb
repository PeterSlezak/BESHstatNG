<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui4Cox
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
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.lbXs = New System.Windows.Forms.ListBox()
        Me.lbStrata = New System.Windows.Forms.ListBox()
        Me.lbCensoring = New System.Windows.Forms.ListBox()
        Me.lbTime = New System.Windows.Forms.ListBox()
        Me.cbSheetsList = New System.Windows.Forms.ComboBox()
        Me.btReload = New System.Windows.Forms.Button()
        Me.btRemoveX = New System.Windows.Forms.Button()
        Me.btAddX = New System.Windows.Forms.Button()
        Me.btRemoveStrata = New System.Windows.Forms.Button()
        Me.btAddStrata = New System.Windows.Forms.Button()
        Me.lblNote = New System.Windows.Forms.Label()
        Me.lblCensor = New System.Windows.Forms.Label()
        Me.btRemoveCensoring = New System.Windows.Forms.Button()
        Me.btAddCensorting = New System.Windows.Forms.Button()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.btRemoveTime = New System.Windows.Forms.Button()
        Me.btAddTime = New System.Windows.Forms.Button()
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.lblStrata = New System.Windows.Forms.Label()
        Me.lblX = New System.Windows.Forms.Label()
        Me.lblSelectedSheet = New System.Windows.Forms.Label()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.btAddEffectCategoricalFactor = New System.Windows.Forms.Button()
        Me.btnCustomInteraction = New System.Windows.Forms.Button()
        Me.btn2Interactions = New System.Windows.Forms.Button()
        Me.spinBtnPoly = New System.Windows.Forms.NumericUpDown()
        Me.btnPoly = New System.Windows.Forms.Button()
        Me.btAddEffect = New System.Windows.Forms.Button()
        Me.btClearAllSelectedEffects = New System.Windows.Forms.Button()
        Me.tbRemoveSelectedEffects = New System.Windows.Forms.Button()
        Me.tbInitValues = New System.Windows.Forms.TextBox()
        Me.lblInitValues = New System.Windows.Forms.Label()
        Me.lbSelectedEffectsList = New System.Windows.Forms.ListBox()
        Me.lbSelectedVariables = New System.Windows.Forms.ListBox()
        Me.lblSelectedEffectsList = New System.Windows.Forms.Label()
        Me.lblSelectedVariables = New System.Windows.Forms.Label()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.grpResiduals = New System.Windows.Forms.GroupBox()
        Me.ckPHtest = New System.Windows.Forms.CheckBox()
        Me.ckResidualPlots = New System.Windows.Forms.CheckBox()
        Me.ckAllResiduals = New System.Windows.Forms.CheckBox()
        Me.ckRobustVariance = New System.Windows.Forms.CheckBox()
        Me.ckCovarMatrix = New System.Windows.Forms.CheckBox()
        Me.grpTiesHandling = New System.Windows.Forms.GroupBox()
        Me.optExact = New System.Windows.Forms.RadioButton()
        Me.optEfron = New System.Windows.Forms.RadioButton()
        Me.optBreslow = New System.Windows.Forms.RadioButton()
        Me.grpIterOptions = New System.Windows.Forms.GroupBox()
        Me.spinBtnMaxIter = New System.Windows.Forms.NumericUpDown()
        Me.ckIterationsDetails = New System.Windows.Forms.CheckBox()
        Me.ckTrace = New System.Windows.Forms.CheckBox()
        Me.lblMaxIter = New System.Windows.Forms.Label()
        Me.lblEps = New System.Windows.Forms.Label()
        Me.tbEps = New System.Windows.Forms.TextBox()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCalculate = New System.Windows.Forms.Button()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage3.SuspendLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpResiduals.SuspendLayout()
        Me.grpTiesHandling.SuspendLayout()
        Me.grpIterOptions.SuspendLayout()
        CType(Me.spinBtnMaxIter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Controls.Add(Me.TabPage3)
        Me.TabControl1.Location = New System.Drawing.Point(12, 12)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(844, 476)
        Me.TabControl1.TabIndex = 3
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.lbXs)
        Me.TabPage1.Controls.Add(Me.lbStrata)
        Me.TabPage1.Controls.Add(Me.lbCensoring)
        Me.TabPage1.Controls.Add(Me.lbTime)
        Me.TabPage1.Controls.Add(Me.cbSheetsList)
        Me.TabPage1.Controls.Add(Me.btReload)
        Me.TabPage1.Controls.Add(Me.btRemoveX)
        Me.TabPage1.Controls.Add(Me.btAddX)
        Me.TabPage1.Controls.Add(Me.btRemoveStrata)
        Me.TabPage1.Controls.Add(Me.btAddStrata)
        Me.TabPage1.Controls.Add(Me.lblNote)
        Me.TabPage1.Controls.Add(Me.lblCensor)
        Me.TabPage1.Controls.Add(Me.btRemoveCensoring)
        Me.TabPage1.Controls.Add(Me.btAddCensorting)
        Me.TabPage1.Controls.Add(Me.lblTime)
        Me.TabPage1.Controls.Add(Me.btRemoveTime)
        Me.TabPage1.Controls.Add(Me.btAddTime)
        Me.TabPage1.Controls.Add(Me.lbAllColumns)
        Me.TabPage1.Controls.Add(Me.lblAllColumns)
        Me.TabPage1.Controls.Add(Me.lblStrata)
        Me.TabPage1.Controls.Add(Me.lblX)
        Me.TabPage1.Controls.Add(Me.lblSelectedSheet)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(836, 447)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Select Variables"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'lbXs
        '
        Me.lbXs.FormattingEnabled = True
        Me.lbXs.ItemHeight = 16
        Me.lbXs.Location = New System.Drawing.Point(322, 151)
        Me.lbXs.Name = "lbXs"
        Me.lbXs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbXs.Size = New System.Drawing.Size(221, 292)
        Me.lbXs.TabIndex = 17
        '
        'lbStrata
        '
        Me.lbStrata.FormattingEnabled = True
        Me.lbStrata.ItemHeight = 16
        Me.lbStrata.Location = New System.Drawing.Point(322, 109)
        Me.lbStrata.Name = "lbStrata"
        Me.lbStrata.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbStrata.Size = New System.Drawing.Size(221, 20)
        Me.lbStrata.TabIndex = 13
        '
        'lbCensoring
        '
        Me.lbCensoring.FormattingEnabled = True
        Me.lbCensoring.ItemHeight = 16
        Me.lbCensoring.Location = New System.Drawing.Point(322, 67)
        Me.lbCensoring.Name = "lbCensoring"
        Me.lbCensoring.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbCensoring.Size = New System.Drawing.Size(221, 20)
        Me.lbCensoring.TabIndex = 8
        '
        'lbTime
        '
        Me.lbTime.FormattingEnabled = True
        Me.lbTime.ItemHeight = 16
        Me.lbTime.Location = New System.Drawing.Point(322, 25)
        Me.lbTime.Name = "lbTime"
        Me.lbTime.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbTime.Size = New System.Drawing.Size(221, 20)
        Me.lbTime.TabIndex = 4
        '
        'cbSheetsList
        '
        Me.cbSheetsList.FormattingEnabled = True
        Me.cbSheetsList.Location = New System.Drawing.Point(579, 25)
        Me.cbSheetsList.Name = "cbSheetsList"
        Me.cbSheetsList.Size = New System.Drawing.Size(240, 24)
        Me.cbSheetsList.TabIndex = 21
        Me.cbSheetsList.Text = "Select Sheet"
        '
        'btReload
        '
        Me.btReload.Location = New System.Drawing.Point(579, 64)
        Me.btReload.Name = "btReload"
        Me.btReload.Size = New System.Drawing.Size(130, 23)
        Me.btReload.TabIndex = 20
        Me.btReload.Text = "Reload Sheet Data"
        Me.btReload.UseVisualStyleBackColor = True
        '
        'btRemoveX
        '
        Me.btRemoveX.Location = New System.Drawing.Point(277, 148)
        Me.btRemoveX.Name = "btRemoveX"
        Me.btRemoveX.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveX.TabIndex = 16
        Me.btRemoveX.Text = "<<"
        Me.btRemoveX.UseVisualStyleBackColor = True
        '
        'btAddX
        '
        Me.btAddX.Location = New System.Drawing.Point(232, 148)
        Me.btAddX.Name = "btAddX"
        Me.btAddX.Size = New System.Drawing.Size(39, 23)
        Me.btAddX.TabIndex = 15
        Me.btAddX.Text = ">>"
        Me.btAddX.UseVisualStyleBackColor = True
        '
        'btRemoveStrata
        '
        Me.btRemoveStrata.Location = New System.Drawing.Point(277, 106)
        Me.btRemoveStrata.Name = "btRemoveStrata"
        Me.btRemoveStrata.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveStrata.TabIndex = 12
        Me.btRemoveStrata.Text = "<<"
        Me.btRemoveStrata.UseVisualStyleBackColor = True
        '
        'btAddStrata
        '
        Me.btAddStrata.Location = New System.Drawing.Point(232, 106)
        Me.btAddStrata.Name = "btAddStrata"
        Me.btAddStrata.Size = New System.Drawing.Size(39, 23)
        Me.btAddStrata.TabIndex = 11
        Me.btAddStrata.Text = ">>"
        Me.btAddStrata.UseVisualStyleBackColor = True
        '
        'lblNote
        '
        Me.lblNote.AutoSize = True
        Me.lblNote.Location = New System.Drawing.Point(549, 427)
        Me.lblNote.Name = "lblNote"
        Me.lblNote.Size = New System.Drawing.Size(164, 16)
        Me.lblNote.TabIndex = 10
        Me.lblNote.Text = "* indicate mandatory fields"
        '
        'lblCensor
        '
        Me.lblCensor.AutoSize = True
        Me.lblCensor.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCensor.Location = New System.Drawing.Point(322, 48)
        Me.lblCensor.Name = "lblCensor"
        Me.lblCensor.Size = New System.Drawing.Size(83, 16)
        Me.lblCensor.TabIndex = 9
        Me.lblCensor.Text = "Censoring*"
        '
        'btRemoveCensoring
        '
        Me.btRemoveCensoring.Location = New System.Drawing.Point(277, 64)
        Me.btRemoveCensoring.Name = "btRemoveCensoring"
        Me.btRemoveCensoring.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveCensoring.TabIndex = 7
        Me.btRemoveCensoring.Text = "<<"
        Me.btRemoveCensoring.UseVisualStyleBackColor = True
        '
        'btAddCensorting
        '
        Me.btAddCensorting.Location = New System.Drawing.Point(232, 64)
        Me.btAddCensorting.Name = "btAddCensorting"
        Me.btAddCensorting.Size = New System.Drawing.Size(39, 23)
        Me.btAddCensorting.TabIndex = 6
        Me.btAddCensorting.Text = ">>"
        Me.btAddCensorting.UseVisualStyleBackColor = True
        '
        'lblTime
        '
        Me.lblTime.AutoSize = True
        Me.lblTime.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTime.Location = New System.Drawing.Point(322, 6)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(48, 16)
        Me.lblTime.TabIndex = 5
        Me.lblTime.Text = "Time*"
        '
        'btRemoveTime
        '
        Me.btRemoveTime.Location = New System.Drawing.Point(277, 22)
        Me.btRemoveTime.Name = "btRemoveTime"
        Me.btRemoveTime.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveTime.TabIndex = 3
        Me.btRemoveTime.Text = "<<"
        Me.btRemoveTime.UseVisualStyleBackColor = True
        '
        'btAddTime
        '
        Me.btAddTime.Location = New System.Drawing.Point(232, 22)
        Me.btAddTime.Name = "btAddTime"
        Me.btAddTime.Size = New System.Drawing.Size(39, 23)
        Me.btAddTime.TabIndex = 2
        Me.btAddTime.Text = ">>"
        Me.btAddTime.UseVisualStyleBackColor = True
        '
        'lbAllColumns
        '
        Me.lbAllColumns.FormattingEnabled = True
        Me.lbAllColumns.ItemHeight = 16
        Me.lbAllColumns.Location = New System.Drawing.Point(5, 22)
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
        'lblStrata
        '
        Me.lblStrata.AutoSize = True
        Me.lblStrata.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStrata.Location = New System.Drawing.Point(322, 90)
        Me.lblStrata.Name = "lblStrata"
        Me.lblStrata.Size = New System.Drawing.Size(48, 16)
        Me.lblStrata.TabIndex = 14
        Me.lblStrata.Text = "Strata"
        '
        'lblX
        '
        Me.lblX.AutoSize = True
        Me.lblX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblX.Location = New System.Drawing.Point(322, 132)
        Me.lblX.Name = "lblX"
        Me.lblX.Size = New System.Drawing.Size(157, 16)
        Me.lblX.TabIndex = 18
        Me.lblX.Text = "Predictor Variable(s)*"
        '
        'lblSelectedSheet
        '
        Me.lblSelectedSheet.AutoSize = True
        Me.lblSelectedSheet.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedSheet.Location = New System.Drawing.Point(576, 6)
        Me.lblSelectedSheet.Name = "lblSelectedSheet"
        Me.lblSelectedSheet.Size = New System.Drawing.Size(132, 16)
        Me.lblSelectedSheet.TabIndex = 22
        Me.lblSelectedSheet.Text = "Active Worksheet:"
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.btAddEffectCategoricalFactor)
        Me.TabPage2.Controls.Add(Me.btnCustomInteraction)
        Me.TabPage2.Controls.Add(Me.btn2Interactions)
        Me.TabPage2.Controls.Add(Me.spinBtnPoly)
        Me.TabPage2.Controls.Add(Me.btnPoly)
        Me.TabPage2.Controls.Add(Me.btAddEffect)
        Me.TabPage2.Controls.Add(Me.btClearAllSelectedEffects)
        Me.TabPage2.Controls.Add(Me.tbRemoveSelectedEffects)
        Me.TabPage2.Controls.Add(Me.tbInitValues)
        Me.TabPage2.Controls.Add(Me.lblInitValues)
        Me.TabPage2.Controls.Add(Me.lbSelectedEffectsList)
        Me.TabPage2.Controls.Add(Me.lbSelectedVariables)
        Me.TabPage2.Controls.Add(Me.lblSelectedEffectsList)
        Me.TabPage2.Controls.Add(Me.lblSelectedVariables)
        Me.TabPage2.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(836, 447)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Build Model"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'btAddEffectCategoricalFactor
        '
        Me.btAddEffectCategoricalFactor.Location = New System.Drawing.Point(325, 75)
        Me.btAddEffectCategoricalFactor.Name = "btAddEffectCategoricalFactor"
        Me.btAddEffectCategoricalFactor.Size = New System.Drawing.Size(191, 23)
        Me.btAddEffectCategoricalFactor.TabIndex = 21
        Me.btAddEffectCategoricalFactor.Text = "Add as Categorical Factor >>"
        Me.btAddEffectCategoricalFactor.UseVisualStyleBackColor = True
        '
        'btnCustomInteraction
        '
        Me.btnCustomInteraction.Location = New System.Drawing.Point(325, 162)
        Me.btnCustomInteraction.Name = "btnCustomInteraction"
        Me.btnCustomInteraction.Size = New System.Drawing.Size(191, 23)
        Me.btnCustomInteraction.TabIndex = 20
        Me.btnCustomInteraction.Text = "Custom Interaction >>"
        Me.btnCustomInteraction.UseVisualStyleBackColor = True
        '
        'btn2Interactions
        '
        Me.btn2Interactions.Location = New System.Drawing.Point(325, 133)
        Me.btn2Interactions.Name = "btn2Interactions"
        Me.btn2Interactions.Size = New System.Drawing.Size(191, 23)
        Me.btn2Interactions.TabIndex = 19
        Me.btn2Interactions.Text = "2-way Interactions >>"
        Me.btn2Interactions.UseVisualStyleBackColor = True
        '
        'spinBtnPoly
        '
        Me.spinBtnPoly.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnPoly.Location = New System.Drawing.Point(472, 104)
        Me.spinBtnPoly.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnPoly.Name = "spinBtnPoly"
        Me.spinBtnPoly.Size = New System.Drawing.Size(44, 22)
        Me.spinBtnPoly.TabIndex = 18
        Me.spinBtnPoly.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'btnPoly
        '
        Me.btnPoly.Location = New System.Drawing.Point(325, 104)
        Me.btnPoly.Name = "btnPoly"
        Me.btnPoly.Size = New System.Drawing.Size(131, 23)
        Me.btnPoly.TabIndex = 17
        Me.btnPoly.Text = "Poly >>"
        Me.btnPoly.UseVisualStyleBackColor = True
        '
        'btAddEffect
        '
        Me.btAddEffect.Location = New System.Drawing.Point(384, 46)
        Me.btAddEffect.Name = "btAddEffect"
        Me.btAddEffect.Size = New System.Drawing.Size(75, 23)
        Me.btAddEffect.TabIndex = 10
        Me.btAddEffect.Text = "Add >>"
        Me.btAddEffect.UseVisualStyleBackColor = True
        '
        'btClearAllSelectedEffects
        '
        Me.btClearAllSelectedEffects.AutoEllipsis = True
        Me.btClearAllSelectedEffects.Location = New System.Drawing.Point(726, 419)
        Me.btClearAllSelectedEffects.Name = "btClearAllSelectedEffects"
        Me.btClearAllSelectedEffects.Size = New System.Drawing.Size(94, 23)
        Me.btClearAllSelectedEffects.TabIndex = 9
        Me.btClearAllSelectedEffects.Text = "Clear All"
        Me.btClearAllSelectedEffects.UseVisualStyleBackColor = True
        '
        'tbRemoveSelectedEffects
        '
        Me.tbRemoveSelectedEffects.AutoEllipsis = True
        Me.tbRemoveSelectedEffects.Location = New System.Drawing.Point(562, 420)
        Me.tbRemoveSelectedEffects.Name = "tbRemoveSelectedEffects"
        Me.tbRemoveSelectedEffects.Size = New System.Drawing.Size(91, 23)
        Me.tbRemoveSelectedEffects.TabIndex = 8
        Me.tbRemoveSelectedEffects.Text = "Remove"
        Me.tbRemoveSelectedEffects.UseVisualStyleBackColor = True
        '
        'tbInitValues
        '
        Me.tbInitValues.Location = New System.Drawing.Point(302, 339)
        Me.tbInitValues.Multiline = True
        Me.tbInitValues.Name = "tbInitValues"
        Me.tbInitValues.Size = New System.Drawing.Size(240, 103)
        Me.tbInitValues.TabIndex = 6
        '
        'lblInitValues
        '
        Me.lblInitValues.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblInitValues.Location = New System.Drawing.Point(302, 303)
        Me.lblInitValues.Name = "lblInitValues"
        Me.lblInitValues.Size = New System.Drawing.Size(240, 33)
        Me.lblInitValues.TabIndex = 7
        Me.lblInitValues.Text = "Initial parameter values (space separated list of numbers) - optional:"
        '
        'lbSelectedEffectsList
        '
        Me.lbSelectedEffectsList.FormattingEnabled = True
        Me.lbSelectedEffectsList.ItemHeight = 16
        Me.lbSelectedEffectsList.Location = New System.Drawing.Point(548, 22)
        Me.lbSelectedEffectsList.Name = "lbSelectedEffectsList"
        Me.lbSelectedEffectsList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedEffectsList.Size = New System.Drawing.Size(282, 388)
        Me.lbSelectedEffectsList.TabIndex = 4
        '
        'lbSelectedVariables
        '
        Me.lbSelectedVariables.FormattingEnabled = True
        Me.lbSelectedVariables.ItemHeight = 16
        Me.lbSelectedVariables.Location = New System.Drawing.Point(5, 22)
        Me.lbSelectedVariables.Name = "lbSelectedVariables"
        Me.lbSelectedVariables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedVariables.Size = New System.Drawing.Size(291, 420)
        Me.lbSelectedVariables.TabIndex = 2
        '
        'lblSelectedEffectsList
        '
        Me.lblSelectedEffectsList.AutoSize = True
        Me.lblSelectedEffectsList.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedEffectsList.Location = New System.Drawing.Point(559, 3)
        Me.lblSelectedEffectsList.Name = "lblSelectedEffectsList"
        Me.lblSelectedEffectsList.Size = New System.Drawing.Size(120, 16)
        Me.lblSelectedEffectsList.TabIndex = 5
        Me.lblSelectedEffectsList.Text = "Selected Effects"
        '
        'lblSelectedVariables
        '
        Me.lblSelectedVariables.AutoSize = True
        Me.lblSelectedVariables.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedVariables.Location = New System.Drawing.Point(15, 3)
        Me.lblSelectedVariables.Name = "lblSelectedVariables"
        Me.lblSelectedVariables.Size = New System.Drawing.Size(140, 16)
        Me.lblSelectedVariables.TabIndex = 3
        Me.lblSelectedVariables.Text = "Selected Variables"
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.spinBtnAlpha)
        Me.TabPage3.Controls.Add(Me.lblAlpha)
        Me.TabPage3.Controls.Add(Me.grpResiduals)
        Me.TabPage3.Controls.Add(Me.ckRobustVariance)
        Me.TabPage3.Controls.Add(Me.ckCovarMatrix)
        Me.TabPage3.Controls.Add(Me.grpTiesHandling)
        Me.TabPage3.Controls.Add(Me.grpIterOptions)
        Me.TabPage3.Location = New System.Drawing.Point(4, 25)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Size = New System.Drawing.Size(836, 447)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.Text = "Options"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(400, 85)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 22
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'lblAlpha
        '
        Me.lblAlpha.AutoSize = True
        Me.lblAlpha.Location = New System.Drawing.Point(348, 87)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(41, 16)
        Me.lblAlpha.TabIndex = 21
        Me.lblAlpha.Text = "alpha"
        '
        'grpResiduals
        '
        Me.grpResiduals.Controls.Add(Me.ckPHtest)
        Me.grpResiduals.Controls.Add(Me.ckResidualPlots)
        Me.grpResiduals.Controls.Add(Me.ckAllResiduals)
        Me.grpResiduals.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpResiduals.Location = New System.Drawing.Point(351, 187)
        Me.grpResiduals.Name = "grpResiduals"
        Me.grpResiduals.Size = New System.Drawing.Size(312, 120)
        Me.grpResiduals.TabIndex = 6
        Me.grpResiduals.TabStop = False
        Me.grpResiduals.Text = "Residual Diagnostics"
        '
        'ckPHtest
        '
        Me.ckPHtest.AutoSize = True
        Me.ckPHtest.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckPHtest.Location = New System.Drawing.Point(16, 88)
        Me.ckPHtest.Name = "ckPHtest"
        Me.ckPHtest.Size = New System.Drawing.Size(277, 20)
        Me.ckPHtest.TabIndex = 9
        Me.ckPHtest.Text = "Tests for Proportional Hazard Assumption"
        Me.ckPHtest.UseVisualStyleBackColor = True
        '
        'ckResidualPlots
        '
        Me.ckResidualPlots.AutoSize = True
        Me.ckResidualPlots.Enabled = False
        Me.ckResidualPlots.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckResidualPlots.Location = New System.Drawing.Point(16, 61)
        Me.ckResidualPlots.Name = "ckResidualPlots"
        Me.ckResidualPlots.Size = New System.Drawing.Size(116, 20)
        Me.ckResidualPlots.TabIndex = 8
        Me.ckResidualPlots.Text = "Residual Plots"
        Me.ckResidualPlots.UseVisualStyleBackColor = True
        '
        'ckAllResiduals
        '
        Me.ckAllResiduals.AutoSize = True
        Me.ckAllResiduals.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckAllResiduals.Location = New System.Drawing.Point(16, 35)
        Me.ckAllResiduals.Name = "ckAllResiduals"
        Me.ckAllResiduals.Size = New System.Drawing.Size(90, 20)
        Me.ckAllResiduals.TabIndex = 7
        Me.ckAllResiduals.Text = "Residuals"
        Me.ckAllResiduals.UseVisualStyleBackColor = True
        '
        'ckRobustVariance
        '
        Me.ckRobustVariance.AutoSize = True
        Me.ckRobustVariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckRobustVariance.Location = New System.Drawing.Point(351, 59)
        Me.ckRobustVariance.Name = "ckRobustVariance"
        Me.ckRobustVariance.Size = New System.Drawing.Size(129, 20)
        Me.ckRobustVariance.TabIndex = 6
        Me.ckRobustVariance.Text = "Robust Variance"
        Me.ckRobustVariance.UseVisualStyleBackColor = True
        '
        'ckCovarMatrix
        '
        Me.ckCovarMatrix.AutoSize = True
        Me.ckCovarMatrix.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckCovarMatrix.Location = New System.Drawing.Point(351, 32)
        Me.ckCovarMatrix.Name = "ckCovarMatrix"
        Me.ckCovarMatrix.Size = New System.Drawing.Size(223, 20)
        Me.ckCovarMatrix.TabIndex = 5
        Me.ckCovarMatrix.Text = "Covariance Matrix of Parameters"
        Me.ckCovarMatrix.UseVisualStyleBackColor = True
        '
        'grpTiesHandling
        '
        Me.grpTiesHandling.Controls.Add(Me.optExact)
        Me.grpTiesHandling.Controls.Add(Me.optEfron)
        Me.grpTiesHandling.Controls.Add(Me.optBreslow)
        Me.grpTiesHandling.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpTiesHandling.Location = New System.Drawing.Point(23, 187)
        Me.grpTiesHandling.Name = "grpTiesHandling"
        Me.grpTiesHandling.Size = New System.Drawing.Size(312, 120)
        Me.grpTiesHandling.TabIndex = 2
        Me.grpTiesHandling.TabStop = False
        Me.grpTiesHandling.Text = "Ties Handling Methods"
        '
        'optExact
        '
        Me.optExact.AutoSize = True
        Me.optExact.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optExact.Location = New System.Drawing.Point(18, 87)
        Me.optExact.Name = "optExact"
        Me.optExact.Size = New System.Drawing.Size(61, 20)
        Me.optExact.TabIndex = 5
        Me.optExact.Text = "Exact"
        Me.optExact.UseVisualStyleBackColor = True
        '
        'optEfron
        '
        Me.optEfron.AutoSize = True
        Me.optEfron.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optEfron.Location = New System.Drawing.Point(18, 61)
        Me.optEfron.Name = "optEfron"
        Me.optEfron.Size = New System.Drawing.Size(59, 20)
        Me.optEfron.TabIndex = 4
        Me.optEfron.Text = "Efron"
        Me.optEfron.UseVisualStyleBackColor = True
        '
        'optBreslow
        '
        Me.optBreslow.AutoSize = True
        Me.optBreslow.Checked = True
        Me.optBreslow.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optBreslow.Location = New System.Drawing.Point(18, 35)
        Me.optBreslow.Name = "optBreslow"
        Me.optBreslow.Size = New System.Drawing.Size(76, 20)
        Me.optBreslow.TabIndex = 3
        Me.optBreslow.TabStop = True
        Me.optBreslow.Text = "Breslow"
        Me.optBreslow.UseVisualStyleBackColor = True
        '
        'grpIterOptions
        '
        Me.grpIterOptions.Controls.Add(Me.spinBtnMaxIter)
        Me.grpIterOptions.Controls.Add(Me.ckIterationsDetails)
        Me.grpIterOptions.Controls.Add(Me.ckTrace)
        Me.grpIterOptions.Controls.Add(Me.lblMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblEps)
        Me.grpIterOptions.Controls.Add(Me.tbEps)
        Me.grpIterOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpIterOptions.Location = New System.Drawing.Point(23, 22)
        Me.grpIterOptions.Name = "grpIterOptions"
        Me.grpIterOptions.Size = New System.Drawing.Size(312, 159)
        Me.grpIterOptions.TabIndex = 0
        Me.grpIterOptions.TabStop = False
        Me.grpIterOptions.Text = "Convergence Options"
        '
        'spinBtnMaxIter
        '
        Me.spinBtnMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnMaxIter.Location = New System.Drawing.Point(171, 63)
        Me.spinBtnMaxIter.Maximum = New Decimal(New Integer() {1000000, 0, 0, 0})
        Me.spinBtnMaxIter.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnMaxIter.Name = "spinBtnMaxIter"
        Me.spinBtnMaxIter.Size = New System.Drawing.Size(125, 22)
        Me.spinBtnMaxIter.TabIndex = 5
        Me.spinBtnMaxIter.Value = New Decimal(New Integer() {50, 0, 0, 0})
        '
        'ckIterationsDetails
        '
        Me.ckIterationsDetails.AutoSize = True
        Me.ckIterationsDetails.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckIterationsDetails.Location = New System.Drawing.Point(18, 128)
        Me.ckIterationsDetails.Name = "ckIterationsDetails"
        Me.ckIterationsDetails.Size = New System.Drawing.Size(167, 20)
        Me.ckIterationsDetails.TabIndex = 4
        Me.ckIterationsDetails.Text = "Iterations Details Table"
        Me.ckIterationsDetails.UseVisualStyleBackColor = True
        '
        'ckTrace
        '
        Me.ckTrace.AutoSize = True
        Me.ckTrace.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckTrace.Location = New System.Drawing.Point(18, 102)
        Me.ckTrace.Name = "ckTrace"
        Me.ckTrace.Size = New System.Drawing.Size(194, 20)
        Me.ckTrace.TabIndex = 1
        Me.ckTrace.Text = "Trace Execution Information"
        Me.ckTrace.UseVisualStyleBackColor = True
        '
        'lblMaxIter
        '
        Me.lblMaxIter.AutoSize = True
        Me.lblMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMaxIter.Location = New System.Drawing.Point(15, 63)
        Me.lblMaxIter.Name = "lblMaxIter"
        Me.lblMaxIter.Size = New System.Drawing.Size(92, 16)
        Me.lblMaxIter.TabIndex = 2
        Me.lblMaxIter.Text = "Max. Iterations"
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
        'lblProgress
        '
        Me.lblProgress.Location = New System.Drawing.Point(13, 512)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(677, 23)
        Me.lblProgress.TabIndex = 8
        Me.lblProgress.Text = "Elapsed Time: "
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(696, 515)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 7
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCalculate
        '
        Me.btCalculate.Location = New System.Drawing.Point(777, 515)
        Me.btCalculate.Name = "btCalculate"
        Me.btCalculate.Size = New System.Drawing.Size(75, 23)
        Me.btCalculate.TabIndex = 6
        Me.btCalculate.Text = "Fit"
        Me.btCalculate.UseVisualStyleBackColor = True
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(15, 486)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(837, 23)
        Me.ProgressBar1.TabIndex = 9
        '
        'Ui4Cox
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(859, 543)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCalculate)
        Me.Controls.Add(Me.TabControl1)
        Me.MinimumSize = New System.Drawing.Size(877, 590)
        Me.Name = "Ui4Cox"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Cox Proportional Hazard Regression"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage3.ResumeLayout(False)
        Me.TabPage3.PerformLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpResiduals.ResumeLayout(False)
        Me.grpResiduals.PerformLayout()
        Me.grpTiesHandling.ResumeLayout(False)
        Me.grpTiesHandling.PerformLayout()
        Me.grpIterOptions.ResumeLayout(False)
        Me.grpIterOptions.PerformLayout()
        CType(Me.spinBtnMaxIter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents lbXs As Windows.Forms.ListBox
    Friend WithEvents lbStrata As Windows.Forms.ListBox
    Friend WithEvents lbCensoring As Windows.Forms.ListBox
    Friend WithEvents lbTime As Windows.Forms.ListBox
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents btRemoveX As Windows.Forms.Button
    Friend WithEvents btAddX As Windows.Forms.Button
    Friend WithEvents btRemoveStrata As Windows.Forms.Button
    Friend WithEvents btAddStrata As Windows.Forms.Button
    Friend WithEvents lblNote As Windows.Forms.Label
    Friend WithEvents lblCensor As Windows.Forms.Label
    Friend WithEvents btRemoveCensoring As Windows.Forms.Button
    Friend WithEvents btAddCensorting As Windows.Forms.Button
    Friend WithEvents lblTime As Windows.Forms.Label
    Friend WithEvents btRemoveTime As Windows.Forms.Button
    Friend WithEvents btAddTime As Windows.Forms.Button
    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents lblStrata As Windows.Forms.Label
    Friend WithEvents lblX As Windows.Forms.Label
    Friend WithEvents TabPage2 As Windows.Forms.TabPage
    Friend WithEvents btAddEffect As Windows.Forms.Button
    Friend WithEvents btClearAllSelectedEffects As Windows.Forms.Button
    Friend WithEvents tbRemoveSelectedEffects As Windows.Forms.Button
    Friend WithEvents tbInitValues As Windows.Forms.TextBox
    Friend WithEvents lblInitValues As Windows.Forms.Label
    Friend WithEvents lbSelectedEffectsList As Windows.Forms.ListBox
    Friend WithEvents lbSelectedVariables As Windows.Forms.ListBox
    Friend WithEvents lblSelectedEffectsList As Windows.Forms.Label
    Friend WithEvents lblSelectedVariables As Windows.Forms.Label
    Friend WithEvents TabPage3 As Windows.Forms.TabPage
    Friend WithEvents grpTiesHandling As Windows.Forms.GroupBox
    Friend WithEvents optEfron As Windows.Forms.RadioButton
    Friend WithEvents optBreslow As Windows.Forms.RadioButton
    Friend WithEvents grpIterOptions As Windows.Forms.GroupBox
    Friend WithEvents ckIterationsDetails As Windows.Forms.CheckBox
    Friend WithEvents ckTrace As Windows.Forms.CheckBox
    Friend WithEvents lblMaxIter As Windows.Forms.Label
    Friend WithEvents lblEps As Windows.Forms.Label
    Friend WithEvents tbEps As Windows.Forms.TextBox
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCalculate As Windows.Forms.Button
    Friend WithEvents ProgressBar1 As Windows.Forms.ProgressBar
    Friend WithEvents spinBtnMaxIter As Windows.Forms.NumericUpDown
    Friend WithEvents optExact As Windows.Forms.RadioButton
    Friend WithEvents ckRobustVariance As Windows.Forms.CheckBox
    Friend WithEvents ckCovarMatrix As Windows.Forms.CheckBox
    Friend WithEvents grpResiduals As Windows.Forms.GroupBox
    Friend WithEvents ckPHtest As Windows.Forms.CheckBox
    Friend WithEvents ckResidualPlots As Windows.Forms.CheckBox
    Friend WithEvents ckAllResiduals As Windows.Forms.CheckBox
    Friend WithEvents lblSelectedSheet As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents btAddEffectCategoricalFactor As Windows.Forms.Button
    Friend WithEvents btnCustomInteraction As Windows.Forms.Button
    Friend WithEvents btn2Interactions As Windows.Forms.Button
    Friend WithEvents spinBtnPoly As Windows.Forms.NumericUpDown
    Friend WithEvents btnPoly As Windows.Forms.Button
End Class
