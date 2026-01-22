<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UibyID
    Inherits System.Windows.Forms.Form

    'UserControl overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UibyID))
        Me.btCompute = New System.Windows.Forms.Button()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.lblRefedit2 = New System.Windows.Forms.Label()
        Me.lblRefedit1 = New System.Windows.Forms.Label()
        Me.RefEdit1 = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit2 = New BESHStatNG.Excel2007RefEdit()
        Me.optByID = New System.Windows.Forms.RadioButton()
        Me.optByColumn = New System.Windows.Forms.RadioButton()
        Me.TabPage_Options = New System.Windows.Forms.TabPage()
        Me.grpHomogeneityVariances = New System.Windows.Forms.GroupBox()
        Me.ckLevene = New System.Windows.Forms.CheckBox()
        Me.ckBartlett = New System.Windows.Forms.CheckBox()
        Me.ckSquaredRanks = New System.Windows.Forms.CheckBox()
        Me.ckFlignerKilleen = New System.Windows.Forms.CheckBox()
        Me.ckWelch = New System.Windows.Forms.CheckBox()
        Me.grpANOVA1MCP = New System.Windows.Forms.GroupBox()
        Me.ckGamesHowell = New System.Windows.Forms.CheckBox()
        Me.ckBonferroni = New System.Windows.Forms.CheckBox()
        Me.ckLSD = New System.Windows.Forms.CheckBox()
        Me.ckTukey = New System.Windows.Forms.CheckBox()
        Me.ckBoxPlot = New System.Windows.Forms.CheckBox()
        Me.ckDescriptiveStatistics = New System.Windows.Forms.CheckBox()
        Me.ckEstimateOfShift = New System.Windows.Forms.CheckBox()
        Me.TabPage_OptionsDescriptive = New System.Windows.Forms.TabPage()
        Me.grpDescriptiveStat = New System.Windows.Forms.GroupBox()
        Me.ckRange = New System.Windows.Forms.CheckBox()
        Me.ckMax = New System.Windows.Forms.CheckBox()
        Me.ckMin = New System.Windows.Forms.CheckBox()
        Me.ckIQR = New System.Windows.Forms.CheckBox()
        Me.ckQ3 = New System.Windows.Forms.CheckBox()
        Me.ckQ1 = New System.Windows.Forms.CheckBox()
        Me.ckKurtosis = New System.Windows.Forms.CheckBox()
        Me.ckSkewness = New System.Windows.Forms.CheckBox()
        Me.ckSEM = New System.Windows.Forms.CheckBox()
        Me.ckSD = New System.Windows.Forms.CheckBox()
        Me.ckShapiroWilk = New System.Windows.Forms.CheckBox()
        Me.ckVariance = New System.Windows.Forms.CheckBox()
        Me.ckCV = New System.Windows.Forms.CheckBox()
        Me.ckMedian = New System.Windows.Forms.CheckBox()
        Me.ckMean = New System.Windows.Forms.CheckBox()
        Me.ckN = New System.Windows.Forms.CheckBox()
        Me.ckBoxPlot_Descriptive = New System.Windows.Forms.CheckBox()
        Me.TabPage_OptionsHistogram = New System.Windows.Forms.TabPage()
        Me.ckOverlay = New System.Windows.Forms.CheckBox()
        Me.grpBinSize = New System.Windows.Forms.GroupBox()
        Me.optScott = New System.Windows.Forms.RadioButton()
        Me.optFreedmanDiaconis = New System.Windows.Forms.RadioButton()
        Me.optDoane = New System.Windows.Forms.RadioButton()
        Me.optSturges = New System.Windows.Forms.RadioButton()
        Me.ckDescriptive_Histogram = New System.Windows.Forms.CheckBox()
        Me.TabPage_OptionsNormalPlot = New System.Windows.Forms.TabPage()
        Me.ckDescriptive_NormalPlot = New System.Windows.Forms.CheckBox()
        Me.grpReferenceLine = New System.Windows.Forms.GroupBox()
        Me.optR = New System.Windows.Forms.RadioButton()
        Me.optOLS = New System.Windows.Forms.RadioButton()
        Me.optSPSS = New System.Windows.Forms.RadioButton()
        Me.grpNormalScores = New System.Windows.Forms.GroupBox()
        Me.optVanDerWaerden = New System.Windows.Forms.RadioButton()
        Me.optRankit = New System.Windows.Forms.RadioButton()
        Me.optBlom = New System.Windows.Forms.RadioButton()
        Me.TabPage_OptionsSymmetry = New System.Windows.Forms.TabPage()
        Me.ckSymmetryPlot = New System.Windows.Forms.CheckBox()
        Me.ckDescriptive_Symmetry = New System.Windows.Forms.CheckBox()
        Me.grpSymmetryTest = New System.Windows.Forms.GroupBox()
        Me.optCM = New System.Windows.Forms.RadioButton()
        Me.optMGG = New System.Windows.Forms.RadioButton()
        Me.TabPage_OptionsOutliers = New System.Windows.Forms.TabPage()
        Me.ckBoxPlot_Outliers = New System.Windows.Forms.CheckBox()
        Me.grpOutlierTests = New System.Windows.Forms.GroupBox()
        Me.lblAlphaOutliers = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
        Me.optRosner = New System.Windows.Forms.RadioButton()
        Me.optGrubbs = New System.Windows.Forms.RadioButton()
        Me.ckDescriptive_Outliers = New System.Windows.Forms.CheckBox()
        Me.progressBarExactCalc = New System.Windows.Forms.ProgressBar()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPage_Options.SuspendLayout()
        Me.grpHomogeneityVariances.SuspendLayout()
        Me.grpANOVA1MCP.SuspendLayout()
        Me.TabPage_OptionsDescriptive.SuspendLayout()
        Me.grpDescriptiveStat.SuspendLayout()
        Me.TabPage_OptionsHistogram.SuspendLayout()
        Me.grpBinSize.SuspendLayout()
        Me.TabPage_OptionsNormalPlot.SuspendLayout()
        Me.grpReferenceLine.SuspendLayout()
        Me.grpNormalScores.SuspendLayout()
        Me.TabPage_OptionsSymmetry.SuspendLayout()
        Me.grpSymmetryTest.SuspendLayout()
        Me.TabPage_OptionsOutliers.SuspendLayout()
        Me.grpOutlierTests.SuspendLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(392, 377)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 1
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(311, 377)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 2
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1)
        Me.TabMultipage.Controls.Add(Me.TabPage_Options)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsDescriptive)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsHistogram)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsNormalPlot)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsSymmetry)
        Me.TabMultipage.Controls.Add(Me.TabPage_OptionsOutliers)
        Me.TabMultipage.Location = New System.Drawing.Point(9, 7)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(462, 364)
        Me.TabMultipage.TabIndex = 3
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.grpOutput)
        Me.TabPage1.Controls.Add(Me.grpInput)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(454, 335)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Input"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(6, 170)
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
        Me.grpInput.Controls.Add(Me.lblRefedit2)
        Me.grpInput.Controls.Add(Me.lblRefedit1)
        Me.grpInput.Controls.Add(Me.RefEdit1)
        Me.grpInput.Controls.Add(Me.RefEdit2)
        Me.grpInput.Controls.Add(Me.optByID)
        Me.grpInput.Controls.Add(Me.optByColumn)
        Me.grpInput.Location = New System.Drawing.Point(6, 6)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(442, 158)
        Me.grpInput.TabIndex = 1
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'lblRefedit2
        '
        Me.lblRefedit2.Location = New System.Drawing.Point(6, 104)
        Me.lblRefedit2.Name = "lblRefedit2"
        Me.lblRefedit2.Size = New System.Drawing.Size(155, 51)
        Me.lblRefedit2.TabIndex = 3
        Me.lblRefedit2.Text = "Data:"
        '
        'lblRefedit1
        '
        Me.lblRefedit1.Location = New System.Drawing.Point(6, 60)
        Me.lblRefedit1.Name = "lblRefedit1"
        Me.lblRefedit1.Size = New System.Drawing.Size(155, 44)
        Me.lblRefedit1.TabIndex = 2
        Me.lblRefedit1.Text = "Group ID:"
        '
        'RefEdit1
        '
        Me.RefEdit1.Address = ""
        Me.RefEdit1.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit1.ExcelConnector = Nothing
        Me.RefEdit1.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit1.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit1.Location = New System.Drawing.Point(159, 64)
        Me.RefEdit1.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit1.Name = "RefEdit1"
        Me.RefEdit1.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit1.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit1.TabIndex = 4
        '
        'RefEdit2
        '
        Me.RefEdit2.Address = ""
        Me.RefEdit2.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit2.ExcelConnector = Nothing
        Me.RefEdit2.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit2.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit2.Location = New System.Drawing.Point(159, 104)
        Me.RefEdit2.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit2.Name = "RefEdit2"
        Me.RefEdit2.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit2.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit2.TabIndex = 5
        '
        'optByID
        '
        Me.optByID.AutoSize = True
        Me.optByID.Checked = True
        Me.optByID.Location = New System.Drawing.Point(38, 37)
        Me.optByID.Name = "optByID"
        Me.optByID.Size = New System.Drawing.Size(99, 20)
        Me.optByID.TabIndex = 1
        Me.optByID.TabStop = True
        Me.optByID.Text = "Group by ID"
        Me.optByID.UseVisualStyleBackColor = True
        '
        'optByColumn
        '
        Me.optByColumn.AutoSize = True
        Me.optByColumn.Location = New System.Drawing.Point(192, 37)
        Me.optByColumn.Name = "optByColumn"
        Me.optByColumn.Size = New System.Drawing.Size(131, 20)
        Me.optByColumn.TabIndex = 0
        Me.optByColumn.Text = "Group by Column"
        Me.optByColumn.UseVisualStyleBackColor = True
        '
        'TabPage_Options
        '
        Me.TabPage_Options.Controls.Add(Me.grpHomogeneityVariances)
        Me.TabPage_Options.Controls.Add(Me.ckWelch)
        Me.TabPage_Options.Controls.Add(Me.grpANOVA1MCP)
        Me.TabPage_Options.Controls.Add(Me.ckBoxPlot)
        Me.TabPage_Options.Controls.Add(Me.ckDescriptiveStatistics)
        Me.TabPage_Options.Controls.Add(Me.ckEstimateOfShift)
        Me.TabPage_Options.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_Options.Name = "TabPage_Options"
        Me.TabPage_Options.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage_Options.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_Options.TabIndex = 1
        Me.TabPage_Options.Text = "Options"
        Me.TabPage_Options.UseVisualStyleBackColor = True
        '
        'grpHomogeneityVariances
        '
        Me.grpHomogeneityVariances.Controls.Add(Me.ckLevene)
        Me.grpHomogeneityVariances.Controls.Add(Me.ckBartlett)
        Me.grpHomogeneityVariances.Controls.Add(Me.ckSquaredRanks)
        Me.grpHomogeneityVariances.Controls.Add(Me.ckFlignerKilleen)
        Me.grpHomogeneityVariances.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHomogeneityVariances.Location = New System.Drawing.Point(17, 74)
        Me.grpHomogeneityVariances.Name = "grpHomogeneityVariances"
        Me.grpHomogeneityVariances.Size = New System.Drawing.Size(294, 91)
        Me.grpHomogeneityVariances.TabIndex = 5
        Me.grpHomogeneityVariances.TabStop = False
        Me.grpHomogeneityVariances.Text = "Homogeneity of Variances"
        Me.grpHomogeneityVariances.Visible = False
        '
        'ckLevene
        '
        Me.ckLevene.AutoSize = True
        Me.ckLevene.Checked = True
        Me.ckLevene.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckLevene.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckLevene.Location = New System.Drawing.Point(13, 53)
        Me.ckLevene.Name = "ckLevene"
        Me.ckLevene.Size = New System.Drawing.Size(114, 20)
        Me.ckLevene.TabIndex = 3
        Me.ckLevene.Text = "Levene's Test"
        Me.ckLevene.UseVisualStyleBackColor = True
        '
        'ckBartlett
        '
        Me.ckBartlett.AutoSize = True
        Me.ckBartlett.Checked = True
        Me.ckBartlett.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBartlett.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBartlett.Location = New System.Drawing.Point(166, 53)
        Me.ckBartlett.Name = "ckBartlett"
        Me.ckBartlett.Size = New System.Drawing.Size(110, 20)
        Me.ckBartlett.TabIndex = 2
        Me.ckBartlett.Text = "Bartlett's Test"
        Me.ckBartlett.UseVisualStyleBackColor = True
        '
        'ckSquaredRanks
        '
        Me.ckSquaredRanks.AutoSize = True
        Me.ckSquaredRanks.Checked = True
        Me.ckSquaredRanks.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckSquaredRanks.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckSquaredRanks.Location = New System.Drawing.Point(166, 27)
        Me.ckSquaredRanks.Name = "ckSquaredRanks"
        Me.ckSquaredRanks.Size = New System.Drawing.Size(123, 20)
        Me.ckSquaredRanks.TabIndex = 1
        Me.ckSquaredRanks.Text = "Squared Ranks"
        Me.ckSquaredRanks.UseVisualStyleBackColor = True
        '
        'ckFlignerKilleen
        '
        Me.ckFlignerKilleen.AutoSize = True
        Me.ckFlignerKilleen.Checked = True
        Me.ckFlignerKilleen.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckFlignerKilleen.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckFlignerKilleen.Location = New System.Drawing.Point(13, 27)
        Me.ckFlignerKilleen.Name = "ckFlignerKilleen"
        Me.ckFlignerKilleen.Size = New System.Drawing.Size(144, 20)
        Me.ckFlignerKilleen.TabIndex = 0
        Me.ckFlignerKilleen.Text = "Fligner-Killeen Test"
        Me.ckFlignerKilleen.UseVisualStyleBackColor = True
        '
        'ckWelch
        '
        Me.ckWelch.AutoSize = True
        Me.ckWelch.Checked = True
        Me.ckWelch.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckWelch.Location = New System.Drawing.Point(17, 171)
        Me.ckWelch.Name = "ckWelch"
        Me.ckWelch.Size = New System.Drawing.Size(157, 20)
        Me.ckWelch.TabIndex = 4
        Me.ckWelch.Text = "Perform Welch's Test"
        Me.ckWelch.UseVisualStyleBackColor = True
        Me.ckWelch.Visible = False
        '
        'grpANOVA1MCP
        '
        Me.grpANOVA1MCP.Controls.Add(Me.ckGamesHowell)
        Me.grpANOVA1MCP.Controls.Add(Me.ckBonferroni)
        Me.grpANOVA1MCP.Controls.Add(Me.ckLSD)
        Me.grpANOVA1MCP.Controls.Add(Me.ckTukey)
        Me.grpANOVA1MCP.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpANOVA1MCP.Location = New System.Drawing.Point(17, 197)
        Me.grpANOVA1MCP.Name = "grpANOVA1MCP"
        Me.grpANOVA1MCP.Size = New System.Drawing.Size(294, 91)
        Me.grpANOVA1MCP.TabIndex = 3
        Me.grpANOVA1MCP.TabStop = False
        Me.grpANOVA1MCP.Text = "Multiple Comparisons"
        Me.grpANOVA1MCP.Visible = False
        '
        'ckGamesHowell
        '
        Me.ckGamesHowell.AutoSize = True
        Me.ckGamesHowell.Checked = True
        Me.ckGamesHowell.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckGamesHowell.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckGamesHowell.Location = New System.Drawing.Point(166, 53)
        Me.ckGamesHowell.Name = "ckGamesHowell"
        Me.ckGamesHowell.Size = New System.Drawing.Size(117, 20)
        Me.ckGamesHowell.TabIndex = 3
        Me.ckGamesHowell.Text = "Games Howell"
        Me.ckGamesHowell.UseVisualStyleBackColor = True
        '
        'ckBonferroni
        '
        Me.ckBonferroni.AutoSize = True
        Me.ckBonferroni.Checked = True
        Me.ckBonferroni.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBonferroni.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBonferroni.Location = New System.Drawing.Point(13, 53)
        Me.ckBonferroni.Name = "ckBonferroni"
        Me.ckBonferroni.Size = New System.Drawing.Size(90, 20)
        Me.ckBonferroni.TabIndex = 2
        Me.ckBonferroni.Text = "Bonferroni"
        Me.ckBonferroni.UseVisualStyleBackColor = True
        '
        'ckLSD
        '
        Me.ckLSD.AutoSize = True
        Me.ckLSD.Checked = True
        Me.ckLSD.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckLSD.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckLSD.Location = New System.Drawing.Point(166, 27)
        Me.ckLSD.Name = "ckLSD"
        Me.ckLSD.Size = New System.Drawing.Size(55, 20)
        Me.ckLSD.TabIndex = 1
        Me.ckLSD.Text = "LSD"
        Me.ckLSD.UseVisualStyleBackColor = True
        '
        'ckTukey
        '
        Me.ckTukey.AutoSize = True
        Me.ckTukey.Checked = True
        Me.ckTukey.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckTukey.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckTukey.Location = New System.Drawing.Point(13, 27)
        Me.ckTukey.Name = "ckTukey"
        Me.ckTukey.Size = New System.Drawing.Size(114, 20)
        Me.ckTukey.TabIndex = 0
        Me.ckTukey.Text = "Tukey-Kramer"
        Me.ckTukey.UseVisualStyleBackColor = True
        '
        'ckBoxPlot
        '
        Me.ckBoxPlot.AutoSize = True
        Me.ckBoxPlot.Checked = True
        Me.ckBoxPlot.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBoxPlot.Location = New System.Drawing.Point(17, 22)
        Me.ckBoxPlot.Name = "ckBoxPlot"
        Me.ckBoxPlot.Size = New System.Drawing.Size(163, 20)
        Me.ckBoxPlot.TabIndex = 2
        Me.ckBoxPlot.Text = "Box and Whiskers Plot"
        Me.ckBoxPlot.UseVisualStyleBackColor = True
        Me.ckBoxPlot.Visible = False
        '
        'ckDescriptiveStatistics
        '
        Me.ckDescriptiveStatistics.AutoSize = True
        Me.ckDescriptiveStatistics.Checked = True
        Me.ckDescriptiveStatistics.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDescriptiveStatistics.Location = New System.Drawing.Point(17, 48)
        Me.ckDescriptiveStatistics.Name = "ckDescriptiveStatistics"
        Me.ckDescriptiveStatistics.Size = New System.Drawing.Size(177, 20)
        Me.ckDescriptiveStatistics.TabIndex = 1
        Me.ckDescriptiveStatistics.Text = "Full Descriptive Statistics"
        Me.ckDescriptiveStatistics.UseVisualStyleBackColor = True
        Me.ckDescriptiveStatistics.Visible = False
        '
        'ckEstimateOfShift
        '
        Me.ckEstimateOfShift.AutoSize = True
        Me.ckEstimateOfShift.Checked = True
        Me.ckEstimateOfShift.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckEstimateOfShift.Location = New System.Drawing.Point(213, 22)
        Me.ckEstimateOfShift.Name = "ckEstimateOfShift"
        Me.ckEstimateOfShift.Size = New System.Drawing.Size(227, 20)
        Me.ckEstimateOfShift.TabIndex = 0
        Me.ckEstimateOfShift.Text = "Hodges-Lehman Estimate of Shift"
        Me.ckEstimateOfShift.UseVisualStyleBackColor = True
        Me.ckEstimateOfShift.Visible = False
        '
        'TabPage_OptionsDescriptive
        '
        Me.TabPage_OptionsDescriptive.Controls.Add(Me.grpDescriptiveStat)
        Me.TabPage_OptionsDescriptive.Controls.Add(Me.ckBoxPlot_Descriptive)
        Me.TabPage_OptionsDescriptive.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsDescriptive.Name = "TabPage_OptionsDescriptive"
        Me.TabPage_OptionsDescriptive.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_OptionsDescriptive.TabIndex = 2
        Me.TabPage_OptionsDescriptive.Text = "Options"
        Me.TabPage_OptionsDescriptive.UseVisualStyleBackColor = True
        '
        'grpDescriptiveStat
        '
        Me.grpDescriptiveStat.Controls.Add(Me.ckRange)
        Me.grpDescriptiveStat.Controls.Add(Me.ckMax)
        Me.grpDescriptiveStat.Controls.Add(Me.ckMin)
        Me.grpDescriptiveStat.Controls.Add(Me.ckIQR)
        Me.grpDescriptiveStat.Controls.Add(Me.ckQ3)
        Me.grpDescriptiveStat.Controls.Add(Me.ckQ1)
        Me.grpDescriptiveStat.Controls.Add(Me.ckKurtosis)
        Me.grpDescriptiveStat.Controls.Add(Me.ckSkewness)
        Me.grpDescriptiveStat.Controls.Add(Me.ckSEM)
        Me.grpDescriptiveStat.Controls.Add(Me.ckSD)
        Me.grpDescriptiveStat.Controls.Add(Me.ckShapiroWilk)
        Me.grpDescriptiveStat.Controls.Add(Me.ckVariance)
        Me.grpDescriptiveStat.Controls.Add(Me.ckCV)
        Me.grpDescriptiveStat.Controls.Add(Me.ckMedian)
        Me.grpDescriptiveStat.Controls.Add(Me.ckMean)
        Me.grpDescriptiveStat.Controls.Add(Me.ckN)
        Me.grpDescriptiveStat.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpDescriptiveStat.Location = New System.Drawing.Point(10, 43)
        Me.grpDescriptiveStat.Name = "grpDescriptiveStat"
        Me.grpDescriptiveStat.Size = New System.Drawing.Size(425, 203)
        Me.grpDescriptiveStat.TabIndex = 4
        Me.grpDescriptiveStat.TabStop = False
        Me.grpDescriptiveStat.Text = "Statistics"
        '
        'ckRange
        '
        Me.ckRange.AutoSize = True
        Me.ckRange.Checked = True
        Me.ckRange.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckRange.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckRange.Location = New System.Drawing.Point(299, 131)
        Me.ckRange.Name = "ckRange"
        Me.ckRange.Size = New System.Drawing.Size(70, 20)
        Me.ckRange.TabIndex = 15
        Me.ckRange.Text = "Range"
        Me.ckRange.UseVisualStyleBackColor = True
        '
        'ckMax
        '
        Me.ckMax.AutoSize = True
        Me.ckMax.Checked = True
        Me.ckMax.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMax.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMax.Location = New System.Drawing.Point(299, 105)
        Me.ckMax.Name = "ckMax"
        Me.ckMax.Size = New System.Drawing.Size(86, 20)
        Me.ckMax.TabIndex = 14
        Me.ckMax.Text = "Maximum"
        Me.ckMax.UseVisualStyleBackColor = True
        '
        'ckMin
        '
        Me.ckMin.AutoSize = True
        Me.ckMin.Checked = True
        Me.ckMin.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMin.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMin.Location = New System.Drawing.Point(299, 79)
        Me.ckMin.Name = "ckMin"
        Me.ckMin.Size = New System.Drawing.Size(82, 20)
        Me.ckMin.TabIndex = 13
        Me.ckMin.Text = "Minimum"
        Me.ckMin.UseVisualStyleBackColor = True
        '
        'ckIQR
        '
        Me.ckIQR.AutoSize = True
        Me.ckIQR.Checked = True
        Me.ckIQR.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckIQR.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckIQR.Location = New System.Drawing.Point(299, 53)
        Me.ckIQR.Name = "ckIQR"
        Me.ckIQR.Size = New System.Drawing.Size(52, 20)
        Me.ckIQR.TabIndex = 12
        Me.ckIQR.Text = "IQR"
        Me.ckIQR.UseVisualStyleBackColor = True
        '
        'ckQ3
        '
        Me.ckQ3.AutoSize = True
        Me.ckQ3.Checked = True
        Me.ckQ3.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckQ3.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckQ3.Location = New System.Drawing.Point(299, 27)
        Me.ckQ3.Name = "ckQ3"
        Me.ckQ3.Size = New System.Drawing.Size(46, 20)
        Me.ckQ3.TabIndex = 11
        Me.ckQ3.Text = "Q3"
        Me.ckQ3.UseVisualStyleBackColor = True
        '
        'ckQ1
        '
        Me.ckQ1.AutoSize = True
        Me.ckQ1.Checked = True
        Me.ckQ1.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckQ1.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckQ1.Location = New System.Drawing.Point(180, 131)
        Me.ckQ1.Name = "ckQ1"
        Me.ckQ1.Size = New System.Drawing.Size(46, 20)
        Me.ckQ1.TabIndex = 10
        Me.ckQ1.Text = "Q1"
        Me.ckQ1.UseVisualStyleBackColor = True
        '
        'ckKurtosis
        '
        Me.ckKurtosis.AutoSize = True
        Me.ckKurtosis.Checked = True
        Me.ckKurtosis.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckKurtosis.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckKurtosis.Location = New System.Drawing.Point(180, 105)
        Me.ckKurtosis.Name = "ckKurtosis"
        Me.ckKurtosis.Size = New System.Drawing.Size(76, 20)
        Me.ckKurtosis.TabIndex = 9
        Me.ckKurtosis.Text = "Kurtosis"
        Me.ckKurtosis.UseVisualStyleBackColor = True
        '
        'ckSkewness
        '
        Me.ckSkewness.AutoSize = True
        Me.ckSkewness.Checked = True
        Me.ckSkewness.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckSkewness.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckSkewness.Location = New System.Drawing.Point(180, 79)
        Me.ckSkewness.Name = "ckSkewness"
        Me.ckSkewness.Size = New System.Drawing.Size(91, 20)
        Me.ckSkewness.TabIndex = 8
        Me.ckSkewness.Text = "Skewness"
        Me.ckSkewness.UseVisualStyleBackColor = True
        '
        'ckSEM
        '
        Me.ckSEM.AutoSize = True
        Me.ckSEM.Checked = True
        Me.ckSEM.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckSEM.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckSEM.Location = New System.Drawing.Point(180, 53)
        Me.ckSEM.Name = "ckSEM"
        Me.ckSEM.Size = New System.Drawing.Size(58, 20)
        Me.ckSEM.TabIndex = 7
        Me.ckSEM.Text = "SEM"
        Me.ckSEM.UseVisualStyleBackColor = True
        '
        'ckSD
        '
        Me.ckSD.AutoSize = True
        Me.ckSD.Checked = True
        Me.ckSD.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckSD.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckSD.Location = New System.Drawing.Point(180, 27)
        Me.ckSD.Name = "ckSD"
        Me.ckSD.Size = New System.Drawing.Size(48, 20)
        Me.ckSD.TabIndex = 6
        Me.ckSD.Text = "SD"
        Me.ckSD.UseVisualStyleBackColor = True
        '
        'ckShapiroWilk
        '
        Me.ckShapiroWilk.AutoSize = True
        Me.ckShapiroWilk.Checked = True
        Me.ckShapiroWilk.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShapiroWilk.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckShapiroWilk.Location = New System.Drawing.Point(13, 169)
        Me.ckShapiroWilk.Name = "ckShapiroWilk"
        Me.ckShapiroWilk.Size = New System.Drawing.Size(136, 20)
        Me.ckShapiroWilk.TabIndex = 5
        Me.ckShapiroWilk.Text = "Shapiro-Wilk Test"
        Me.ckShapiroWilk.UseVisualStyleBackColor = True
        '
        'ckVariance
        '
        Me.ckVariance.AutoSize = True
        Me.ckVariance.Checked = True
        Me.ckVariance.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckVariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckVariance.Location = New System.Drawing.Point(13, 131)
        Me.ckVariance.Name = "ckVariance"
        Me.ckVariance.Size = New System.Drawing.Size(83, 20)
        Me.ckVariance.TabIndex = 4
        Me.ckVariance.Text = "Variance"
        Me.ckVariance.UseVisualStyleBackColor = True
        '
        'ckCV
        '
        Me.ckCV.AutoSize = True
        Me.ckCV.Checked = True
        Me.ckCV.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckCV.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckCV.Location = New System.Drawing.Point(13, 105)
        Me.ckCV.Name = "ckCV"
        Me.ckCV.Size = New System.Drawing.Size(161, 20)
        Me.ckCV.TabIndex = 3
        Me.ckCV.Text = "Coefficient of Variation"
        Me.ckCV.UseVisualStyleBackColor = True
        '
        'ckMedian
        '
        Me.ckMedian.AutoSize = True
        Me.ckMedian.Checked = True
        Me.ckMedian.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMedian.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMedian.Location = New System.Drawing.Point(13, 79)
        Me.ckMedian.Name = "ckMedian"
        Me.ckMedian.Size = New System.Drawing.Size(74, 20)
        Me.ckMedian.TabIndex = 2
        Me.ckMedian.Text = "Median"
        Me.ckMedian.UseVisualStyleBackColor = True
        '
        'ckMean
        '
        Me.ckMean.AutoSize = True
        Me.ckMean.Checked = True
        Me.ckMean.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckMean.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckMean.Location = New System.Drawing.Point(13, 53)
        Me.ckMean.Name = "ckMean"
        Me.ckMean.Size = New System.Drawing.Size(63, 20)
        Me.ckMean.TabIndex = 1
        Me.ckMean.Text = "Mean"
        Me.ckMean.UseVisualStyleBackColor = True
        '
        'ckN
        '
        Me.ckN.AutoSize = True
        Me.ckN.Checked = True
        Me.ckN.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckN.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckN.Location = New System.Drawing.Point(13, 27)
        Me.ckN.Name = "ckN"
        Me.ckN.Size = New System.Drawing.Size(109, 20)
        Me.ckN.TabIndex = 0
        Me.ckN.Text = "N (valid data)"
        Me.ckN.UseVisualStyleBackColor = True
        '
        'ckBoxPlot_Descriptive
        '
        Me.ckBoxPlot_Descriptive.AutoSize = True
        Me.ckBoxPlot_Descriptive.Checked = True
        Me.ckBoxPlot_Descriptive.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBoxPlot_Descriptive.Location = New System.Drawing.Point(21, 17)
        Me.ckBoxPlot_Descriptive.Name = "ckBoxPlot_Descriptive"
        Me.ckBoxPlot_Descriptive.Size = New System.Drawing.Size(163, 20)
        Me.ckBoxPlot_Descriptive.TabIndex = 3
        Me.ckBoxPlot_Descriptive.Text = "Box and Whiskers Plot"
        Me.ckBoxPlot_Descriptive.UseVisualStyleBackColor = True
        Me.ckBoxPlot_Descriptive.Visible = False
        '
        'TabPage_OptionsHistogram
        '
        Me.TabPage_OptionsHistogram.Controls.Add(Me.ckOverlay)
        Me.TabPage_OptionsHistogram.Controls.Add(Me.grpBinSize)
        Me.TabPage_OptionsHistogram.Controls.Add(Me.ckDescriptive_Histogram)
        Me.TabPage_OptionsHistogram.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsHistogram.Name = "TabPage_OptionsHistogram"
        Me.TabPage_OptionsHistogram.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_OptionsHistogram.TabIndex = 3
        Me.TabPage_OptionsHistogram.Text = "Options"
        Me.TabPage_OptionsHistogram.UseVisualStyleBackColor = True
        '
        'ckOverlay
        '
        Me.ckOverlay.AutoSize = True
        Me.ckOverlay.Checked = True
        Me.ckOverlay.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckOverlay.Location = New System.Drawing.Point(22, 50)
        Me.ckOverlay.Name = "ckOverlay"
        Me.ckOverlay.Size = New System.Drawing.Size(195, 20)
        Me.ckOverlay.TabIndex = 4
        Me.ckOverlay.Text = "Superimpose Normal Curve"
        Me.ckOverlay.UseVisualStyleBackColor = True
        '
        'grpBinSize
        '
        Me.grpBinSize.Controls.Add(Me.optScott)
        Me.grpBinSize.Controls.Add(Me.optFreedmanDiaconis)
        Me.grpBinSize.Controls.Add(Me.optDoane)
        Me.grpBinSize.Controls.Add(Me.optSturges)
        Me.grpBinSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpBinSize.Location = New System.Drawing.Point(22, 103)
        Me.grpBinSize.Name = "grpBinSize"
        Me.grpBinSize.Size = New System.Drawing.Size(319, 142)
        Me.grpBinSize.TabIndex = 3
        Me.grpBinSize.TabStop = False
        Me.grpBinSize.Text = "Bin-sizing Method"
        '
        'optScott
        '
        Me.optScott.AutoSize = True
        Me.optScott.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optScott.Location = New System.Drawing.Point(18, 110)
        Me.optScott.Name = "optScott"
        Me.optScott.Size = New System.Drawing.Size(58, 20)
        Me.optScott.TabIndex = 3
        Me.optScott.Text = "Scott"
        Me.optScott.UseVisualStyleBackColor = True
        '
        'optFreedmanDiaconis
        '
        Me.optFreedmanDiaconis.AutoSize = True
        Me.optFreedmanDiaconis.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optFreedmanDiaconis.Location = New System.Drawing.Point(18, 84)
        Me.optFreedmanDiaconis.Name = "optFreedmanDiaconis"
        Me.optFreedmanDiaconis.Size = New System.Drawing.Size(147, 20)
        Me.optFreedmanDiaconis.TabIndex = 2
        Me.optFreedmanDiaconis.Text = "Freedman-Diaconis"
        Me.optFreedmanDiaconis.UseVisualStyleBackColor = True
        '
        'optDoane
        '
        Me.optDoane.AutoSize = True
        Me.optDoane.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optDoane.Location = New System.Drawing.Point(18, 58)
        Me.optDoane.Name = "optDoane"
        Me.optDoane.Size = New System.Drawing.Size(61, 20)
        Me.optDoane.TabIndex = 1
        Me.optDoane.Text = "Doan"
        Me.optDoane.UseVisualStyleBackColor = True
        '
        'optSturges
        '
        Me.optSturges.AutoSize = True
        Me.optSturges.Checked = True
        Me.optSturges.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optSturges.Location = New System.Drawing.Point(18, 32)
        Me.optSturges.Name = "optSturges"
        Me.optSturges.Size = New System.Drawing.Size(74, 20)
        Me.optSturges.TabIndex = 0
        Me.optSturges.TabStop = True
        Me.optSturges.Text = "Sturges"
        Me.optSturges.UseVisualStyleBackColor = True
        '
        'ckDescriptive_Histogram
        '
        Me.ckDescriptive_Histogram.AutoSize = True
        Me.ckDescriptive_Histogram.Checked = True
        Me.ckDescriptive_Histogram.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDescriptive_Histogram.Location = New System.Drawing.Point(22, 24)
        Me.ckDescriptive_Histogram.Name = "ckDescriptive_Histogram"
        Me.ckDescriptive_Histogram.Size = New System.Drawing.Size(177, 20)
        Me.ckDescriptive_Histogram.TabIndex = 2
        Me.ckDescriptive_Histogram.Text = "Full Descriptive Statistics"
        Me.ckDescriptive_Histogram.UseVisualStyleBackColor = True
        '
        'TabPage_OptionsNormalPlot
        '
        Me.TabPage_OptionsNormalPlot.Controls.Add(Me.ckDescriptive_NormalPlot)
        Me.TabPage_OptionsNormalPlot.Controls.Add(Me.grpReferenceLine)
        Me.TabPage_OptionsNormalPlot.Controls.Add(Me.grpNormalScores)
        Me.TabPage_OptionsNormalPlot.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsNormalPlot.Name = "TabPage_OptionsNormalPlot"
        Me.TabPage_OptionsNormalPlot.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_OptionsNormalPlot.TabIndex = 4
        Me.TabPage_OptionsNormalPlot.Text = "Options"
        Me.TabPage_OptionsNormalPlot.UseVisualStyleBackColor = True
        '
        'ckDescriptive_NormalPlot
        '
        Me.ckDescriptive_NormalPlot.AutoSize = True
        Me.ckDescriptive_NormalPlot.Checked = True
        Me.ckDescriptive_NormalPlot.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDescriptive_NormalPlot.Location = New System.Drawing.Point(29, 16)
        Me.ckDescriptive_NormalPlot.Name = "ckDescriptive_NormalPlot"
        Me.ckDescriptive_NormalPlot.Size = New System.Drawing.Size(177, 20)
        Me.ckDescriptive_NormalPlot.TabIndex = 8
        Me.ckDescriptive_NormalPlot.Text = "Full Descriptive Statistics"
        Me.ckDescriptive_NormalPlot.UseVisualStyleBackColor = True
        '
        'grpReferenceLine
        '
        Me.grpReferenceLine.Controls.Add(Me.optR)
        Me.grpReferenceLine.Controls.Add(Me.optOLS)
        Me.grpReferenceLine.Controls.Add(Me.optSPSS)
        Me.grpReferenceLine.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpReferenceLine.Location = New System.Drawing.Point(29, 167)
        Me.grpReferenceLine.Name = "grpReferenceLine"
        Me.grpReferenceLine.Size = New System.Drawing.Size(339, 100)
        Me.grpReferenceLine.TabIndex = 1
        Me.grpReferenceLine.TabStop = False
        Me.grpReferenceLine.Text = "Reference Line"
        '
        'optR
        '
        Me.optR.AutoSize = True
        Me.optR.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optR.Location = New System.Drawing.Point(18, 73)
        Me.optR.Name = "optR"
        Me.optR.Size = New System.Drawing.Size(229, 20)
        Me.optR.TabIndex = 5
        Me.optR.Text = "Line trough 1st and 3rd quartile (R)"
        Me.optR.UseVisualStyleBackColor = True
        '
        'optOLS
        '
        Me.optOLS.AutoSize = True
        Me.optOLS.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optOLS.Location = New System.Drawing.Point(18, 47)
        Me.optOLS.Name = "optOLS"
        Me.optOLS.Size = New System.Drawing.Size(155, 20)
        Me.optOLS.TabIndex = 4
        Me.optOLS.Text = "OLS Regression Line"
        Me.optOLS.UseVisualStyleBackColor = True
        '
        'optSPSS
        '
        Me.optSPSS.AutoSize = True
        Me.optSPSS.Checked = True
        Me.optSPSS.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optSPSS.Location = New System.Drawing.Point(18, 21)
        Me.optSPSS.Name = "optSPSS"
        Me.optSPSS.Size = New System.Drawing.Size(257, 20)
        Me.optSPSS.TabIndex = 3
        Me.optSPSS.TabStop = True
        Me.optSPSS.Text = "Normal quatiles of scaled data (SPSS)"
        Me.optSPSS.UseVisualStyleBackColor = True
        '
        'grpNormalScores
        '
        Me.grpNormalScores.Controls.Add(Me.optVanDerWaerden)
        Me.grpNormalScores.Controls.Add(Me.optRankit)
        Me.grpNormalScores.Controls.Add(Me.optBlom)
        Me.grpNormalScores.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpNormalScores.Location = New System.Drawing.Point(29, 52)
        Me.grpNormalScores.Name = "grpNormalScores"
        Me.grpNormalScores.Size = New System.Drawing.Size(339, 100)
        Me.grpNormalScores.TabIndex = 0
        Me.grpNormalScores.TabStop = False
        Me.grpNormalScores.Text = "Normal Scores"
        '
        'optVanDerWaerden
        '
        Me.optVanDerWaerden.AutoSize = True
        Me.optVanDerWaerden.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optVanDerWaerden.Location = New System.Drawing.Point(18, 74)
        Me.optVanDerWaerden.Name = "optVanDerWaerden"
        Me.optVanDerWaerden.Size = New System.Drawing.Size(148, 20)
        Me.optVanDerWaerden.TabIndex = 2
        Me.optVanDerWaerden.Text = "van  der  Waerden's"
        Me.optVanDerWaerden.UseVisualStyleBackColor = True
        '
        'optRankit
        '
        Me.optRankit.AutoSize = True
        Me.optRankit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optRankit.Location = New System.Drawing.Point(18, 48)
        Me.optRankit.Name = "optRankit"
        Me.optRankit.Size = New System.Drawing.Size(66, 20)
        Me.optRankit.TabIndex = 1
        Me.optRankit.Text = "Rankit"
        Me.optRankit.UseVisualStyleBackColor = True
        '
        'optBlom
        '
        Me.optBlom.AutoSize = True
        Me.optBlom.Checked = True
        Me.optBlom.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optBlom.Location = New System.Drawing.Point(18, 22)
        Me.optBlom.Name = "optBlom"
        Me.optBlom.Size = New System.Drawing.Size(69, 20)
        Me.optBlom.TabIndex = 0
        Me.optBlom.TabStop = True
        Me.optBlom.Text = "Blom's"
        Me.optBlom.UseVisualStyleBackColor = True
        '
        'TabPage_OptionsSymmetry
        '
        Me.TabPage_OptionsSymmetry.Controls.Add(Me.ckSymmetryPlot)
        Me.TabPage_OptionsSymmetry.Controls.Add(Me.ckDescriptive_Symmetry)
        Me.TabPage_OptionsSymmetry.Controls.Add(Me.grpSymmetryTest)
        Me.TabPage_OptionsSymmetry.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsSymmetry.Name = "TabPage_OptionsSymmetry"
        Me.TabPage_OptionsSymmetry.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_OptionsSymmetry.TabIndex = 5
        Me.TabPage_OptionsSymmetry.Text = "Options"
        Me.TabPage_OptionsSymmetry.UseVisualStyleBackColor = True
        '
        'ckSymmetryPlot
        '
        Me.ckSymmetryPlot.AutoSize = True
        Me.ckSymmetryPlot.Checked = True
        Me.ckSymmetryPlot.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckSymmetryPlot.Location = New System.Drawing.Point(22, 155)
        Me.ckSymmetryPlot.Name = "ckSymmetryPlot"
        Me.ckSymmetryPlot.Size = New System.Drawing.Size(115, 20)
        Me.ckSymmetryPlot.TabIndex = 8
        Me.ckSymmetryPlot.Text = "Symmetry Plot"
        Me.ckSymmetryPlot.UseVisualStyleBackColor = True
        '
        'ckDescriptive_Symmetry
        '
        Me.ckDescriptive_Symmetry.AutoSize = True
        Me.ckDescriptive_Symmetry.Checked = True
        Me.ckDescriptive_Symmetry.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDescriptive_Symmetry.Location = New System.Drawing.Point(22, 129)
        Me.ckDescriptive_Symmetry.Name = "ckDescriptive_Symmetry"
        Me.ckDescriptive_Symmetry.Size = New System.Drawing.Size(177, 20)
        Me.ckDescriptive_Symmetry.TabIndex = 7
        Me.ckDescriptive_Symmetry.Text = "Full Descriptive Statistics"
        Me.ckDescriptive_Symmetry.UseVisualStyleBackColor = True
        '
        'grpSymmetryTest
        '
        Me.grpSymmetryTest.Controls.Add(Me.optCM)
        Me.grpSymmetryTest.Controls.Add(Me.optMGG)
        Me.grpSymmetryTest.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpSymmetryTest.Location = New System.Drawing.Point(22, 34)
        Me.grpSymmetryTest.Name = "grpSymmetryTest"
        Me.grpSymmetryTest.Size = New System.Drawing.Size(252, 73)
        Me.grpSymmetryTest.TabIndex = 6
        Me.grpSymmetryTest.TabStop = False
        Me.grpSymmetryTest.Text = "(A)Symmetry Test"
        '
        'optCM
        '
        Me.optCM.AutoSize = True
        Me.optCM.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCM.Location = New System.Drawing.Point(6, 47)
        Me.optCM.Name = "optCM"
        Me.optCM.Size = New System.Drawing.Size(150, 20)
        Me.optCM.TabIndex = 1
        Me.optCM.Text = "Cabilio-Masaro Test"
        Me.optCM.UseVisualStyleBackColor = True
        '
        'optMGG
        '
        Me.optMGG.AutoSize = True
        Me.optMGG.Checked = True
        Me.optMGG.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optMGG.Location = New System.Drawing.Point(6, 21)
        Me.optMGG.Name = "optMGG"
        Me.optMGG.Size = New System.Drawing.Size(171, 20)
        Me.optMGG.TabIndex = 0
        Me.optMGG.TabStop = True
        Me.optMGG.Text = "Miao-Gel-Gastwirth Test"
        Me.optMGG.UseVisualStyleBackColor = True
        '
        'TabPage_OptionsOutliers
        '
        Me.TabPage_OptionsOutliers.Controls.Add(Me.ckBoxPlot_Outliers)
        Me.TabPage_OptionsOutliers.Controls.Add(Me.grpOutlierTests)
        Me.TabPage_OptionsOutliers.Controls.Add(Me.ckDescriptive_Outliers)
        Me.TabPage_OptionsOutliers.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TabPage_OptionsOutliers.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsOutliers.Name = "TabPage_OptionsOutliers"
        Me.TabPage_OptionsOutliers.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_OptionsOutliers.TabIndex = 6
        Me.TabPage_OptionsOutliers.Text = "Options"
        Me.TabPage_OptionsOutliers.UseVisualStyleBackColor = True
        '
        'ckBoxPlot_Outliers
        '
        Me.ckBoxPlot_Outliers.AutoSize = True
        Me.ckBoxPlot_Outliers.Checked = True
        Me.ckBoxPlot_Outliers.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBoxPlot_Outliers.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckBoxPlot_Outliers.Location = New System.Drawing.Point(18, 45)
        Me.ckBoxPlot_Outliers.Name = "ckBoxPlot_Outliers"
        Me.ckBoxPlot_Outliers.Size = New System.Drawing.Size(163, 20)
        Me.ckBoxPlot_Outliers.TabIndex = 10
        Me.ckBoxPlot_Outliers.Text = "Box and Whiskers Plot"
        Me.ckBoxPlot_Outliers.UseVisualStyleBackColor = True
        '
        'grpOutlierTests
        '
        Me.grpOutlierTests.Controls.Add(Me.lblAlphaOutliers)
        Me.grpOutlierTests.Controls.Add(Me.spinBtnAlpha)
        Me.grpOutlierTests.Controls.Add(Me.optRosner)
        Me.grpOutlierTests.Controls.Add(Me.optGrubbs)
        Me.grpOutlierTests.Location = New System.Drawing.Point(18, 88)
        Me.grpOutlierTests.Name = "grpOutlierTests"
        Me.grpOutlierTests.Size = New System.Drawing.Size(323, 129)
        Me.grpOutlierTests.TabIndex = 9
        Me.grpOutlierTests.TabStop = False
        Me.grpOutlierTests.Text = "Outlier Test"
        '
        'lblAlphaOutliers
        '
        Me.lblAlphaOutliers.AutoSize = True
        Me.lblAlphaOutliers.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlphaOutliers.Location = New System.Drawing.Point(17, 32)
        Me.lblAlphaOutliers.Name = "lblAlphaOutliers"
        Me.lblAlphaOutliers.Size = New System.Drawing.Size(42, 16)
        Me.lblAlphaOutliers.TabIndex = 3
        Me.lblAlphaOutliers.Text = "Alpha"
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(65, 30)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 2
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'optRosner
        '
        Me.optRosner.AutoSize = True
        Me.optRosner.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optRosner.Location = New System.Drawing.Point(15, 81)
        Me.optRosner.Name = "optRosner"
        Me.optRosner.Size = New System.Drawing.Size(300, 20)
        Me.optRosner.TabIndex = 1
        Me.optRosner.Text = " Rosner Generalized ESD Test (<= 10 outliers)"
        Me.optRosner.UseVisualStyleBackColor = True
        '
        'optGrubbs
        '
        Me.optGrubbs.AutoSize = True
        Me.optGrubbs.Checked = True
        Me.optGrubbs.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optGrubbs.Location = New System.Drawing.Point(15, 55)
        Me.optGrubbs.Name = "optGrubbs"
        Me.optGrubbs.Size = New System.Drawing.Size(188, 20)
        Me.optGrubbs.TabIndex = 0
        Me.optGrubbs.TabStop = True
        Me.optGrubbs.Text = "Grubbs Test (single outlier)"
        Me.optGrubbs.UseVisualStyleBackColor = True
        '
        'ckDescriptive_Outliers
        '
        Me.ckDescriptive_Outliers.AutoSize = True
        Me.ckDescriptive_Outliers.Checked = True
        Me.ckDescriptive_Outliers.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDescriptive_Outliers.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckDescriptive_Outliers.Location = New System.Drawing.Point(18, 19)
        Me.ckDescriptive_Outliers.Name = "ckDescriptive_Outliers"
        Me.ckDescriptive_Outliers.Size = New System.Drawing.Size(177, 20)
        Me.ckDescriptive_Outliers.TabIndex = 8
        Me.ckDescriptive_Outliers.Text = "Full Descriptive Statistics"
        Me.ckDescriptive_Outliers.UseVisualStyleBackColor = True
        '
        'progressBarExactCalc
        '
        Me.progressBarExactCalc.Location = New System.Drawing.Point(9, 376)
        Me.progressBarExactCalc.Name = "progressBarExactCalc"
        Me.progressBarExactCalc.Size = New System.Drawing.Size(296, 23)
        Me.progressBarExactCalc.TabIndex = 4
        '
        'UibyID
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(474, 410)
        Me.Controls.Add(Me.progressBarExactCalc)
        Me.Controls.Add(Me.TabMultipage)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimumSize = New System.Drawing.Size(483, 457)
        Me.Name = "UibyID"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPage_Options.ResumeLayout(False)
        Me.TabPage_Options.PerformLayout()
        Me.grpHomogeneityVariances.ResumeLayout(False)
        Me.grpHomogeneityVariances.PerformLayout()
        Me.grpANOVA1MCP.ResumeLayout(False)
        Me.grpANOVA1MCP.PerformLayout()
        Me.TabPage_OptionsDescriptive.ResumeLayout(False)
        Me.TabPage_OptionsDescriptive.PerformLayout()
        Me.grpDescriptiveStat.ResumeLayout(False)
        Me.grpDescriptiveStat.PerformLayout()
        Me.TabPage_OptionsHistogram.ResumeLayout(False)
        Me.TabPage_OptionsHistogram.PerformLayout()
        Me.grpBinSize.ResumeLayout(False)
        Me.grpBinSize.PerformLayout()
        Me.TabPage_OptionsNormalPlot.ResumeLayout(False)
        Me.TabPage_OptionsNormalPlot.PerformLayout()
        Me.grpReferenceLine.ResumeLayout(False)
        Me.grpReferenceLine.PerformLayout()
        Me.grpNormalScores.ResumeLayout(False)
        Me.grpNormalScores.PerformLayout()
        Me.TabPage_OptionsSymmetry.ResumeLayout(False)
        Me.TabPage_OptionsSymmetry.PerformLayout()
        Me.grpSymmetryTest.ResumeLayout(False)
        Me.grpSymmetryTest.PerformLayout()
        Me.TabPage_OptionsOutliers.ResumeLayout(False)
        Me.TabPage_OptionsOutliers.PerformLayout()
        Me.grpOutlierTests.ResumeLayout(False)
        Me.grpOutlierTests.PerformLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents lblRefedit2 As Windows.Forms.Label
    Friend WithEvents lblRefedit1 As Windows.Forms.Label
    Friend WithEvents optByID As Windows.Forms.RadioButton
    Friend WithEvents optByColumn As Windows.Forms.RadioButton
    Friend WithEvents TabPage_Options As Windows.Forms.TabPage
    Friend WithEvents RefEdit2 As Excel2007RefEdit
    Friend WithEvents RefEdit1 As Excel2007RefEdit
    Friend WithEvents ckBoxPlot As Windows.Forms.CheckBox
    Friend WithEvents ckDescriptiveStatistics As Windows.Forms.CheckBox
    Friend WithEvents ckEstimateOfShift As Windows.Forms.CheckBox
    Friend WithEvents progressBarExactCalc As Windows.Forms.ProgressBar
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents ckWelch As Windows.Forms.CheckBox
    Friend WithEvents grpANOVA1MCP As Windows.Forms.GroupBox
    Friend WithEvents ckGamesHowell As Windows.Forms.CheckBox
    Friend WithEvents ckBonferroni As Windows.Forms.CheckBox
    Friend WithEvents ckLSD As Windows.Forms.CheckBox
    Friend WithEvents ckTukey As Windows.Forms.CheckBox
    Friend WithEvents grpHomogeneityVariances As Windows.Forms.GroupBox
    Friend WithEvents ckLevene As Windows.Forms.CheckBox
    Friend WithEvents ckBartlett As Windows.Forms.CheckBox
    Friend WithEvents ckSquaredRanks As Windows.Forms.CheckBox
    Friend WithEvents ckFlignerKilleen As Windows.Forms.CheckBox
    Friend WithEvents TabPage_OptionsDescriptive As Windows.Forms.TabPage
    Friend WithEvents grpDescriptiveStat As Windows.Forms.GroupBox
    Friend WithEvents ckShapiroWilk As Windows.Forms.CheckBox
    Friend WithEvents ckVariance As Windows.Forms.CheckBox
    Friend WithEvents ckCV As Windows.Forms.CheckBox
    Friend WithEvents ckMedian As Windows.Forms.CheckBox
    Friend WithEvents ckMean As Windows.Forms.CheckBox
    Friend WithEvents ckN As Windows.Forms.CheckBox
    Friend WithEvents ckBoxPlot_Descriptive As Windows.Forms.CheckBox
    Friend WithEvents ckRange As Windows.Forms.CheckBox
    Friend WithEvents ckMax As Windows.Forms.CheckBox
    Friend WithEvents ckMin As Windows.Forms.CheckBox
    Friend WithEvents ckIQR As Windows.Forms.CheckBox
    Friend WithEvents ckQ3 As Windows.Forms.CheckBox
    Friend WithEvents ckQ1 As Windows.Forms.CheckBox
    Friend WithEvents ckKurtosis As Windows.Forms.CheckBox
    Friend WithEvents ckSkewness As Windows.Forms.CheckBox
    Friend WithEvents ckSEM As Windows.Forms.CheckBox
    Friend WithEvents ckSD As Windows.Forms.CheckBox
    Friend WithEvents TabPage_OptionsHistogram As Windows.Forms.TabPage
    Friend WithEvents grpBinSize As Windows.Forms.GroupBox
    Friend WithEvents optScott As Windows.Forms.RadioButton
    Friend WithEvents optFreedmanDiaconis As Windows.Forms.RadioButton
    Friend WithEvents optDoane As Windows.Forms.RadioButton
    Friend WithEvents optSturges As Windows.Forms.RadioButton
    Friend WithEvents ckDescriptive_Histogram As Windows.Forms.CheckBox
    Friend WithEvents ckOverlay As Windows.Forms.CheckBox
    Friend WithEvents TabPage_OptionsNormalPlot As Windows.Forms.TabPage
    Friend WithEvents grpReferenceLine As Windows.Forms.GroupBox
    Friend WithEvents optR As Windows.Forms.RadioButton
    Friend WithEvents optOLS As Windows.Forms.RadioButton
    Friend WithEvents optSPSS As Windows.Forms.RadioButton
    Friend WithEvents grpNormalScores As Windows.Forms.GroupBox
    Friend WithEvents optVanDerWaerden As Windows.Forms.RadioButton
    Friend WithEvents optRankit As Windows.Forms.RadioButton
    Friend WithEvents optBlom As Windows.Forms.RadioButton
    Friend WithEvents TabPage_OptionsSymmetry As Windows.Forms.TabPage
    Friend WithEvents ckSymmetryPlot As Windows.Forms.CheckBox
    Friend WithEvents ckDescriptive_Symmetry As Windows.Forms.CheckBox
    Friend WithEvents grpSymmetryTest As Windows.Forms.GroupBox
    Friend WithEvents optCM As Windows.Forms.RadioButton
    Friend WithEvents optMGG As Windows.Forms.RadioButton
    Friend WithEvents TabPage_OptionsOutliers As Windows.Forms.TabPage
    Friend WithEvents ckDescriptive_Outliers As Windows.Forms.CheckBox
    Friend WithEvents grpOutlierTests As Windows.Forms.GroupBox
    Friend WithEvents optGrubbs As Windows.Forms.RadioButton
    Friend WithEvents optRosner As Windows.Forms.RadioButton
    Friend WithEvents lblAlphaOutliers As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents ckBoxPlot_Outliers As Windows.Forms.CheckBox
    Friend WithEvents ckDescriptive_NormalPlot As Windows.Forms.CheckBox
End Class
