<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui13GEE
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
        Me.btRemoveTime = New System.Windows.Forms.Button()
        Me.btAddTime = New System.Windows.Forms.Button()
        Me.btRemoveClusterID = New System.Windows.Forms.Button()
        Me.btAddClusterID = New System.Windows.Forms.Button()
        Me.lbTime = New System.Windows.Forms.ListBox()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.lbClusterID = New System.Windows.Forms.ListBox()
        Me.lblClusterID = New System.Windows.Forms.Label()
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
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.lblWeights = New System.Windows.Forms.Label()
        Me.lblX = New System.Windows.Forms.Label()
        Me.lblSelectedSheet = New System.Windows.Forms.Label()
        Me.TabPageBuildModel = New System.Windows.Forms.TabPage()
        Me.cbIntercept = New System.Windows.Forms.CheckBox()
        Me.btAddEffect = New System.Windows.Forms.Button()
        Me.btClearAllSelectedEffects = New System.Windows.Forms.Button()
        Me.tbRemoveSelectedEffects = New System.Windows.Forms.Button()
        Me.tbInitValues = New System.Windows.Forms.TextBox()
        Me.lblInitValues = New System.Windows.Forms.Label()
        Me.lbSelectedEffectsList = New System.Windows.Forms.ListBox()
        Me.lbSelectedVariables = New System.Windows.Forms.ListBox()
        Me.lblSelectedEffectsList = New System.Windows.Forms.Label()
        Me.lblSelectedVariables = New System.Windows.Forms.Label()
        Me.TabPageOptions = New System.Windows.Forms.TabPage()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.ckResiduals = New System.Windows.Forms.CheckBox()
        Me.grpModelSpecification = New System.Windows.Forms.GroupBox()
        Me.cbStandardErr = New System.Windows.Forms.ComboBox()
        Me.lblStdErr = New System.Windows.Forms.Label()
        Me.cbCovarStruct = New System.Windows.Forms.ComboBox()
        Me.lblCovarStruct = New System.Windows.Forms.Label()
        Me.ckUseP = New System.Windows.Forms.CheckBox()
        Me.tbDispersionParameterNB2 = New System.Windows.Forms.TextBox()
        Me.lblDisperisionParameter = New System.Windows.Forms.Label()
        Me.tbPower = New System.Windows.Forms.TextBox()
        Me.lblPower = New System.Windows.Forms.Label()
        Me.cbLink = New System.Windows.Forms.ComboBox()
        Me.lblLink = New System.Windows.Forms.Label()
        Me.cbFamily = New System.Windows.Forms.ComboBox()
        Me.lblFamily = New System.Windows.Forms.Label()
        Me.grpIterOptions = New System.Windows.Forms.GroupBox()
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
        Me.btAddEffectCategoricalFactor = New System.Windows.Forms.Button()
        Me.btnCustomInteraction = New System.Windows.Forms.Button()
        Me.btn2Interactions = New System.Windows.Forms.Button()
        Me.spinBtnPoly = New System.Windows.Forms.NumericUpDown()
        Me.btnPoly = New System.Windows.Forms.Button()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPageBuildModel.SuspendLayout()
        Me.TabPageOptions.SuspendLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpModelSpecification.SuspendLayout()
        Me.grpIterOptions.SuspendLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPageBuildModel)
        Me.TabControl1.Controls.Add(Me.TabPageOptions)
        Me.TabControl1.Location = New System.Drawing.Point(4, 8)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(844, 494)
        Me.TabControl1.TabIndex = 3
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
        'btRemoveTime
        '
        Me.btRemoveTime.Location = New System.Drawing.Point(289, 193)
        Me.btRemoveTime.Name = "btRemoveTime"
        Me.btRemoveTime.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveTime.TabIndex = 31
        Me.btRemoveTime.Text = "<<"
        Me.btRemoveTime.UseVisualStyleBackColor = True
        '
        'btAddTime
        '
        Me.btAddTime.Location = New System.Drawing.Point(244, 193)
        Me.btAddTime.Name = "btAddTime"
        Me.btAddTime.Size = New System.Drawing.Size(39, 23)
        Me.btAddTime.TabIndex = 30
        Me.btAddTime.Text = ">>"
        Me.btAddTime.UseVisualStyleBackColor = True
        '
        'btRemoveClusterID
        '
        Me.btRemoveClusterID.Location = New System.Drawing.Point(289, 151)
        Me.btRemoveClusterID.Name = "btRemoveClusterID"
        Me.btRemoveClusterID.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveClusterID.TabIndex = 29
        Me.btRemoveClusterID.Text = "<<"
        Me.btRemoveClusterID.UseVisualStyleBackColor = True
        '
        'btAddClusterID
        '
        Me.btAddClusterID.Location = New System.Drawing.Point(244, 151)
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
        Me.lbTime.Location = New System.Drawing.Point(334, 193)
        Me.lbTime.Name = "lbTime"
        Me.lbTime.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbTime.Size = New System.Drawing.Size(221, 20)
        Me.lbTime.TabIndex = 26
        '
        'lblTime
        '
        Me.lblTime.AutoSize = True
        Me.lblTime.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTime.Location = New System.Drawing.Point(334, 174)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(177, 16)
        Me.lblTime.TabIndex = 27
        Me.lblTime.Text = "Within Cluster Ordering**"
        '
        'lbClusterID
        '
        Me.lbClusterID.FormattingEnabled = True
        Me.lbClusterID.ItemHeight = 16
        Me.lbClusterID.Location = New System.Drawing.Point(334, 151)
        Me.lbClusterID.Name = "lbClusterID"
        Me.lbClusterID.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbClusterID.Size = New System.Drawing.Size(221, 20)
        Me.lbClusterID.TabIndex = 24
        '
        'lblClusterID
        '
        Me.lblClusterID.AutoSize = True
        Me.lblClusterID.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblClusterID.Location = New System.Drawing.Point(334, 132)
        Me.lblClusterID.Name = "lblClusterID"
        Me.lblClusterID.Size = New System.Drawing.Size(80, 16)
        Me.lblClusterID.TabIndex = 25
        Me.lblClusterID.Text = "Cluster ID*"
        '
        'lbXs
        '
        Me.lbXs.FormattingEnabled = True
        Me.lbXs.ItemHeight = 16
        Me.lbXs.Location = New System.Drawing.Point(334, 231)
        Me.lbXs.Name = "lbXs"
        Me.lbXs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbXs.Size = New System.Drawing.Size(221, 228)
        Me.lbXs.TabIndex = 17
        '
        'lbWeights
        '
        Me.lbWeights.Enabled = False
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
        Me.btRemoveX.Location = New System.Drawing.Point(289, 231)
        Me.btRemoveX.Name = "btRemoveX"
        Me.btRemoveX.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveX.TabIndex = 16
        Me.btRemoveX.Text = "<<"
        Me.btRemoveX.UseVisualStyleBackColor = True
        '
        'btAddX
        '
        Me.btAddX.Location = New System.Drawing.Point(244, 231)
        Me.btAddX.Name = "btAddX"
        Me.btAddX.Size = New System.Drawing.Size(39, 23)
        Me.btAddX.TabIndex = 15
        Me.btAddX.Text = ">>"
        Me.btAddX.UseVisualStyleBackColor = True
        '
        'btRemoveWeights
        '
        Me.btRemoveWeights.Enabled = False
        Me.btRemoveWeights.Location = New System.Drawing.Point(289, 106)
        Me.btRemoveWeights.Name = "btRemoveWeights"
        Me.btRemoveWeights.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveWeights.TabIndex = 12
        Me.btRemoveWeights.Text = "<<"
        Me.btRemoveWeights.UseVisualStyleBackColor = True
        '
        'btAddWeights
        '
        Me.btAddWeights.Enabled = False
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
        Me.lblNote.Size = New System.Drawing.Size(233, 32)
        Me.lblNote.TabIndex = 10
        Me.lblNote.Text = "* indicate mandatory fields" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "** indicate conditionally required fields"
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
        'lblWeights
        '
        Me.lblWeights.AutoSize = True
        Me.lblWeights.Enabled = False
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
        Me.lblX.Location = New System.Drawing.Point(334, 216)
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
        Me.TabPageBuildModel.Controls.Add(Me.cbIntercept)
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
        'cbIntercept
        '
        Me.cbIntercept.AutoSize = True
        Me.cbIntercept.Checked = True
        Me.cbIntercept.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbIntercept.Enabled = False
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
        'TabPageOptions
        '
        Me.TabPageOptions.Controls.Add(Me.lblAlpha)
        Me.TabPageOptions.Controls.Add(Me.spinBtnAlpha)
        Me.TabPageOptions.Controls.Add(Me.ckResiduals)
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
        Me.lblAlpha.Location = New System.Drawing.Point(346, 59)
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
        Me.spinBtnAlpha.Location = New System.Drawing.Point(394, 57)
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
        Me.ckResiduals.Location = New System.Drawing.Point(349, 31)
        Me.ckResiduals.Name = "ckResiduals"
        Me.ckResiduals.Size = New System.Drawing.Size(147, 20)
        Me.ckResiduals.TabIndex = 3
        Me.ckResiduals.Text = "Compute Residuals"
        Me.ckResiduals.UseVisualStyleBackColor = True
        '
        'grpModelSpecification
        '
        Me.grpModelSpecification.Controls.Add(Me.cbStandardErr)
        Me.grpModelSpecification.Controls.Add(Me.lblStdErr)
        Me.grpModelSpecification.Controls.Add(Me.cbCovarStruct)
        Me.grpModelSpecification.Controls.Add(Me.lblCovarStruct)
        Me.grpModelSpecification.Controls.Add(Me.ckUseP)
        Me.grpModelSpecification.Controls.Add(Me.tbDispersionParameterNB2)
        Me.grpModelSpecification.Controls.Add(Me.lblDisperisionParameter)
        Me.grpModelSpecification.Controls.Add(Me.tbPower)
        Me.grpModelSpecification.Controls.Add(Me.lblPower)
        Me.grpModelSpecification.Controls.Add(Me.cbLink)
        Me.grpModelSpecification.Controls.Add(Me.lblLink)
        Me.grpModelSpecification.Controls.Add(Me.cbFamily)
        Me.grpModelSpecification.Controls.Add(Me.lblFamily)
        Me.grpModelSpecification.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpModelSpecification.Location = New System.Drawing.Point(23, 166)
        Me.grpModelSpecification.Name = "grpModelSpecification"
        Me.grpModelSpecification.Size = New System.Drawing.Size(312, 270)
        Me.grpModelSpecification.TabIndex = 1
        Me.grpModelSpecification.TabStop = False
        Me.grpModelSpecification.Text = "Model Specification"
        '
        'cbStandardErr
        '
        Me.cbStandardErr.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbStandardErr.FormattingEnabled = True
        Me.cbStandardErr.Location = New System.Drawing.Point(124, 166)
        Me.cbStandardErr.Name = "cbStandardErr"
        Me.cbStandardErr.Size = New System.Drawing.Size(172, 24)
        Me.cbStandardErr.TabIndex = 12
        '
        'lblStdErr
        '
        Me.lblStdErr.AutoSize = True
        Me.lblStdErr.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStdErr.Location = New System.Drawing.Point(15, 166)
        Me.lblStdErr.Name = "lblStdErr"
        Me.lblStdErr.Size = New System.Drawing.Size(85, 16)
        Me.lblStdErr.TabIndex = 11
        Me.lblStdErr.Text = "Standard Err."
        '
        'cbCovarStruct
        '
        Me.cbCovarStruct.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbCovarStruct.FormattingEnabled = True
        Me.cbCovarStruct.Location = New System.Drawing.Point(124, 126)
        Me.cbCovarStruct.Name = "cbCovarStruct"
        Me.cbCovarStruct.Size = New System.Drawing.Size(172, 24)
        Me.cbCovarStruct.TabIndex = 9
        '
        'lblCovarStruct
        '
        Me.lblCovarStruct.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCovarStruct.Location = New System.Drawing.Point(15, 126)
        Me.lblCovarStruct.Name = "lblCovarStruct"
        Me.lblCovarStruct.Size = New System.Drawing.Size(103, 37)
        Me.lblCovarStruct.TabIndex = 10
        Me.lblCovarStruct.Text = "Covariance Structure"
        '
        'ckUseP
        '
        Me.ckUseP.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckUseP.Location = New System.Drawing.Point(18, 214)
        Me.ckUseP.Name = "ckUseP"
        Me.ckUseP.Size = New System.Drawing.Size(248, 38)
        Me.ckUseP.TabIndex = 8
        Me.ckUseP.Text = "Use the n-p correction for dispersion and correlation estimates"
        Me.ckUseP.UseVisualStyleBackColor = True
        '
        'tbDispersionParameterNB2
        '
        Me.tbDispersionParameterNB2.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbDispersionParameterNB2.Location = New System.Drawing.Point(246, 96)
        Me.tbDispersionParameterNB2.Name = "tbDispersionParameterNB2"
        Me.tbDispersionParameterNB2.Size = New System.Drawing.Size(50, 22)
        Me.tbDispersionParameterNB2.TabIndex = 7
        '
        'lblDisperisionParameter
        '
        Me.lblDisperisionParameter.AutoSize = True
        Me.lblDisperisionParameter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDisperisionParameter.Location = New System.Drawing.Point(15, 99)
        Me.lblDisperisionParameter.Name = "lblDisperisionParameter"
        Me.lblDisperisionParameter.Size = New System.Drawing.Size(228, 16)
        Me.lblDisperisionParameter.TabIndex = 6
        Me.lblDisperisionParameter.Text = "Dispersion Parameter for NB2 Family"
        '
        'tbPower
        '
        Me.tbPower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbPower.Location = New System.Drawing.Point(246, 62)
        Me.tbPower.Name = "tbPower"
        Me.tbPower.Size = New System.Drawing.Size(50, 22)
        Me.tbPower.TabIndex = 2
        Me.tbPower.Text = "1"
        '
        'lblPower
        '
        Me.lblPower.AutoSize = True
        Me.lblPower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPower.Location = New System.Drawing.Point(195, 68)
        Me.lblPower.Name = "lblPower"
        Me.lblPower.Size = New System.Drawing.Size(45, 16)
        Me.lblPower.TabIndex = 5
        Me.lblPower.Text = "Power"
        '
        'cbLink
        '
        Me.cbLink.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbLink.FormattingEnabled = True
        Me.cbLink.Location = New System.Drawing.Point(68, 60)
        Me.cbLink.Name = "cbLink"
        Me.cbLink.Size = New System.Drawing.Size(121, 24)
        Me.cbLink.TabIndex = 4
        '
        'lblLink
        '
        Me.lblLink.AutoSize = True
        Me.lblLink.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLink.Location = New System.Drawing.Point(15, 60)
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
        Me.grpIterOptions.Controls.Add(Me.ckTrace)
        Me.grpIterOptions.Controls.Add(Me.ckIterationsDetails)
        Me.grpIterOptions.Controls.Add(Me.tbMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblEps)
        Me.grpIterOptions.Controls.Add(Me.tbEps)
        Me.grpIterOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpIterOptions.Location = New System.Drawing.Point(23, 22)
        Me.grpIterOptions.Name = "grpIterOptions"
        Me.grpIterOptions.Size = New System.Drawing.Size(312, 138)
        Me.grpIterOptions.TabIndex = 0
        Me.grpIterOptions.TabStop = False
        Me.grpIterOptions.Text = "Convergence Options"
        '
        'ckTrace
        '
        Me.ckTrace.AutoSize = True
        Me.ckTrace.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckTrace.Location = New System.Drawing.Point(170, 102)
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
        Me.ckIterationsDetails.Location = New System.Drawing.Point(18, 102)
        Me.ckIterationsDetails.Name = "ckIterationsDetails"
        Me.ckIterationsDetails.Size = New System.Drawing.Size(128, 20)
        Me.ckIterationsDetails.TabIndex = 4
        Me.ckIterationsDetails.Text = "Iterations Details"
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
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(8, 504)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(837, 23)
        Me.ProgressBar1.TabIndex = 14
        '
        'lblProgress
        '
        Me.lblProgress.Location = New System.Drawing.Point(6, 530)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(664, 23)
        Me.lblProgress.TabIndex = 13
        Me.lblProgress.Text = "Elapsed Time: "
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(689, 533)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 12
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCalculate
        '
        Me.btCalculate.Location = New System.Drawing.Point(770, 533)
        Me.btCalculate.Name = "btCalculate"
        Me.btCalculate.Size = New System.Drawing.Size(75, 23)
        Me.btCalculate.TabIndex = 11
        Me.btCalculate.Text = "Fit"
        Me.btCalculate.UseVisualStyleBackColor = True
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
        'Ui13GEE
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(853, 566)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCalculate)
        Me.Controls.Add(Me.TabControl1)
        Me.MinimumSize = New System.Drawing.Size(871, 613)
        Me.Name = "Ui13GEE"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Generalized Estimating Equations"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPageBuildModel.ResumeLayout(False)
        Me.TabPageBuildModel.PerformLayout()
        Me.TabPageOptions.ResumeLayout(False)
        Me.TabPageOptions.PerformLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpModelSpecification.ResumeLayout(False)
        Me.grpModelSpecification.PerformLayout()
        Me.grpIterOptions.ResumeLayout(False)
        Me.grpIterOptions.PerformLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents lbXs As Windows.Forms.ListBox
    Friend WithEvents lbWeights As Windows.Forms.ListBox
    Friend WithEvents lbOffset As Windows.Forms.ListBox
    Friend WithEvents lbY As Windows.Forms.ListBox
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents btRemoveX As Windows.Forms.Button
    Friend WithEvents btAddX As Windows.Forms.Button
    Friend WithEvents btRemoveWeights As Windows.Forms.Button
    Friend WithEvents btAddWeights As Windows.Forms.Button
    Friend WithEvents lblNote As Windows.Forms.Label
    Friend WithEvents lblOffset As Windows.Forms.Label
    Friend WithEvents btRemoveOffset As Windows.Forms.Button
    Friend WithEvents btAddOffset As Windows.Forms.Button
    Friend WithEvents lblY As Windows.Forms.Label
    Friend WithEvents btRemoveY As Windows.Forms.Button
    Friend WithEvents btAddY As Windows.Forms.Button
    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents lblWeights As Windows.Forms.Label
    Friend WithEvents lblX As Windows.Forms.Label
    Friend WithEvents lblSelectedSheet As Windows.Forms.Label
    Friend WithEvents TabPageBuildModel As Windows.Forms.TabPage
    Friend WithEvents cbIntercept As Windows.Forms.CheckBox
    Friend WithEvents btAddEffect As Windows.Forms.Button
    Friend WithEvents btClearAllSelectedEffects As Windows.Forms.Button
    Friend WithEvents tbRemoveSelectedEffects As Windows.Forms.Button
    Friend WithEvents tbInitValues As Windows.Forms.TextBox
    Friend WithEvents lblInitValues As Windows.Forms.Label
    Friend WithEvents lbSelectedEffectsList As Windows.Forms.ListBox
    Friend WithEvents lbSelectedVariables As Windows.Forms.ListBox
    Friend WithEvents lblSelectedEffectsList As Windows.Forms.Label
    Friend WithEvents lblSelectedVariables As Windows.Forms.Label
    Friend WithEvents TabPageOptions As Windows.Forms.TabPage
    Friend WithEvents ckResiduals As Windows.Forms.CheckBox
    Friend WithEvents grpModelSpecification As Windows.Forms.GroupBox
    Friend WithEvents tbDispersionParameterNB2 As Windows.Forms.TextBox
    Friend WithEvents lblDisperisionParameter As Windows.Forms.Label
    Friend WithEvents tbPower As Windows.Forms.TextBox
    Friend WithEvents lblPower As Windows.Forms.Label
    Friend WithEvents cbLink As Windows.Forms.ComboBox
    Friend WithEvents lblLink As Windows.Forms.Label
    Friend WithEvents cbFamily As Windows.Forms.ComboBox
    Friend WithEvents lblFamily As Windows.Forms.Label
    Friend WithEvents grpIterOptions As Windows.Forms.GroupBox
    Friend WithEvents ckIterationsDetails As Windows.Forms.CheckBox
    Friend WithEvents tbMaxIter As Windows.Forms.TextBox
    Friend WithEvents lblMaxIter As Windows.Forms.Label
    Friend WithEvents lblEps As Windows.Forms.Label
    Friend WithEvents tbEps As Windows.Forms.TextBox
    Friend WithEvents lbTime As Windows.Forms.ListBox
    Friend WithEvents lblTime As Windows.Forms.Label
    Friend WithEvents lbClusterID As Windows.Forms.ListBox
    Friend WithEvents lblClusterID As Windows.Forms.Label
    Friend WithEvents ProgressBar1 As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCalculate As Windows.Forms.Button
    Friend WithEvents btRemoveTime As Windows.Forms.Button
    Friend WithEvents btAddTime As Windows.Forms.Button
    Friend WithEvents btRemoveClusterID As Windows.Forms.Button
    Friend WithEvents btAddClusterID As Windows.Forms.Button
    Friend WithEvents ckTrace As Windows.Forms.CheckBox
    Friend WithEvents cbCovarStruct As Windows.Forms.ComboBox
    Friend WithEvents lblCovarStruct As Windows.Forms.Label
    Friend WithEvents ckUseP As Windows.Forms.CheckBox
    Friend WithEvents cbStandardErr As Windows.Forms.ComboBox
    Friend WithEvents lblStdErr As Windows.Forms.Label
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents btAddEffectCategoricalFactor As Windows.Forms.Button
    Friend WithEvents btnCustomInteraction As Windows.Forms.Button
    Friend WithEvents btn2Interactions As Windows.Forms.Button
    Friend WithEvents spinBtnPoly As Windows.Forms.NumericUpDown
    Friend WithEvents btnPoly As Windows.Forms.Button
End Class
