<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui01SankeyPlot
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui01SankeyPlot))
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1_StagedData = New System.Windows.Forms.TabPage()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.lblNote = New System.Windows.Forms.Label()
        Me.ckConnectAcrossMissingStages = New System.Windows.Forms.CheckBox()
        Me.ckStagedFirstRowLabels = New System.Windows.Forms.CheckBox()
        Me.RefEdit_StagedWeight = New BESHStatNG.Excel2007RefEdit()
        Me.lblStagedWeight = New System.Windows.Forms.Label()
        Me.RefEdit_Stages = New BESHStatNG.Excel2007RefEdit()
        Me.lblStages = New System.Windows.Forms.Label()
        Me.TabPage2_SourceTarget = New System.Windows.Forms.TabPage()
        Me.lblNoteSourceTarget = New System.Windows.Forms.Label()
        Me.ckLinkFirstRowLabels = New System.Windows.Forms.CheckBox()
        Me.RefEdit_TargetStage = New BESHStatNG.Excel2007RefEdit()
        Me.lblTargetStage = New System.Windows.Forms.Label()
        Me.RefEdit_SourceStage = New BESHStatNG.Excel2007RefEdit()
        Me.lblSourceStage = New System.Windows.Forms.Label()
        Me.RefEdit_LinkWeight = New BESHStatNG.Excel2007RefEdit()
        Me.lblLinkWeight = New System.Windows.Forms.Label()
        Me.RefEdit_Target = New BESHStatNG.Excel2007RefEdit()
        Me.lblTarget = New System.Windows.Forms.Label()
        Me.RefEdit_Source = New BESHStatNG.Excel2007RefEdit()
        Me.lblSource = New System.Windows.Forms.Label()
        Me.TabPage3_Appearance = New System.Windows.Forms.TabPage()
        Me.grpChartSize = New System.Windows.Forms.GroupBox()
        Me.nudChartHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblChartHeight = New System.Windows.Forms.Label()
        Me.nudChartWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblChartWidth = New System.Windows.Forms.Label()
        Me.grpColors = New System.Windows.Forms.GroupBox()
        Me.ckShowRibbonOutline = New System.Windows.Forms.CheckBox()
        Me.nudRibbonTransparency = New System.Windows.Forms.NumericUpDown()
        Me.lblRibbonTransparency = New System.Windows.Forms.Label()
        Me.cmbRibbonColorMode = New System.Windows.Forms.ComboBox()
        Me.lblRibbonColorMode = New System.Windows.Forms.Label()
        Me.cmbPalette = New System.Windows.Forms.ComboBox()
        Me.lblPalette = New System.Windows.Forms.Label()
        Me.grpLabels = New System.Windows.Forms.GroupBox()
        Me.txtChartTitle = New System.Windows.Forms.TextBox()
        Me.ckShowLegend = New System.Windows.Forms.CheckBox()
        Me.ckShowStageLabels = New System.Windows.Forms.CheckBox()
        Me.cmbPercentageBasis = New System.Windows.Forms.ComboBox()
        Me.ckShowNodeLabels = New System.Windows.Forms.CheckBox()
        Me.lblChartTitle = New System.Windows.Forms.Label()
        Me.lblPercentageBasis = New System.Windows.Forms.Label()
        Me.cmbNodeLabelMode = New System.Windows.Forms.ComboBox()
        Me.lblNodeLabelMode = New System.Windows.Forms.Label()
        Me.grpLayout = New System.Windows.Forms.GroupBox()
        Me.nudRibbonCurvature = New System.Windows.Forms.NumericUpDown()
        Me.lblRibbonCurvature = New System.Windows.Forms.Label()
        Me.nudNodeWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblNodeWidth = New System.Windows.Forms.Label()
        Me.nudNodeGap = New System.Windows.Forms.NumericUpDown()
        Me.lblNodeGap = New System.Windows.Forms.Label()
        Me.cmbVerticalAlignment = New System.Windows.Forms.ComboBox()
        Me.lblVerticalAlignment = New System.Windows.Forms.Label()
        Me.cmbNodeOrder = New System.Windows.Forms.ComboBox()
        Me.lblNodeOrder = New System.Windows.Forms.Label()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1_StagedData.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPage2_SourceTarget.SuspendLayout()
        Me.TabPage3_Appearance.SuspendLayout()
        Me.grpChartSize.SuspendLayout()
        CType(Me.nudChartHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudChartWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpColors.SuspendLayout()
        CType(Me.nudRibbonTransparency, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpLabels.SuspendLayout()
        Me.grpLayout.SuspendLayout()
        CType(Me.nudRibbonCurvature, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudNodeWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudNodeGap, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(295, 517)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 12
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(376, 517)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 11
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1_StagedData)
        Me.TabMultipage.Controls.Add(Me.TabPage2_SourceTarget)
        Me.TabMultipage.Controls.Add(Me.TabPage3_Appearance)
        Me.TabMultipage.Location = New System.Drawing.Point(3, 3)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(456, 512)
        Me.TabMultipage.TabIndex = 13
        '
        'TabPage1_StagedData
        '
        Me.TabPage1_StagedData.Controls.Add(Me.grpOutput)
        Me.TabPage1_StagedData.Controls.Add(Me.grpInput)
        Me.TabPage1_StagedData.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1_StagedData.Name = "TabPage1_StagedData"
        Me.TabPage1_StagedData.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1_StagedData.Size = New System.Drawing.Size(448, 483)
        Me.TabPage1_StagedData.TabIndex = 0
        Me.TabPage1_StagedData.Text = "Staged Data"
        Me.TabPage1_StagedData.UseVisualStyleBackColor = True
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(3, 329)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(442, 130)
        Me.grpOutput.TabIndex = 7
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
        Me.RefEditOutput.Location = New System.Drawing.Point(168, 16)
        Me.RefEditOutput.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEditOutput.Name = "RefEditOutput"
        Me.RefEditOutput.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEditOutput.Size = New System.Drawing.Size(267, 32)
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
        Me.grpInput.Controls.Add(Me.lblNote)
        Me.grpInput.Controls.Add(Me.ckConnectAcrossMissingStages)
        Me.grpInput.Controls.Add(Me.ckStagedFirstRowLabels)
        Me.grpInput.Controls.Add(Me.RefEdit_StagedWeight)
        Me.grpInput.Controls.Add(Me.lblStagedWeight)
        Me.grpInput.Controls.Add(Me.RefEdit_Stages)
        Me.grpInput.Controls.Add(Me.lblStages)
        Me.grpInput.Location = New System.Drawing.Point(6, 6)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(436, 277)
        Me.grpInput.TabIndex = 2
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'lblNote
        '
        Me.lblNote.Location = New System.Drawing.Point(11, 218)
        Me.lblNote.Name = "lblNote"
        Me.lblNote.Size = New System.Drawing.Size(415, 32)
        Me.lblNote.TabIndex = 11
        Me.lblNote.Text = "Leading missing stages are allowed and represent flows entering at a later stage." &
    ""
        '
        'ckConnectAcrossMissingStages
        '
        Me.ckConnectAcrossMissingStages.AutoSize = True
        Me.ckConnectAcrossMissingStages.Location = New System.Drawing.Point(14, 178)
        Me.ckConnectAcrossMissingStages.Name = "ckConnectAcrossMissingStages"
        Me.ckConnectAcrossMissingStages.Size = New System.Drawing.Size(292, 20)
        Me.ckConnectAcrossMissingStages.TabIndex = 10
        Me.ckConnectAcrossMissingStages.Text = "Connect across missing intermediate stages"
        Me.ckConnectAcrossMissingStages.UseVisualStyleBackColor = True
        '
        'ckStagedFirstRowLabels
        '
        Me.ckStagedFirstRowLabels.AutoSize = True
        Me.ckStagedFirstRowLabels.Checked = True
        Me.ckStagedFirstRowLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckStagedFirstRowLabels.Location = New System.Drawing.Point(14, 152)
        Me.ckStagedFirstRowLabels.Name = "ckStagedFirstRowLabels"
        Me.ckStagedFirstRowLabels.Size = New System.Drawing.Size(208, 20)
        Me.ckStagedFirstRowLabels.TabIndex = 9
        Me.ckStagedFirstRowLabels.Text = "First row contains stage labels"
        Me.ckStagedFirstRowLabels.UseVisualStyleBackColor = True
        '
        'RefEdit_StagedWeight
        '
        Me.RefEdit_StagedWeight.Address = ""
        Me.RefEdit_StagedWeight.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_StagedWeight.ExcelConnector = Nothing
        Me.RefEdit_StagedWeight.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_StagedWeight.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_StagedWeight.Location = New System.Drawing.Point(146, 86)
        Me.RefEdit_StagedWeight.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_StagedWeight.Name = "RefEdit_StagedWeight"
        Me.RefEdit_StagedWeight.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_StagedWeight.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_StagedWeight.TabIndex = 8
        '
        'lblStagedWeight
        '
        Me.lblStagedWeight.Location = New System.Drawing.Point(11, 86)
        Me.lblStagedWeight.Name = "lblStagedWeight"
        Me.lblStagedWeight.Size = New System.Drawing.Size(128, 32)
        Me.lblStagedWeight.TabIndex = 7
        Me.lblStagedWeight.Text = "Weight / frequency (optional):"
        '
        'RefEdit_Stages
        '
        Me.RefEdit_Stages.Address = ""
        Me.RefEdit_Stages.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_Stages.ExcelConnector = Nothing
        Me.RefEdit_Stages.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_Stages.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_Stages.Location = New System.Drawing.Point(146, 46)
        Me.RefEdit_Stages.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_Stages.Name = "RefEdit_Stages"
        Me.RefEdit_Stages.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_Stages.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_Stages.TabIndex = 6
        '
        'lblStages
        '
        Me.lblStages.AutoSize = True
        Me.lblStages.Location = New System.Drawing.Point(11, 46)
        Me.lblStages.Name = "lblStages"
        Me.lblStages.Size = New System.Drawing.Size(105, 16)
        Me.lblStages.TabIndex = 5
        Me.lblStages.Text = "Stage variables:"
        '
        'TabPage2_SourceTarget
        '
        Me.TabPage2_SourceTarget.Controls.Add(Me.lblNoteSourceTarget)
        Me.TabPage2_SourceTarget.Controls.Add(Me.ckLinkFirstRowLabels)
        Me.TabPage2_SourceTarget.Controls.Add(Me.RefEdit_TargetStage)
        Me.TabPage2_SourceTarget.Controls.Add(Me.lblTargetStage)
        Me.TabPage2_SourceTarget.Controls.Add(Me.RefEdit_SourceStage)
        Me.TabPage2_SourceTarget.Controls.Add(Me.lblSourceStage)
        Me.TabPage2_SourceTarget.Controls.Add(Me.RefEdit_LinkWeight)
        Me.TabPage2_SourceTarget.Controls.Add(Me.lblLinkWeight)
        Me.TabPage2_SourceTarget.Controls.Add(Me.RefEdit_Target)
        Me.TabPage2_SourceTarget.Controls.Add(Me.lblTarget)
        Me.TabPage2_SourceTarget.Controls.Add(Me.RefEdit_Source)
        Me.TabPage2_SourceTarget.Controls.Add(Me.lblSource)
        Me.TabPage2_SourceTarget.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2_SourceTarget.Name = "TabPage2_SourceTarget"
        Me.TabPage2_SourceTarget.Size = New System.Drawing.Size(448, 483)
        Me.TabPage2_SourceTarget.TabIndex = 2
        Me.TabPage2_SourceTarget.Text = "Source / Target"
        Me.TabPage2_SourceTarget.UseVisualStyleBackColor = True
        '
        'lblNoteSourceTarget
        '
        Me.lblNoteSourceTarget.Location = New System.Drawing.Point(16, 288)
        Me.lblNoteSourceTarget.Name = "lblNoteSourceTarget"
        Me.lblNoteSourceTarget.Size = New System.Drawing.Size(415, 51)
        Me.lblNoteSourceTarget.TabIndex = 18
        Me.lblNoteSourceTarget.Text = "Leave Source stage and Target stage blank for automatic stage placement."
        '
        'ckLinkFirstRowLabels
        '
        Me.ckLinkFirstRowLabels.AutoSize = True
        Me.ckLinkFirstRowLabels.Checked = True
        Me.ckLinkFirstRowLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckLinkFirstRowLabels.Location = New System.Drawing.Point(148, 247)
        Me.ckLinkFirstRowLabels.Name = "ckLinkFirstRowLabels"
        Me.ckLinkFirstRowLabels.Size = New System.Drawing.Size(171, 20)
        Me.ckLinkFirstRowLabels.TabIndex = 17
        Me.ckLinkFirstRowLabels.Text = "First row contains labels"
        Me.ckLinkFirstRowLabels.UseVisualStyleBackColor = True
        '
        'RefEdit_TargetStage
        '
        Me.RefEdit_TargetStage.Address = ""
        Me.RefEdit_TargetStage.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_TargetStage.ExcelConnector = Nothing
        Me.RefEdit_TargetStage.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_TargetStage.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_TargetStage.Location = New System.Drawing.Point(148, 184)
        Me.RefEdit_TargetStage.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_TargetStage.Name = "RefEdit_TargetStage"
        Me.RefEdit_TargetStage.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_TargetStage.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_TargetStage.TabIndex = 16
        '
        'lblTargetStage
        '
        Me.lblTargetStage.Location = New System.Drawing.Point(13, 184)
        Me.lblTargetStage.Name = "lblTargetStage"
        Me.lblTargetStage.Size = New System.Drawing.Size(128, 32)
        Me.lblTargetStage.TabIndex = 15
        Me.lblTargetStage.Text = "Target stage (optional):"
        '
        'RefEdit_SourceStage
        '
        Me.RefEdit_SourceStage.Address = ""
        Me.RefEdit_SourceStage.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_SourceStage.ExcelConnector = Nothing
        Me.RefEdit_SourceStage.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_SourceStage.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_SourceStage.Location = New System.Drawing.Point(148, 144)
        Me.RefEdit_SourceStage.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_SourceStage.Name = "RefEdit_SourceStage"
        Me.RefEdit_SourceStage.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_SourceStage.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_SourceStage.TabIndex = 14
        '
        'lblSourceStage
        '
        Me.lblSourceStage.Location = New System.Drawing.Point(13, 144)
        Me.lblSourceStage.Name = "lblSourceStage"
        Me.lblSourceStage.Size = New System.Drawing.Size(128, 32)
        Me.lblSourceStage.TabIndex = 13
        Me.lblSourceStage.Text = "Source stage (optional):"
        '
        'RefEdit_LinkWeight
        '
        Me.RefEdit_LinkWeight.Address = ""
        Me.RefEdit_LinkWeight.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_LinkWeight.ExcelConnector = Nothing
        Me.RefEdit_LinkWeight.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_LinkWeight.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_LinkWeight.Location = New System.Drawing.Point(148, 104)
        Me.RefEdit_LinkWeight.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_LinkWeight.Name = "RefEdit_LinkWeight"
        Me.RefEdit_LinkWeight.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_LinkWeight.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_LinkWeight.TabIndex = 12
        '
        'lblLinkWeight
        '
        Me.lblLinkWeight.Location = New System.Drawing.Point(13, 104)
        Me.lblLinkWeight.Name = "lblLinkWeight"
        Me.lblLinkWeight.Size = New System.Drawing.Size(128, 32)
        Me.lblLinkWeight.TabIndex = 11
        Me.lblLinkWeight.Text = "Weight / frequency (optional):"
        '
        'RefEdit_Target
        '
        Me.RefEdit_Target.Address = ""
        Me.RefEdit_Target.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_Target.ExcelConnector = Nothing
        Me.RefEdit_Target.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_Target.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_Target.Location = New System.Drawing.Point(148, 64)
        Me.RefEdit_Target.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_Target.Name = "RefEdit_Target"
        Me.RefEdit_Target.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_Target.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_Target.TabIndex = 10
        '
        'lblTarget
        '
        Me.lblTarget.AutoSize = True
        Me.lblTarget.Location = New System.Drawing.Point(13, 64)
        Me.lblTarget.Name = "lblTarget"
        Me.lblTarget.Size = New System.Drawing.Size(50, 16)
        Me.lblTarget.TabIndex = 9
        Me.lblTarget.Text = "Target:"
        '
        'RefEdit_Source
        '
        Me.RefEdit_Source.Address = ""
        Me.RefEdit_Source.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_Source.ExcelConnector = Nothing
        Me.RefEdit_Source.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_Source.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_Source.Location = New System.Drawing.Point(148, 24)
        Me.RefEdit_Source.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_Source.Name = "RefEdit_Source"
        Me.RefEdit_Source.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_Source.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_Source.TabIndex = 8
        '
        'lblSource
        '
        Me.lblSource.AutoSize = True
        Me.lblSource.Location = New System.Drawing.Point(13, 24)
        Me.lblSource.Name = "lblSource"
        Me.lblSource.Size = New System.Drawing.Size(53, 16)
        Me.lblSource.TabIndex = 7
        Me.lblSource.Text = "Source:"
        '
        'TabPage3_Appearance
        '
        Me.TabPage3_Appearance.Controls.Add(Me.grpChartSize)
        Me.TabPage3_Appearance.Controls.Add(Me.grpColors)
        Me.TabPage3_Appearance.Controls.Add(Me.grpLabels)
        Me.TabPage3_Appearance.Controls.Add(Me.grpLayout)
        Me.TabPage3_Appearance.Location = New System.Drawing.Point(4, 25)
        Me.TabPage3_Appearance.Name = "TabPage3_Appearance"
        Me.TabPage3_Appearance.Size = New System.Drawing.Size(448, 483)
        Me.TabPage3_Appearance.TabIndex = 1
        Me.TabPage3_Appearance.Text = "Appearance"
        Me.TabPage3_Appearance.UseVisualStyleBackColor = True
        '
        'grpChartSize
        '
        Me.grpChartSize.Controls.Add(Me.nudChartHeight)
        Me.grpChartSize.Controls.Add(Me.lblChartHeight)
        Me.grpChartSize.Controls.Add(Me.nudChartWidth)
        Me.grpChartSize.Controls.Add(Me.lblChartWidth)
        Me.grpChartSize.Location = New System.Drawing.Point(5, 417)
        Me.grpChartSize.Name = "grpChartSize"
        Me.grpChartSize.Size = New System.Drawing.Size(432, 54)
        Me.grpChartSize.TabIndex = 14
        Me.grpChartSize.TabStop = False
        Me.grpChartSize.Text = "Chart size"
        '
        'nudChartHeight
        '
        Me.nudChartHeight.Location = New System.Drawing.Point(263, 26)
        Me.nudChartHeight.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        Me.nudChartHeight.Minimum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.nudChartHeight.Name = "nudChartHeight"
        Me.nudChartHeight.Size = New System.Drawing.Size(74, 22)
        Me.nudChartHeight.TabIndex = 13
        Me.nudChartHeight.Value = New Decimal(New Integer() {460, 0, 0, 0})
        '
        'lblChartHeight
        '
        Me.lblChartHeight.AutoSize = True
        Me.lblChartHeight.Location = New System.Drawing.Point(198, 28)
        Me.lblChartHeight.Name = "lblChartHeight"
        Me.lblChartHeight.Size = New System.Drawing.Size(46, 16)
        Me.lblChartHeight.TabIndex = 12
        Me.lblChartHeight.Text = "Height"
        '
        'nudChartWidth
        '
        Me.nudChartWidth.Location = New System.Drawing.Point(71, 26)
        Me.nudChartWidth.Maximum = New Decimal(New Integer() {3000, 0, 0, 0})
        Me.nudChartWidth.Minimum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.nudChartWidth.Name = "nudChartWidth"
        Me.nudChartWidth.Size = New System.Drawing.Size(74, 22)
        Me.nudChartWidth.TabIndex = 11
        Me.nudChartWidth.Value = New Decimal(New Integer() {760, 0, 0, 0})
        '
        'lblChartWidth
        '
        Me.lblChartWidth.AutoSize = True
        Me.lblChartWidth.Location = New System.Drawing.Point(6, 28)
        Me.lblChartWidth.Name = "lblChartWidth"
        Me.lblChartWidth.Size = New System.Drawing.Size(41, 16)
        Me.lblChartWidth.TabIndex = 10
        Me.lblChartWidth.Text = "Width"
        '
        'grpColors
        '
        Me.grpColors.Controls.Add(Me.ckShowRibbonOutline)
        Me.grpColors.Controls.Add(Me.nudRibbonTransparency)
        Me.grpColors.Controls.Add(Me.lblRibbonTransparency)
        Me.grpColors.Controls.Add(Me.cmbRibbonColorMode)
        Me.grpColors.Controls.Add(Me.lblRibbonColorMode)
        Me.grpColors.Controls.Add(Me.cmbPalette)
        Me.grpColors.Controls.Add(Me.lblPalette)
        Me.grpColors.Location = New System.Drawing.Point(5, 300)
        Me.grpColors.Name = "grpColors"
        Me.grpColors.Size = New System.Drawing.Size(439, 111)
        Me.grpColors.TabIndex = 13
        Me.grpColors.TabStop = False
        Me.grpColors.Text = "Colors"
        '
        'ckShowRibbonOutline
        '
        Me.ckShowRibbonOutline.AutoSize = True
        Me.ckShowRibbonOutline.Location = New System.Drawing.Point(275, 82)
        Me.ckShowRibbonOutline.Name = "ckShowRibbonOutline"
        Me.ckShowRibbonOutline.Size = New System.Drawing.Size(145, 20)
        Me.ckShowRibbonOutline.TabIndex = 14
        Me.ckShowRibbonOutline.Text = "Show ribbon outline"
        Me.ckShowRibbonOutline.UseVisualStyleBackColor = True
        '
        'nudRibbonTransparency
        '
        Me.nudRibbonTransparency.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudRibbonTransparency.Location = New System.Drawing.Point(170, 81)
        Me.nudRibbonTransparency.Name = "nudRibbonTransparency"
        Me.nudRibbonTransparency.Size = New System.Drawing.Size(74, 22)
        Me.nudRibbonTransparency.TabIndex = 13
        Me.nudRibbonTransparency.Value = New Decimal(New Integer() {25, 0, 0, 0})
        '
        'lblRibbonTransparency
        '
        Me.lblRibbonTransparency.AutoSize = True
        Me.lblRibbonTransparency.Location = New System.Drawing.Point(6, 83)
        Me.lblRibbonTransparency.Name = "lblRibbonTransparency"
        Me.lblRibbonTransparency.Size = New System.Drawing.Size(158, 16)
        Me.lblRibbonTransparency.TabIndex = 12
        Me.lblRibbonTransparency.Text = "Ribbon transparency (%):"
        '
        'cmbRibbonColorMode
        '
        Me.cmbRibbonColorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRibbonColorMode.FormattingEnabled = True
        Me.cmbRibbonColorMode.Location = New System.Drawing.Point(170, 51)
        Me.cmbRibbonColorMode.Name = "cmbRibbonColorMode"
        Me.cmbRibbonColorMode.Size = New System.Drawing.Size(250, 24)
        Me.cmbRibbonColorMode.TabIndex = 6
        '
        'lblRibbonColorMode
        '
        Me.lblRibbonColorMode.AutoSize = True
        Me.lblRibbonColorMode.Location = New System.Drawing.Point(6, 54)
        Me.lblRibbonColorMode.Name = "lblRibbonColorMode"
        Me.lblRibbonColorMode.Size = New System.Drawing.Size(94, 16)
        Me.lblRibbonColorMode.TabIndex = 5
        Me.lblRibbonColorMode.Text = "Ribbon colors:"
        '
        'cmbPalette
        '
        Me.cmbPalette.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPalette.FormattingEnabled = True
        Me.cmbPalette.Location = New System.Drawing.Point(170, 21)
        Me.cmbPalette.Name = "cmbPalette"
        Me.cmbPalette.Size = New System.Drawing.Size(250, 24)
        Me.cmbPalette.TabIndex = 4
        '
        'lblPalette
        '
        Me.lblPalette.AutoSize = True
        Me.lblPalette.Location = New System.Drawing.Point(6, 24)
        Me.lblPalette.Name = "lblPalette"
        Me.lblPalette.Size = New System.Drawing.Size(86, 16)
        Me.lblPalette.TabIndex = 3
        Me.lblPalette.Text = "Color palette:"
        '
        'grpLabels
        '
        Me.grpLabels.Controls.Add(Me.txtChartTitle)
        Me.grpLabels.Controls.Add(Me.ckShowLegend)
        Me.grpLabels.Controls.Add(Me.ckShowStageLabels)
        Me.grpLabels.Controls.Add(Me.cmbPercentageBasis)
        Me.grpLabels.Controls.Add(Me.ckShowNodeLabels)
        Me.grpLabels.Controls.Add(Me.lblChartTitle)
        Me.grpLabels.Controls.Add(Me.lblPercentageBasis)
        Me.grpLabels.Controls.Add(Me.cmbNodeLabelMode)
        Me.grpLabels.Controls.Add(Me.lblNodeLabelMode)
        Me.grpLabels.Location = New System.Drawing.Point(5, 150)
        Me.grpLabels.Name = "grpLabels"
        Me.grpLabels.Size = New System.Drawing.Size(439, 144)
        Me.grpLabels.TabIndex = 12
        Me.grpLabels.TabStop = False
        Me.grpLabels.Text = "Labels"
        '
        'txtChartTitle
        '
        Me.txtChartTitle.Location = New System.Drawing.Point(144, 110)
        Me.txtChartTitle.Name = "txtChartTitle"
        Me.txtChartTitle.Size = New System.Drawing.Size(276, 22)
        Me.txtChartTitle.TabIndex = 16
        Me.txtChartTitle.Text = "Sankey chart"
        '
        'ckShowLegend
        '
        Me.ckShowLegend.AutoSize = True
        Me.ckShowLegend.Location = New System.Drawing.Point(313, 25)
        Me.ckShowLegend.Name = "ckShowLegend"
        Me.ckShowLegend.Size = New System.Drawing.Size(107, 20)
        Me.ckShowLegend.TabIndex = 15
        Me.ckShowLegend.Text = "Show legend"
        Me.ckShowLegend.UseVisualStyleBackColor = True
        '
        'ckShowStageLabels
        '
        Me.ckShowStageLabels.AutoSize = True
        Me.ckShowStageLabels.Checked = True
        Me.ckShowStageLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowStageLabels.Location = New System.Drawing.Point(170, 25)
        Me.ckShowStageLabels.Name = "ckShowStageLabels"
        Me.ckShowStageLabels.Size = New System.Drawing.Size(139, 20)
        Me.ckShowStageLabels.TabIndex = 14
        Me.ckShowStageLabels.Text = "Show stage labels"
        Me.ckShowStageLabels.UseVisualStyleBackColor = True
        '
        'cmbPercentageBasis
        '
        Me.cmbPercentageBasis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPercentageBasis.FormattingEnabled = True
        Me.cmbPercentageBasis.Location = New System.Drawing.Point(144, 80)
        Me.cmbPercentageBasis.Name = "cmbPercentageBasis"
        Me.cmbPercentageBasis.Size = New System.Drawing.Size(276, 24)
        Me.cmbPercentageBasis.TabIndex = 13
        '
        'ckShowNodeLabels
        '
        Me.ckShowNodeLabels.AutoSize = True
        Me.ckShowNodeLabels.Checked = True
        Me.ckShowNodeLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowNodeLabels.Location = New System.Drawing.Point(9, 25)
        Me.ckShowNodeLabels.Name = "ckShowNodeLabels"
        Me.ckShowNodeLabels.Size = New System.Drawing.Size(136, 20)
        Me.ckShowNodeLabels.TabIndex = 12
        Me.ckShowNodeLabels.Text = "Show node labels"
        Me.ckShowNodeLabels.UseVisualStyleBackColor = True
        '
        'lblChartTitle
        '
        Me.lblChartTitle.AutoSize = True
        Me.lblChartTitle.Location = New System.Drawing.Point(6, 113)
        Me.lblChartTitle.Name = "lblChartTitle"
        Me.lblChartTitle.Size = New System.Drawing.Size(64, 16)
        Me.lblChartTitle.TabIndex = 10
        Me.lblChartTitle.Text = "Chart title:"
        '
        'lblPercentageBasis
        '
        Me.lblPercentageBasis.AutoSize = True
        Me.lblPercentageBasis.Location = New System.Drawing.Point(6, 83)
        Me.lblPercentageBasis.Name = "lblPercentageBasis"
        Me.lblPercentageBasis.Size = New System.Drawing.Size(116, 16)
        Me.lblPercentageBasis.TabIndex = 6
        Me.lblPercentageBasis.Text = "Percentage basis:"
        '
        'cmbNodeLabelMode
        '
        Me.cmbNodeLabelMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbNodeLabelMode.FormattingEnabled = True
        Me.cmbNodeLabelMode.Location = New System.Drawing.Point(144, 51)
        Me.cmbNodeLabelMode.Name = "cmbNodeLabelMode"
        Me.cmbNodeLabelMode.Size = New System.Drawing.Size(276, 24)
        Me.cmbNodeLabelMode.TabIndex = 5
        '
        'lblNodeLabelMode
        '
        Me.lblNodeLabelMode.AutoSize = True
        Me.lblNodeLabelMode.Location = New System.Drawing.Point(6, 54)
        Me.lblNodeLabelMode.Name = "lblNodeLabelMode"
        Me.lblNodeLabelMode.Size = New System.Drawing.Size(84, 16)
        Me.lblNodeLabelMode.TabIndex = 4
        Me.lblNodeLabelMode.Text = "Node labels:"
        '
        'grpLayout
        '
        Me.grpLayout.Controls.Add(Me.nudRibbonCurvature)
        Me.grpLayout.Controls.Add(Me.lblRibbonCurvature)
        Me.grpLayout.Controls.Add(Me.nudNodeWidth)
        Me.grpLayout.Controls.Add(Me.lblNodeWidth)
        Me.grpLayout.Controls.Add(Me.nudNodeGap)
        Me.grpLayout.Controls.Add(Me.lblNodeGap)
        Me.grpLayout.Controls.Add(Me.cmbVerticalAlignment)
        Me.grpLayout.Controls.Add(Me.lblVerticalAlignment)
        Me.grpLayout.Controls.Add(Me.cmbNodeOrder)
        Me.grpLayout.Controls.Add(Me.lblNodeOrder)
        Me.grpLayout.Location = New System.Drawing.Point(5, 3)
        Me.grpLayout.Name = "grpLayout"
        Me.grpLayout.Size = New System.Drawing.Size(439, 141)
        Me.grpLayout.TabIndex = 0
        Me.grpLayout.TabStop = False
        Me.grpLayout.Text = "Layout"
        '
        'nudRibbonCurvature
        '
        Me.nudRibbonCurvature.Location = New System.Drawing.Point(144, 109)
        Me.nudRibbonCurvature.Maximum = New Decimal(New Integer() {95, 0, 0, 0})
        Me.nudRibbonCurvature.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudRibbonCurvature.Name = "nudRibbonCurvature"
        Me.nudRibbonCurvature.Size = New System.Drawing.Size(74, 22)
        Me.nudRibbonCurvature.TabIndex = 11
        Me.nudRibbonCurvature.Value = New Decimal(New Integer() {50, 0, 0, 0})
        '
        'lblRibbonCurvature
        '
        Me.lblRibbonCurvature.AutoSize = True
        Me.lblRibbonCurvature.Location = New System.Drawing.Point(6, 111)
        Me.lblRibbonCurvature.Name = "lblRibbonCurvature"
        Me.lblRibbonCurvature.Size = New System.Drawing.Size(135, 16)
        Me.lblRibbonCurvature.TabIndex = 10
        Me.lblRibbonCurvature.Text = "Ribbon curvature (%):"
        '
        'nudNodeWidth
        '
        Me.nudNodeWidth.Location = New System.Drawing.Point(346, 83)
        Me.nudNodeWidth.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.nudNodeWidth.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudNodeWidth.Name = "nudNodeWidth"
        Me.nudNodeWidth.Size = New System.Drawing.Size(74, 22)
        Me.nudNodeWidth.TabIndex = 9
        Me.nudNodeWidth.Value = New Decimal(New Integer() {18, 0, 0, 0})
        '
        'lblNodeWidth
        '
        Me.lblNodeWidth.AutoSize = True
        Me.lblNodeWidth.Location = New System.Drawing.Point(246, 85)
        Me.lblNodeWidth.Name = "lblNodeWidth"
        Me.lblNodeWidth.Size = New System.Drawing.Size(77, 16)
        Me.lblNodeWidth.TabIndex = 8
        Me.lblNodeWidth.Text = "Node width:"
        '
        'nudNodeGap
        '
        Me.nudNodeGap.DecimalPlaces = 1
        Me.nudNodeGap.Location = New System.Drawing.Point(144, 81)
        Me.nudNodeGap.Maximum = New Decimal(New Integer() {20, 0, 0, 0})
        Me.nudNodeGap.Name = "nudNodeGap"
        Me.nudNodeGap.Size = New System.Drawing.Size(74, 22)
        Me.nudNodeGap.TabIndex = 7
        Me.nudNodeGap.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'lblNodeGap
        '
        Me.lblNodeGap.AutoSize = True
        Me.lblNodeGap.Location = New System.Drawing.Point(6, 83)
        Me.lblNodeGap.Name = "lblNodeGap"
        Me.lblNodeGap.Size = New System.Drawing.Size(94, 16)
        Me.lblNodeGap.TabIndex = 6
        Me.lblNodeGap.Text = "Node gap (%):"
        '
        'cmbVerticalAlignment
        '
        Me.cmbVerticalAlignment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbVerticalAlignment.FormattingEnabled = True
        Me.cmbVerticalAlignment.Location = New System.Drawing.Point(144, 51)
        Me.cmbVerticalAlignment.Name = "cmbVerticalAlignment"
        Me.cmbVerticalAlignment.Size = New System.Drawing.Size(276, 24)
        Me.cmbVerticalAlignment.TabIndex = 5
        '
        'lblVerticalAlignment
        '
        Me.lblVerticalAlignment.AutoSize = True
        Me.lblVerticalAlignment.Location = New System.Drawing.Point(6, 54)
        Me.lblVerticalAlignment.Name = "lblVerticalAlignment"
        Me.lblVerticalAlignment.Size = New System.Drawing.Size(116, 16)
        Me.lblVerticalAlignment.TabIndex = 4
        Me.lblVerticalAlignment.Text = "Vertical alignment:"
        '
        'cmbNodeOrder
        '
        Me.cmbNodeOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbNodeOrder.FormattingEnabled = True
        Me.cmbNodeOrder.Location = New System.Drawing.Point(144, 21)
        Me.cmbNodeOrder.Name = "cmbNodeOrder"
        Me.cmbNodeOrder.Size = New System.Drawing.Size(276, 24)
        Me.cmbNodeOrder.TabIndex = 3
        '
        'lblNodeOrder
        '
        Me.lblNodeOrder.AutoSize = True
        Me.lblNodeOrder.Location = New System.Drawing.Point(6, 24)
        Me.lblNodeOrder.Name = "lblNodeOrder"
        Me.lblNodeOrder.Size = New System.Drawing.Size(79, 16)
        Me.lblNodeOrder.TabIndex = 2
        Me.lblNodeOrder.Text = "Node order:"
        '
        'Ui01SankeyPlot
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(461, 544)
        Me.Controls.Add(Me.TabMultipage)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(479, 591)
        Me.Name = "Ui01SankeyPlot"
        Me.ShowIcon = False
        Me.Text = "Sankey Plot"
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1_StagedData.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPage2_SourceTarget.ResumeLayout(False)
        Me.TabPage2_SourceTarget.PerformLayout()
        Me.TabPage3_Appearance.ResumeLayout(False)
        Me.grpChartSize.ResumeLayout(False)
        Me.grpChartSize.PerformLayout()
        CType(Me.nudChartHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudChartWidth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpColors.ResumeLayout(False)
        Me.grpColors.PerformLayout()
        CType(Me.nudRibbonTransparency, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpLabels.ResumeLayout(False)
        Me.grpLabels.PerformLayout()
        Me.grpLayout.ResumeLayout(False)
        Me.grpLayout.PerformLayout()
        CType(Me.nudRibbonCurvature, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudNodeWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudNodeGap, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1_StagedData As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents TabPage3_Appearance As Windows.Forms.TabPage
    Friend WithEvents grpLayout As Windows.Forms.GroupBox
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents RefEdit_StagedWeight As Excel2007RefEdit
    Friend WithEvents lblStagedWeight As Windows.Forms.Label
    Friend WithEvents RefEdit_Stages As Excel2007RefEdit
    Friend WithEvents lblStages As Windows.Forms.Label
    Friend WithEvents ckConnectAcrossMissingStages As Windows.Forms.CheckBox
    Friend WithEvents ckStagedFirstRowLabels As Windows.Forms.CheckBox
    Friend WithEvents lblNote As Windows.Forms.Label
    Friend WithEvents lblNodeGap As Windows.Forms.Label
    Friend WithEvents cmbVerticalAlignment As Windows.Forms.ComboBox
    Friend WithEvents lblVerticalAlignment As Windows.Forms.Label
    Friend WithEvents cmbNodeOrder As Windows.Forms.ComboBox
    Friend WithEvents lblNodeOrder As Windows.Forms.Label
    Friend WithEvents nudNodeGap As Windows.Forms.NumericUpDown
    Friend WithEvents nudNodeWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblNodeWidth As Windows.Forms.Label
    Friend WithEvents nudRibbonCurvature As Windows.Forms.NumericUpDown
    Friend WithEvents lblRibbonCurvature As Windows.Forms.Label
    Friend WithEvents grpLabels As Windows.Forms.GroupBox
    Friend WithEvents ckShowNodeLabels As Windows.Forms.CheckBox
    Friend WithEvents lblChartTitle As Windows.Forms.Label
    Friend WithEvents lblPercentageBasis As Windows.Forms.Label
    Friend WithEvents cmbNodeLabelMode As Windows.Forms.ComboBox
    Friend WithEvents lblNodeLabelMode As Windows.Forms.Label
    Friend WithEvents ckShowStageLabels As Windows.Forms.CheckBox
    Friend WithEvents cmbPercentageBasis As Windows.Forms.ComboBox
    Friend WithEvents txtChartTitle As Windows.Forms.TextBox
    Friend WithEvents ckShowLegend As Windows.Forms.CheckBox
    Friend WithEvents grpColors As Windows.Forms.GroupBox
    Friend WithEvents nudRibbonTransparency As Windows.Forms.NumericUpDown
    Friend WithEvents lblRibbonTransparency As Windows.Forms.Label
    Friend WithEvents cmbRibbonColorMode As Windows.Forms.ComboBox
    Friend WithEvents lblRibbonColorMode As Windows.Forms.Label
    Friend WithEvents cmbPalette As Windows.Forms.ComboBox
    Friend WithEvents lblPalette As Windows.Forms.Label
    Friend WithEvents ckShowRibbonOutline As Windows.Forms.CheckBox
    Friend WithEvents grpChartSize As Windows.Forms.GroupBox
    Friend WithEvents nudChartHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartHeight As Windows.Forms.Label
    Friend WithEvents nudChartWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartWidth As Windows.Forms.Label
    Friend WithEvents TabPage2_SourceTarget As Windows.Forms.TabPage
    Friend WithEvents RefEdit_LinkWeight As Excel2007RefEdit
    Friend WithEvents lblLinkWeight As Windows.Forms.Label
    Friend WithEvents RefEdit_Target As Excel2007RefEdit
    Friend WithEvents lblTarget As Windows.Forms.Label
    Friend WithEvents RefEdit_Source As Excel2007RefEdit
    Friend WithEvents lblSource As Windows.Forms.Label
    Friend WithEvents RefEdit_SourceStage As Excel2007RefEdit
    Friend WithEvents lblSourceStage As Windows.Forms.Label
    Friend WithEvents RefEdit_TargetStage As Excel2007RefEdit
    Friend WithEvents lblTargetStage As Windows.Forms.Label
    Friend WithEvents ckLinkFirstRowLabels As Windows.Forms.CheckBox
    Friend WithEvents lblNoteSourceTarget As Windows.Forms.Label
End Class
