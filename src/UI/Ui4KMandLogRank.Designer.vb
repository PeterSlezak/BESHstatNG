<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui4KMandLogRank
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui4KMandLogRank))
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.RefEdit1_SurvivalTime = New BESHStatNG.Excel2007RefEdit()
        Me.lblStrata = New System.Windows.Forms.Label()
        Me.RefEdit4_StrataID = New BESHStatNG.Excel2007RefEdit()
        Me.lblGroup = New System.Windows.Forms.Label()
        Me.RefEdit3_GroupID = New BESHStatNG.Excel2007RefEdit()
        Me.lblCensor = New System.Windows.Forms.Label()
        Me.RefEdit2_Censor = New BESHStatNG.Excel2007RefEdit()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.TabPage_OptionsLogRank = New System.Windows.Forms.TabPage()
        Me.grpLogRankWeights = New System.Windows.Forms.GroupBox()
        Me.optModPeto = New System.Windows.Forms.RadioButton()
        Me.optPeto = New System.Windows.Forms.RadioButton()
        Me.optTaroneWare = New System.Windows.Forms.RadioButton()
        Me.optGehanBreslow = New System.Windows.Forms.RadioButton()
        Me.optLogRank = New System.Windows.Forms.RadioButton()
        Me.TabPage_OptionsKM = New System.Windows.Forms.TabPage()
        Me.grpChartOptions = New System.Windows.Forms.GroupBox()
        Me.tbTitleText = New System.Windows.Forms.TextBox()
        Me.ckPlotCI = New System.Windows.Forms.CheckBox()
        Me.ckDisplayTitle = New System.Windows.Forms.CheckBox()
        Me.ckShowLegend = New System.Windows.Forms.CheckBox()
        Me.cbXunits = New System.Windows.Forms.ComboBox()
        Me.lblXaxisUnit = New System.Windows.Forms.Label()
        Me.grpKMOptions = New System.Windows.Forms.GroupBox()
        Me.ckBCtest = New System.Windows.Forms.CheckBox()
        Me.ckCSCatFTP = New System.Windows.Forms.CheckBox()
        Me.ckCIoutput = New System.Windows.Forms.CheckBox()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.TabPage_OptionsLogRank.SuspendLayout()
        Me.grpLogRankWeights.SuspendLayout()
        Me.TabPage_OptionsKM.SuspendLayout()
        Me.grpChartOptions.SuspendLayout()
        Me.grpKMOptions.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsLogRank)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsKM)
        Me.TabMultipage.Location = New System.Drawing.Point(12, 12)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(465, 364)
        Me.TabMultipage.TabIndex = 4
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.grpInput)
        Me.TabPage1.Controls.Add(Me.grpOutput)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(457, 335)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Input"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.RefEdit1_SurvivalTime)
        Me.grpInput.Controls.Add(Me.lblStrata)
        Me.grpInput.Controls.Add(Me.RefEdit4_StrataID)
        Me.grpInput.Controls.Add(Me.lblGroup)
        Me.grpInput.Controls.Add(Me.RefEdit3_GroupID)
        Me.grpInput.Controls.Add(Me.lblCensor)
        Me.grpInput.Controls.Add(Me.RefEdit2_Censor)
        Me.grpInput.Controls.Add(Me.lblTime)
        Me.grpInput.Location = New System.Drawing.Point(6, 10)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(442, 183)
        Me.grpInput.TabIndex = 13
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'RefEdit1_SurvivalTime
        '
        Me.RefEdit1_SurvivalTime.Address = ""
        Me.RefEdit1_SurvivalTime.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit1_SurvivalTime.ExcelConnector = Nothing
        Me.RefEdit1_SurvivalTime.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit1_SurvivalTime.ImageMinimized = CType(resources.GetObject("RefEdit1_SurvivalTime.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit1_SurvivalTime.Location = New System.Drawing.Point(161, 22)
        Me.RefEdit1_SurvivalTime.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit1_SurvivalTime.Name = "RefEdit1_SurvivalTime"
        Me.RefEdit1_SurvivalTime.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit1_SurvivalTime.Size = New System.Drawing.Size(280, 32)
        Me.RefEdit1_SurvivalTime.TabIndex = 8
        '
        'lblStrata
        '
        Me.lblStrata.AutoSize = True
        Me.lblStrata.Location = New System.Drawing.Point(6, 142)
        Me.lblStrata.Name = "lblStrata"
        Me.lblStrata.Size = New System.Drawing.Size(117, 16)
        Me.lblStrata.TabIndex = 12
        Me.lblStrata.Text = "Strata ID (optional)"
        '
        'RefEdit4_StrataID
        '
        Me.RefEdit4_StrataID.Address = ""
        Me.RefEdit4_StrataID.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit4_StrataID.ExcelConnector = Nothing
        Me.RefEdit4_StrataID.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit4_StrataID.ImageMinimized = CType(resources.GetObject("RefEdit4_StrataID.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit4_StrataID.Location = New System.Drawing.Point(161, 142)
        Me.RefEdit4_StrataID.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit4_StrataID.Name = "RefEdit4_StrataID"
        Me.RefEdit4_StrataID.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit4_StrataID.Size = New System.Drawing.Size(280, 32)
        Me.RefEdit4_StrataID.TabIndex = 5
        '
        'lblGroup
        '
        Me.lblGroup.AutoSize = True
        Me.lblGroup.Location = New System.Drawing.Point(6, 102)
        Me.lblGroup.Name = "lblGroup"
        Me.lblGroup.Size = New System.Drawing.Size(121, 16)
        Me.lblGroup.TabIndex = 11
        Me.lblGroup.Text = "Group ID (Optional)"
        '
        'RefEdit3_GroupID
        '
        Me.RefEdit3_GroupID.Address = ""
        Me.RefEdit3_GroupID.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit3_GroupID.ExcelConnector = Nothing
        Me.RefEdit3_GroupID.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit3_GroupID.ImageMinimized = CType(resources.GetObject("RefEdit3_GroupID.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit3_GroupID.Location = New System.Drawing.Point(161, 102)
        Me.RefEdit3_GroupID.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit3_GroupID.Name = "RefEdit3_GroupID"
        Me.RefEdit3_GroupID.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit3_GroupID.Size = New System.Drawing.Size(280, 32)
        Me.RefEdit3_GroupID.TabIndex = 6
        '
        'lblCensor
        '
        Me.lblCensor.Location = New System.Drawing.Point(6, 62)
        Me.lblCensor.Name = "lblCensor"
        Me.lblCensor.Size = New System.Drawing.Size(153, 32)
        Me.lblCensor.TabIndex = 10
        Me.lblCensor.Text = "Censorship identificator (1 - event; 0 - censored)"
        '
        'RefEdit2_Censor
        '
        Me.RefEdit2_Censor.Address = ""
        Me.RefEdit2_Censor.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit2_Censor.ExcelConnector = Nothing
        Me.RefEdit2_Censor.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit2_Censor.ImageMinimized = CType(resources.GetObject("RefEdit2_Censor.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit2_Censor.Location = New System.Drawing.Point(161, 62)
        Me.RefEdit2_Censor.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit2_Censor.Name = "RefEdit2_Censor"
        Me.RefEdit2_Censor.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit2_Censor.Size = New System.Drawing.Size(280, 32)
        Me.RefEdit2_Censor.TabIndex = 7
        '
        'lblTime
        '
        Me.lblTime.AutoSize = True
        Me.lblTime.Location = New System.Drawing.Point(6, 22)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(96, 16)
        Me.lblTime.TabIndex = 9
        Me.lblTime.Text = "Survival Times"
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(6, 199)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(442, 130)
        Me.grpOutput.TabIndex = 4
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
        Me.RefEditOutput.Size = New System.Drawing.Size(280, 32)
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
        'TabPage_OptionsLogRank
        '
        Me.TabPage_OptionsLogRank.Controls.Add(Me.grpLogRankWeights)
        Me.TabPage_OptionsLogRank.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsLogRank.Name = "TabPage_OptionsLogRank"
        Me.TabPage_OptionsLogRank.Size = New System.Drawing.Size(457, 335)
        Me.TabPage_OptionsLogRank.TabIndex = 3
        Me.TabPage_OptionsLogRank.Text = "Options"
        Me.TabPage_OptionsLogRank.UseVisualStyleBackColor = True
        '
        'grpLogRankWeights
        '
        Me.grpLogRankWeights.Controls.Add(Me.optModPeto)
        Me.grpLogRankWeights.Controls.Add(Me.optPeto)
        Me.grpLogRankWeights.Controls.Add(Me.optTaroneWare)
        Me.grpLogRankWeights.Controls.Add(Me.optGehanBreslow)
        Me.grpLogRankWeights.Controls.Add(Me.optLogRank)
        Me.grpLogRankWeights.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpLogRankWeights.Location = New System.Drawing.Point(17, 20)
        Me.grpLogRankWeights.Name = "grpLogRankWeights"
        Me.grpLogRankWeights.Size = New System.Drawing.Size(424, 177)
        Me.grpLogRankWeights.TabIndex = 3
        Me.grpLogRankWeights.TabStop = False
        Me.grpLogRankWeights.Text = "Log-Rank Weights"
        '
        'optModPeto
        '
        Me.optModPeto.AutoSize = True
        Me.optModPeto.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optModPeto.Location = New System.Drawing.Point(18, 136)
        Me.optModPeto.Name = "optModPeto"
        Me.optModPeto.Size = New System.Drawing.Size(143, 20)
        Me.optModPeto.TabIndex = 4
        Me.optModPeto.Text = "Modified Peto-Peto"
        Me.optModPeto.UseVisualStyleBackColor = True
        '
        'optPeto
        '
        Me.optPeto.AutoSize = True
        Me.optPeto.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optPeto.Location = New System.Drawing.Point(18, 110)
        Me.optPeto.Name = "optPeto"
        Me.optPeto.Size = New System.Drawing.Size(88, 20)
        Me.optPeto.TabIndex = 3
        Me.optPeto.Text = "Peto-Peto"
        Me.optPeto.UseVisualStyleBackColor = True
        '
        'optTaroneWare
        '
        Me.optTaroneWare.AutoSize = True
        Me.optTaroneWare.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optTaroneWare.Location = New System.Drawing.Point(18, 84)
        Me.optTaroneWare.Name = "optTaroneWare"
        Me.optTaroneWare.Size = New System.Drawing.Size(109, 20)
        Me.optTaroneWare.TabIndex = 2
        Me.optTaroneWare.Text = "Tarone-Ware"
        Me.optTaroneWare.UseVisualStyleBackColor = True
        '
        'optGehanBreslow
        '
        Me.optGehanBreslow.AutoSize = True
        Me.optGehanBreslow.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optGehanBreslow.Location = New System.Drawing.Point(18, 58)
        Me.optGehanBreslow.Name = "optGehanBreslow"
        Me.optGehanBreslow.Size = New System.Drawing.Size(120, 20)
        Me.optGehanBreslow.TabIndex = 1
        Me.optGehanBreslow.Text = "Gehan-Breslow"
        Me.optGehanBreslow.UseVisualStyleBackColor = True
        '
        'optLogRank
        '
        Me.optLogRank.AutoSize = True
        Me.optLogRank.Checked = True
        Me.optLogRank.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optLogRank.Location = New System.Drawing.Point(18, 32)
        Me.optLogRank.Name = "optLogRank"
        Me.optLogRank.Size = New System.Drawing.Size(81, 20)
        Me.optLogRank.TabIndex = 0
        Me.optLogRank.TabStop = True
        Me.optLogRank.Text = "Log-rank"
        Me.optLogRank.UseVisualStyleBackColor = True
        '
        'TabPage_OptionsKM
        '
        Me.TabPage_OptionsKM.Controls.Add(Me.grpChartOptions)
        Me.TabPage_OptionsKM.Controls.Add(Me.grpKMOptions)
        Me.TabPage_OptionsKM.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsKM.Name = "TabPage_OptionsKM"
        Me.TabPage_OptionsKM.Size = New System.Drawing.Size(457, 335)
        Me.TabPage_OptionsKM.TabIndex = 4
        Me.TabPage_OptionsKM.Text = "Options"
        Me.TabPage_OptionsKM.UseVisualStyleBackColor = True
        '
        'grpChartOptions
        '
        Me.grpChartOptions.Controls.Add(Me.tbTitleText)
        Me.grpChartOptions.Controls.Add(Me.ckPlotCI)
        Me.grpChartOptions.Controls.Add(Me.ckDisplayTitle)
        Me.grpChartOptions.Controls.Add(Me.ckShowLegend)
        Me.grpChartOptions.Controls.Add(Me.cbXunits)
        Me.grpChartOptions.Controls.Add(Me.lblXaxisUnit)
        Me.grpChartOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpChartOptions.Location = New System.Drawing.Point(9, 139)
        Me.grpChartOptions.Name = "grpChartOptions"
        Me.grpChartOptions.Size = New System.Drawing.Size(432, 154)
        Me.grpChartOptions.TabIndex = 1
        Me.grpChartOptions.TabStop = False
        Me.grpChartOptions.Text = "Chart Options"
        '
        'tbTitleText
        '
        Me.tbTitleText.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbTitleText.Location = New System.Drawing.Point(119, 94)
        Me.tbTitleText.Name = "tbTitleText"
        Me.tbTitleText.Size = New System.Drawing.Size(227, 22)
        Me.tbTitleText.TabIndex = 5
        Me.tbTitleText.Text = "Kaplan–Meier Plot"
        '
        'ckPlotCI
        '
        Me.ckPlotCI.AutoSize = True
        Me.ckPlotCI.Checked = True
        Me.ckPlotCI.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckPlotCI.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckPlotCI.Location = New System.Drawing.Point(6, 120)
        Me.ckPlotCI.Name = "ckPlotCI"
        Me.ckPlotCI.Size = New System.Drawing.Size(205, 20)
        Me.ckPlotCI.TabIndex = 4
        Me.ckPlotCI.Text = "Plot 95% Confidence Intervals"
        Me.ckPlotCI.UseVisualStyleBackColor = True
        '
        'ckDisplayTitle
        '
        Me.ckDisplayTitle.AutoSize = True
        Me.ckDisplayTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckDisplayTitle.Location = New System.Drawing.Point(6, 94)
        Me.ckDisplayTitle.Name = "ckDisplayTitle"
        Me.ckDisplayTitle.Size = New System.Drawing.Size(107, 20)
        Me.ckDisplayTitle.TabIndex = 3
        Me.ckDisplayTitle.Text = "Display Title:"
        Me.ckDisplayTitle.UseVisualStyleBackColor = True
        '
        'ckShowLegend
        '
        Me.ckShowLegend.AutoSize = True
        Me.ckShowLegend.Checked = True
        Me.ckShowLegend.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowLegend.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckShowLegend.Location = New System.Drawing.Point(6, 68)
        Me.ckShowLegend.Name = "ckShowLegend"
        Me.ckShowLegend.Size = New System.Drawing.Size(256, 20)
        Me.ckShowLegend.TabIndex = 2
        Me.ckShowLegend.Text = "Show Legend if More Than One Group"
        Me.ckShowLegend.UseVisualStyleBackColor = True
        '
        'cbXunits
        '
        Me.cbXunits.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbXunits.FormattingEnabled = True
        Me.cbXunits.Location = New System.Drawing.Point(119, 28)
        Me.cbXunits.Name = "cbXunits"
        Me.cbXunits.Size = New System.Drawing.Size(227, 24)
        Me.cbXunits.TabIndex = 1
        '
        'lblXaxisUnit
        '
        Me.lblXaxisUnit.AutoSize = True
        Me.lblXaxisUnit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblXaxisUnit.Location = New System.Drawing.Point(3, 34)
        Me.lblXaxisUnit.Name = "lblXaxisUnit"
        Me.lblXaxisUnit.Size = New System.Drawing.Size(69, 16)
        Me.lblXaxisUnit.TabIndex = 0
        Me.lblXaxisUnit.Text = "X-axis Unit"
        '
        'grpKMOptions
        '
        Me.grpKMOptions.Controls.Add(Me.ckBCtest)
        Me.grpKMOptions.Controls.Add(Me.ckCSCatFTP)
        Me.grpKMOptions.Controls.Add(Me.ckCIoutput)
        Me.grpKMOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpKMOptions.Location = New System.Drawing.Point(3, 24)
        Me.grpKMOptions.Name = "grpKMOptions"
        Me.grpKMOptions.Size = New System.Drawing.Size(438, 100)
        Me.grpKMOptions.TabIndex = 0
        Me.grpKMOptions.TabStop = False
        Me.grpKMOptions.Text = "KM Options"
        '
        'ckBCtest
        '
        Me.ckBCtest.AutoSize = True
        Me.ckBCtest.Checked = True
        Me.ckBCtest.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBCtest.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBCtest.Location = New System.Drawing.Point(6, 73)
        Me.ckBCtest.Name = "ckBCtest"
        Me.ckBCtest.Size = New System.Drawing.Size(194, 20)
        Me.ckBCtest.TabIndex = 2
        Me.ckBCtest.Text = "Test for Equality of Medians"
        Me.ckBCtest.UseVisualStyleBackColor = True
        '
        'ckCSCatFTP
        '
        Me.ckCSCatFTP.AutoSize = True
        Me.ckCSCatFTP.Checked = True
        Me.ckCSCatFTP.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckCSCatFTP.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckCSCatFTP.Location = New System.Drawing.Point(6, 47)
        Me.ckCSCatFTP.Name = "ckCSCatFTP"
        Me.ckCSCatFTP.Size = New System.Drawing.Size(419, 20)
        Me.ckCSCatFTP.TabIndex = 1
        Me.ckCSCatFTP.Text = "Compare of Survival Curves at a Fixed Time Point (No Groups = 2)"
        Me.ckCSCatFTP.UseVisualStyleBackColor = True
        '
        'ckCIoutput
        '
        Me.ckCIoutput.AutoSize = True
        Me.ckCIoutput.Checked = True
        Me.ckCIoutput.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckCIoutput.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckCIoutput.Location = New System.Drawing.Point(6, 21)
        Me.ckCIoutput.Name = "ckCIoutput"
        Me.ckCIoutput.Size = New System.Drawing.Size(210, 20)
        Me.ckCIoutput.TabIndex = 0
        Me.ckCIoutput.Text = "Detailed Survival Curve Output"
        Me.ckCIoutput.UseVisualStyleBackColor = True
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(317, 378)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 6
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(398, 378)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 5
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'Ui4KMandLogRank
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(483, 408)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.TabMultipage)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimumSize = New System.Drawing.Size(501, 455)
        Me.Name = "Ui4KMandLogRank"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Ui4KMandLogRank"
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.TabPage_OptionsLogRank.ResumeLayout(False)
        Me.grpLogRankWeights.ResumeLayout(False)
        Me.grpLogRankWeights.PerformLayout()
        Me.TabPage_OptionsKM.ResumeLayout(False)
        Me.grpChartOptions.ResumeLayout(False)
        Me.grpChartOptions.PerformLayout()
        Me.grpKMOptions.ResumeLayout(False)
        Me.grpKMOptions.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents TabPage_OptionsLogRank As Windows.Forms.TabPage
    Friend WithEvents grpLogRankWeights As Windows.Forms.GroupBox
    Friend WithEvents optPeto As Windows.Forms.RadioButton
    Friend WithEvents optTaroneWare As Windows.Forms.RadioButton
    Friend WithEvents optGehanBreslow As Windows.Forms.RadioButton
    Friend WithEvents optLogRank As Windows.Forms.RadioButton
    Friend WithEvents TabPage_OptionsKM As Windows.Forms.TabPage
    Friend WithEvents grpChartOptions As Windows.Forms.GroupBox
    Friend WithEvents grpKMOptions As Windows.Forms.GroupBox
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents ckBCtest As Windows.Forms.CheckBox
    Friend WithEvents ckCSCatFTP As Windows.Forms.CheckBox
    Friend WithEvents ckCIoutput As Windows.Forms.CheckBox
    Friend WithEvents ckPlotCI As Windows.Forms.CheckBox
    Friend WithEvents ckDisplayTitle As Windows.Forms.CheckBox
    Friend WithEvents ckShowLegend As Windows.Forms.CheckBox
    Friend WithEvents cbXunits As Windows.Forms.ComboBox
    Friend WithEvents lblXaxisUnit As Windows.Forms.Label
    Friend WithEvents tbTitleText As Windows.Forms.TextBox
    Friend WithEvents optModPeto As Windows.Forms.RadioButton
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents RefEdit1_SurvivalTime As Excel2007RefEdit
    Friend WithEvents RefEdit2_Censor As Excel2007RefEdit
    Friend WithEvents RefEdit3_GroupID As Excel2007RefEdit
    Friend WithEvents RefEdit4_StrataID As Excel2007RefEdit
    Friend WithEvents lblStrata As Windows.Forms.Label
    Friend WithEvents lblGroup As Windows.Forms.Label
    Friend WithEvents lblCensor As Windows.Forms.Label
    Friend WithEvents lblTime As Windows.Forms.Label
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    'Friend WithEvents RefEdit1_StrataID As Excel2007RefEdit
End Class
