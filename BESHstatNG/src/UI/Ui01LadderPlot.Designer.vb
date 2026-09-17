<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui01LadderPlot
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui01LadderPlot))
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1_Input = New System.Windows.Forms.TabPage()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.ckOmitIncompleteRows = New System.Windows.Forms.CheckBox()
        Me.RefEdit_SecondValues = New BESHStatNG.Excel2007RefEdit()
        Me.lblSecondValues = New System.Windows.Forms.Label()
        Me.RefEdit_RecordLabels = New BESHStatNG.Excel2007RefEdit()
        Me.lblRecordLabels = New System.Windows.Forms.Label()
        Me.RefEdit_FirstValues = New BESHStatNG.Excel2007RefEdit()
        Me.lblFirstValues = New System.Windows.Forms.Label()
        Me.RefEdit_GroupingVariable = New BESHStatNG.Excel2007RefEdit()
        Me.lblGroupingVariable = New System.Windows.Forms.Label()
        Me.TabPage2_Options = New System.Windows.Forms.TabPage()
        Me.grpJitter = New System.Windows.Forms.GroupBox()
        Me.nudHorizontalJitter = New System.Windows.Forms.NumericUpDown()
        Me.ckHorizontalJitter = New System.Windows.Forms.CheckBox()
        Me.grpAxisChart = New System.Windows.Forms.GroupBox()
        Me.ckYAxisIncludeZero = New System.Windows.Forms.CheckBox()
        Me.ckShowHorizontalGridlines = New System.Windows.Forms.CheckBox()
        Me.ckShowLegend = New System.Windows.Forms.CheckBox()
        Me.nudChartHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblChartHeight = New System.Windows.Forms.Label()
        Me.nudChartWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblChartWidth = New System.Windows.Forms.Label()
        Me.txtYAxisNumberFormat = New System.Windows.Forms.TextBox()
        Me.lblYAxisNumberFormat = New System.Windows.Forms.Label()
        Me.txtYAxisMajorUnit = New System.Windows.Forms.TextBox()
        Me.lblYAxisMajorUnit = New System.Windows.Forms.Label()
        Me.txtYAxisMaximum = New System.Windows.Forms.TextBox()
        Me.lblYAxisMaximum = New System.Windows.Forms.Label()
        Me.txtYAxisMinimum = New System.Windows.Forms.TextBox()
        Me.lblYAxisMinimum = New System.Windows.Forms.Label()
        Me.grpTitlesSeries = New System.Windows.Forms.GroupBox()
        Me.cmbEndpointLabelContent = New System.Windows.Forms.ComboBox()
        Me.ckShowSideLabels = New System.Windows.Forms.CheckBox()
        Me.ckColorEndpointLabelsBySeries = New System.Windows.Forms.CheckBox()
        Me.ckAvoidEndpointLabelOverlap = New System.Windows.Forms.CheckBox()
        Me.txtEndpointValueFormat = New System.Windows.Forms.TextBox()
        Me.lblEndpointValueFormat = New System.Windows.Forms.Label()
        Me.lblEndpointLabelContent = New System.Windows.Forms.Label()
        Me.ckShowSecondEndpointLabels = New System.Windows.Forms.CheckBox()
        Me.ckShowFirstEndpointLabels = New System.Windows.Forms.CheckBox()
        Me.txtYAxisTitle = New System.Windows.Forms.TextBox()
        Me.lblYAxisTitle = New System.Windows.Forms.Label()
        Me.TabPage3_Appearance = New System.Windows.Forms.TabPage()
        Me.grpConnector = New System.Windows.Forms.GroupBox()
        Me.ckShowLines = New System.Windows.Forms.CheckBox()
        Me.cmbConnectorLineStyle = New System.Windows.Forms.ComboBox()
        Me.nudConnectorWidth = New System.Windows.Forms.NumericUpDown()
        Me.pnlConnectorColor = New System.Windows.Forms.Panel()
        Me.btnConnectorColor = New System.Windows.Forms.Button()
        Me.nudConnectorTransparency = New System.Windows.Forms.NumericUpDown()
        Me.lblTransparency = New System.Windows.Forms.Label()
        Me.lblLineStyle = New System.Windows.Forms.Label()
        Me.lblLineWidth = New System.Windows.Forms.Label()
        Me.lblConnectorColor = New System.Windows.Forms.Label()
        Me.grpMarkers = New System.Windows.Forms.GroupBox()
        Me.pnlSingleColor = New System.Windows.Forms.Panel()
        Me.btnSingleColor = New System.Windows.Forms.Button()
        Me.lblSingleColor = New System.Windows.Forms.Label()
        Me.ckShowMarkers = New System.Windows.Forms.CheckBox()
        Me.cmbColorMode = New System.Windows.Forms.ComboBox()
        Me.lblColorMode = New System.Windows.Forms.Label()
        Me.cmdMarkerStyle = New System.Windows.Forms.ComboBox()
        Me.nudMarkerSize = New System.Windows.Forms.NumericUpDown()
        Me.lblMarkerSize = New System.Windows.Forms.Label()
        Me.lblMarkerStyle = New System.Windows.Forms.Label()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1_Input.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPage2_Options.SuspendLayout()
        Me.grpJitter.SuspendLayout()
        CType(Me.nudHorizontalJitter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpAxisChart.SuspendLayout()
        CType(Me.nudChartHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudChartWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpTitlesSeries.SuspendLayout()
        Me.TabPage3_Appearance.SuspendLayout()
        Me.grpConnector.SuspendLayout()
        CType(Me.nudConnectorWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudConnectorTransparency, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpMarkers.SuspendLayout()
        CType(Me.nudMarkerSize, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1_Input)
        Me.TabMultipage.Controls.Add(Me.TabPage2_Options)
        Me.TabMultipage.Controls.Add(Me.TabPage3_Appearance)
        Me.TabMultipage.Location = New System.Drawing.Point(3, 2)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(456, 447)
        Me.TabMultipage.TabIndex = 19
        '
        'TabPage1_Input
        '
        Me.TabPage1_Input.Controls.Add(Me.grpOutput)
        Me.TabPage1_Input.Controls.Add(Me.grpInput)
        Me.TabPage1_Input.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1_Input.Name = "TabPage1_Input"
        Me.TabPage1_Input.Size = New System.Drawing.Size(448, 418)
        Me.TabPage1_Input.TabIndex = 2
        Me.TabPage1_Input.Text = "Input"
        Me.TabPage1_Input.UseVisualStyleBackColor = True
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(0, 285)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(442, 130)
        Me.grpOutput.TabIndex = 8
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
        Me.grpInput.Controls.Add(Me.ckOmitIncompleteRows)
        Me.grpInput.Controls.Add(Me.RefEdit_SecondValues)
        Me.grpInput.Controls.Add(Me.lblSecondValues)
        Me.grpInput.Controls.Add(Me.RefEdit_RecordLabels)
        Me.grpInput.Controls.Add(Me.lblRecordLabels)
        Me.grpInput.Controls.Add(Me.RefEdit_FirstValues)
        Me.grpInput.Controls.Add(Me.lblFirstValues)
        Me.grpInput.Controls.Add(Me.RefEdit_GroupingVariable)
        Me.grpInput.Controls.Add(Me.lblGroupingVariable)
        Me.grpInput.Location = New System.Drawing.Point(3, 3)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(436, 260)
        Me.grpInput.TabIndex = 3
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'ckOmitIncompleteRows
        '
        Me.ckOmitIncompleteRows.AutoSize = True
        Me.ckOmitIncompleteRows.Location = New System.Drawing.Point(146, 197)
        Me.ckOmitIncompleteRows.Name = "ckOmitIncompleteRows"
        Me.ckOmitIncompleteRows.Size = New System.Drawing.Size(156, 20)
        Me.ckOmitIncompleteRows.TabIndex = 17
        Me.ckOmitIncompleteRows.Text = "Omit incomplete rows"
        Me.ckOmitIncompleteRows.UseVisualStyleBackColor = True
        '
        'RefEdit_SecondValues
        '
        Me.RefEdit_SecondValues.Address = ""
        Me.RefEdit_SecondValues.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_SecondValues.ExcelConnector = Nothing
        Me.RefEdit_SecondValues.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_SecondValues.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_SecondValues.Location = New System.Drawing.Point(146, 62)
        Me.RefEdit_SecondValues.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_SecondValues.Name = "RefEdit_SecondValues"
        Me.RefEdit_SecondValues.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_SecondValues.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_SecondValues.TabIndex = 15
        '
        'lblSecondValues
        '
        Me.lblSecondValues.AutoSize = True
        Me.lblSecondValues.Location = New System.Drawing.Point(11, 62)
        Me.lblSecondValues.Name = "lblSecondValues"
        Me.lblSecondValues.Size = New System.Drawing.Size(100, 16)
        Me.lblSecondValues.TabIndex = 14
        Me.lblSecondValues.Text = "Second values:"
        '
        'RefEdit_RecordLabels
        '
        Me.RefEdit_RecordLabels.Address = ""
        Me.RefEdit_RecordLabels.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_RecordLabels.ExcelConnector = Nothing
        Me.RefEdit_RecordLabels.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_RecordLabels.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_RecordLabels.Location = New System.Drawing.Point(146, 102)
        Me.RefEdit_RecordLabels.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_RecordLabels.Name = "RefEdit_RecordLabels"
        Me.RefEdit_RecordLabels.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_RecordLabels.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_RecordLabels.TabIndex = 10
        '
        'lblRecordLabels
        '
        Me.lblRecordLabels.Location = New System.Drawing.Point(11, 102)
        Me.lblRecordLabels.Name = "lblRecordLabels"
        Me.lblRecordLabels.Size = New System.Drawing.Size(137, 32)
        Me.lblRecordLabels.TabIndex = 9
        Me.lblRecordLabels.Text = "Record labels (optional):"
        '
        'RefEdit_FirstValues
        '
        Me.RefEdit_FirstValues.Address = ""
        Me.RefEdit_FirstValues.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_FirstValues.ExcelConnector = Nothing
        Me.RefEdit_FirstValues.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_FirstValues.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_FirstValues.Location = New System.Drawing.Point(146, 22)
        Me.RefEdit_FirstValues.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_FirstValues.Name = "RefEdit_FirstValues"
        Me.RefEdit_FirstValues.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_FirstValues.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_FirstValues.TabIndex = 8
        '
        'lblFirstValues
        '
        Me.lblFirstValues.AutoSize = True
        Me.lblFirstValues.Location = New System.Drawing.Point(11, 22)
        Me.lblFirstValues.Name = "lblFirstValues"
        Me.lblFirstValues.Size = New System.Drawing.Size(78, 16)
        Me.lblFirstValues.TabIndex = 7
        Me.lblFirstValues.Text = "First values:"
        '
        'RefEdit_GroupingVariable
        '
        Me.RefEdit_GroupingVariable.Address = ""
        Me.RefEdit_GroupingVariable.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_GroupingVariable.ExcelConnector = Nothing
        Me.RefEdit_GroupingVariable.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_GroupingVariable.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_GroupingVariable.Location = New System.Drawing.Point(146, 142)
        Me.RefEdit_GroupingVariable.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_GroupingVariable.Name = "RefEdit_GroupingVariable"
        Me.RefEdit_GroupingVariable.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_GroupingVariable.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_GroupingVariable.TabIndex = 6
        '
        'lblGroupingVariable
        '
        Me.lblGroupingVariable.Location = New System.Drawing.Point(14, 142)
        Me.lblGroupingVariable.Name = "lblGroupingVariable"
        Me.lblGroupingVariable.Size = New System.Drawing.Size(137, 32)
        Me.lblGroupingVariable.TabIndex = 5
        Me.lblGroupingVariable.Text = "Grouping variable (optional):"
        '
        'TabPage2_Options
        '
        Me.TabPage2_Options.Controls.Add(Me.grpJitter)
        Me.TabPage2_Options.Controls.Add(Me.grpAxisChart)
        Me.TabPage2_Options.Controls.Add(Me.grpTitlesSeries)
        Me.TabPage2_Options.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2_Options.Name = "TabPage2_Options"
        Me.TabPage2_Options.Size = New System.Drawing.Size(448, 418)
        Me.TabPage2_Options.TabIndex = 3
        Me.TabPage2_Options.Text = "Options"
        Me.TabPage2_Options.UseVisualStyleBackColor = True
        '
        'grpJitter
        '
        Me.grpJitter.Controls.Add(Me.nudHorizontalJitter)
        Me.grpJitter.Controls.Add(Me.ckHorizontalJitter)
        Me.grpJitter.Location = New System.Drawing.Point(3, 340)
        Me.grpJitter.Name = "grpJitter"
        Me.grpJitter.Size = New System.Drawing.Size(439, 53)
        Me.grpJitter.TabIndex = 27
        Me.grpJitter.TabStop = False
        Me.grpJitter.Text = "Point collisions"
        '
        'nudHorizontalJitter
        '
        Me.nudHorizontalJitter.DecimalPlaces = 2
        Me.nudHorizontalJitter.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudHorizontalJitter.Location = New System.Drawing.Point(133, 20)
        Me.nudHorizontalJitter.Maximum = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.nudHorizontalJitter.Minimum = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudHorizontalJitter.Name = "nudHorizontalJitter"
        Me.nudHorizontalJitter.Size = New System.Drawing.Size(76, 22)
        Me.nudHorizontalJitter.TabIndex = 30
        Me.nudHorizontalJitter.Value = New Decimal(New Integer() {8, 0, 0, 131072})
        '
        'ckHorizontalJitter
        '
        Me.ckHorizontalJitter.AutoSize = True
        Me.ckHorizontalJitter.Checked = True
        Me.ckHorizontalJitter.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckHorizontalJitter.Location = New System.Drawing.Point(11, 21)
        Me.ckHorizontalJitter.Name = "ckHorizontalJitter"
        Me.ckHorizontalJitter.Size = New System.Drawing.Size(116, 20)
        Me.ckHorizontalJitter.TabIndex = 29
        Me.ckHorizontalJitter.Text = "Horizontal jitter"
        Me.ckHorizontalJitter.UseVisualStyleBackColor = True
        '
        'grpAxisChart
        '
        Me.grpAxisChart.Controls.Add(Me.ckYAxisIncludeZero)
        Me.grpAxisChart.Controls.Add(Me.ckShowHorizontalGridlines)
        Me.grpAxisChart.Controls.Add(Me.ckShowLegend)
        Me.grpAxisChart.Controls.Add(Me.nudChartHeight)
        Me.grpAxisChart.Controls.Add(Me.lblChartHeight)
        Me.grpAxisChart.Controls.Add(Me.nudChartWidth)
        Me.grpAxisChart.Controls.Add(Me.lblChartWidth)
        Me.grpAxisChart.Controls.Add(Me.txtYAxisNumberFormat)
        Me.grpAxisChart.Controls.Add(Me.lblYAxisNumberFormat)
        Me.grpAxisChart.Controls.Add(Me.txtYAxisMajorUnit)
        Me.grpAxisChart.Controls.Add(Me.lblYAxisMajorUnit)
        Me.grpAxisChart.Controls.Add(Me.txtYAxisMaximum)
        Me.grpAxisChart.Controls.Add(Me.lblYAxisMaximum)
        Me.grpAxisChart.Controls.Add(Me.txtYAxisMinimum)
        Me.grpAxisChart.Controls.Add(Me.lblYAxisMinimum)
        Me.grpAxisChart.Location = New System.Drawing.Point(3, 197)
        Me.grpAxisChart.Name = "grpAxisChart"
        Me.grpAxisChart.Size = New System.Drawing.Size(439, 137)
        Me.grpAxisChart.TabIndex = 26
        Me.grpAxisChart.TabStop = False
        Me.grpAxisChart.Text = "Axis and chart"
        '
        'ckYAxisIncludeZero
        '
        Me.ckYAxisIncludeZero.AutoSize = True
        Me.ckYAxisIncludeZero.Checked = True
        Me.ckYAxisIncludeZero.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckYAxisIncludeZero.Location = New System.Drawing.Point(299, 105)
        Me.ckYAxisIncludeZero.Name = "ckYAxisIncludeZero"
        Me.ckYAxisIncludeZero.Size = New System.Drawing.Size(140, 20)
        Me.ckYAxisIncludeZero.TabIndex = 32
        Me.ckYAxisIncludeZero.Text = "Y axis include zero"
        Me.ckYAxisIncludeZero.UseVisualStyleBackColor = True
        '
        'ckShowHorizontalGridlines
        '
        Me.ckShowHorizontalGridlines.AutoSize = True
        Me.ckShowHorizontalGridlines.Checked = True
        Me.ckShowHorizontalGridlines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowHorizontalGridlines.Location = New System.Drawing.Point(124, 105)
        Me.ckShowHorizontalGridlines.Name = "ckShowHorizontalGridlines"
        Me.ckShowHorizontalGridlines.Size = New System.Drawing.Size(176, 20)
        Me.ckShowHorizontalGridlines.TabIndex = 31
        Me.ckShowHorizontalGridlines.Text = "Show horizontal gridlines"
        Me.ckShowHorizontalGridlines.UseVisualStyleBackColor = True
        '
        'ckShowLegend
        '
        Me.ckShowLegend.AutoSize = True
        Me.ckShowLegend.Checked = True
        Me.ckShowLegend.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowLegend.Location = New System.Drawing.Point(11, 105)
        Me.ckShowLegend.Name = "ckShowLegend"
        Me.ckShowLegend.Size = New System.Drawing.Size(107, 20)
        Me.ckShowLegend.TabIndex = 28
        Me.ckShowLegend.Text = "Show legend"
        Me.ckShowLegend.UseVisualStyleBackColor = True
        '
        'nudChartHeight
        '
        Me.nudChartHeight.Location = New System.Drawing.Point(323, 79)
        Me.nudChartHeight.Maximum = New Decimal(New Integer() {3000, 0, 0, 0})
        Me.nudChartHeight.Minimum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.nudChartHeight.Name = "nudChartHeight"
        Me.nudChartHeight.Size = New System.Drawing.Size(104, 22)
        Me.nudChartHeight.TabIndex = 27
        Me.nudChartHeight.Value = New Decimal(New Integer() {460, 0, 0, 0})
        '
        'lblChartHeight
        '
        Me.lblChartHeight.AutoSize = True
        Me.lblChartHeight.Location = New System.Drawing.Point(226, 81)
        Me.lblChartHeight.Name = "lblChartHeight"
        Me.lblChartHeight.Size = New System.Drawing.Size(80, 16)
        Me.lblChartHeight.TabIndex = 26
        Me.lblChartHeight.Text = "Chart height:"
        '
        'nudChartWidth
        '
        Me.nudChartWidth.Location = New System.Drawing.Point(105, 77)
        Me.nudChartWidth.Maximum = New Decimal(New Integer() {3000, 0, 0, 0})
        Me.nudChartWidth.Minimum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.nudChartWidth.Name = "nudChartWidth"
        Me.nudChartWidth.Size = New System.Drawing.Size(104, 22)
        Me.nudChartWidth.TabIndex = 25
        Me.nudChartWidth.Value = New Decimal(New Integer() {700, 0, 0, 0})
        '
        'lblChartWidth
        '
        Me.lblChartWidth.AutoSize = True
        Me.lblChartWidth.Location = New System.Drawing.Point(6, 79)
        Me.lblChartWidth.Name = "lblChartWidth"
        Me.lblChartWidth.Size = New System.Drawing.Size(74, 16)
        Me.lblChartWidth.TabIndex = 24
        Me.lblChartWidth.Text = "Chart width:"
        '
        'txtYAxisNumberFormat
        '
        Me.txtYAxisNumberFormat.Location = New System.Drawing.Point(323, 52)
        Me.txtYAxisNumberFormat.Name = "txtYAxisNumberFormat"
        Me.txtYAxisNumberFormat.Size = New System.Drawing.Size(104, 22)
        Me.txtYAxisNumberFormat.TabIndex = 23
        '
        'lblYAxisNumberFormat
        '
        Me.lblYAxisNumberFormat.AutoSize = True
        Me.lblYAxisNumberFormat.Location = New System.Drawing.Point(224, 55)
        Me.lblYAxisNumberFormat.Name = "lblYAxisNumberFormat"
        Me.lblYAxisNumberFormat.Size = New System.Drawing.Size(98, 16)
        Me.lblYAxisNumberFormat.TabIndex = 22
        Me.lblYAxisNumberFormat.Text = "Number format:"
        '
        'txtYAxisMajorUnit
        '
        Me.txtYAxisMajorUnit.Location = New System.Drawing.Point(105, 49)
        Me.txtYAxisMajorUnit.Name = "txtYAxisMajorUnit"
        Me.txtYAxisMajorUnit.Size = New System.Drawing.Size(104, 22)
        Me.txtYAxisMajorUnit.TabIndex = 21
        '
        'lblYAxisMajorUnit
        '
        Me.lblYAxisMajorUnit.AutoSize = True
        Me.lblYAxisMajorUnit.Location = New System.Drawing.Point(6, 52)
        Me.lblYAxisMajorUnit.Name = "lblYAxisMajorUnit"
        Me.lblYAxisMajorUnit.Size = New System.Drawing.Size(90, 16)
        Me.lblYAxisMajorUnit.TabIndex = 20
        Me.lblYAxisMajorUnit.Text = "Major interval:"
        '
        'txtYAxisMaximum
        '
        Me.txtYAxisMaximum.Location = New System.Drawing.Point(323, 21)
        Me.txtYAxisMaximum.Name = "txtYAxisMaximum"
        Me.txtYAxisMaximum.Size = New System.Drawing.Size(104, 22)
        Me.txtYAxisMaximum.TabIndex = 19
        '
        'lblYAxisMaximum
        '
        Me.lblYAxisMaximum.AutoSize = True
        Me.lblYAxisMaximum.Location = New System.Drawing.Point(226, 24)
        Me.lblYAxisMaximum.Name = "lblYAxisMaximum"
        Me.lblYAxisMaximum.Size = New System.Drawing.Size(79, 16)
        Me.lblYAxisMaximum.TabIndex = 18
        Me.lblYAxisMaximum.Text = "Y maximum:"
        '
        'txtYAxisMinimum
        '
        Me.txtYAxisMinimum.Location = New System.Drawing.Point(105, 21)
        Me.txtYAxisMinimum.Name = "txtYAxisMinimum"
        Me.txtYAxisMinimum.Size = New System.Drawing.Size(104, 22)
        Me.txtYAxisMinimum.TabIndex = 17
        '
        'lblYAxisMinimum
        '
        Me.lblYAxisMinimum.AutoSize = True
        Me.lblYAxisMinimum.Location = New System.Drawing.Point(6, 24)
        Me.lblYAxisMinimum.Name = "lblYAxisMinimum"
        Me.lblYAxisMinimum.Size = New System.Drawing.Size(75, 16)
        Me.lblYAxisMinimum.TabIndex = 2
        Me.lblYAxisMinimum.Text = "Y minimum:"
        '
        'grpTitlesSeries
        '
        Me.grpTitlesSeries.Controls.Add(Me.cmbEndpointLabelContent)
        Me.grpTitlesSeries.Controls.Add(Me.ckShowSideLabels)
        Me.grpTitlesSeries.Controls.Add(Me.ckColorEndpointLabelsBySeries)
        Me.grpTitlesSeries.Controls.Add(Me.ckAvoidEndpointLabelOverlap)
        Me.grpTitlesSeries.Controls.Add(Me.txtEndpointValueFormat)
        Me.grpTitlesSeries.Controls.Add(Me.lblEndpointValueFormat)
        Me.grpTitlesSeries.Controls.Add(Me.lblEndpointLabelContent)
        Me.grpTitlesSeries.Controls.Add(Me.ckShowSecondEndpointLabels)
        Me.grpTitlesSeries.Controls.Add(Me.ckShowFirstEndpointLabels)
        Me.grpTitlesSeries.Controls.Add(Me.txtYAxisTitle)
        Me.grpTitlesSeries.Controls.Add(Me.lblYAxisTitle)
        Me.grpTitlesSeries.Location = New System.Drawing.Point(3, 3)
        Me.grpTitlesSeries.Name = "grpTitlesSeries"
        Me.grpTitlesSeries.Size = New System.Drawing.Size(439, 188)
        Me.grpTitlesSeries.TabIndex = 15
        Me.grpTitlesSeries.TabStop = False
        Me.grpTitlesSeries.Text = "Titles and series"
        '
        'cmbEndpointLabelContent
        '
        Me.cmbEndpointLabelContent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbEndpointLabelContent.FormattingEnabled = True
        Me.cmbEndpointLabelContent.Location = New System.Drawing.Point(154, 75)
        Me.cmbEndpointLabelContent.Name = "cmbEndpointLabelContent"
        Me.cmbEndpointLabelContent.Size = New System.Drawing.Size(273, 24)
        Me.cmbEndpointLabelContent.TabIndex = 41
        '
        'ckShowSideLabels
        '
        Me.ckShowSideLabels.AutoSize = True
        Me.ckShowSideLabels.Checked = True
        Me.ckShowSideLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowSideLabels.Location = New System.Drawing.Point(14, 156)
        Me.ckShowSideLabels.Name = "ckShowSideLabels"
        Me.ckShowSideLabels.Size = New System.Drawing.Size(210, 20)
        Me.ckShowSideLabels.TabIndex = 40
        Me.ckShowSideLabels.Text = "Show First/Second side labels"
        Me.ckShowSideLabels.UseVisualStyleBackColor = True
        '
        'ckColorEndpointLabelsBySeries
        '
        Me.ckColorEndpointLabelsBySeries.AutoSize = True
        Me.ckColorEndpointLabelsBySeries.Checked = True
        Me.ckColorEndpointLabelsBySeries.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckColorEndpointLabelsBySeries.Location = New System.Drawing.Point(169, 130)
        Me.ckColorEndpointLabelsBySeries.Name = "ckColorEndpointLabelsBySeries"
        Me.ckColorEndpointLabelsBySeries.Size = New System.Drawing.Size(166, 20)
        Me.ckColorEndpointLabelsBySeries.TabIndex = 39
        Me.ckColorEndpointLabelsBySeries.Text = "Colour labels by series"
        Me.ckColorEndpointLabelsBySeries.UseVisualStyleBackColor = True
        '
        'ckAvoidEndpointLabelOverlap
        '
        Me.ckAvoidEndpointLabelOverlap.AutoSize = True
        Me.ckAvoidEndpointLabelOverlap.Checked = True
        Me.ckAvoidEndpointLabelOverlap.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckAvoidEndpointLabelOverlap.Location = New System.Drawing.Point(14, 130)
        Me.ckAvoidEndpointLabelOverlap.Name = "ckAvoidEndpointLabelOverlap"
        Me.ckAvoidEndpointLabelOverlap.Size = New System.Drawing.Size(146, 20)
        Me.ckAvoidEndpointLabelOverlap.TabIndex = 38
        Me.ckAvoidEndpointLabelOverlap.Text = "Avoid label overlap"
        Me.ckAvoidEndpointLabelOverlap.UseVisualStyleBackColor = True
        '
        'txtEndpointValueFormat
        '
        Me.txtEndpointValueFormat.Location = New System.Drawing.Point(154, 102)
        Me.txtEndpointValueFormat.Name = "txtEndpointValueFormat"
        Me.txtEndpointValueFormat.Size = New System.Drawing.Size(104, 22)
        Me.txtEndpointValueFormat.TabIndex = 37
        '
        'lblEndpointValueFormat
        '
        Me.lblEndpointValueFormat.AutoSize = True
        Me.lblEndpointValueFormat.Location = New System.Drawing.Point(9, 105)
        Me.lblEndpointValueFormat.Name = "lblEndpointValueFormat"
        Me.lblEndpointValueFormat.Size = New System.Drawing.Size(139, 16)
        Me.lblEndpointValueFormat.TabIndex = 36
        Me.lblEndpointValueFormat.Text = "Endpoint value format:"
        '
        'lblEndpointLabelContent
        '
        Me.lblEndpointLabelContent.AutoSize = True
        Me.lblEndpointLabelContent.Location = New System.Drawing.Point(6, 78)
        Me.lblEndpointLabelContent.Name = "lblEndpointLabelContent"
        Me.lblEndpointLabelContent.Size = New System.Drawing.Size(142, 16)
        Me.lblEndpointLabelContent.TabIndex = 34
        Me.lblEndpointLabelContent.Text = "Endpoint label content:"
        '
        'ckShowSecondEndpointLabels
        '
        Me.ckShowSecondEndpointLabels.AutoSize = True
        Me.ckShowSecondEndpointLabels.Checked = True
        Me.ckShowSecondEndpointLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowSecondEndpointLabels.Location = New System.Drawing.Point(169, 49)
        Me.ckShowSecondEndpointLabels.Name = "ckShowSecondEndpointLabels"
        Me.ckShowSecondEndpointLabels.Size = New System.Drawing.Size(171, 20)
        Me.ckShowSecondEndpointLabels.TabIndex = 33
        Me.ckShowSecondEndpointLabels.Text = "Second endpoint labels"
        Me.ckShowSecondEndpointLabels.UseVisualStyleBackColor = True
        '
        'ckShowFirstEndpointLabels
        '
        Me.ckShowFirstEndpointLabels.AutoSize = True
        Me.ckShowFirstEndpointLabels.Checked = True
        Me.ckShowFirstEndpointLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowFirstEndpointLabels.Location = New System.Drawing.Point(11, 47)
        Me.ckShowFirstEndpointLabels.Name = "ckShowFirstEndpointLabels"
        Me.ckShowFirstEndpointLabels.Size = New System.Drawing.Size(149, 20)
        Me.ckShowFirstEndpointLabels.TabIndex = 32
        Me.ckShowFirstEndpointLabels.Text = "First endpoint labels"
        Me.ckShowFirstEndpointLabels.UseVisualStyleBackColor = True
        '
        'txtYAxisTitle
        '
        Me.txtYAxisTitle.Location = New System.Drawing.Point(154, 21)
        Me.txtYAxisTitle.Name = "txtYAxisTitle"
        Me.txtYAxisTitle.Size = New System.Drawing.Size(273, 22)
        Me.txtYAxisTitle.TabIndex = 19
        '
        'lblYAxisTitle
        '
        Me.lblYAxisTitle.AutoSize = True
        Me.lblYAxisTitle.Location = New System.Drawing.Point(6, 19)
        Me.lblYAxisTitle.Name = "lblYAxisTitle"
        Me.lblYAxisTitle.Size = New System.Drawing.Size(70, 16)
        Me.lblYAxisTitle.TabIndex = 18
        Me.lblYAxisTitle.Text = "Y-axis title:"
        '
        'TabPage3_Appearance
        '
        Me.TabPage3_Appearance.Controls.Add(Me.grpConnector)
        Me.TabPage3_Appearance.Controls.Add(Me.grpMarkers)
        Me.TabPage3_Appearance.Location = New System.Drawing.Point(4, 25)
        Me.TabPage3_Appearance.Name = "TabPage3_Appearance"
        Me.TabPage3_Appearance.Size = New System.Drawing.Size(448, 418)
        Me.TabPage3_Appearance.TabIndex = 1
        Me.TabPage3_Appearance.Text = "Appearance"
        Me.TabPage3_Appearance.UseVisualStyleBackColor = True
        '
        'grpConnector
        '
        Me.grpConnector.Controls.Add(Me.ckShowLines)
        Me.grpConnector.Controls.Add(Me.cmbConnectorLineStyle)
        Me.grpConnector.Controls.Add(Me.nudConnectorWidth)
        Me.grpConnector.Controls.Add(Me.pnlConnectorColor)
        Me.grpConnector.Controls.Add(Me.btnConnectorColor)
        Me.grpConnector.Controls.Add(Me.nudConnectorTransparency)
        Me.grpConnector.Controls.Add(Me.lblTransparency)
        Me.grpConnector.Controls.Add(Me.lblLineStyle)
        Me.grpConnector.Controls.Add(Me.lblLineWidth)
        Me.grpConnector.Controls.Add(Me.lblConnectorColor)
        Me.grpConnector.Location = New System.Drawing.Point(5, 156)
        Me.grpConnector.Name = "grpConnector"
        Me.grpConnector.Size = New System.Drawing.Size(439, 117)
        Me.grpConnector.TabIndex = 14
        Me.grpConnector.TabStop = False
        Me.grpConnector.Text = "Connector"
        '
        'ckShowLines
        '
        Me.ckShowLines.AutoSize = True
        Me.ckShowLines.Checked = True
        Me.ckShowLines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowLines.Location = New System.Drawing.Point(309, 17)
        Me.ckShowLines.Name = "ckShowLines"
        Me.ckShowLines.Size = New System.Drawing.Size(93, 20)
        Me.ckShowLines.TabIndex = 30
        Me.ckShowLines.Text = "Show lines"
        Me.ckShowLines.UseVisualStyleBackColor = True
        '
        'cmbConnectorLineStyle
        '
        Me.cmbConnectorLineStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbConnectorLineStyle.FormattingEnabled = True
        Me.cmbConnectorLineStyle.Location = New System.Drawing.Point(83, 81)
        Me.cmbConnectorLineStyle.Name = "cmbConnectorLineStyle"
        Me.cmbConnectorLineStyle.Size = New System.Drawing.Size(193, 24)
        Me.cmbConnectorLineStyle.TabIndex = 23
        '
        'nudConnectorWidth
        '
        Me.nudConnectorWidth.DecimalPlaces = 2
        Me.nudConnectorWidth.Increment = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.nudConnectorWidth.Location = New System.Drawing.Point(84, 52)
        Me.nudConnectorWidth.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudConnectorWidth.Minimum = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.nudConnectorWidth.Name = "nudConnectorWidth"
        Me.nudConnectorWidth.Size = New System.Drawing.Size(74, 22)
        Me.nudConnectorWidth.TabIndex = 22
        Me.nudConnectorWidth.Value = New Decimal(New Integer() {15, 0, 0, 65536})
        '
        'pnlConnectorColor
        '
        Me.pnlConnectorColor.Location = New System.Drawing.Point(164, 17)
        Me.pnlConnectorColor.Name = "pnlConnectorColor"
        Me.pnlConnectorColor.Size = New System.Drawing.Size(54, 24)
        Me.pnlConnectorColor.TabIndex = 21
        '
        'btnConnectorColor
        '
        Me.btnConnectorColor.Location = New System.Drawing.Point(83, 17)
        Me.btnConnectorColor.Name = "btnConnectorColor"
        Me.btnConnectorColor.Size = New System.Drawing.Size(75, 23)
        Me.btnConnectorColor.TabIndex = 20
        Me.btnConnectorColor.Text = "Select..."
        Me.btnConnectorColor.UseVisualStyleBackColor = True
        '
        'nudConnectorTransparency
        '
        Me.nudConnectorTransparency.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudConnectorTransparency.Location = New System.Drawing.Point(359, 52)
        Me.nudConnectorTransparency.Name = "nudConnectorTransparency"
        Me.nudConnectorTransparency.Size = New System.Drawing.Size(74, 22)
        Me.nudConnectorTransparency.TabIndex = 14
        Me.nudConnectorTransparency.Value = New Decimal(New Integer() {25, 0, 0, 0})
        '
        'lblTransparency
        '
        Me.lblTransparency.AutoSize = True
        Me.lblTransparency.Location = New System.Drawing.Point(225, 54)
        Me.lblTransparency.Name = "lblTransparency"
        Me.lblTransparency.Size = New System.Drawing.Size(114, 16)
        Me.lblTransparency.TabIndex = 10
        Me.lblTransparency.Text = "Transparency (%)"
        '
        'lblLineStyle
        '
        Me.lblLineStyle.AutoSize = True
        Me.lblLineStyle.Location = New System.Drawing.Point(8, 84)
        Me.lblLineStyle.Name = "lblLineStyle"
        Me.lblLineStyle.Size = New System.Drawing.Size(63, 16)
        Me.lblLineStyle.TabIndex = 6
        Me.lblLineStyle.Text = "Line style"
        '
        'lblLineWidth
        '
        Me.lblLineWidth.AutoSize = True
        Me.lblLineWidth.Location = New System.Drawing.Point(6, 54)
        Me.lblLineWidth.Name = "lblLineWidth"
        Me.lblLineWidth.Size = New System.Drawing.Size(65, 16)
        Me.lblLineWidth.TabIndex = 4
        Me.lblLineWidth.Text = "Line width"
        '
        'lblConnectorColor
        '
        Me.lblConnectorColor.AutoSize = True
        Me.lblConnectorColor.Location = New System.Drawing.Point(32, 21)
        Me.lblConnectorColor.Name = "lblConnectorColor"
        Me.lblConnectorColor.Size = New System.Drawing.Size(39, 16)
        Me.lblConnectorColor.TabIndex = 2
        Me.lblConnectorColor.Text = "Color"
        '
        'grpMarkers
        '
        Me.grpMarkers.Controls.Add(Me.pnlSingleColor)
        Me.grpMarkers.Controls.Add(Me.btnSingleColor)
        Me.grpMarkers.Controls.Add(Me.lblSingleColor)
        Me.grpMarkers.Controls.Add(Me.ckShowMarkers)
        Me.grpMarkers.Controls.Add(Me.cmbColorMode)
        Me.grpMarkers.Controls.Add(Me.lblColorMode)
        Me.grpMarkers.Controls.Add(Me.cmdMarkerStyle)
        Me.grpMarkers.Controls.Add(Me.nudMarkerSize)
        Me.grpMarkers.Controls.Add(Me.lblMarkerSize)
        Me.grpMarkers.Controls.Add(Me.lblMarkerStyle)
        Me.grpMarkers.Location = New System.Drawing.Point(5, 3)
        Me.grpMarkers.Name = "grpMarkers"
        Me.grpMarkers.Size = New System.Drawing.Size(439, 147)
        Me.grpMarkers.TabIndex = 0
        Me.grpMarkers.TabStop = False
        Me.grpMarkers.Text = "Markers"
        '
        'pnlSingleColor
        '
        Me.pnlSingleColor.Location = New System.Drawing.Point(175, 118)
        Me.pnlSingleColor.Name = "pnlSingleColor"
        Me.pnlSingleColor.Size = New System.Drawing.Size(54, 24)
        Me.pnlSingleColor.TabIndex = 32
        '
        'btnSingleColor
        '
        Me.btnSingleColor.Location = New System.Drawing.Point(94, 118)
        Me.btnSingleColor.Name = "btnSingleColor"
        Me.btnSingleColor.Size = New System.Drawing.Size(75, 23)
        Me.btnSingleColor.TabIndex = 31
        Me.btnSingleColor.Text = "Select..."
        Me.btnSingleColor.UseVisualStyleBackColor = True
        '
        'lblSingleColor
        '
        Me.lblSingleColor.AutoSize = True
        Me.lblSingleColor.Location = New System.Drawing.Point(10, 121)
        Me.lblSingleColor.Name = "lblSingleColor"
        Me.lblSingleColor.Size = New System.Drawing.Size(78, 16)
        Me.lblSingleColor.TabIndex = 30
        Me.lblSingleColor.Text = "Single color"
        '
        'ckShowMarkers
        '
        Me.ckShowMarkers.AutoSize = True
        Me.ckShowMarkers.Checked = True
        Me.ckShowMarkers.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowMarkers.Location = New System.Drawing.Point(309, 25)
        Me.ckShowMarkers.Name = "ckShowMarkers"
        Me.ckShowMarkers.Size = New System.Drawing.Size(114, 20)
        Me.ckShowMarkers.TabIndex = 29
        Me.ckShowMarkers.Text = "Show markers"
        Me.ckShowMarkers.UseVisualStyleBackColor = True
        '
        'cmbColorMode
        '
        Me.cmbColorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbColorMode.FormattingEnabled = True
        Me.cmbColorMode.Location = New System.Drawing.Point(94, 84)
        Me.cmbColorMode.Name = "cmbColorMode"
        Me.cmbColorMode.Size = New System.Drawing.Size(193, 24)
        Me.cmbColorMode.TabIndex = 18
        '
        'lblColorMode
        '
        Me.lblColorMode.AutoSize = True
        Me.lblColorMode.Location = New System.Drawing.Point(11, 87)
        Me.lblColorMode.Name = "lblColorMode"
        Me.lblColorMode.Size = New System.Drawing.Size(77, 16)
        Me.lblColorMode.TabIndex = 17
        Me.lblColorMode.Text = "Color mode"
        '
        'cmdMarkerStyle
        '
        Me.cmdMarkerStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmdMarkerStyle.FormattingEnabled = True
        Me.cmdMarkerStyle.Location = New System.Drawing.Point(94, 21)
        Me.cmdMarkerStyle.Name = "cmdMarkerStyle"
        Me.cmdMarkerStyle.Size = New System.Drawing.Size(193, 24)
        Me.cmdMarkerStyle.TabIndex = 16
        '
        'nudMarkerSize
        '
        Me.nudMarkerSize.Location = New System.Drawing.Point(94, 51)
        Me.nudMarkerSize.Maximum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.nudMarkerSize.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudMarkerSize.Name = "nudMarkerSize"
        Me.nudMarkerSize.Size = New System.Drawing.Size(74, 22)
        Me.nudMarkerSize.TabIndex = 11
        Me.nudMarkerSize.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'lblMarkerSize
        '
        Me.lblMarkerSize.AutoSize = True
        Me.lblMarkerSize.Location = New System.Drawing.Point(12, 53)
        Me.lblMarkerSize.Name = "lblMarkerSize"
        Me.lblMarkerSize.Size = New System.Drawing.Size(76, 16)
        Me.lblMarkerSize.TabIndex = 10
        Me.lblMarkerSize.Text = "Marker size"
        '
        'lblMarkerStyle
        '
        Me.lblMarkerStyle.AutoSize = True
        Me.lblMarkerStyle.Location = New System.Drawing.Point(8, 24)
        Me.lblMarkerStyle.Name = "lblMarkerStyle"
        Me.lblMarkerStyle.Size = New System.Drawing.Size(80, 16)
        Me.lblMarkerStyle.TabIndex = 6
        Me.lblMarkerStyle.Text = "Marker style"
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(293, 451)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 18
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(374, 451)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 17
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'Ui01LadderPlot
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(463, 478)
        Me.Controls.Add(Me.TabMultipage)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(481, 525)
        Me.Name = "Ui01LadderPlot"
        Me.ShowIcon = False
        Me.Text = "Ladder Plot"
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1_Input.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPage2_Options.ResumeLayout(False)
        Me.grpJitter.ResumeLayout(False)
        Me.grpJitter.PerformLayout()
        CType(Me.nudHorizontalJitter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpAxisChart.ResumeLayout(False)
        Me.grpAxisChart.PerformLayout()
        CType(Me.nudChartHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudChartWidth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpTitlesSeries.ResumeLayout(False)
        Me.grpTitlesSeries.PerformLayout()
        Me.TabPage3_Appearance.ResumeLayout(False)
        Me.grpConnector.ResumeLayout(False)
        Me.grpConnector.PerformLayout()
        CType(Me.nudConnectorWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudConnectorTransparency, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpMarkers.ResumeLayout(False)
        Me.grpMarkers.PerformLayout()
        CType(Me.nudMarkerSize, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1_Input As Windows.Forms.TabPage
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents RefEdit_SecondValues As Excel2007RefEdit
    Friend WithEvents lblSecondValues As Windows.Forms.Label
    Friend WithEvents RefEdit_RecordLabels As Excel2007RefEdit
    Friend WithEvents lblRecordLabels As Windows.Forms.Label
    Friend WithEvents RefEdit_FirstValues As Excel2007RefEdit
    Friend WithEvents lblFirstValues As Windows.Forms.Label
    Friend WithEvents RefEdit_GroupingVariable As Excel2007RefEdit
    Friend WithEvents lblGroupingVariable As Windows.Forms.Label
    Friend WithEvents TabPage2_Options As Windows.Forms.TabPage
    Friend WithEvents grpAxisChart As Windows.Forms.GroupBox
    Friend WithEvents ckShowHorizontalGridlines As Windows.Forms.CheckBox
    Friend WithEvents ckShowLegend As Windows.Forms.CheckBox
    Friend WithEvents nudChartHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartHeight As Windows.Forms.Label
    Friend WithEvents nudChartWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartWidth As Windows.Forms.Label
    Friend WithEvents txtYAxisNumberFormat As Windows.Forms.TextBox
    Friend WithEvents lblYAxisNumberFormat As Windows.Forms.Label
    Friend WithEvents txtYAxisMajorUnit As Windows.Forms.TextBox
    Friend WithEvents lblYAxisMajorUnit As Windows.Forms.Label
    Friend WithEvents txtYAxisMaximum As Windows.Forms.TextBox
    Friend WithEvents lblYAxisMaximum As Windows.Forms.Label
    Friend WithEvents txtYAxisMinimum As Windows.Forms.TextBox
    Friend WithEvents lblYAxisMinimum As Windows.Forms.Label
    Friend WithEvents grpTitlesSeries As Windows.Forms.GroupBox
    Friend WithEvents txtYAxisTitle As Windows.Forms.TextBox
    Friend WithEvents lblYAxisTitle As Windows.Forms.Label
    Friend WithEvents TabPage3_Appearance As Windows.Forms.TabPage
    Friend WithEvents grpConnector As Windows.Forms.GroupBox
    Friend WithEvents cmbConnectorLineStyle As Windows.Forms.ComboBox
    Friend WithEvents nudConnectorWidth As Windows.Forms.NumericUpDown
    Friend WithEvents pnlConnectorColor As Windows.Forms.Panel
    Friend WithEvents btnConnectorColor As Windows.Forms.Button
    Friend WithEvents nudConnectorTransparency As Windows.Forms.NumericUpDown
    Friend WithEvents lblTransparency As Windows.Forms.Label
    Friend WithEvents lblLineStyle As Windows.Forms.Label
    Friend WithEvents lblLineWidth As Windows.Forms.Label
    Friend WithEvents lblConnectorColor As Windows.Forms.Label
    Friend WithEvents grpMarkers As Windows.Forms.GroupBox
    Friend WithEvents cmdMarkerStyle As Windows.Forms.ComboBox
    Friend WithEvents nudMarkerSize As Windows.Forms.NumericUpDown
    Friend WithEvents lblMarkerSize As Windows.Forms.Label
    Friend WithEvents lblMarkerStyle As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents ckOmitIncompleteRows As Windows.Forms.CheckBox
    Friend WithEvents ckShowLines As Windows.Forms.CheckBox
    Friend WithEvents ckShowMarkers As Windows.Forms.CheckBox
    Friend WithEvents cmbColorMode As Windows.Forms.ComboBox
    Friend WithEvents lblColorMode As Windows.Forms.Label
    Friend WithEvents pnlSingleColor As Windows.Forms.Panel
    Friend WithEvents btnSingleColor As Windows.Forms.Button
    Friend WithEvents lblSingleColor As Windows.Forms.Label
    Friend WithEvents grpJitter As Windows.Forms.GroupBox
    Friend WithEvents nudHorizontalJitter As Windows.Forms.NumericUpDown
    Friend WithEvents ckHorizontalJitter As Windows.Forms.CheckBox
    Friend WithEvents ckShowSideLabels As Windows.Forms.CheckBox
    Friend WithEvents ckColorEndpointLabelsBySeries As Windows.Forms.CheckBox
    Friend WithEvents ckAvoidEndpointLabelOverlap As Windows.Forms.CheckBox
    Friend WithEvents txtEndpointValueFormat As Windows.Forms.TextBox
    Friend WithEvents lblEndpointValueFormat As Windows.Forms.Label
    Friend WithEvents lblEndpointLabelContent As Windows.Forms.Label
    Friend WithEvents ckShowSecondEndpointLabels As Windows.Forms.CheckBox
    Friend WithEvents ckShowFirstEndpointLabels As Windows.Forms.CheckBox
    Friend WithEvents ckYAxisIncludeZero As Windows.Forms.CheckBox
    Friend WithEvents cmbEndpointLabelContent As Windows.Forms.ComboBox
End Class
