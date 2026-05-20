<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui20PropensityScoreMatching
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui20PropensityScoreMatching))
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1_Data = New System.Windows.Forms.TabPage()
        Me.btRemoveExact = New System.Windows.Forms.Button()
        Me.btAddExact = New System.Windows.Forms.Button()
        Me.lbExact = New System.Windows.Forms.ListBox()
        Me.lblExact = New System.Windows.Forms.Label()
        Me.btRemoveScore = New System.Windows.Forms.Button()
        Me.btAddScore = New System.Windows.Forms.Button()
        Me.lbScore = New System.Windows.Forms.ListBox()
        Me.lblScore = New System.Windows.Forms.Label()
        Me.lbCovariates = New System.Windows.Forms.ListBox()
        Me.btRemoveCovariates = New System.Windows.Forms.Button()
        Me.btAddCovariates = New System.Windows.Forms.Button()
        Me.lblCovariates = New System.Windows.Forms.Label()
        Me.lbTreatment = New System.Windows.Forms.ListBox()
        Me.lblTreatment = New System.Windows.Forms.Label()
        Me.btRemoveTreatment = New System.Windows.Forms.Button()
        Me.btAddTreatment = New System.Windows.Forms.Button()
        Me.lblNote = New System.Windows.Forms.Label()
        Me.btRemoveID = New System.Windows.Forms.Button()
        Me.btAddID = New System.Windows.Forms.Button()
        Me.lbId = New System.Windows.Forms.ListBox()
        Me.lblClusterID = New System.Windows.Forms.Label()
        Me.lbOutcome = New System.Windows.Forms.ListBox()
        Me.lblOutcome = New System.Windows.Forms.Label()
        Me.btRemoveOutcome = New System.Windows.Forms.Button()
        Me.btAddOutcome = New System.Windows.Forms.Button()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.cbSheetsList = New System.Windows.Forms.ComboBox()
        Me.btReload = New System.Windows.Forms.Button()
        Me.lblSelectedSheet = New System.Windows.Forms.Label()
        Me.TabPage2_PropensityModel = New System.Windows.Forms.TabPage()
        Me.btAddEffectCategoricalFactor = New System.Windows.Forms.Button()
        Me.btnCustomInteraction = New System.Windows.Forms.Button()
        Me.btn2Interactions = New System.Windows.Forms.Button()
        Me.spinBtnPoly = New System.Windows.Forms.NumericUpDown()
        Me.btnPoly = New System.Windows.Forms.Button()
        Me.ckIntercept = New System.Windows.Forms.CheckBox()
        Me.btAddEffect = New System.Windows.Forms.Button()
        Me.btClearAllSelectedEffects = New System.Windows.Forms.Button()
        Me.tbRemoveSelectedEffects = New System.Windows.Forms.Button()
        Me.lbSelectedEffectsList = New System.Windows.Forms.ListBox()
        Me.lblSelectedEffectsList = New System.Windows.Forms.Label()
        Me.lbSelectedVariables = New System.Windows.Forms.ListBox()
        Me.lblSelectedVariables = New System.Windows.Forms.Label()
        Me.TabPage3_Options = New System.Windows.Forms.TabPage()
        Me.grpAdjustmentMethod = New System.Windows.Forms.GroupBox()
        Me.cbMatchingOrder = New System.Windows.Forms.ComboBox()
        Me.lblMatchingOrder = New System.Windows.Forms.Label()
        Me.tbCaliper = New System.Windows.Forms.TextBox()
        Me.lblCaliper = New System.Windows.Forms.Label()
        Me.tbCemBins = New System.Windows.Forms.TextBox()
        Me.lblCemBins = New System.Windows.Forms.Label()
        Me.tbStrata = New System.Windows.Forms.TextBox()
        Me.lblStrata = New System.Windows.Forms.Label()
        Me.cbCaliperScale = New System.Windows.Forms.ComboBox()
        Me.lblCaliperScale = New System.Windows.Forms.Label()
        Me.chkWithReplacement = New System.Windows.Forms.CheckBox()
        Me.cbDistanceMetric = New System.Windows.Forms.ComboBox()
        Me.lblDistanceMetric = New System.Windows.Forms.Label()
        Me.cbEstimand = New System.Windows.Forms.ComboBox()
        Me.lblEstimand = New System.Windows.Forms.Label()
        Me.tbMatchingRatio = New System.Windows.Forms.TextBox()
        Me.lblMatchingRatio = New System.Windows.Forms.Label()
        Me.cbRunMethod = New System.Windows.Forms.ComboBox()
        Me.lblRunMethod = New System.Windows.Forms.Label()
        Me.grpIterOptions = New System.Windows.Forms.GroupBox()
        Me.cbScoreMethod = New System.Windows.Forms.ComboBox()
        Me.lblScoreMethod = New System.Windows.Forms.Label()
        Me.tbTrimUpper = New System.Windows.Forms.TextBox()
        Me.lblTrimUpper = New System.Windows.Forms.Label()
        Me.tbTrimLower = New System.Windows.Forms.TextBox()
        Me.lblTrimLower = New System.Windows.Forms.Label()
        Me.lblRidgePenalty = New System.Windows.Forms.Label()
        Me.tbRidgePenalty = New System.Windows.Forms.TextBox()
        Me.cbCommonSupport = New System.Windows.Forms.ComboBox()
        Me.lblCommonSupport = New System.Windows.Forms.Label()
        Me.tbMaxIterations = New System.Windows.Forms.TextBox()
        Me.lblMaxIterations = New System.Windows.Forms.Label()
        Me.lblTolerance = New System.Windows.Forms.Label()
        Me.tbTolerance = New System.Windows.Forms.TextBox()
        Me.chkStandardizeCovariates = New System.Windows.Forms.CheckBox()
        Me.TabPage4_DiagnosticsOutputs = New System.Windows.Forms.TabPage()
        Me.grpOutputs = New System.Windows.Forms.GroupBox()
        Me.chkWriteDiagnostics = New System.Windows.Forms.CheckBox()
        Me.chkWriteMatches = New System.Windows.Forms.CheckBox()
        Me.grpDiagnostics = New System.Windows.Forms.GroupBox()
        Me.tbExtremeWeight = New System.Windows.Forms.TextBox()
        Me.lblExtremeWeight = New System.Windows.Forms.Label()
        Me.tbOverlapBins = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkLovePlot = New System.Windows.Forms.CheckBox()
        Me.chkWeightDiagnostics = New System.Windows.Forms.CheckBox()
        Me.chkOverlapDiagnostics = New System.Windows.Forms.CheckBox()
        Me.chkDoublyRobust = New System.Windows.Forms.CheckBox()
        Me.lblDiagnosticsNote = New System.Windows.Forms.Label()
        Me.tbLoveThreshold = New System.Windows.Forms.TextBox()
        Me.lblLoveThreshold = New System.Windows.Forms.Label()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCalculate = New System.Windows.Forms.Button()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1_Data.SuspendLayout()
        Me.TabPage2_PropensityModel.SuspendLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage3_Options.SuspendLayout()
        Me.grpAdjustmentMethod.SuspendLayout()
        Me.grpIterOptions.SuspendLayout()
        Me.TabPage4_DiagnosticsOutputs.SuspendLayout()
        Me.grpOutputs.SuspendLayout()
        Me.grpDiagnostics.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1_Data)
        Me.TabControl1.Controls.Add(Me.TabPage2_PropensityModel)
        Me.TabControl1.Controls.Add(Me.TabPage3_Options)
        Me.TabControl1.Controls.Add(Me.TabPage4_DiagnosticsOutputs)
        Me.TabControl1.Location = New System.Drawing.Point(-1, -1)
        Me.TabControl1.MinimumSize = New System.Drawing.Size(837, 457)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(837, 457)
        Me.TabControl1.TabIndex = 0
        '
        'TabPage1_Data
        '
        Me.TabPage1_Data.Controls.Add(Me.btRemoveExact)
        Me.TabPage1_Data.Controls.Add(Me.btAddExact)
        Me.TabPage1_Data.Controls.Add(Me.lbExact)
        Me.TabPage1_Data.Controls.Add(Me.lblExact)
        Me.TabPage1_Data.Controls.Add(Me.btRemoveScore)
        Me.TabPage1_Data.Controls.Add(Me.btAddScore)
        Me.TabPage1_Data.Controls.Add(Me.lbScore)
        Me.TabPage1_Data.Controls.Add(Me.lblScore)
        Me.TabPage1_Data.Controls.Add(Me.lbCovariates)
        Me.TabPage1_Data.Controls.Add(Me.btRemoveCovariates)
        Me.TabPage1_Data.Controls.Add(Me.btAddCovariates)
        Me.TabPage1_Data.Controls.Add(Me.lblCovariates)
        Me.TabPage1_Data.Controls.Add(Me.lbTreatment)
        Me.TabPage1_Data.Controls.Add(Me.lblTreatment)
        Me.TabPage1_Data.Controls.Add(Me.btRemoveTreatment)
        Me.TabPage1_Data.Controls.Add(Me.btAddTreatment)
        Me.TabPage1_Data.Controls.Add(Me.lblNote)
        Me.TabPage1_Data.Controls.Add(Me.btRemoveID)
        Me.TabPage1_Data.Controls.Add(Me.btAddID)
        Me.TabPage1_Data.Controls.Add(Me.lbId)
        Me.TabPage1_Data.Controls.Add(Me.lblClusterID)
        Me.TabPage1_Data.Controls.Add(Me.lbOutcome)
        Me.TabPage1_Data.Controls.Add(Me.lblOutcome)
        Me.TabPage1_Data.Controls.Add(Me.btRemoveOutcome)
        Me.TabPage1_Data.Controls.Add(Me.btAddOutcome)
        Me.TabPage1_Data.Controls.Add(Me.lblAllColumns)
        Me.TabPage1_Data.Controls.Add(Me.lbAllColumns)
        Me.TabPage1_Data.Controls.Add(Me.cbSheetsList)
        Me.TabPage1_Data.Controls.Add(Me.btReload)
        Me.TabPage1_Data.Controls.Add(Me.lblSelectedSheet)
        Me.TabPage1_Data.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1_Data.Name = "TabPage1_Data"
        Me.TabPage1_Data.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1_Data.Size = New System.Drawing.Size(829, 428)
        Me.TabPage1_Data.TabIndex = 0
        Me.TabPage1_Data.Text = "Data"
        Me.TabPage1_Data.UseVisualStyleBackColor = True
        '
        'btRemoveExact
        '
        Me.btRemoveExact.Location = New System.Drawing.Point(291, 190)
        Me.btRemoveExact.Name = "btRemoveExact"
        Me.btRemoveExact.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveExact.TabIndex = 54
        Me.btRemoveExact.Text = "<<"
        Me.btRemoveExact.UseVisualStyleBackColor = True
        '
        'btAddExact
        '
        Me.btAddExact.Location = New System.Drawing.Point(246, 190)
        Me.btAddExact.Name = "btAddExact"
        Me.btAddExact.Size = New System.Drawing.Size(39, 23)
        Me.btAddExact.TabIndex = 53
        Me.btAddExact.Text = ">>"
        Me.btAddExact.UseVisualStyleBackColor = True
        '
        'lbExact
        '
        Me.lbExact.FormattingEnabled = True
        Me.lbExact.ItemHeight = 16
        Me.lbExact.Location = New System.Drawing.Point(336, 190)
        Me.lbExact.Name = "lbExact"
        Me.lbExact.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbExact.Size = New System.Drawing.Size(221, 20)
        Me.lbExact.TabIndex = 51
        '
        'lblExact
        '
        Me.lblExact.AutoSize = True
        Me.lblExact.BackColor = System.Drawing.Color.Transparent
        Me.lblExact.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblExact.Location = New System.Drawing.Point(336, 171)
        Me.lblExact.Name = "lblExact"
        Me.lblExact.Size = New System.Drawing.Size(101, 16)
        Me.lblExact.TabIndex = 52
        Me.lblExact.Text = "Exact group**"
        '
        'btRemoveScore
        '
        Me.btRemoveScore.Location = New System.Drawing.Point(291, 148)
        Me.btRemoveScore.Name = "btRemoveScore"
        Me.btRemoveScore.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveScore.TabIndex = 50
        Me.btRemoveScore.Text = "<<"
        Me.btRemoveScore.UseVisualStyleBackColor = True
        '
        'btAddScore
        '
        Me.btAddScore.Location = New System.Drawing.Point(246, 148)
        Me.btAddScore.Name = "btAddScore"
        Me.btAddScore.Size = New System.Drawing.Size(39, 23)
        Me.btAddScore.TabIndex = 49
        Me.btAddScore.Text = ">>"
        Me.btAddScore.UseVisualStyleBackColor = True
        '
        'lbScore
        '
        Me.lbScore.FormattingEnabled = True
        Me.lbScore.ItemHeight = 16
        Me.lbScore.Location = New System.Drawing.Point(336, 148)
        Me.lbScore.Name = "lbScore"
        Me.lbScore.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbScore.Size = New System.Drawing.Size(221, 20)
        Me.lbScore.TabIndex = 47
        '
        'lblScore
        '
        Me.lblScore.AutoSize = True
        Me.lblScore.BackColor = System.Drawing.Color.Transparent
        Me.lblScore.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblScore.Location = New System.Drawing.Point(336, 129)
        Me.lblScore.Name = "lblScore"
        Me.lblScore.Size = New System.Drawing.Size(130, 16)
        Me.lblScore.TabIndex = 48
        Me.lblScore.Text = "Supplied score***"
        '
        'lbCovariates
        '
        Me.lbCovariates.FormattingEnabled = True
        Me.lbCovariates.ItemHeight = 16
        Me.lbCovariates.Location = New System.Drawing.Point(336, 232)
        Me.lbCovariates.Name = "lbCovariates"
        Me.lbCovariates.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbCovariates.Size = New System.Drawing.Size(221, 196)
        Me.lbCovariates.TabIndex = 45
        '
        'btRemoveCovariates
        '
        Me.btRemoveCovariates.Location = New System.Drawing.Point(291, 232)
        Me.btRemoveCovariates.Name = "btRemoveCovariates"
        Me.btRemoveCovariates.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveCovariates.TabIndex = 44
        Me.btRemoveCovariates.Text = "<<"
        Me.btRemoveCovariates.UseVisualStyleBackColor = True
        '
        'btAddCovariates
        '
        Me.btAddCovariates.Location = New System.Drawing.Point(246, 232)
        Me.btAddCovariates.Name = "btAddCovariates"
        Me.btAddCovariates.Size = New System.Drawing.Size(39, 23)
        Me.btAddCovariates.TabIndex = 43
        Me.btAddCovariates.Text = ">>"
        Me.btAddCovariates.UseVisualStyleBackColor = True
        '
        'lblCovariates
        '
        Me.lblCovariates.AutoSize = True
        Me.lblCovariates.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCovariates.Location = New System.Drawing.Point(336, 213)
        Me.lblCovariates.Name = "lblCovariates"
        Me.lblCovariates.Size = New System.Drawing.Size(88, 16)
        Me.lblCovariates.TabIndex = 46
        Me.lblCovariates.Text = "Covariates*"
        '
        'lbTreatment
        '
        Me.lbTreatment.FormattingEnabled = True
        Me.lbTreatment.ItemHeight = 16
        Me.lbTreatment.Location = New System.Drawing.Point(336, 23)
        Me.lbTreatment.Name = "lbTreatment"
        Me.lbTreatment.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbTreatment.Size = New System.Drawing.Size(221, 20)
        Me.lbTreatment.TabIndex = 41
        '
        'lblTreatment
        '
        Me.lblTreatment.AutoSize = True
        Me.lblTreatment.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTreatment.Location = New System.Drawing.Point(336, 3)
        Me.lblTreatment.Name = "lblTreatment"
        Me.lblTreatment.Size = New System.Drawing.Size(118, 16)
        Me.lblTreatment.TabIndex = 42
        Me.lblTreatment.Text = "Treatment (0/1)*"
        '
        'btRemoveTreatment
        '
        Me.btRemoveTreatment.Location = New System.Drawing.Point(291, 23)
        Me.btRemoveTreatment.Name = "btRemoveTreatment"
        Me.btRemoveTreatment.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveTreatment.TabIndex = 40
        Me.btRemoveTreatment.Text = "<<"
        Me.btRemoveTreatment.UseVisualStyleBackColor = True
        '
        'btAddTreatment
        '
        Me.btAddTreatment.Location = New System.Drawing.Point(246, 23)
        Me.btAddTreatment.Name = "btAddTreatment"
        Me.btAddTreatment.Size = New System.Drawing.Size(39, 23)
        Me.btAddTreatment.TabIndex = 39
        Me.btAddTreatment.Text = ">>"
        Me.btAddTreatment.UseVisualStyleBackColor = True
        '
        'lblNote
        '
        Me.lblNote.Location = New System.Drawing.Point(559, 302)
        Me.lblNote.Name = "lblNote"
        Me.lblNote.Size = New System.Drawing.Size(264, 123)
        Me.lblNote.TabIndex = 38
        Me.lblNote.Text = resources.GetString("lblNote.Text")
        '
        'btRemoveID
        '
        Me.btRemoveID.Location = New System.Drawing.Point(291, 106)
        Me.btRemoveID.Name = "btRemoveID"
        Me.btRemoveID.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveID.TabIndex = 37
        Me.btRemoveID.Text = "<<"
        Me.btRemoveID.UseVisualStyleBackColor = True
        '
        'btAddID
        '
        Me.btAddID.Location = New System.Drawing.Point(246, 106)
        Me.btAddID.Name = "btAddID"
        Me.btAddID.Size = New System.Drawing.Size(39, 23)
        Me.btAddID.TabIndex = 36
        Me.btAddID.Text = ">>"
        Me.btAddID.UseVisualStyleBackColor = True
        '
        'lbId
        '
        Me.lbId.FormattingEnabled = True
        Me.lbId.ItemHeight = 16
        Me.lbId.Location = New System.Drawing.Point(336, 106)
        Me.lbId.Name = "lbId"
        Me.lbId.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbId.Size = New System.Drawing.Size(221, 20)
        Me.lbId.TabIndex = 34
        '
        'lblClusterID
        '
        Me.lblClusterID.AutoSize = True
        Me.lblClusterID.BackColor = System.Drawing.Color.Transparent
        Me.lblClusterID.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblClusterID.Location = New System.Drawing.Point(336, 87)
        Me.lblClusterID.Name = "lblClusterID"
        Me.lblClusterID.Size = New System.Drawing.Size(34, 16)
        Me.lblClusterID.TabIndex = 35
        Me.lblClusterID.Text = "ID**"
        '
        'lbOutcome
        '
        Me.lbOutcome.FormattingEnabled = True
        Me.lbOutcome.ItemHeight = 16
        Me.lbOutcome.Location = New System.Drawing.Point(336, 64)
        Me.lbOutcome.Name = "lbOutcome"
        Me.lbOutcome.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbOutcome.Size = New System.Drawing.Size(221, 20)
        Me.lbOutcome.TabIndex = 32
        '
        'lblOutcome
        '
        Me.lblOutcome.AutoSize = True
        Me.lblOutcome.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOutcome.Location = New System.Drawing.Point(336, 45)
        Me.lblOutcome.Name = "lblOutcome"
        Me.lblOutcome.Size = New System.Drawing.Size(80, 16)
        Me.lblOutcome.TabIndex = 33
        Me.lblOutcome.Text = "Outcome**"
        '
        'btRemoveOutcome
        '
        Me.btRemoveOutcome.Location = New System.Drawing.Point(291, 61)
        Me.btRemoveOutcome.Name = "btRemoveOutcome"
        Me.btRemoveOutcome.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveOutcome.TabIndex = 31
        Me.btRemoveOutcome.Text = "<<"
        Me.btRemoveOutcome.UseVisualStyleBackColor = True
        '
        'btAddOutcome
        '
        Me.btAddOutcome.Location = New System.Drawing.Point(246, 61)
        Me.btAddOutcome.Name = "btAddOutcome"
        Me.btAddOutcome.Size = New System.Drawing.Size(39, 23)
        Me.btAddOutcome.TabIndex = 30
        Me.btAddOutcome.Text = ">>"
        Me.btAddOutcome.UseVisualStyleBackColor = True
        '
        'lblAllColumns
        '
        Me.lblAllColumns.AutoSize = True
        Me.lblAllColumns.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAllColumns.Location = New System.Drawing.Point(9, 3)
        Me.lblAllColumns.Name = "lblAllColumns"
        Me.lblAllColumns.Size = New System.Drawing.Size(134, 16)
        Me.lblAllColumns.TabIndex = 28
        Me.lblAllColumns.Text = "Available columns"
        '
        'lbAllColumns
        '
        Me.lbAllColumns.FormattingEnabled = True
        Me.lbAllColumns.ItemHeight = 16
        Me.lbAllColumns.Location = New System.Drawing.Point(3, 23)
        Me.lbAllColumns.Name = "lbAllColumns"
        Me.lbAllColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbAllColumns.Size = New System.Drawing.Size(231, 404)
        Me.lbAllColumns.TabIndex = 27
        '
        'cbSheetsList
        '
        Me.cbSheetsList.FormattingEnabled = True
        Me.cbSheetsList.Location = New System.Drawing.Point(583, 23)
        Me.cbSheetsList.Name = "cbSheetsList"
        Me.cbSheetsList.Size = New System.Drawing.Size(240, 24)
        Me.cbSheetsList.TabIndex = 25
        Me.cbSheetsList.Text = "Select Sheet"
        '
        'btReload
        '
        Me.btReload.Location = New System.Drawing.Point(582, 52)
        Me.btReload.Name = "btReload"
        Me.btReload.Size = New System.Drawing.Size(130, 23)
        Me.btReload.TabIndex = 24
        Me.btReload.Text = "Reload Sheet Data"
        Me.btReload.UseVisualStyleBackColor = True
        '
        'lblSelectedSheet
        '
        Me.lblSelectedSheet.AutoSize = True
        Me.lblSelectedSheet.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedSheet.Location = New System.Drawing.Point(580, 3)
        Me.lblSelectedSheet.Name = "lblSelectedSheet"
        Me.lblSelectedSheet.Size = New System.Drawing.Size(132, 16)
        Me.lblSelectedSheet.TabIndex = 26
        Me.lblSelectedSheet.Text = "Active Worksheet:"
        '
        'TabPage2_PropensityModel
        '
        Me.TabPage2_PropensityModel.Controls.Add(Me.btAddEffectCategoricalFactor)
        Me.TabPage2_PropensityModel.Controls.Add(Me.btnCustomInteraction)
        Me.TabPage2_PropensityModel.Controls.Add(Me.btn2Interactions)
        Me.TabPage2_PropensityModel.Controls.Add(Me.spinBtnPoly)
        Me.TabPage2_PropensityModel.Controls.Add(Me.btnPoly)
        Me.TabPage2_PropensityModel.Controls.Add(Me.ckIntercept)
        Me.TabPage2_PropensityModel.Controls.Add(Me.btAddEffect)
        Me.TabPage2_PropensityModel.Controls.Add(Me.btClearAllSelectedEffects)
        Me.TabPage2_PropensityModel.Controls.Add(Me.tbRemoveSelectedEffects)
        Me.TabPage2_PropensityModel.Controls.Add(Me.lbSelectedEffectsList)
        Me.TabPage2_PropensityModel.Controls.Add(Me.lblSelectedEffectsList)
        Me.TabPage2_PropensityModel.Controls.Add(Me.lbSelectedVariables)
        Me.TabPage2_PropensityModel.Controls.Add(Me.lblSelectedVariables)
        Me.TabPage2_PropensityModel.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2_PropensityModel.Name = "TabPage2_PropensityModel"
        Me.TabPage2_PropensityModel.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2_PropensityModel.Size = New System.Drawing.Size(829, 428)
        Me.TabPage2_PropensityModel.TabIndex = 1
        Me.TabPage2_PropensityModel.Text = "Propensity model"
        Me.TabPage2_PropensityModel.UseVisualStyleBackColor = True
        '
        'btAddEffectCategoricalFactor
        '
        Me.btAddEffectCategoricalFactor.Location = New System.Drawing.Point(325, 51)
        Me.btAddEffectCategoricalFactor.Name = "btAddEffectCategoricalFactor"
        Me.btAddEffectCategoricalFactor.Size = New System.Drawing.Size(191, 23)
        Me.btAddEffectCategoricalFactor.TabIndex = 37
        Me.btAddEffectCategoricalFactor.Text = "Add as Categorical Factor >>"
        Me.btAddEffectCategoricalFactor.UseVisualStyleBackColor = True
        '
        'btnCustomInteraction
        '
        Me.btnCustomInteraction.Location = New System.Drawing.Point(325, 138)
        Me.btnCustomInteraction.Name = "btnCustomInteraction"
        Me.btnCustomInteraction.Size = New System.Drawing.Size(191, 23)
        Me.btnCustomInteraction.TabIndex = 36
        Me.btnCustomInteraction.Text = "Custom Interaction >>"
        Me.btnCustomInteraction.UseVisualStyleBackColor = True
        '
        'btn2Interactions
        '
        Me.btn2Interactions.Location = New System.Drawing.Point(325, 109)
        Me.btn2Interactions.Name = "btn2Interactions"
        Me.btn2Interactions.Size = New System.Drawing.Size(191, 23)
        Me.btn2Interactions.TabIndex = 35
        Me.btn2Interactions.Text = "2-way Interactions >>"
        Me.btn2Interactions.UseVisualStyleBackColor = True
        '
        'spinBtnPoly
        '
        Me.spinBtnPoly.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnPoly.Location = New System.Drawing.Point(472, 80)
        Me.spinBtnPoly.Maximum = New Decimal(New Integer() {1000000, 0, 0, 196608})
        Me.spinBtnPoly.Name = "spinBtnPoly"
        Me.spinBtnPoly.Size = New System.Drawing.Size(44, 22)
        Me.spinBtnPoly.TabIndex = 34
        Me.spinBtnPoly.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'btnPoly
        '
        Me.btnPoly.Location = New System.Drawing.Point(325, 80)
        Me.btnPoly.Name = "btnPoly"
        Me.btnPoly.Size = New System.Drawing.Size(131, 23)
        Me.btnPoly.TabIndex = 33
        Me.btnPoly.Text = "Poly >>"
        Me.btnPoly.UseVisualStyleBackColor = True
        '
        'ckIntercept
        '
        Me.ckIntercept.AutoSize = True
        Me.ckIntercept.Checked = True
        Me.ckIntercept.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckIntercept.Location = New System.Drawing.Point(374, 167)
        Me.ckIntercept.Name = "ckIntercept"
        Me.ckIntercept.Size = New System.Drawing.Size(80, 20)
        Me.ckIntercept.TabIndex = 32
        Me.ckIntercept.Text = "Intercept"
        Me.ckIntercept.UseVisualStyleBackColor = True
        '
        'btAddEffect
        '
        Me.btAddEffect.Location = New System.Drawing.Point(379, 22)
        Me.btAddEffect.Name = "btAddEffect"
        Me.btAddEffect.Size = New System.Drawing.Size(75, 23)
        Me.btAddEffect.TabIndex = 31
        Me.btAddEffect.Text = "Add >>"
        Me.btAddEffect.UseVisualStyleBackColor = True
        '
        'btClearAllSelectedEffects
        '
        Me.btClearAllSelectedEffects.AutoEllipsis = True
        Me.btClearAllSelectedEffects.Location = New System.Drawing.Point(719, 399)
        Me.btClearAllSelectedEffects.Name = "btClearAllSelectedEffects"
        Me.btClearAllSelectedEffects.Size = New System.Drawing.Size(94, 23)
        Me.btClearAllSelectedEffects.TabIndex = 30
        Me.btClearAllSelectedEffects.Text = "Clear All"
        Me.btClearAllSelectedEffects.UseVisualStyleBackColor = True
        '
        'tbRemoveSelectedEffects
        '
        Me.tbRemoveSelectedEffects.AutoEllipsis = True
        Me.tbRemoveSelectedEffects.Location = New System.Drawing.Point(555, 400)
        Me.tbRemoveSelectedEffects.Name = "tbRemoveSelectedEffects"
        Me.tbRemoveSelectedEffects.Size = New System.Drawing.Size(91, 23)
        Me.tbRemoveSelectedEffects.TabIndex = 29
        Me.tbRemoveSelectedEffects.Text = "Remove"
        Me.tbRemoveSelectedEffects.UseVisualStyleBackColor = True
        '
        'lbSelectedEffectsList
        '
        Me.lbSelectedEffectsList.FormattingEnabled = True
        Me.lbSelectedEffectsList.ItemHeight = 16
        Me.lbSelectedEffectsList.Location = New System.Drawing.Point(541, 22)
        Me.lbSelectedEffectsList.Name = "lbSelectedEffectsList"
        Me.lbSelectedEffectsList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedEffectsList.Size = New System.Drawing.Size(282, 372)
        Me.lbSelectedEffectsList.TabIndex = 27
        '
        'lblSelectedEffectsList
        '
        Me.lblSelectedEffectsList.AutoSize = True
        Me.lblSelectedEffectsList.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedEffectsList.Location = New System.Drawing.Point(552, 3)
        Me.lblSelectedEffectsList.Name = "lblSelectedEffectsList"
        Me.lblSelectedEffectsList.Size = New System.Drawing.Size(120, 16)
        Me.lblSelectedEffectsList.TabIndex = 28
        Me.lblSelectedEffectsList.Text = "Selected Effects"
        '
        'lbSelectedVariables
        '
        Me.lbSelectedVariables.FormattingEnabled = True
        Me.lbSelectedVariables.ItemHeight = 16
        Me.lbSelectedVariables.Location = New System.Drawing.Point(3, 22)
        Me.lbSelectedVariables.Name = "lbSelectedVariables"
        Me.lbSelectedVariables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbSelectedVariables.Size = New System.Drawing.Size(292, 404)
        Me.lbSelectedVariables.TabIndex = 4
        '
        'lblSelectedVariables
        '
        Me.lblSelectedVariables.AutoSize = True
        Me.lblSelectedVariables.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedVariables.Location = New System.Drawing.Point(3, 3)
        Me.lblSelectedVariables.Name = "lblSelectedVariables"
        Me.lblSelectedVariables.Size = New System.Drawing.Size(140, 16)
        Me.lblSelectedVariables.TabIndex = 5
        Me.lblSelectedVariables.Text = "Selected Variables"
        '
        'TabPage3_Options
        '
        Me.TabPage3_Options.Controls.Add(Me.grpAdjustmentMethod)
        Me.TabPage3_Options.Controls.Add(Me.grpIterOptions)
        Me.TabPage3_Options.Controls.Add(Me.chkStandardizeCovariates)
        Me.TabPage3_Options.Location = New System.Drawing.Point(4, 25)
        Me.TabPage3_Options.Name = "TabPage3_Options"
        Me.TabPage3_Options.Size = New System.Drawing.Size(829, 428)
        Me.TabPage3_Options.TabIndex = 2
        Me.TabPage3_Options.Text = "Options"
        Me.TabPage3_Options.UseVisualStyleBackColor = True
        '
        'grpAdjustmentMethod
        '
        Me.grpAdjustmentMethod.Controls.Add(Me.cbMatchingOrder)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblMatchingOrder)
        Me.grpAdjustmentMethod.Controls.Add(Me.tbCaliper)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblCaliper)
        Me.grpAdjustmentMethod.Controls.Add(Me.tbCemBins)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblCemBins)
        Me.grpAdjustmentMethod.Controls.Add(Me.tbStrata)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblStrata)
        Me.grpAdjustmentMethod.Controls.Add(Me.cbCaliperScale)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblCaliperScale)
        Me.grpAdjustmentMethod.Controls.Add(Me.chkWithReplacement)
        Me.grpAdjustmentMethod.Controls.Add(Me.cbDistanceMetric)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblDistanceMetric)
        Me.grpAdjustmentMethod.Controls.Add(Me.cbEstimand)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblEstimand)
        Me.grpAdjustmentMethod.Controls.Add(Me.tbMatchingRatio)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblMatchingRatio)
        Me.grpAdjustmentMethod.Controls.Add(Me.cbRunMethod)
        Me.grpAdjustmentMethod.Controls.Add(Me.lblRunMethod)
        Me.grpAdjustmentMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpAdjustmentMethod.Location = New System.Drawing.Point(383, 12)
        Me.grpAdjustmentMethod.Name = "grpAdjustmentMethod"
        Me.grpAdjustmentMethod.Size = New System.Drawing.Size(398, 312)
        Me.grpAdjustmentMethod.TabIndex = 41
        Me.grpAdjustmentMethod.TabStop = False
        Me.grpAdjustmentMethod.Text = "Adjustment method"
        '
        'cbMatchingOrder
        '
        Me.cbMatchingOrder.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbMatchingOrder.FormattingEnabled = True
        Me.cbMatchingOrder.Location = New System.Drawing.Point(159, 227)
        Me.cbMatchingOrder.Name = "cbMatchingOrder"
        Me.cbMatchingOrder.Size = New System.Drawing.Size(222, 24)
        Me.cbMatchingOrder.TabIndex = 50
        '
        'lblMatchingOrder
        '
        Me.lblMatchingOrder.AutoSize = True
        Me.lblMatchingOrder.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMatchingOrder.Location = New System.Drawing.Point(52, 230)
        Me.lblMatchingOrder.Name = "lblMatchingOrder"
        Me.lblMatchingOrder.Size = New System.Drawing.Size(96, 16)
        Me.lblMatchingOrder.TabIndex = 49
        Me.lblMatchingOrder.Text = "Matching order"
        '
        'tbCaliper
        '
        Me.tbCaliper.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbCaliper.Location = New System.Drawing.Point(159, 198)
        Me.tbCaliper.Name = "tbCaliper"
        Me.tbCaliper.Size = New System.Drawing.Size(125, 22)
        Me.tbCaliper.TabIndex = 48
        '
        'lblCaliper
        '
        Me.lblCaliper.AutoSize = True
        Me.lblCaliper.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCaliper.Location = New System.Drawing.Point(62, 201)
        Me.lblCaliper.Name = "lblCaliper"
        Me.lblCaliper.Size = New System.Drawing.Size(86, 16)
        Me.lblCaliper.TabIndex = 47
        Me.lblCaliper.Text = "Caliper value"
        '
        'tbCemBins
        '
        Me.tbCemBins.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbCemBins.Location = New System.Drawing.Point(160, 285)
        Me.tbCemBins.Name = "tbCemBins"
        Me.tbCemBins.Size = New System.Drawing.Size(125, 22)
        Me.tbCemBins.TabIndex = 46
        Me.tbCemBins.Text = "5"
        '
        'lblCemBins
        '
        Me.lblCemBins.AutoSize = True
        Me.lblCemBins.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCemBins.Location = New System.Drawing.Point(34, 288)
        Me.lblCemBins.Name = "lblCemBins"
        Me.lblCemBins.Size = New System.Drawing.Size(114, 16)
        Me.lblCemBins.TabIndex = 45
        Me.lblCemBins.Text = "CEM quantile bins"
        '
        'tbStrata
        '
        Me.tbStrata.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbStrata.Location = New System.Drawing.Point(160, 257)
        Me.tbStrata.Name = "tbStrata"
        Me.tbStrata.Size = New System.Drawing.Size(125, 22)
        Me.tbStrata.TabIndex = 44
        Me.tbStrata.Text = "5"
        '
        'lblStrata
        '
        Me.lblStrata.AutoSize = True
        Me.lblStrata.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStrata.Location = New System.Drawing.Point(4, 260)
        Me.lblStrata.Name = "lblStrata"
        Me.lblStrata.Size = New System.Drawing.Size(144, 16)
        Me.lblStrata.TabIndex = 43
        Me.lblStrata.Text = "Subclassification strata"
        '
        'cbCaliperScale
        '
        Me.cbCaliperScale.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbCaliperScale.FormattingEnabled = True
        Me.cbCaliperScale.Location = New System.Drawing.Point(159, 168)
        Me.cbCaliperScale.Name = "cbCaliperScale"
        Me.cbCaliperScale.Size = New System.Drawing.Size(222, 24)
        Me.cbCaliperScale.TabIndex = 42
        '
        'lblCaliperScale
        '
        Me.lblCaliperScale.AutoSize = True
        Me.lblCaliperScale.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCaliperScale.Location = New System.Drawing.Point(60, 171)
        Me.lblCaliperScale.Name = "lblCaliperScale"
        Me.lblCaliperScale.Size = New System.Drawing.Size(86, 16)
        Me.lblCaliperScale.TabIndex = 41
        Me.lblCaliperScale.Text = "Caliper scale"
        '
        'chkWithReplacement
        '
        Me.chkWithReplacement.AutoSize = True
        Me.chkWithReplacement.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWithReplacement.Location = New System.Drawing.Point(159, 142)
        Me.chkWithReplacement.Name = "chkWithReplacement"
        Me.chkWithReplacement.Size = New System.Drawing.Size(133, 20)
        Me.chkWithReplacement.TabIndex = 40
        Me.chkWithReplacement.Text = "With replacement"
        Me.chkWithReplacement.UseVisualStyleBackColor = True
        '
        'cbDistanceMetric
        '
        Me.cbDistanceMetric.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDistanceMetric.FormattingEnabled = True
        Me.cbDistanceMetric.Location = New System.Drawing.Point(159, 84)
        Me.cbDistanceMetric.Name = "cbDistanceMetric"
        Me.cbDistanceMetric.Size = New System.Drawing.Size(222, 24)
        Me.cbDistanceMetric.TabIndex = 31
        '
        'lblDistanceMetric
        '
        Me.lblDistanceMetric.AutoSize = True
        Me.lblDistanceMetric.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDistanceMetric.Location = New System.Drawing.Point(49, 87)
        Me.lblDistanceMetric.Name = "lblDistanceMetric"
        Me.lblDistanceMetric.Size = New System.Drawing.Size(99, 16)
        Me.lblDistanceMetric.TabIndex = 30
        Me.lblDistanceMetric.Text = "Distance metric"
        '
        'cbEstimand
        '
        Me.cbEstimand.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbEstimand.FormattingEnabled = True
        Me.cbEstimand.Location = New System.Drawing.Point(159, 54)
        Me.cbEstimand.Name = "cbEstimand"
        Me.cbEstimand.Size = New System.Drawing.Size(222, 24)
        Me.cbEstimand.TabIndex = 29
        '
        'lblEstimand
        '
        Me.lblEstimand.AutoSize = True
        Me.lblEstimand.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEstimand.Location = New System.Drawing.Point(85, 57)
        Me.lblEstimand.Name = "lblEstimand"
        Me.lblEstimand.Size = New System.Drawing.Size(63, 16)
        Me.lblEstimand.TabIndex = 28
        Me.lblEstimand.Text = "Estimand"
        '
        'tbMatchingRatio
        '
        Me.tbMatchingRatio.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMatchingRatio.Location = New System.Drawing.Point(160, 114)
        Me.tbMatchingRatio.Name = "tbMatchingRatio"
        Me.tbMatchingRatio.Size = New System.Drawing.Size(125, 22)
        Me.tbMatchingRatio.TabIndex = 25
        Me.tbMatchingRatio.Text = "1"
        '
        'lblMatchingRatio
        '
        Me.lblMatchingRatio.AutoSize = True
        Me.lblMatchingRatio.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMatchingRatio.Location = New System.Drawing.Point(52, 117)
        Me.lblMatchingRatio.Name = "lblMatchingRatio"
        Me.lblMatchingRatio.Size = New System.Drawing.Size(90, 16)
        Me.lblMatchingRatio.TabIndex = 24
        Me.lblMatchingRatio.Text = "Matching ratio"
        '
        'cbRunMethod
        '
        Me.cbRunMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbRunMethod.FormattingEnabled = True
        Me.cbRunMethod.Location = New System.Drawing.Point(159, 23)
        Me.cbRunMethod.Name = "cbRunMethod"
        Me.cbRunMethod.Size = New System.Drawing.Size(222, 24)
        Me.cbRunMethod.TabIndex = 20
        '
        'lblRunMethod
        '
        Me.lblRunMethod.AutoSize = True
        Me.lblRunMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRunMethod.Location = New System.Drawing.Point(69, 26)
        Me.lblRunMethod.Name = "lblRunMethod"
        Me.lblRunMethod.Size = New System.Drawing.Size(79, 16)
        Me.lblRunMethod.TabIndex = 19
        Me.lblRunMethod.Text = "Run method"
        '
        'grpIterOptions
        '
        Me.grpIterOptions.Controls.Add(Me.cbScoreMethod)
        Me.grpIterOptions.Controls.Add(Me.lblScoreMethod)
        Me.grpIterOptions.Controls.Add(Me.tbTrimUpper)
        Me.grpIterOptions.Controls.Add(Me.lblTrimUpper)
        Me.grpIterOptions.Controls.Add(Me.tbTrimLower)
        Me.grpIterOptions.Controls.Add(Me.lblTrimLower)
        Me.grpIterOptions.Controls.Add(Me.lblRidgePenalty)
        Me.grpIterOptions.Controls.Add(Me.tbRidgePenalty)
        Me.grpIterOptions.Controls.Add(Me.cbCommonSupport)
        Me.grpIterOptions.Controls.Add(Me.lblCommonSupport)
        Me.grpIterOptions.Controls.Add(Me.tbMaxIterations)
        Me.grpIterOptions.Controls.Add(Me.lblMaxIterations)
        Me.grpIterOptions.Controls.Add(Me.lblTolerance)
        Me.grpIterOptions.Controls.Add(Me.tbTolerance)
        Me.grpIterOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpIterOptions.Location = New System.Drawing.Point(9, 12)
        Me.grpIterOptions.Name = "grpIterOptions"
        Me.grpIterOptions.Size = New System.Drawing.Size(368, 231)
        Me.grpIterOptions.TabIndex = 40
        Me.grpIterOptions.TabStop = False
        Me.grpIterOptions.Text = "Score / trimming options"
        '
        'cbScoreMethod
        '
        Me.cbScoreMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbScoreMethod.FormattingEnabled = True
        Me.cbScoreMethod.Location = New System.Drawing.Point(142, 21)
        Me.cbScoreMethod.Name = "cbScoreMethod"
        Me.cbScoreMethod.Size = New System.Drawing.Size(222, 24)
        Me.cbScoreMethod.TabIndex = 29
        '
        'lblScoreMethod
        '
        Me.lblScoreMethod.AutoSize = True
        Me.lblScoreMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblScoreMethod.Location = New System.Drawing.Point(26, 24)
        Me.lblScoreMethod.Name = "lblScoreMethod"
        Me.lblScoreMethod.Size = New System.Drawing.Size(91, 16)
        Me.lblScoreMethod.TabIndex = 28
        Me.lblScoreMethod.Text = "Score method"
        '
        'tbTrimUpper
        '
        Me.tbTrimUpper.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbTrimUpper.Location = New System.Drawing.Point(142, 195)
        Me.tbTrimUpper.Name = "tbTrimUpper"
        Me.tbTrimUpper.Size = New System.Drawing.Size(125, 22)
        Me.tbTrimUpper.TabIndex = 27
        Me.tbTrimUpper.Text = "100"
        '
        'lblTrimUpper
        '
        Me.lblTrimUpper.AutoSize = True
        Me.lblTrimUpper.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTrimUpper.Location = New System.Drawing.Point(1, 198)
        Me.lblTrimUpper.Name = "lblTrimUpper"
        Me.lblTrimUpper.Size = New System.Drawing.Size(138, 16)
        Me.lblTrimUpper.TabIndex = 26
        Me.lblTrimUpper.Text = "Trim propensity upper"
        '
        'tbTrimLower
        '
        Me.tbTrimLower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbTrimLower.Location = New System.Drawing.Point(142, 167)
        Me.tbTrimLower.Name = "tbTrimLower"
        Me.tbTrimLower.Size = New System.Drawing.Size(125, 22)
        Me.tbTrimLower.TabIndex = 25
        Me.tbTrimLower.Text = "100"
        '
        'lblTrimLower
        '
        Me.lblTrimLower.AutoSize = True
        Me.lblTrimLower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTrimLower.Location = New System.Drawing.Point(1, 170)
        Me.lblTrimLower.Name = "lblTrimLower"
        Me.lblTrimLower.Size = New System.Drawing.Size(135, 16)
        Me.lblTrimLower.TabIndex = 24
        Me.lblTrimLower.Text = "Trim propensity lower"
        '
        'lblRidgePenalty
        '
        Me.lblRidgePenalty.AutoSize = True
        Me.lblRidgePenalty.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRidgePenalty.Location = New System.Drawing.Point(44, 109)
        Me.lblRidgePenalty.Name = "lblRidgePenalty"
        Me.lblRidgePenalty.Size = New System.Drawing.Size(91, 16)
        Me.lblRidgePenalty.TabIndex = 22
        Me.lblRidgePenalty.Text = "Ridge penalty"
        '
        'tbRidgePenalty
        '
        Me.tbRidgePenalty.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbRidgePenalty.Location = New System.Drawing.Point(142, 106)
        Me.tbRidgePenalty.Name = "tbRidgePenalty"
        Me.tbRidgePenalty.Size = New System.Drawing.Size(125, 22)
        Me.tbRidgePenalty.TabIndex = 23
        Me.tbRidgePenalty.Text = "0.000001"
        '
        'cbCommonSupport
        '
        Me.cbCommonSupport.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbCommonSupport.FormattingEnabled = True
        Me.cbCommonSupport.Location = New System.Drawing.Point(142, 137)
        Me.cbCommonSupport.Name = "cbCommonSupport"
        Me.cbCommonSupport.Size = New System.Drawing.Size(222, 24)
        Me.cbCommonSupport.TabIndex = 20
        '
        'lblCommonSupport
        '
        Me.lblCommonSupport.AutoSize = True
        Me.lblCommonSupport.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCommonSupport.Location = New System.Drawing.Point(26, 140)
        Me.lblCommonSupport.Name = "lblCommonSupport"
        Me.lblCommonSupport.Size = New System.Drawing.Size(109, 16)
        Me.lblCommonSupport.TabIndex = 19
        Me.lblCommonSupport.Text = "Common support"
        '
        'tbMaxIterations
        '
        Me.tbMaxIterations.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMaxIterations.Location = New System.Drawing.Point(142, 78)
        Me.tbMaxIterations.Name = "tbMaxIterations"
        Me.tbMaxIterations.Size = New System.Drawing.Size(125, 22)
        Me.tbMaxIterations.TabIndex = 3
        Me.tbMaxIterations.Text = "100"
        '
        'lblMaxIterations
        '
        Me.lblMaxIterations.AutoSize = True
        Me.lblMaxIterations.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMaxIterations.Location = New System.Drawing.Point(43, 81)
        Me.lblMaxIterations.Name = "lblMaxIterations"
        Me.lblMaxIterations.Size = New System.Drawing.Size(92, 16)
        Me.lblMaxIterations.TabIndex = 2
        Me.lblMaxIterations.Text = "Max. Iterations"
        '
        'lblTolerance
        '
        Me.lblTolerance.AutoSize = True
        Me.lblTolerance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTolerance.Location = New System.Drawing.Point(66, 55)
        Me.lblTolerance.Name = "lblTolerance"
        Me.lblTolerance.Size = New System.Drawing.Size(69, 16)
        Me.lblTolerance.TabIndex = 1
        Me.lblTolerance.Text = "Tolerance"
        '
        'tbTolerance
        '
        Me.tbTolerance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbTolerance.Location = New System.Drawing.Point(142, 52)
        Me.tbTolerance.Name = "tbTolerance"
        Me.tbTolerance.Size = New System.Drawing.Size(125, 22)
        Me.tbTolerance.TabIndex = 1
        Me.tbTolerance.Text = "0.000001"
        '
        'chkStandardizeCovariates
        '
        Me.chkStandardizeCovariates.AutoSize = True
        Me.chkStandardizeCovariates.Checked = True
        Me.chkStandardizeCovariates.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkStandardizeCovariates.Location = New System.Drawing.Point(12, 268)
        Me.chkStandardizeCovariates.Name = "chkStandardizeCovariates"
        Me.chkStandardizeCovariates.Size = New System.Drawing.Size(292, 20)
        Me.chkStandardizeCovariates.TabIndex = 39
        Me.chkStandardizeCovariates.Text = "Standardize covariates for propensity model"
        Me.chkStandardizeCovariates.UseVisualStyleBackColor = True
        '
        'TabPage4_DiagnosticsOutputs
        '
        Me.TabPage4_DiagnosticsOutputs.Controls.Add(Me.grpOutputs)
        Me.TabPage4_DiagnosticsOutputs.Controls.Add(Me.grpDiagnostics)
        Me.TabPage4_DiagnosticsOutputs.Location = New System.Drawing.Point(4, 25)
        Me.TabPage4_DiagnosticsOutputs.Name = "TabPage4_DiagnosticsOutputs"
        Me.TabPage4_DiagnosticsOutputs.Size = New System.Drawing.Size(829, 428)
        Me.TabPage4_DiagnosticsOutputs.TabIndex = 3
        Me.TabPage4_DiagnosticsOutputs.Text = "Diagnostics and Outputs"
        Me.TabPage4_DiagnosticsOutputs.UseVisualStyleBackColor = True
        '
        'grpOutputs
        '
        Me.grpOutputs.Controls.Add(Me.chkWriteDiagnostics)
        Me.grpOutputs.Controls.Add(Me.chkWriteMatches)
        Me.grpOutputs.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpOutputs.Location = New System.Drawing.Point(388, 14)
        Me.grpOutputs.Name = "grpOutputs"
        Me.grpOutputs.Size = New System.Drawing.Size(364, 83)
        Me.grpOutputs.TabIndex = 48
        Me.grpOutputs.TabStop = False
        Me.grpOutputs.Text = "Outputs"
        '
        'chkWriteDiagnostics
        '
        Me.chkWriteDiagnostics.AutoSize = True
        Me.chkWriteDiagnostics.Checked = True
        Me.chkWriteDiagnostics.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWriteDiagnostics.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWriteDiagnostics.Location = New System.Drawing.Point(17, 54)
        Me.chkWriteDiagnostics.Name = "chkWriteDiagnostics"
        Me.chkWriteDiagnostics.Size = New System.Drawing.Size(172, 20)
        Me.chkWriteDiagnostics.TabIndex = 41
        Me.chkWriteDiagnostics.Text = "Write diagnostics tables"
        Me.chkWriteDiagnostics.UseVisualStyleBackColor = True
        '
        'chkWriteMatches
        '
        Me.chkWriteMatches.AutoSize = True
        Me.chkWriteMatches.Checked = True
        Me.chkWriteMatches.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWriteMatches.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWriteMatches.Location = New System.Drawing.Point(17, 28)
        Me.chkWriteMatches.Name = "chkWriteMatches"
        Me.chkWriteMatches.Size = New System.Drawing.Size(175, 20)
        Me.chkWriteMatches.TabIndex = 40
        Me.chkWriteMatches.Text = "Write matched-pair table"
        Me.chkWriteMatches.UseVisualStyleBackColor = True
        '
        'grpDiagnostics
        '
        Me.grpDiagnostics.Controls.Add(Me.tbExtremeWeight)
        Me.grpDiagnostics.Controls.Add(Me.lblExtremeWeight)
        Me.grpDiagnostics.Controls.Add(Me.tbOverlapBins)
        Me.grpDiagnostics.Controls.Add(Me.Label1)
        Me.grpDiagnostics.Controls.Add(Me.chkLovePlot)
        Me.grpDiagnostics.Controls.Add(Me.chkWeightDiagnostics)
        Me.grpDiagnostics.Controls.Add(Me.chkOverlapDiagnostics)
        Me.grpDiagnostics.Controls.Add(Me.chkDoublyRobust)
        Me.grpDiagnostics.Controls.Add(Me.lblDiagnosticsNote)
        Me.grpDiagnostics.Controls.Add(Me.tbLoveThreshold)
        Me.grpDiagnostics.Controls.Add(Me.lblLoveThreshold)
        Me.grpDiagnostics.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpDiagnostics.Location = New System.Drawing.Point(9, 14)
        Me.grpDiagnostics.Name = "grpDiagnostics"
        Me.grpDiagnostics.Size = New System.Drawing.Size(364, 291)
        Me.grpDiagnostics.TabIndex = 42
        Me.grpDiagnostics.TabStop = False
        Me.grpDiagnostics.Text = "Diagnostics"
        '
        'tbExtremeWeight
        '
        Me.tbExtremeWeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbExtremeWeight.Location = New System.Drawing.Point(191, 188)
        Me.tbExtremeWeight.Name = "tbExtremeWeight"
        Me.tbExtremeWeight.Size = New System.Drawing.Size(125, 22)
        Me.tbExtremeWeight.TabIndex = 47
        Me.tbExtremeWeight.Text = "10"
        '
        'lblExtremeWeight
        '
        Me.lblExtremeWeight.AutoSize = True
        Me.lblExtremeWeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblExtremeWeight.Location = New System.Drawing.Point(40, 191)
        Me.lblExtremeWeight.Name = "lblExtremeWeight"
        Me.lblExtremeWeight.Size = New System.Drawing.Size(131, 16)
        Me.lblExtremeWeight.TabIndex = 46
        Me.lblExtremeWeight.Text = "Extreme weight cutoff"
        '
        'tbOverlapBins
        '
        Me.tbOverlapBins.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbOverlapBins.Location = New System.Drawing.Point(191, 160)
        Me.tbOverlapBins.Name = "tbOverlapBins"
        Me.tbOverlapBins.Size = New System.Drawing.Size(125, 22)
        Me.tbOverlapBins.TabIndex = 45
        Me.tbOverlapBins.Text = "20"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(29, 163)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(145, 16)
        Me.Label1.TabIndex = 44
        Me.Label1.Text = "Overlap histogram bins"
        '
        'chkLovePlot
        '
        Me.chkLovePlot.AutoSize = True
        Me.chkLovePlot.Checked = True
        Me.chkLovePlot.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkLovePlot.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkLovePlot.Location = New System.Drawing.Point(17, 106)
        Me.chkLovePlot.Name = "chkLovePlot"
        Me.chkLovePlot.Size = New System.Drawing.Size(161, 20)
        Me.chkLovePlot.TabIndex = 43
        Me.chkLovePlot.Text = "Include Love-plot data"
        Me.chkLovePlot.UseVisualStyleBackColor = True
        '
        'chkWeightDiagnostics
        '
        Me.chkWeightDiagnostics.AutoSize = True
        Me.chkWeightDiagnostics.Checked = True
        Me.chkWeightDiagnostics.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWeightDiagnostics.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkWeightDiagnostics.Location = New System.Drawing.Point(17, 80)
        Me.chkWeightDiagnostics.Name = "chkWeightDiagnostics"
        Me.chkWeightDiagnostics.Size = New System.Drawing.Size(185, 20)
        Me.chkWeightDiagnostics.TabIndex = 42
        Me.chkWeightDiagnostics.Text = "Include weight diagnostics"
        Me.chkWeightDiagnostics.UseVisualStyleBackColor = True
        '
        'chkOverlapDiagnostics
        '
        Me.chkOverlapDiagnostics.AutoSize = True
        Me.chkOverlapDiagnostics.Checked = True
        Me.chkOverlapDiagnostics.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkOverlapDiagnostics.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkOverlapDiagnostics.Location = New System.Drawing.Point(17, 54)
        Me.chkOverlapDiagnostics.Name = "chkOverlapDiagnostics"
        Me.chkOverlapDiagnostics.Size = New System.Drawing.Size(193, 20)
        Me.chkOverlapDiagnostics.TabIndex = 41
        Me.chkOverlapDiagnostics.Text = "Include overlap diagnostics"
        Me.chkOverlapDiagnostics.UseVisualStyleBackColor = True
        '
        'chkDoublyRobust
        '
        Me.chkDoublyRobust.AutoSize = True
        Me.chkDoublyRobust.Checked = True
        Me.chkDoublyRobust.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkDoublyRobust.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkDoublyRobust.Location = New System.Drawing.Point(17, 28)
        Me.chkDoublyRobust.Name = "chkDoublyRobust"
        Me.chkDoublyRobust.Size = New System.Drawing.Size(247, 20)
        Me.chkDoublyRobust.TabIndex = 40
        Me.chkDoublyRobust.Text = "Include doubly robust AIPW estimate"
        Me.chkDoublyRobust.UseVisualStyleBackColor = True
        '
        'lblDiagnosticsNote
        '
        Me.lblDiagnosticsNote.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDiagnosticsNote.Location = New System.Drawing.Point(14, 226)
        Me.lblDiagnosticsNote.Name = "lblDiagnosticsNote"
        Me.lblDiagnosticsNote.Size = New System.Drawing.Size(351, 56)
        Me.lblDiagnosticsNote.TabIndex = 30
        Me.lblDiagnosticsNote.Text = "Diagnostics are computed in the backend and written as worksheet tables. Love-plo" &
    "t and overlap-bin outputs are designed to be charted in Excel."
        '
        'tbLoveThreshold
        '
        Me.tbLoveThreshold.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbLoveThreshold.Location = New System.Drawing.Point(191, 132)
        Me.tbLoveThreshold.Name = "tbLoveThreshold"
        Me.tbLoveThreshold.Size = New System.Drawing.Size(125, 22)
        Me.tbLoveThreshold.TabIndex = 25
        Me.tbLoveThreshold.Text = "0.1"
        '
        'lblLoveThreshold
        '
        Me.lblLoveThreshold.AutoSize = True
        Me.lblLoveThreshold.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLoveThreshold.Location = New System.Drawing.Point(20, 135)
        Me.lblLoveThreshold.Name = "lblLoveThreshold"
        Me.lblLoveThreshold.Size = New System.Drawing.Size(154, 16)
        Me.lblLoveThreshold.TabIndex = 24
        Me.lblLoveThreshold.Text = "Love-plot SMD threshold"
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(-1, 462)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(837, 23)
        Me.ProgressBar1.TabIndex = 27
        '
        'lblProgress
        '
        Me.lblProgress.Location = New System.Drawing.Point(3, 488)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(596, 32)
        Me.lblProgress.TabIndex = 26
        Me.lblProgress.Text = "Elapsed Time: "
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(680, 491)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 25
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCalculate
        '
        Me.btCalculate.Location = New System.Drawing.Point(761, 491)
        Me.btCalculate.Name = "btCalculate"
        Me.btCalculate.Size = New System.Drawing.Size(75, 23)
        Me.btCalculate.TabIndex = 24
        Me.btCalculate.Text = "Fit"
        Me.btCalculate.UseVisualStyleBackColor = True
        '
        'Ui20PropensityScoreMatching
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(841, 519)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCalculate)
        Me.Controls.Add(Me.TabControl1)
        Me.Name = "Ui20PropensityScoreMatching"
        Me.ShowIcon = False
        Me.Text = "Ui20PropensityScoreMatching"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1_Data.ResumeLayout(False)
        Me.TabPage1_Data.PerformLayout()
        Me.TabPage2_PropensityModel.ResumeLayout(False)
        Me.TabPage2_PropensityModel.PerformLayout()
        CType(Me.spinBtnPoly, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage3_Options.ResumeLayout(False)
        Me.TabPage3_Options.PerformLayout()
        Me.grpAdjustmentMethod.ResumeLayout(False)
        Me.grpAdjustmentMethod.PerformLayout()
        Me.grpIterOptions.ResumeLayout(False)
        Me.grpIterOptions.PerformLayout()
        Me.TabPage4_DiagnosticsOutputs.ResumeLayout(False)
        Me.grpOutputs.ResumeLayout(False)
        Me.grpOutputs.PerformLayout()
        Me.grpDiagnostics.ResumeLayout(False)
        Me.grpDiagnostics.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents TabPage1_Data As Windows.Forms.TabPage
    Friend WithEvents TabPage2_PropensityModel As Windows.Forms.TabPage
    Friend WithEvents ProgressBar1 As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCalculate As Windows.Forms.Button
    Friend WithEvents TabPage3_Options As Windows.Forms.TabPage
    Friend WithEvents TabPage4_DiagnosticsOutputs As Windows.Forms.TabPage
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents lblSelectedSheet As Windows.Forms.Label
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents btRemoveID As Windows.Forms.Button
    Friend WithEvents btAddID As Windows.Forms.Button
    Friend WithEvents lbId As Windows.Forms.ListBox
    Friend WithEvents lblClusterID As Windows.Forms.Label
    Friend WithEvents lbOutcome As Windows.Forms.ListBox
    Friend WithEvents lblOutcome As Windows.Forms.Label
    Friend WithEvents btRemoveOutcome As Windows.Forms.Button
    Friend WithEvents btAddOutcome As Windows.Forms.Button
    Friend WithEvents lblNote As Windows.Forms.Label
    Friend WithEvents lbTreatment As Windows.Forms.ListBox
    Friend WithEvents lblTreatment As Windows.Forms.Label
    Friend WithEvents btRemoveTreatment As Windows.Forms.Button
    Friend WithEvents btAddTreatment As Windows.Forms.Button
    Friend WithEvents lbCovariates As Windows.Forms.ListBox
    Friend WithEvents btRemoveCovariates As Windows.Forms.Button
    Friend WithEvents btAddCovariates As Windows.Forms.Button
    Friend WithEvents lblCovariates As Windows.Forms.Label
    Friend WithEvents btRemoveExact As Windows.Forms.Button
    Friend WithEvents btAddExact As Windows.Forms.Button
    Friend WithEvents lbExact As Windows.Forms.ListBox
    Friend WithEvents lblExact As Windows.Forms.Label
    Friend WithEvents btRemoveScore As Windows.Forms.Button
    Friend WithEvents btAddScore As Windows.Forms.Button
    Friend WithEvents lbScore As Windows.Forms.ListBox
    Friend WithEvents lblScore As Windows.Forms.Label
    Friend WithEvents lbSelectedVariables As Windows.Forms.ListBox
    Friend WithEvents lblSelectedVariables As Windows.Forms.Label
    Friend WithEvents btAddEffectCategoricalFactor As Windows.Forms.Button
    Friend WithEvents btnCustomInteraction As Windows.Forms.Button
    Friend WithEvents btn2Interactions As Windows.Forms.Button
    Friend WithEvents spinBtnPoly As Windows.Forms.NumericUpDown
    Friend WithEvents btnPoly As Windows.Forms.Button
    Friend WithEvents ckIntercept As Windows.Forms.CheckBox
    Friend WithEvents btAddEffect As Windows.Forms.Button
    Friend WithEvents btClearAllSelectedEffects As Windows.Forms.Button
    Friend WithEvents tbRemoveSelectedEffects As Windows.Forms.Button
    Friend WithEvents lbSelectedEffectsList As Windows.Forms.ListBox
    Friend WithEvents lblSelectedEffectsList As Windows.Forms.Label
    Friend WithEvents chkStandardizeCovariates As Windows.Forms.CheckBox
    Friend WithEvents grpIterOptions As Windows.Forms.GroupBox
    Friend WithEvents cbCommonSupport As Windows.Forms.ComboBox
    Friend WithEvents lblCommonSupport As Windows.Forms.Label
    Friend WithEvents tbMaxIterations As Windows.Forms.TextBox
    Friend WithEvents lblMaxIterations As Windows.Forms.Label
    Friend WithEvents lblTolerance As Windows.Forms.Label
    Friend WithEvents tbTolerance As Windows.Forms.TextBox
    Friend WithEvents lblRidgePenalty As Windows.Forms.Label
    Friend WithEvents tbRidgePenalty As Windows.Forms.TextBox
    Friend WithEvents tbTrimUpper As Windows.Forms.TextBox
    Friend WithEvents lblTrimUpper As Windows.Forms.Label
    Friend WithEvents tbTrimLower As Windows.Forms.TextBox
    Friend WithEvents lblTrimLower As Windows.Forms.Label
    Friend WithEvents grpAdjustmentMethod As Windows.Forms.GroupBox
    Friend WithEvents tbMatchingRatio As Windows.Forms.TextBox
    Friend WithEvents lblMatchingRatio As Windows.Forms.Label
    Friend WithEvents cbRunMethod As Windows.Forms.ComboBox
    Friend WithEvents lblRunMethod As Windows.Forms.Label
    Friend WithEvents cbDistanceMetric As Windows.Forms.ComboBox
    Friend WithEvents lblDistanceMetric As Windows.Forms.Label
    Friend WithEvents cbEstimand As Windows.Forms.ComboBox
    Friend WithEvents lblEstimand As Windows.Forms.Label
    Friend WithEvents cbMatchingOrder As Windows.Forms.ComboBox
    Friend WithEvents lblMatchingOrder As Windows.Forms.Label
    Friend WithEvents tbCaliper As Windows.Forms.TextBox
    Friend WithEvents lblCaliper As Windows.Forms.Label
    Friend WithEvents tbCemBins As Windows.Forms.TextBox
    Friend WithEvents lblCemBins As Windows.Forms.Label
    Friend WithEvents tbStrata As Windows.Forms.TextBox
    Friend WithEvents lblStrata As Windows.Forms.Label
    Friend WithEvents cbCaliperScale As Windows.Forms.ComboBox
    Friend WithEvents lblCaliperScale As Windows.Forms.Label
    Friend WithEvents chkWithReplacement As Windows.Forms.CheckBox
    Friend WithEvents grpDiagnostics As Windows.Forms.GroupBox
    Friend WithEvents chkDoublyRobust As Windows.Forms.CheckBox
    Friend WithEvents lblDiagnosticsNote As Windows.Forms.Label
    Friend WithEvents tbLoveThreshold As Windows.Forms.TextBox
    Friend WithEvents lblLoveThreshold As Windows.Forms.Label
    Friend WithEvents chkLovePlot As Windows.Forms.CheckBox
    Friend WithEvents chkWeightDiagnostics As Windows.Forms.CheckBox
    Friend WithEvents chkOverlapDiagnostics As Windows.Forms.CheckBox
    Friend WithEvents tbExtremeWeight As Windows.Forms.TextBox
    Friend WithEvents lblExtremeWeight As Windows.Forms.Label
    Friend WithEvents tbOverlapBins As Windows.Forms.TextBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents grpOutputs As Windows.Forms.GroupBox
    Friend WithEvents chkWriteDiagnostics As Windows.Forms.CheckBox
    Friend WithEvents chkWriteMatches As Windows.Forms.CheckBox
    Friend WithEvents cbScoreMethod As Windows.Forms.ComboBox
    Friend WithEvents lblScoreMethod As Windows.Forms.Label
End Class
