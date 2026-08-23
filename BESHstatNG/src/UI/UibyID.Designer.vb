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
        Me.TabControl1 = New System.Windows.Forms.TabControl()
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
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.spinBtnAlpha = New System.Windows.Forms.NumericUpDown()
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
        Me.spinBtnAlphaOutliers = New System.Windows.Forms.NumericUpDown()
        Me.optRosner = New System.Windows.Forms.RadioButton()
        Me.optGrubbs = New System.Windows.Forms.RadioButton()
        Me.ckDescriptive_Outliers = New System.Windows.Forms.CheckBox()
        Me.TabPage_OptionsUTT = New System.Windows.Forms.TabPage()
        Me.lblMarginHint_UTT = New System.Windows.Forms.Label()
        Me.tbMargin_UTT = New System.Windows.Forms.TextBox()
        Me.lblMargin_UTT = New System.Windows.Forms.Label()
        Me.grpVarianceModel_UTT = New System.Windows.Forms.GroupBox()
        Me.optVarianceEqual_UTT = New System.Windows.Forms.RadioButton()
        Me.optVarianceWelch_UTT = New System.Windows.Forms.RadioButton()
        Me.grpHypothesisType_UTT = New System.Windows.Forms.GroupBox()
        Me.optHypothesisEquivalence_UTT = New System.Windows.Forms.RadioButton()
        Me.optHypothesisNonInferiority_UTT = New System.Windows.Forms.RadioButton()
        Me.optHypothesisSuperiority_UTT = New System.Windows.Forms.RadioButton()
        Me.lblAlpha_UTT = New System.Windows.Forms.Label()
        Me.spinBtnAlpha_UTT = New System.Windows.Forms.NumericUpDown()
        Me.ckBoxPlot_UTT = New System.Windows.Forms.CheckBox()
        Me.ckDescriptiveStatistics_UTT = New System.Windows.Forms.CheckBox()
        Me.TabPage_OptionsCategoricalHistogram = New System.Windows.Forms.TabPage()
        Me.grpCatHistAppearance = New System.Windows.Forms.GroupBox()
        Me.cmbCatHistPalette = New System.Windows.Forms.ComboBox()
        Me.lblCatHistPalette = New System.Windows.Forms.Label()
        Me.nudCatHistSeriesOverlap = New System.Windows.Forms.NumericUpDown()
        Me.lblCatHistSeriesOverlap = New System.Windows.Forms.Label()
        Me.nudCatHistGapWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblCatHistGapWidth = New System.Windows.Forms.Label()
        Me.grpCatHistPlotType = New System.Windows.Forms.GroupBox()
        Me.optCatHistDifferentSampleSizes = New System.Windows.Forms.RadioButton()
        Me.optCatHistStackedBar = New System.Windows.Forms.RadioButton()
        Me.optCatHistBarsWithLegend = New System.Windows.Forms.RadioButton()
        Me.grpCatHistBinSize = New System.Windows.Forms.GroupBox()
        Me.optCatHistScott = New System.Windows.Forms.RadioButton()
        Me.optCatHistFreedmanDiaconis = New System.Windows.Forms.RadioButton()
        Me.optCatHistDoan = New System.Windows.Forms.RadioButton()
        Me.optCatHistSturges = New System.Windows.Forms.RadioButton()
        Me.TabPage_OptionsViolin = New System.Windows.Forms.TabPage()
        Me.grpScalingDisplay = New System.Windows.Forms.GroupBox()
        Me.cmdViolinTrimDensity = New System.Windows.Forms.CheckBox()
        Me.cmdViolinIndividualObs = New System.Windows.Forms.CheckBox()
        Me.cmdViolinMean = New System.Windows.Forms.CheckBox()
        Me.cmdViolinMedian = New System.Windows.Forms.CheckBox()
        Me.cbViolinInnerBoxPlot = New System.Windows.Forms.CheckBox()
        Me.cmdViolinScaling = New System.Windows.Forms.ComboBox()
        Me.lblViolinScaling = New System.Windows.Forms.Label()
        Me.grpViolinDensity = New System.Windows.Forms.GroupBox()
        Me.nudViolinDensityPoints = New System.Windows.Forms.NumericUpDown()
        Me.lblViolinDensityPoints = New System.Windows.Forms.Label()
        Me.nudViolinBandwidthAdjustment = New System.Windows.Forms.NumericUpDown()
        Me.lblViolinBandwidthAdjustment = New System.Windows.Forms.Label()
        Me.cmbViolinBandwidth = New System.Windows.Forms.ComboBox()
        Me.lblViolinBandwidth = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.nudViolinChartHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblViolinChartHeight = New System.Windows.Forms.Label()
        Me.nudViolinChartWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblViolinChartWidth = New System.Windows.Forms.Label()
        Me.cbViolinHorizontalGridlines = New System.Windows.Forms.CheckBox()
        Me.cbViolinOutline = New System.Windows.Forms.CheckBox()
        Me.cmbViolinPalette = New System.Windows.Forms.ComboBox()
        Me.lblViolinPalette = New System.Windows.Forms.Label()
        Me.nudViolinFillTransparency = New System.Windows.Forms.NumericUpDown()
        Me.lblViolinFillTransparency = New System.Windows.Forms.Label()
        Me.progressBarExactCalc = New System.Windows.Forms.ProgressBar()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPage_Options.SuspendLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
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
        CType(Me.spinBtnAlphaOutliers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage_OptionsUTT.SuspendLayout()
        Me.grpVarianceModel_UTT.SuspendLayout()
        Me.grpHypothesisType_UTT.SuspendLayout()
        CType(Me.spinBtnAlpha_UTT, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage_OptionsCategoricalHistogram.SuspendLayout()
        Me.grpCatHistAppearance.SuspendLayout()
        CType(Me.nudCatHistSeriesOverlap, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudCatHistGapWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpCatHistPlotType.SuspendLayout()
        Me.grpCatHistBinSize.SuspendLayout()
        Me.TabPage_OptionsViolin.SuspendLayout()
        Me.grpScalingDisplay.SuspendLayout()
        Me.grpViolinDensity.SuspendLayout()
        CType(Me.nudViolinDensityPoints, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudViolinBandwidthAdjustment, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        CType(Me.nudViolinChartHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudViolinChartWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudViolinFillTransparency, System.ComponentModel.ISupportInitialize).BeginInit()
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
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage_Options)
        Me.TabControl1.Controls.Add(Me.TabPage_OptionsDescriptive)
        Me.TabControl1.Controls.Add(Me.TabPage_OptionsHistogram)
        Me.TabControl1.Controls.Add(Me.TabPage_OptionsNormalPlot)
        Me.TabControl1.Controls.Add(Me.TabPage_OptionsSymmetry)
        Me.TabControl1.Controls.Add(Me.TabPage_OptionsOutliers)
        Me.TabControl1.Controls.Add(Me.TabPage_OptionsUTT)
        Me.TabControl1.Controls.Add(Me.TabPage_OptionsCategoricalHistogram)
        Me.TabControl1.Controls.Add(Me.TabPage_OptionsViolin)
        Me.TabControl1.Location = New System.Drawing.Point(9, 7)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(462, 364)
        Me.TabControl1.TabIndex = 3
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
        Me.TabPage_Options.Controls.Add(Me.lblAlpha)
        Me.TabPage_Options.Controls.Add(Me.spinBtnAlpha)
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
        'lblAlpha
        '
        Me.lblAlpha.AutoSize = True
        Me.lblAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlpha.Location = New System.Drawing.Point(210, 49)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(41, 16)
        Me.lblAlpha.TabIndex = 7
        Me.lblAlpha.Text = "alpha"
        Me.lblAlpha.Visible = False
        '
        'spinBtnAlpha
        '
        Me.spinBtnAlpha.DecimalPlaces = 3
        Me.spinBtnAlpha.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Location = New System.Drawing.Point(258, 47)
        Me.spinBtnAlpha.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha.Name = "spinBtnAlpha"
        Me.spinBtnAlpha.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha.TabIndex = 6
        Me.spinBtnAlpha.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        Me.spinBtnAlpha.Visible = False
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
        Me.optDoane.Size = New System.Drawing.Size(69, 20)
        Me.optDoane.TabIndex = 1
        Me.optDoane.Text = "Doane"
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
        Me.grpOutlierTests.Controls.Add(Me.spinBtnAlphaOutliers)
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
        Me.lblAlphaOutliers.Size = New System.Drawing.Size(41, 16)
        Me.lblAlphaOutliers.TabIndex = 3
        Me.lblAlphaOutliers.Text = "alpha"
        '
        'spinBtnAlphaOutliers
        '
        Me.spinBtnAlphaOutliers.DecimalPlaces = 3
        Me.spinBtnAlphaOutliers.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlphaOutliers.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaOutliers.Location = New System.Drawing.Point(65, 30)
        Me.spinBtnAlphaOutliers.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlphaOutliers.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlphaOutliers.Name = "spinBtnAlphaOutliers"
        Me.spinBtnAlphaOutliers.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlphaOutliers.TabIndex = 2
        Me.spinBtnAlphaOutliers.Value = New Decimal(New Integer() {5, 0, 0, 131072})
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
        'TabPage_OptionsUTT
        '
        Me.TabPage_OptionsUTT.Controls.Add(Me.lblMarginHint_UTT)
        Me.TabPage_OptionsUTT.Controls.Add(Me.tbMargin_UTT)
        Me.TabPage_OptionsUTT.Controls.Add(Me.lblMargin_UTT)
        Me.TabPage_OptionsUTT.Controls.Add(Me.grpVarianceModel_UTT)
        Me.TabPage_OptionsUTT.Controls.Add(Me.grpHypothesisType_UTT)
        Me.TabPage_OptionsUTT.Controls.Add(Me.lblAlpha_UTT)
        Me.TabPage_OptionsUTT.Controls.Add(Me.spinBtnAlpha_UTT)
        Me.TabPage_OptionsUTT.Controls.Add(Me.ckBoxPlot_UTT)
        Me.TabPage_OptionsUTT.Controls.Add(Me.ckDescriptiveStatistics_UTT)
        Me.TabPage_OptionsUTT.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsUTT.Name = "TabPage_OptionsUTT"
        Me.TabPage_OptionsUTT.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_OptionsUTT.TabIndex = 7
        Me.TabPage_OptionsUTT.Text = "Options"
        Me.TabPage_OptionsUTT.UseVisualStyleBackColor = True
        '
        'lblMarginHint_UTT
        '
        Me.lblMarginHint_UTT.AutoSize = True
        Me.lblMarginHint_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMarginHint_UTT.Location = New System.Drawing.Point(10, 240)
        Me.lblMarginHint_UTT.Name = "lblMarginHint_UTT"
        Me.lblMarginHint_UTT.Size = New System.Drawing.Size(362, 16)
        Me.lblMarginHint_UTT.TabIndex = 16
        Me.lblMarginHint_UTT.Text = "Enter a positive margin on the (Experimental - Control) scale."
        '
        'tbMargin_UTT
        '
        Me.tbMargin_UTT.Location = New System.Drawing.Point(191, 208)
        Me.tbMargin_UTT.Name = "tbMargin_UTT"
        Me.tbMargin_UTT.Size = New System.Drawing.Size(100, 22)
        Me.tbMargin_UTT.TabIndex = 15
        '
        'lblMargin_UTT
        '
        Me.lblMargin_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMargin_UTT.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.lblMargin_UTT.Location = New System.Drawing.Point(28, 211)
        Me.lblMargin_UTT.Name = "lblMargin_UTT"
        Me.lblMargin_UTT.Size = New System.Drawing.Size(148, 19)
        Me.lblMargin_UTT.TabIndex = 14
        Me.lblMargin_UTT.Text = "Margin:"
        Me.lblMargin_UTT.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'grpVarianceModel_UTT
        '
        Me.grpVarianceModel_UTT.Controls.Add(Me.optVarianceEqual_UTT)
        Me.grpVarianceModel_UTT.Controls.Add(Me.optVarianceWelch_UTT)
        Me.grpVarianceModel_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpVarianceModel_UTT.Location = New System.Drawing.Point(191, 79)
        Me.grpVarianceModel_UTT.Name = "grpVarianceModel_UTT"
        Me.grpVarianceModel_UTT.Size = New System.Drawing.Size(245, 114)
        Me.grpVarianceModel_UTT.TabIndex = 13
        Me.grpVarianceModel_UTT.TabStop = False
        Me.grpVarianceModel_UTT.Text = "Variance assumption"
        '
        'optVarianceEqual_UTT
        '
        Me.optVarianceEqual_UTT.AutoSize = True
        Me.optVarianceEqual_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optVarianceEqual_UTT.Location = New System.Drawing.Point(15, 56)
        Me.optVarianceEqual_UTT.Name = "optVarianceEqual_UTT"
        Me.optVarianceEqual_UTT.Size = New System.Drawing.Size(179, 20)
        Me.optVarianceEqual_UTT.TabIndex = 4
        Me.optVarianceEqual_UTT.Text = "Equal variances (pooled)"
        Me.optVarianceEqual_UTT.UseVisualStyleBackColor = True
        '
        'optVarianceWelch_UTT
        '
        Me.optVarianceWelch_UTT.AutoSize = True
        Me.optVarianceWelch_UTT.Checked = True
        Me.optVarianceWelch_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optVarianceWelch_UTT.Location = New System.Drawing.Point(15, 30)
        Me.optVarianceWelch_UTT.Name = "optVarianceWelch_UTT"
        Me.optVarianceWelch_UTT.Size = New System.Drawing.Size(187, 20)
        Me.optVarianceWelch_UTT.TabIndex = 3
        Me.optVarianceWelch_UTT.TabStop = True
        Me.optVarianceWelch_UTT.Text = "Welch (unequal variances)"
        Me.optVarianceWelch_UTT.UseVisualStyleBackColor = True
        '
        'grpHypothesisType_UTT
        '
        Me.grpHypothesisType_UTT.Controls.Add(Me.optHypothesisEquivalence_UTT)
        Me.grpHypothesisType_UTT.Controls.Add(Me.optHypothesisNonInferiority_UTT)
        Me.grpHypothesisType_UTT.Controls.Add(Me.optHypothesisSuperiority_UTT)
        Me.grpHypothesisType_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHypothesisType_UTT.Location = New System.Drawing.Point(13, 79)
        Me.grpHypothesisType_UTT.Name = "grpHypothesisType_UTT"
        Me.grpHypothesisType_UTT.Size = New System.Drawing.Size(163, 114)
        Me.grpHypothesisType_UTT.TabIndex = 12
        Me.grpHypothesisType_UTT.TabStop = False
        Me.grpHypothesisType_UTT.Text = "Hypothesis type"
        '
        'optHypothesisEquivalence_UTT
        '
        Me.optHypothesisEquivalence_UTT.AutoSize = True
        Me.optHypothesisEquivalence_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optHypothesisEquivalence_UTT.Location = New System.Drawing.Point(15, 82)
        Me.optHypothesisEquivalence_UTT.Name = "optHypothesisEquivalence_UTT"
        Me.optHypothesisEquivalence_UTT.Size = New System.Drawing.Size(103, 20)
        Me.optHypothesisEquivalence_UTT.TabIndex = 5
        Me.optHypothesisEquivalence_UTT.Text = "Equivalence"
        Me.optHypothesisEquivalence_UTT.UseVisualStyleBackColor = True
        '
        'optHypothesisNonInferiority_UTT
        '
        Me.optHypothesisNonInferiority_UTT.AutoSize = True
        Me.optHypothesisNonInferiority_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optHypothesisNonInferiority_UTT.Location = New System.Drawing.Point(15, 56)
        Me.optHypothesisNonInferiority_UTT.Name = "optHypothesisNonInferiority_UTT"
        Me.optHypothesisNonInferiority_UTT.Size = New System.Drawing.Size(106, 20)
        Me.optHypothesisNonInferiority_UTT.TabIndex = 4
        Me.optHypothesisNonInferiority_UTT.Text = "NonInferiority"
        Me.optHypothesisNonInferiority_UTT.UseVisualStyleBackColor = True
        '
        'optHypothesisSuperiority_UTT
        '
        Me.optHypothesisSuperiority_UTT.AutoSize = True
        Me.optHypothesisSuperiority_UTT.Checked = True
        Me.optHypothesisSuperiority_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optHypothesisSuperiority_UTT.Location = New System.Drawing.Point(15, 30)
        Me.optHypothesisSuperiority_UTT.Name = "optHypothesisSuperiority_UTT"
        Me.optHypothesisSuperiority_UTT.Size = New System.Drawing.Size(92, 20)
        Me.optHypothesisSuperiority_UTT.TabIndex = 3
        Me.optHypothesisSuperiority_UTT.TabStop = True
        Me.optHypothesisSuperiority_UTT.Text = "Superiority"
        Me.optHypothesisSuperiority_UTT.UseVisualStyleBackColor = True
        '
        'lblAlpha_UTT
        '
        Me.lblAlpha_UTT.AutoSize = True
        Me.lblAlpha_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAlpha_UTT.Location = New System.Drawing.Point(243, 45)
        Me.lblAlpha_UTT.Name = "lblAlpha_UTT"
        Me.lblAlpha_UTT.Size = New System.Drawing.Size(111, 16)
        Me.lblAlpha_UTT.TabIndex = 11
        Me.lblAlpha_UTT.Text = "Two-sided alpha:"
        '
        'spinBtnAlpha_UTT
        '
        Me.spinBtnAlpha_UTT.DecimalPlaces = 3
        Me.spinBtnAlpha_UTT.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnAlpha_UTT.Increment = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha_UTT.Location = New System.Drawing.Point(369, 43)
        Me.spinBtnAlpha_UTT.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
        Me.spinBtnAlpha_UTT.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.spinBtnAlpha_UTT.Name = "spinBtnAlpha_UTT"
        Me.spinBtnAlpha_UTT.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnAlpha_UTT.TabIndex = 10
        Me.spinBtnAlpha_UTT.Value = New Decimal(New Integer() {5, 0, 0, 131072})
        '
        'ckBoxPlot_UTT
        '
        Me.ckBoxPlot_UTT.AutoSize = True
        Me.ckBoxPlot_UTT.Checked = True
        Me.ckBoxPlot_UTT.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckBoxPlot_UTT.Location = New System.Drawing.Point(13, 17)
        Me.ckBoxPlot_UTT.Name = "ckBoxPlot_UTT"
        Me.ckBoxPlot_UTT.Size = New System.Drawing.Size(163, 20)
        Me.ckBoxPlot_UTT.TabIndex = 9
        Me.ckBoxPlot_UTT.Text = "Box and Whiskers Plot"
        Me.ckBoxPlot_UTT.UseVisualStyleBackColor = True
        '
        'ckDescriptiveStatistics_UTT
        '
        Me.ckDescriptiveStatistics_UTT.AutoSize = True
        Me.ckDescriptiveStatistics_UTT.Checked = True
        Me.ckDescriptiveStatistics_UTT.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDescriptiveStatistics_UTT.Location = New System.Drawing.Point(13, 43)
        Me.ckDescriptiveStatistics_UTT.Name = "ckDescriptiveStatistics_UTT"
        Me.ckDescriptiveStatistics_UTT.Size = New System.Drawing.Size(177, 20)
        Me.ckDescriptiveStatistics_UTT.TabIndex = 8
        Me.ckDescriptiveStatistics_UTT.Text = "Full Descriptive Statistics"
        Me.ckDescriptiveStatistics_UTT.UseVisualStyleBackColor = True
        '
        'TabPage_OptionsCategoricalHistogram
        '
        Me.TabPage_OptionsCategoricalHistogram.Controls.Add(Me.grpCatHistAppearance)
        Me.TabPage_OptionsCategoricalHistogram.Controls.Add(Me.grpCatHistPlotType)
        Me.TabPage_OptionsCategoricalHistogram.Controls.Add(Me.grpCatHistBinSize)
        Me.TabPage_OptionsCategoricalHistogram.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsCategoricalHistogram.Name = "TabPage_OptionsCategoricalHistogram"
        Me.TabPage_OptionsCategoricalHistogram.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_OptionsCategoricalHistogram.TabIndex = 8
        Me.TabPage_OptionsCategoricalHistogram.Text = "Options"
        Me.TabPage_OptionsCategoricalHistogram.UseVisualStyleBackColor = True
        '
        'grpCatHistAppearance
        '
        Me.grpCatHistAppearance.Controls.Add(Me.cmbCatHistPalette)
        Me.grpCatHistAppearance.Controls.Add(Me.lblCatHistPalette)
        Me.grpCatHistAppearance.Controls.Add(Me.nudCatHistSeriesOverlap)
        Me.grpCatHistAppearance.Controls.Add(Me.lblCatHistSeriesOverlap)
        Me.grpCatHistAppearance.Controls.Add(Me.nudCatHistGapWidth)
        Me.grpCatHistAppearance.Controls.Add(Me.lblCatHistGapWidth)
        Me.grpCatHistAppearance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpCatHistAppearance.Location = New System.Drawing.Point(12, 137)
        Me.grpCatHistAppearance.Name = "grpCatHistAppearance"
        Me.grpCatHistAppearance.Size = New System.Drawing.Size(423, 128)
        Me.grpCatHistAppearance.TabIndex = 5
        Me.grpCatHistAppearance.TabStop = False
        Me.grpCatHistAppearance.Text = "Appearance"
        '
        'cmbCatHistPalette
        '
        Me.cmbCatHistPalette.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbCatHistPalette.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbCatHistPalette.FormattingEnabled = True
        Me.cmbCatHistPalette.Location = New System.Drawing.Point(136, 83)
        Me.cmbCatHistPalette.Name = "cmbCatHistPalette"
        Me.cmbCatHistPalette.Size = New System.Drawing.Size(186, 24)
        Me.cmbCatHistPalette.TabIndex = 9
        '
        'lblCatHistPalette
        '
        Me.lblCatHistPalette.AutoSize = True
        Me.lblCatHistPalette.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCatHistPalette.Location = New System.Drawing.Point(6, 87)
        Me.lblCatHistPalette.Name = "lblCatHistPalette"
        Me.lblCatHistPalette.Size = New System.Drawing.Size(124, 16)
        Me.lblCatHistPalette.TabIndex = 8
        Me.lblCatHistPalette.Text = "Group color palette:"
        '
        'nudCatHistSeriesOverlap
        '
        Me.nudCatHistSeriesOverlap.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudCatHistSeriesOverlap.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudCatHistSeriesOverlap.Location = New System.Drawing.Point(136, 55)
        Me.nudCatHistSeriesOverlap.Minimum = New Decimal(New Integer() {100, 0, 0, -2147483648})
        Me.nudCatHistSeriesOverlap.Name = "nudCatHistSeriesOverlap"
        Me.nudCatHistSeriesOverlap.Size = New System.Drawing.Size(54, 22)
        Me.nudCatHistSeriesOverlap.TabIndex = 7
        '
        'lblCatHistSeriesOverlap
        '
        Me.lblCatHistSeriesOverlap.AutoSize = True
        Me.lblCatHistSeriesOverlap.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCatHistSeriesOverlap.Location = New System.Drawing.Point(6, 57)
        Me.lblCatHistSeriesOverlap.Name = "lblCatHistSeriesOverlap"
        Me.lblCatHistSeriesOverlap.Size = New System.Drawing.Size(98, 16)
        Me.lblCatHistSeriesOverlap.TabIndex = 6
        Me.lblCatHistSeriesOverlap.Text = "Series overlap:"
        '
        'nudCatHistGapWidth
        '
        Me.nudCatHistGapWidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudCatHistGapWidth.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudCatHistGapWidth.Location = New System.Drawing.Point(136, 27)
        Me.nudCatHistGapWidth.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        Me.nudCatHistGapWidth.Name = "nudCatHistGapWidth"
        Me.nudCatHistGapWidth.Size = New System.Drawing.Size(54, 22)
        Me.nudCatHistGapWidth.TabIndex = 5
        Me.nudCatHistGapWidth.Value = New Decimal(New Integer() {30, 0, 0, 0})
        '
        'lblCatHistGapWidth
        '
        Me.lblCatHistGapWidth.AutoSize = True
        Me.lblCatHistGapWidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCatHistGapWidth.Location = New System.Drawing.Point(6, 29)
        Me.lblCatHistGapWidth.Name = "lblCatHistGapWidth"
        Me.lblCatHistGapWidth.Size = New System.Drawing.Size(69, 16)
        Me.lblCatHistGapWidth.TabIndex = 4
        Me.lblCatHistGapWidth.Text = "Gap width:"
        '
        'grpCatHistPlotType
        '
        Me.grpCatHistPlotType.Controls.Add(Me.optCatHistDifferentSampleSizes)
        Me.grpCatHistPlotType.Controls.Add(Me.optCatHistStackedBar)
        Me.grpCatHistPlotType.Controls.Add(Me.optCatHistBarsWithLegend)
        Me.grpCatHistPlotType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpCatHistPlotType.Location = New System.Drawing.Point(200, 3)
        Me.grpCatHistPlotType.Name = "grpCatHistPlotType"
        Me.grpCatHistPlotType.Size = New System.Drawing.Size(235, 128)
        Me.grpCatHistPlotType.TabIndex = 5
        Me.grpCatHistPlotType.TabStop = False
        Me.grpCatHistPlotType.Text = "Histogram Type"
        '
        'optCatHistDifferentSampleSizes
        '
        Me.optCatHistDifferentSampleSizes.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCatHistDifferentSampleSizes.Location = New System.Drawing.Point(18, 73)
        Me.optCatHistDifferentSampleSizes.Name = "optCatHistDifferentSampleSizes"
        Me.optCatHistDifferentSampleSizes.Size = New System.Drawing.Size(190, 46)
        Me.optCatHistDifferentSampleSizes.TabIndex = 3
        Me.optCatHistDifferentSampleSizes.Text = "Grouped bars (frequency / different sample sizes)"
        Me.optCatHistDifferentSampleSizes.UseVisualStyleBackColor = True
        '
        'optCatHistStackedBar
        '
        Me.optCatHistStackedBar.AutoSize = True
        Me.optCatHistStackedBar.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCatHistStackedBar.Location = New System.Drawing.Point(18, 47)
        Me.optCatHistStackedBar.Name = "optCatHistStackedBar"
        Me.optCatHistStackedBar.Size = New System.Drawing.Size(162, 20)
        Me.optCatHistStackedBar.TabIndex = 1
        Me.optCatHistStackedBar.Text = "Stacked bars (density)"
        Me.optCatHistStackedBar.UseVisualStyleBackColor = True
        '
        'optCatHistBarsWithLegend
        '
        Me.optCatHistBarsWithLegend.AutoSize = True
        Me.optCatHistBarsWithLegend.Checked = True
        Me.optCatHistBarsWithLegend.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCatHistBarsWithLegend.Location = New System.Drawing.Point(18, 21)
        Me.optCatHistBarsWithLegend.Name = "optCatHistBarsWithLegend"
        Me.optCatHistBarsWithLegend.Size = New System.Drawing.Size(165, 20)
        Me.optCatHistBarsWithLegend.TabIndex = 0
        Me.optCatHistBarsWithLegend.TabStop = True
        Me.optCatHistBarsWithLegend.Text = "Grouped bars (density)"
        Me.optCatHistBarsWithLegend.UseVisualStyleBackColor = True
        '
        'grpCatHistBinSize
        '
        Me.grpCatHistBinSize.Controls.Add(Me.optCatHistScott)
        Me.grpCatHistBinSize.Controls.Add(Me.optCatHistFreedmanDiaconis)
        Me.grpCatHistBinSize.Controls.Add(Me.optCatHistDoan)
        Me.grpCatHistBinSize.Controls.Add(Me.optCatHistSturges)
        Me.grpCatHistBinSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpCatHistBinSize.Location = New System.Drawing.Point(12, 3)
        Me.grpCatHistBinSize.Name = "grpCatHistBinSize"
        Me.grpCatHistBinSize.Size = New System.Drawing.Size(173, 128)
        Me.grpCatHistBinSize.TabIndex = 4
        Me.grpCatHistBinSize.TabStop = False
        Me.grpCatHistBinSize.Text = "Bin-sizing Method"
        '
        'optCatHistScott
        '
        Me.optCatHistScott.AutoSize = True
        Me.optCatHistScott.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCatHistScott.Location = New System.Drawing.Point(18, 99)
        Me.optCatHistScott.Name = "optCatHistScott"
        Me.optCatHistScott.Size = New System.Drawing.Size(58, 20)
        Me.optCatHistScott.TabIndex = 3
        Me.optCatHistScott.Text = "Scott"
        Me.optCatHistScott.UseVisualStyleBackColor = True
        '
        'optCatHistFreedmanDiaconis
        '
        Me.optCatHistFreedmanDiaconis.AutoSize = True
        Me.optCatHistFreedmanDiaconis.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCatHistFreedmanDiaconis.Location = New System.Drawing.Point(18, 73)
        Me.optCatHistFreedmanDiaconis.Name = "optCatHistFreedmanDiaconis"
        Me.optCatHistFreedmanDiaconis.Size = New System.Drawing.Size(147, 20)
        Me.optCatHistFreedmanDiaconis.TabIndex = 2
        Me.optCatHistFreedmanDiaconis.Text = "Freedman-Diaconis"
        Me.optCatHistFreedmanDiaconis.UseVisualStyleBackColor = True
        '
        'optCatHistDoan
        '
        Me.optCatHistDoan.AutoSize = True
        Me.optCatHistDoan.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCatHistDoan.Location = New System.Drawing.Point(18, 47)
        Me.optCatHistDoan.Name = "optCatHistDoan"
        Me.optCatHistDoan.Size = New System.Drawing.Size(69, 20)
        Me.optCatHistDoan.TabIndex = 1
        Me.optCatHistDoan.Text = "Doane"
        Me.optCatHistDoan.UseVisualStyleBackColor = True
        '
        'optCatHistSturges
        '
        Me.optCatHistSturges.AutoSize = True
        Me.optCatHistSturges.Checked = True
        Me.optCatHistSturges.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCatHistSturges.Location = New System.Drawing.Point(18, 21)
        Me.optCatHistSturges.Name = "optCatHistSturges"
        Me.optCatHistSturges.Size = New System.Drawing.Size(74, 20)
        Me.optCatHistSturges.TabIndex = 0
        Me.optCatHistSturges.TabStop = True
        Me.optCatHistSturges.Text = "Sturges"
        Me.optCatHistSturges.UseVisualStyleBackColor = True
        '
        'TabPage_OptionsViolin
        '
        Me.TabPage_OptionsViolin.Controls.Add(Me.grpScalingDisplay)
        Me.TabPage_OptionsViolin.Controls.Add(Me.grpViolinDensity)
        Me.TabPage_OptionsViolin.Controls.Add(Me.GroupBox1)
        Me.TabPage_OptionsViolin.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_OptionsViolin.Name = "TabPage_OptionsViolin"
        Me.TabPage_OptionsViolin.Size = New System.Drawing.Size(454, 335)
        Me.TabPage_OptionsViolin.TabIndex = 9
        Me.TabPage_OptionsViolin.Text = "Options"
        Me.TabPage_OptionsViolin.UseVisualStyleBackColor = True
        '
        'grpScalingDisplay
        '
        Me.grpScalingDisplay.Controls.Add(Me.cmdViolinTrimDensity)
        Me.grpScalingDisplay.Controls.Add(Me.cmdViolinIndividualObs)
        Me.grpScalingDisplay.Controls.Add(Me.cmdViolinMean)
        Me.grpScalingDisplay.Controls.Add(Me.cmdViolinMedian)
        Me.grpScalingDisplay.Controls.Add(Me.cbViolinInnerBoxPlot)
        Me.grpScalingDisplay.Controls.Add(Me.cmdViolinScaling)
        Me.grpScalingDisplay.Controls.Add(Me.lblViolinScaling)
        Me.grpScalingDisplay.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpScalingDisplay.Location = New System.Drawing.Point(3, 95)
        Me.grpScalingDisplay.Name = "grpScalingDisplay"
        Me.grpScalingDisplay.Size = New System.Drawing.Size(446, 97)
        Me.grpScalingDisplay.TabIndex = 12
        Me.grpScalingDisplay.TabStop = False
        Me.grpScalingDisplay.Text = "Scaling and Display"
        '
        'cmdViolinTrimDensity
        '
        Me.cmdViolinTrimDensity.AutoSize = True
        Me.cmdViolinTrimDensity.Checked = True
        Me.cmdViolinTrimDensity.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cmdViolinTrimDensity.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdViolinTrimDensity.Location = New System.Drawing.Point(184, 71)
        Me.cmdViolinTrimDensity.Name = "cmdViolinTrimDensity"
        Me.cmdViolinTrimDensity.Size = New System.Drawing.Size(184, 20)
        Me.cmdViolinTrimDensity.TabIndex = 14
        Me.cmdViolinTrimDensity.Text = "Trim density to data range"
        Me.cmdViolinTrimDensity.UseVisualStyleBackColor = True
        '
        'cmdViolinIndividualObs
        '
        Me.cmdViolinIndividualObs.AutoSize = True
        Me.cmdViolinIndividualObs.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdViolinIndividualObs.Location = New System.Drawing.Point(11, 71)
        Me.cmdViolinIndividualObs.Name = "cmdViolinIndividualObs"
        Me.cmdViolinIndividualObs.Size = New System.Drawing.Size(167, 20)
        Me.cmdViolinIndividualObs.TabIndex = 13
        Me.cmdViolinIndividualObs.Text = "Individual observations"
        Me.cmdViolinIndividualObs.UseVisualStyleBackColor = True
        '
        'cmdViolinMean
        '
        Me.cmdViolinMean.AutoSize = True
        Me.cmdViolinMean.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdViolinMean.Location = New System.Drawing.Point(368, 51)
        Me.cmdViolinMean.Name = "cmdViolinMean"
        Me.cmdViolinMean.Size = New System.Drawing.Size(63, 20)
        Me.cmdViolinMean.TabIndex = 12
        Me.cmdViolinMean.Text = "Mean"
        Me.cmdViolinMean.UseVisualStyleBackColor = True
        '
        'cmdViolinMedian
        '
        Me.cmdViolinMedian.AutoSize = True
        Me.cmdViolinMedian.Checked = True
        Me.cmdViolinMedian.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cmdViolinMedian.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdViolinMedian.Location = New System.Drawing.Point(184, 51)
        Me.cmdViolinMedian.Name = "cmdViolinMedian"
        Me.cmdViolinMedian.Size = New System.Drawing.Size(74, 20)
        Me.cmdViolinMedian.TabIndex = 11
        Me.cmdViolinMedian.Text = "Median"
        Me.cmdViolinMedian.UseVisualStyleBackColor = True
        '
        'cbViolinInnerBoxPlot
        '
        Me.cbViolinInnerBoxPlot.AutoSize = True
        Me.cbViolinInnerBoxPlot.Checked = True
        Me.cbViolinInnerBoxPlot.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbViolinInnerBoxPlot.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbViolinInnerBoxPlot.Location = New System.Drawing.Point(11, 51)
        Me.cbViolinInnerBoxPlot.Name = "cbViolinInnerBoxPlot"
        Me.cbViolinInnerBoxPlot.Size = New System.Drawing.Size(108, 20)
        Me.cbViolinInnerBoxPlot.TabIndex = 10
        Me.cbViolinInnerBoxPlot.Text = "Inner box plot"
        Me.cbViolinInnerBoxPlot.UseVisualStyleBackColor = True
        '
        'cmdViolinScaling
        '
        Me.cmdViolinScaling.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmdViolinScaling.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdViolinScaling.FormattingEnabled = True
        Me.cmdViolinScaling.Location = New System.Drawing.Point(155, 21)
        Me.cmdViolinScaling.Name = "cmdViolinScaling"
        Me.cmdViolinScaling.Size = New System.Drawing.Size(200, 24)
        Me.cmdViolinScaling.TabIndex = 9
        '
        'lblViolinScaling
        '
        Me.lblViolinScaling.AutoSize = True
        Me.lblViolinScaling.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblViolinScaling.Location = New System.Drawing.Point(6, 25)
        Me.lblViolinScaling.Name = "lblViolinScaling"
        Me.lblViolinScaling.Size = New System.Drawing.Size(89, 16)
        Me.lblViolinScaling.TabIndex = 8
        Me.lblViolinScaling.Text = "Violin scaling:"
        '
        'grpViolinDensity
        '
        Me.grpViolinDensity.Controls.Add(Me.nudViolinDensityPoints)
        Me.grpViolinDensity.Controls.Add(Me.lblViolinDensityPoints)
        Me.grpViolinDensity.Controls.Add(Me.nudViolinBandwidthAdjustment)
        Me.grpViolinDensity.Controls.Add(Me.lblViolinBandwidthAdjustment)
        Me.grpViolinDensity.Controls.Add(Me.cmbViolinBandwidth)
        Me.grpViolinDensity.Controls.Add(Me.lblViolinBandwidth)
        Me.grpViolinDensity.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpViolinDensity.Location = New System.Drawing.Point(3, 3)
        Me.grpViolinDensity.Name = "grpViolinDensity"
        Me.grpViolinDensity.Size = New System.Drawing.Size(446, 86)
        Me.grpViolinDensity.TabIndex = 7
        Me.grpViolinDensity.TabStop = False
        Me.grpViolinDensity.Text = "Density"
        '
        'nudViolinDensityPoints
        '
        Me.nudViolinDensityPoints.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudViolinDensityPoints.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudViolinDensityPoints.Location = New System.Drawing.Point(377, 52)
        Me.nudViolinDensityPoints.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudViolinDensityPoints.Minimum = New Decimal(New Integer() {32, 0, 0, 0})
        Me.nudViolinDensityPoints.Name = "nudViolinDensityPoints"
        Me.nudViolinDensityPoints.Size = New System.Drawing.Size(54, 22)
        Me.nudViolinDensityPoints.TabIndex = 15
        Me.nudViolinDensityPoints.Value = New Decimal(New Integer() {128, 0, 0, 0})
        '
        'lblViolinDensityPoints
        '
        Me.lblViolinDensityPoints.AutoSize = True
        Me.lblViolinDensityPoints.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblViolinDensityPoints.Location = New System.Drawing.Point(261, 54)
        Me.lblViolinDensityPoints.Name = "lblViolinDensityPoints"
        Me.lblViolinDensityPoints.Size = New System.Drawing.Size(94, 16)
        Me.lblViolinDensityPoints.TabIndex = 14
        Me.lblViolinDensityPoints.Text = "Density points:"
        '
        'nudViolinBandwidthAdjustment
        '
        Me.nudViolinBandwidthAdjustment.DecimalPlaces = 2
        Me.nudViolinBandwidthAdjustment.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudViolinBandwidthAdjustment.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.nudViolinBandwidthAdjustment.Location = New System.Drawing.Point(155, 50)
        Me.nudViolinBandwidthAdjustment.Name = "nudViolinBandwidthAdjustment"
        Me.nudViolinBandwidthAdjustment.Size = New System.Drawing.Size(54, 22)
        Me.nudViolinBandwidthAdjustment.TabIndex = 13
        Me.nudViolinBandwidthAdjustment.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblViolinBandwidthAdjustment
        '
        Me.lblViolinBandwidthAdjustment.AutoSize = True
        Me.lblViolinBandwidthAdjustment.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblViolinBandwidthAdjustment.Location = New System.Drawing.Point(8, 52)
        Me.lblViolinBandwidthAdjustment.Name = "lblViolinBandwidthAdjustment"
        Me.lblViolinBandwidthAdjustment.Size = New System.Drawing.Size(141, 16)
        Me.lblViolinBandwidthAdjustment.TabIndex = 12
        Me.lblViolinBandwidthAdjustment.Text = "Bandwidth Adjustment:"
        '
        'cmbViolinBandwidth
        '
        Me.cmbViolinBandwidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbViolinBandwidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbViolinBandwidth.FormattingEnabled = True
        Me.cmbViolinBandwidth.Location = New System.Drawing.Point(155, 21)
        Me.cmbViolinBandwidth.Name = "cmbViolinBandwidth"
        Me.cmbViolinBandwidth.Size = New System.Drawing.Size(200, 24)
        Me.cmbViolinBandwidth.TabIndex = 11
        '
        'lblViolinBandwidth
        '
        Me.lblViolinBandwidth.AutoSize = True
        Me.lblViolinBandwidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblViolinBandwidth.Location = New System.Drawing.Point(8, 24)
        Me.lblViolinBandwidth.Name = "lblViolinBandwidth"
        Me.lblViolinBandwidth.Size = New System.Drawing.Size(72, 16)
        Me.lblViolinBandwidth.TabIndex = 10
        Me.lblViolinBandwidth.Text = "Bandwidth:"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.nudViolinChartHeight)
        Me.GroupBox1.Controls.Add(Me.lblViolinChartHeight)
        Me.GroupBox1.Controls.Add(Me.nudViolinChartWidth)
        Me.GroupBox1.Controls.Add(Me.lblViolinChartWidth)
        Me.GroupBox1.Controls.Add(Me.cbViolinHorizontalGridlines)
        Me.GroupBox1.Controls.Add(Me.cbViolinOutline)
        Me.GroupBox1.Controls.Add(Me.cmbViolinPalette)
        Me.GroupBox1.Controls.Add(Me.lblViolinPalette)
        Me.GroupBox1.Controls.Add(Me.nudViolinFillTransparency)
        Me.GroupBox1.Controls.Add(Me.lblViolinFillTransparency)
        Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.Location = New System.Drawing.Point(3, 198)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(446, 134)
        Me.GroupBox1.TabIndex = 6
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Appearance"
        '
        'nudViolinChartHeight
        '
        Me.nudViolinChartHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudViolinChartHeight.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudViolinChartHeight.Location = New System.Drawing.Point(155, 110)
        Me.nudViolinChartHeight.Maximum = New Decimal(New Integer() {5000, 0, 0, 0})
        Me.nudViolinChartHeight.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudViolinChartHeight.Name = "nudViolinChartHeight"
        Me.nudViolinChartHeight.Size = New System.Drawing.Size(78, 22)
        Me.nudViolinChartHeight.TabIndex = 15
        Me.nudViolinChartHeight.Value = New Decimal(New Integer() {440, 0, 0, 0})
        '
        'lblViolinChartHeight
        '
        Me.lblViolinChartHeight.AutoSize = True
        Me.lblViolinChartHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblViolinChartHeight.Location = New System.Drawing.Point(37, 112)
        Me.lblViolinChartHeight.Name = "lblViolinChartHeight"
        Me.lblViolinChartHeight.Size = New System.Drawing.Size(83, 16)
        Me.lblViolinChartHeight.TabIndex = 14
        Me.lblViolinChartHeight.Text = "Chart Height:"
        '
        'nudViolinChartWidth
        '
        Me.nudViolinChartWidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudViolinChartWidth.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudViolinChartWidth.Location = New System.Drawing.Point(155, 82)
        Me.nudViolinChartWidth.Maximum = New Decimal(New Integer() {5000, 0, 0, 0})
        Me.nudViolinChartWidth.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudViolinChartWidth.Name = "nudViolinChartWidth"
        Me.nudViolinChartWidth.Size = New System.Drawing.Size(78, 22)
        Me.nudViolinChartWidth.TabIndex = 13
        Me.nudViolinChartWidth.Value = New Decimal(New Integer() {720, 0, 0, 0})
        '
        'lblViolinChartWidth
        '
        Me.lblViolinChartWidth.AutoSize = True
        Me.lblViolinChartWidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblViolinChartWidth.Location = New System.Drawing.Point(42, 84)
        Me.lblViolinChartWidth.Name = "lblViolinChartWidth"
        Me.lblViolinChartWidth.Size = New System.Drawing.Size(78, 16)
        Me.lblViolinChartWidth.TabIndex = 12
        Me.lblViolinChartWidth.Text = "Chart Width:"
        '
        'cbViolinHorizontalGridlines
        '
        Me.cbViolinHorizontalGridlines.AutoSize = True
        Me.cbViolinHorizontalGridlines.Checked = True
        Me.cbViolinHorizontalGridlines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbViolinHorizontalGridlines.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbViolinHorizontalGridlines.Location = New System.Drawing.Point(286, 57)
        Me.cbViolinHorizontalGridlines.Name = "cbViolinHorizontalGridlines"
        Me.cbViolinHorizontalGridlines.Size = New System.Drawing.Size(145, 20)
        Me.cbViolinHorizontalGridlines.TabIndex = 11
        Me.cbViolinHorizontalGridlines.Text = "Horizontal Gridlines"
        Me.cbViolinHorizontalGridlines.UseVisualStyleBackColor = True
        '
        'cbViolinOutline
        '
        Me.cbViolinOutline.AutoSize = True
        Me.cbViolinOutline.Checked = True
        Me.cbViolinOutline.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbViolinOutline.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbViolinOutline.Location = New System.Drawing.Point(286, 83)
        Me.cbViolinOutline.Name = "cbViolinOutline"
        Me.cbViolinOutline.Size = New System.Drawing.Size(70, 20)
        Me.cbViolinOutline.TabIndex = 10
        Me.cbViolinOutline.Text = "Outline"
        Me.cbViolinOutline.UseVisualStyleBackColor = True
        '
        'cmbViolinPalette
        '
        Me.cmbViolinPalette.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbViolinPalette.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbViolinPalette.FormattingEnabled = True
        Me.cmbViolinPalette.Location = New System.Drawing.Point(155, 21)
        Me.cmbViolinPalette.Name = "cmbViolinPalette"
        Me.cmbViolinPalette.Size = New System.Drawing.Size(200, 24)
        Me.cmbViolinPalette.TabIndex = 9
        '
        'lblViolinPalette
        '
        Me.lblViolinPalette.AutoSize = True
        Me.lblViolinPalette.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblViolinPalette.Location = New System.Drawing.Point(6, 25)
        Me.lblViolinPalette.Name = "lblViolinPalette"
        Me.lblViolinPalette.Size = New System.Drawing.Size(124, 16)
        Me.lblViolinPalette.TabIndex = 8
        Me.lblViolinPalette.Text = "Group color palette:"
        '
        'nudViolinFillTransparency
        '
        Me.nudViolinFillTransparency.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudViolinFillTransparency.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudViolinFillTransparency.Location = New System.Drawing.Point(155, 55)
        Me.nudViolinFillTransparency.Name = "nudViolinFillTransparency"
        Me.nudViolinFillTransparency.Size = New System.Drawing.Size(78, 22)
        Me.nudViolinFillTransparency.TabIndex = 7
        Me.nudViolinFillTransparency.Value = New Decimal(New Integer() {20, 0, 0, 0})
        '
        'lblViolinFillTransparency
        '
        Me.lblViolinFillTransparency.AutoSize = True
        Me.lblViolinFillTransparency.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblViolinFillTransparency.Location = New System.Drawing.Point(6, 57)
        Me.lblViolinFillTransparency.Name = "lblViolinFillTransparency"
        Me.lblViolinFillTransparency.Size = New System.Drawing.Size(114, 16)
        Me.lblViolinFillTransparency.TabIndex = 6
        Me.lblViolinFillTransparency.Text = "Fill Transparency:"
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
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimumSize = New System.Drawing.Size(483, 457)
        Me.Name = "UibyID"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPage_Options.ResumeLayout(False)
        Me.TabPage_Options.PerformLayout()
        CType(Me.spinBtnAlpha, System.ComponentModel.ISupportInitialize).EndInit()
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
        CType(Me.spinBtnAlphaOutliers, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage_OptionsUTT.ResumeLayout(False)
        Me.TabPage_OptionsUTT.PerformLayout()
        Me.grpVarianceModel_UTT.ResumeLayout(False)
        Me.grpVarianceModel_UTT.PerformLayout()
        Me.grpHypothesisType_UTT.ResumeLayout(False)
        Me.grpHypothesisType_UTT.PerformLayout()
        CType(Me.spinBtnAlpha_UTT, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage_OptionsCategoricalHistogram.ResumeLayout(False)
        Me.grpCatHistAppearance.ResumeLayout(False)
        Me.grpCatHistAppearance.PerformLayout()
        CType(Me.nudCatHistSeriesOverlap, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudCatHistGapWidth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpCatHistPlotType.ResumeLayout(False)
        Me.grpCatHistPlotType.PerformLayout()
        Me.grpCatHistBinSize.ResumeLayout(False)
        Me.grpCatHistBinSize.PerformLayout()
        Me.TabPage_OptionsViolin.ResumeLayout(False)
        Me.grpScalingDisplay.ResumeLayout(False)
        Me.grpScalingDisplay.PerformLayout()
        Me.grpViolinDensity.ResumeLayout(False)
        Me.grpViolinDensity.PerformLayout()
        CType(Me.nudViolinDensityPoints, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudViolinBandwidthAdjustment, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.nudViolinChartHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudViolinChartWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudViolinFillTransparency, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents lblRefedit2 As Windows.Forms.Label
    Friend WithEvents lblRefedit1 As Windows.Forms.Label
    Friend WithEvents optByID As Windows.Forms.RadioButton
    Friend WithEvents optByColumn As Windows.Forms.RadioButton
    Friend WithEvents TabPage_Options As Windows.Forms.TabPage
    Friend WithEvents RefEdit2 As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents RefEdit1 As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents ckBoxPlot As Windows.Forms.CheckBox
    Friend WithEvents ckDescriptiveStatistics As Windows.Forms.CheckBox
    Friend WithEvents ckEstimateOfShift As Windows.Forms.CheckBox
    Friend WithEvents progressBarExactCalc As Windows.Forms.ProgressBar
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Global.BESHStatNG.Excel2007RefEdit
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
    Friend WithEvents spinBtnAlphaOutliers As Windows.Forms.NumericUpDown
    Friend WithEvents ckBoxPlot_Outliers As Windows.Forms.CheckBox
    Friend WithEvents ckDescriptive_NormalPlot As Windows.Forms.CheckBox
    Friend WithEvents lblAlpha As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha As Windows.Forms.NumericUpDown
    Friend WithEvents TabPage_OptionsUTT As Windows.Forms.TabPage
    Friend WithEvents grpHypothesisType_UTT As Windows.Forms.GroupBox
    Friend WithEvents optHypothesisEquivalence_UTT As Windows.Forms.RadioButton
    Friend WithEvents optHypothesisNonInferiority_UTT As Windows.Forms.RadioButton
    Friend WithEvents optHypothesisSuperiority_UTT As Windows.Forms.RadioButton
    Friend WithEvents lblAlpha_UTT As Windows.Forms.Label
    Friend WithEvents spinBtnAlpha_UTT As Windows.Forms.NumericUpDown
    Friend WithEvents ckBoxPlot_UTT As Windows.Forms.CheckBox
    Friend WithEvents ckDescriptiveStatistics_UTT As Windows.Forms.CheckBox
    Friend WithEvents tbMargin_UTT As Windows.Forms.TextBox
    Friend WithEvents lblMargin_UTT As Windows.Forms.Label
    Friend WithEvents grpVarianceModel_UTT As Windows.Forms.GroupBox
    Friend WithEvents optVarianceEqual_UTT As Windows.Forms.RadioButton
    Friend WithEvents optVarianceWelch_UTT As Windows.Forms.RadioButton
    Friend WithEvents lblMarginHint_UTT As Windows.Forms.Label
    Friend WithEvents TabPage_OptionsCategoricalHistogram As Windows.Forms.TabPage
    Friend WithEvents grpCatHistBinSize As Windows.Forms.GroupBox
    Friend WithEvents optCatHistScott As Windows.Forms.RadioButton
    Friend WithEvents optCatHistFreedmanDiaconis As Windows.Forms.RadioButton
    Friend WithEvents optCatHistDoan As Windows.Forms.RadioButton
    Friend WithEvents optCatHistSturges As Windows.Forms.RadioButton
    Friend WithEvents grpCatHistAppearance As Windows.Forms.GroupBox
    Friend WithEvents grpCatHistPlotType As Windows.Forms.GroupBox
    Friend WithEvents optCatHistDifferentSampleSizes As Windows.Forms.RadioButton
    Friend WithEvents optCatHistStackedBar As Windows.Forms.RadioButton
    Friend WithEvents optCatHistBarsWithLegend As Windows.Forms.RadioButton
    Friend WithEvents lblCatHistGapWidth As Windows.Forms.Label
    Friend WithEvents nudCatHistGapWidth As Windows.Forms.NumericUpDown
    Friend WithEvents nudCatHistSeriesOverlap As Windows.Forms.NumericUpDown
    Friend WithEvents lblCatHistSeriesOverlap As Windows.Forms.Label
    Friend WithEvents cmbCatHistPalette As Windows.Forms.ComboBox
    Friend WithEvents lblCatHistPalette As Windows.Forms.Label
    Friend WithEvents TabPage_OptionsViolin As Windows.Forms.TabPage
    Friend WithEvents GroupBox1 As Windows.Forms.GroupBox
    Friend WithEvents cmbViolinPalette As Windows.Forms.ComboBox
    Friend WithEvents lblViolinPalette As Windows.Forms.Label
    Friend WithEvents nudViolinFillTransparency As Windows.Forms.NumericUpDown
    Friend WithEvents lblViolinFillTransparency As Windows.Forms.Label
    Friend WithEvents cbViolinHorizontalGridlines As Windows.Forms.CheckBox
    Friend WithEvents cbViolinOutline As Windows.Forms.CheckBox
    Friend WithEvents grpViolinDensity As Windows.Forms.GroupBox
    Friend WithEvents nudViolinBandwidthAdjustment As Windows.Forms.NumericUpDown
    Friend WithEvents lblViolinBandwidthAdjustment As Windows.Forms.Label
    Friend WithEvents cmbViolinBandwidth As Windows.Forms.ComboBox
    Friend WithEvents lblViolinBandwidth As Windows.Forms.Label
    Friend WithEvents nudViolinDensityPoints As Windows.Forms.NumericUpDown
    Friend WithEvents lblViolinDensityPoints As Windows.Forms.Label
    Friend WithEvents grpScalingDisplay As Windows.Forms.GroupBox
    Friend WithEvents cmdViolinMedian As Windows.Forms.CheckBox
    Friend WithEvents cbViolinInnerBoxPlot As Windows.Forms.CheckBox
    Friend WithEvents cmdViolinScaling As Windows.Forms.ComboBox
    Friend WithEvents lblViolinScaling As Windows.Forms.Label
    Friend WithEvents cmdViolinMean As Windows.Forms.CheckBox
    Friend WithEvents cmdViolinTrimDensity As Windows.Forms.CheckBox
    Friend WithEvents cmdViolinIndividualObs As Windows.Forms.CheckBox
    Friend WithEvents nudViolinChartHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblViolinChartHeight As Windows.Forms.Label
    Friend WithEvents nudViolinChartWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblViolinChartWidth As Windows.Forms.Label
End Class
