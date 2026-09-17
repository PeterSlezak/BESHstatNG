<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui01DumbbellPlot
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui01DumbbellPlot))
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1_Input = New System.Windows.Forms.TabPage()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.RefEdit_SecondValues = New BESHStatNG.Excel2007RefEdit()
        Me.lblSecondValues = New System.Windows.Forms.Label()
        Me.RefEdit_AdditionalObservationCat = New BESHStatNG.Excel2007RefEdit()
        Me.lblAdditionalObservationCat = New System.Windows.Forms.Label()
        Me.lblAdditionalObsLabel = New System.Windows.Forms.Label()
        Me.RefEdit_AdditionalObservationValues = New BESHStatNG.Excel2007RefEdit()
        Me.lblAdditionalObservationValues = New System.Windows.Forms.Label()
        Me.RefEdit_FirstValues = New BESHStatNG.Excel2007RefEdit()
        Me.lblFirstValues = New System.Windows.Forms.Label()
        Me.RefEdit_CategoryLabels = New BESHStatNG.Excel2007RefEdit()
        Me.lblCategoryLabels = New System.Windows.Forms.Label()
        Me.TabPage2_Options = New System.Windows.Forms.TabPage()
        Me.grpAxisChart = New System.Windows.Forms.GroupBox()
        Me.ckShowHorizontalGridlines = New System.Windows.Forms.CheckBox()
        Me.ckShowVerticalGridlines = New System.Windows.Forms.CheckBox()
        Me.ckShowEndpointValueLabels = New System.Windows.Forms.CheckBox()
        Me.ckShowLegend = New System.Windows.Forms.CheckBox()
        Me.nudChartHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblChartHeight = New System.Windows.Forms.Label()
        Me.nudChartWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblChartWidth = New System.Windows.Forms.Label()
        Me.txtXAxisNumberFormat = New System.Windows.Forms.TextBox()
        Me.lblXAxisNumberFormat = New System.Windows.Forms.Label()
        Me.txtXAxisMajorUnit = New System.Windows.Forms.TextBox()
        Me.lblXAxisMajorUnit = New System.Windows.Forms.Label()
        Me.txtXAxisMaximum = New System.Windows.Forms.TextBox()
        Me.lblXAxisMaximum = New System.Windows.Forms.Label()
        Me.txtXAxisMinimum = New System.Windows.Forms.TextBox()
        Me.lblXAxisMinimum = New System.Windows.Forms.Label()
        Me.grpTitlesSeries = New System.Windows.Forms.GroupBox()
        Me.txtAdditionalSeriesName = New System.Windows.Forms.TextBox()
        Me.lblAdditionalSeriesName = New System.Windows.Forms.Label()
        Me.txtSecondSeriesName = New System.Windows.Forms.TextBox()
        Me.lblSecondSeriesName = New System.Windows.Forms.Label()
        Me.txtFirstSeriesName = New System.Windows.Forms.TextBox()
        Me.lblFirstSeriesName = New System.Windows.Forms.Label()
        Me.txtXAxisTitle = New System.Windows.Forms.TextBox()
        Me.lblXAxisTitle = New System.Windows.Forms.Label()
        Me.txtChartTitle = New System.Windows.Forms.TextBox()
        Me.lblChartTitle = New System.Windows.Forms.Label()
        Me.grpOrdering = New System.Windows.Forms.GroupBox()
        Me.ckOmitIncompleteRows = New System.Windows.Forms.CheckBox()
        Me.ckReverseCategoryOrder = New System.Windows.Forms.CheckBox()
        Me.cmbSortMode = New System.Windows.Forms.ComboBox()
        Me.lblSortMode = New System.Windows.Forms.Label()
        Me.TabPage3_Appearance = New System.Windows.Forms.TabPage()
        Me.grpConnector = New System.Windows.Forms.GroupBox()
        Me.cmbConnectorLineStyle = New System.Windows.Forms.ComboBox()
        Me.nudConnectorWidth = New System.Windows.Forms.NumericUpDown()
        Me.pnlConnectorColor = New System.Windows.Forms.Panel()
        Me.btnConnectorColor = New System.Windows.Forms.Button()
        Me.nudConnectorTransparency = New System.Windows.Forms.NumericUpDown()
        Me.lblTransparency = New System.Windows.Forms.Label()
        Me.lblLineStyle = New System.Windows.Forms.Label()
        Me.lblLineWidth = New System.Windows.Forms.Label()
        Me.lblConnectorColor = New System.Windows.Forms.Label()
        Me.grpAdditionalObs = New System.Windows.Forms.GroupBox()
        Me.pnlAddObsColor = New System.Windows.Forms.Panel()
        Me.btnAddObsColor = New System.Windows.Forms.Button()
        Me.nudAddObsJitter = New System.Windows.Forms.NumericUpDown()
        Me.lblAddObsJitter = New System.Windows.Forms.Label()
        Me.nudAddObsTransparency = New System.Windows.Forms.NumericUpDown()
        Me.nudAddObsMarkerSize = New System.Windows.Forms.NumericUpDown()
        Me.lblAddObsTransparency = New System.Windows.Forms.Label()
        Me.lblAddObsMarkerSize = New System.Windows.Forms.Label()
        Me.lblAddObsColor = New System.Windows.Forms.Label()
        Me.grpMarkers = New System.Windows.Forms.GroupBox()
        Me.cmdMarkerStyle = New System.Windows.Forms.ComboBox()
        Me.pnlSecondMarkerColor = New System.Windows.Forms.Panel()
        Me.btnSecondMarkerColor = New System.Windows.Forms.Button()
        Me.pnlFirstMarkerColor = New System.Windows.Forms.Panel()
        Me.btnFirstMarkerColor = New System.Windows.Forms.Button()
        Me.nudMarkerSize = New System.Windows.Forms.NumericUpDown()
        Me.lblMarkerSize = New System.Windows.Forms.Label()
        Me.lblMarkerStyle = New System.Windows.Forms.Label()
        Me.lblSecondMarkerColor = New System.Windows.Forms.Label()
        Me.lblFirstMarkerColor = New System.Windows.Forms.Label()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1_Input.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPage2_Options.SuspendLayout()
        Me.grpAxisChart.SuspendLayout()
        CType(Me.nudChartHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudChartWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpTitlesSeries.SuspendLayout()
        Me.grpOrdering.SuspendLayout()
        Me.TabPage3_Appearance.SuspendLayout()
        Me.grpConnector.SuspendLayout()
        CType(Me.nudConnectorWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudConnectorTransparency, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpAdditionalObs.SuspendLayout()
        CType(Me.nudAddObsJitter, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudAddObsTransparency, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudAddObsMarkerSize, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpMarkers.SuspendLayout()
        CType(Me.nudMarkerSize, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1_Input)
        Me.TabMultipage.Controls.Add(Me.TabPage2_Options)
        Me.TabMultipage.Controls.Add(Me.TabPage3_Appearance)
        Me.TabMultipage.Location = New System.Drawing.Point(2, 0)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(456, 447)
        Me.TabMultipage.TabIndex = 16
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
        Me.grpInput.Controls.Add(Me.RefEdit_SecondValues)
        Me.grpInput.Controls.Add(Me.lblSecondValues)
        Me.grpInput.Controls.Add(Me.RefEdit_AdditionalObservationCat)
        Me.grpInput.Controls.Add(Me.lblAdditionalObservationCat)
        Me.grpInput.Controls.Add(Me.lblAdditionalObsLabel)
        Me.grpInput.Controls.Add(Me.RefEdit_AdditionalObservationValues)
        Me.grpInput.Controls.Add(Me.lblAdditionalObservationValues)
        Me.grpInput.Controls.Add(Me.RefEdit_FirstValues)
        Me.grpInput.Controls.Add(Me.lblFirstValues)
        Me.grpInput.Controls.Add(Me.RefEdit_CategoryLabels)
        Me.grpInput.Controls.Add(Me.lblCategoryLabels)
        Me.grpInput.Location = New System.Drawing.Point(3, 3)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(436, 276)
        Me.grpInput.TabIndex = 3
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'RefEdit_SecondValues
        '
        Me.RefEdit_SecondValues.Address = ""
        Me.RefEdit_SecondValues.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_SecondValues.ExcelConnector = Nothing
        Me.RefEdit_SecondValues.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_SecondValues.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_SecondValues.Location = New System.Drawing.Point(146, 102)
        Me.RefEdit_SecondValues.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_SecondValues.Name = "RefEdit_SecondValues"
        Me.RefEdit_SecondValues.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_SecondValues.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_SecondValues.TabIndex = 15
        '
        'lblSecondValues
        '
        Me.lblSecondValues.AutoSize = True
        Me.lblSecondValues.Location = New System.Drawing.Point(11, 102)
        Me.lblSecondValues.Name = "lblSecondValues"
        Me.lblSecondValues.Size = New System.Drawing.Size(100, 16)
        Me.lblSecondValues.TabIndex = 14
        Me.lblSecondValues.Text = "Second values:"
        '
        'RefEdit_AdditionalObservationCat
        '
        Me.RefEdit_AdditionalObservationCat.Address = ""
        Me.RefEdit_AdditionalObservationCat.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_AdditionalObservationCat.ExcelConnector = Nothing
        Me.RefEdit_AdditionalObservationCat.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_AdditionalObservationCat.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_AdditionalObservationCat.Location = New System.Drawing.Point(146, 232)
        Me.RefEdit_AdditionalObservationCat.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_AdditionalObservationCat.Name = "RefEdit_AdditionalObservationCat"
        Me.RefEdit_AdditionalObservationCat.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_AdditionalObservationCat.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_AdditionalObservationCat.TabIndex = 13
        '
        'lblAdditionalObservationCat
        '
        Me.lblAdditionalObservationCat.Location = New System.Drawing.Point(11, 232)
        Me.lblAdditionalObservationCat.Name = "lblAdditionalObservationCat"
        Me.lblAdditionalObservationCat.Size = New System.Drawing.Size(137, 32)
        Me.lblAdditionalObservationCat.TabIndex = 12
        Me.lblAdditionalObservationCat.Text = "Observation categories (optional):"
        '
        'lblAdditionalObsLabel
        '
        Me.lblAdditionalObsLabel.AutoSize = True
        Me.lblAdditionalObsLabel.Location = New System.Drawing.Point(143, 159)
        Me.lblAdditionalObsLabel.Name = "lblAdditionalObsLabel"
        Me.lblAdditionalObsLabel.Size = New System.Drawing.Size(148, 16)
        Me.lblAdditionalObsLabel.TabIndex = 11
        Me.lblAdditionalObsLabel.Text = "Additional observations"
        '
        'RefEdit_AdditionalObservationValues
        '
        Me.RefEdit_AdditionalObservationValues.Address = ""
        Me.RefEdit_AdditionalObservationValues.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_AdditionalObservationValues.ExcelConnector = Nothing
        Me.RefEdit_AdditionalObservationValues.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_AdditionalObservationValues.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_AdditionalObservationValues.Location = New System.Drawing.Point(146, 192)
        Me.RefEdit_AdditionalObservationValues.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_AdditionalObservationValues.Name = "RefEdit_AdditionalObservationValues"
        Me.RefEdit_AdditionalObservationValues.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_AdditionalObservationValues.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_AdditionalObservationValues.TabIndex = 10
        '
        'lblAdditionalObservationValues
        '
        Me.lblAdditionalObservationValues.Location = New System.Drawing.Point(11, 192)
        Me.lblAdditionalObservationValues.Name = "lblAdditionalObservationValues"
        Me.lblAdditionalObservationValues.Size = New System.Drawing.Size(137, 32)
        Me.lblAdditionalObservationValues.TabIndex = 9
        Me.lblAdditionalObservationValues.Text = "Observation values (optional):"
        '
        'RefEdit_FirstValues
        '
        Me.RefEdit_FirstValues.Address = ""
        Me.RefEdit_FirstValues.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_FirstValues.ExcelConnector = Nothing
        Me.RefEdit_FirstValues.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_FirstValues.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_FirstValues.Location = New System.Drawing.Point(146, 62)
        Me.RefEdit_FirstValues.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_FirstValues.Name = "RefEdit_FirstValues"
        Me.RefEdit_FirstValues.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_FirstValues.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_FirstValues.TabIndex = 8
        '
        'lblFirstValues
        '
        Me.lblFirstValues.AutoSize = True
        Me.lblFirstValues.Location = New System.Drawing.Point(11, 62)
        Me.lblFirstValues.Name = "lblFirstValues"
        Me.lblFirstValues.Size = New System.Drawing.Size(78, 16)
        Me.lblFirstValues.TabIndex = 7
        Me.lblFirstValues.Text = "First values:"
        '
        'RefEdit_CategoryLabels
        '
        Me.RefEdit_CategoryLabels.Address = ""
        Me.RefEdit_CategoryLabels.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_CategoryLabels.ExcelConnector = Nothing
        Me.RefEdit_CategoryLabels.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_CategoryLabels.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_CategoryLabels.Location = New System.Drawing.Point(146, 22)
        Me.RefEdit_CategoryLabels.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_CategoryLabels.Name = "RefEdit_CategoryLabels"
        Me.RefEdit_CategoryLabels.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_CategoryLabels.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_CategoryLabels.TabIndex = 6
        '
        'lblCategoryLabels
        '
        Me.lblCategoryLabels.AutoSize = True
        Me.lblCategoryLabels.Location = New System.Drawing.Point(11, 22)
        Me.lblCategoryLabels.Name = "lblCategoryLabels"
        Me.lblCategoryLabels.Size = New System.Drawing.Size(105, 16)
        Me.lblCategoryLabels.TabIndex = 5
        Me.lblCategoryLabels.Text = "Category labels:"
        '
        'TabPage2_Options
        '
        Me.TabPage2_Options.Controls.Add(Me.grpAxisChart)
        Me.TabPage2_Options.Controls.Add(Me.grpTitlesSeries)
        Me.TabPage2_Options.Controls.Add(Me.grpOrdering)
        Me.TabPage2_Options.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2_Options.Name = "TabPage2_Options"
        Me.TabPage2_Options.Size = New System.Drawing.Size(448, 418)
        Me.TabPage2_Options.TabIndex = 3
        Me.TabPage2_Options.Text = "Options"
        Me.TabPage2_Options.UseVisualStyleBackColor = True
        '
        'grpAxisChart
        '
        Me.grpAxisChart.Controls.Add(Me.ckShowHorizontalGridlines)
        Me.grpAxisChart.Controls.Add(Me.ckShowVerticalGridlines)
        Me.grpAxisChart.Controls.Add(Me.ckShowEndpointValueLabels)
        Me.grpAxisChart.Controls.Add(Me.ckShowLegend)
        Me.grpAxisChart.Controls.Add(Me.nudChartHeight)
        Me.grpAxisChart.Controls.Add(Me.lblChartHeight)
        Me.grpAxisChart.Controls.Add(Me.nudChartWidth)
        Me.grpAxisChart.Controls.Add(Me.lblChartWidth)
        Me.grpAxisChart.Controls.Add(Me.txtXAxisNumberFormat)
        Me.grpAxisChart.Controls.Add(Me.lblXAxisNumberFormat)
        Me.grpAxisChart.Controls.Add(Me.txtXAxisMajorUnit)
        Me.grpAxisChart.Controls.Add(Me.lblXAxisMajorUnit)
        Me.grpAxisChart.Controls.Add(Me.txtXAxisMaximum)
        Me.grpAxisChart.Controls.Add(Me.lblXAxisMaximum)
        Me.grpAxisChart.Controls.Add(Me.txtXAxisMinimum)
        Me.grpAxisChart.Controls.Add(Me.lblXAxisMinimum)
        Me.grpAxisChart.Location = New System.Drawing.Point(3, 255)
        Me.grpAxisChart.Name = "grpAxisChart"
        Me.grpAxisChart.Size = New System.Drawing.Size(439, 160)
        Me.grpAxisChart.TabIndex = 26
        Me.grpAxisChart.TabStop = False
        Me.grpAxisChart.Text = "Axis and chart"
        '
        'ckShowHorizontalGridlines
        '
        Me.ckShowHorizontalGridlines.AutoSize = True
        Me.ckShowHorizontalGridlines.Checked = True
        Me.ckShowHorizontalGridlines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowHorizontalGridlines.Location = New System.Drawing.Point(227, 131)
        Me.ckShowHorizontalGridlines.Name = "ckShowHorizontalGridlines"
        Me.ckShowHorizontalGridlines.Size = New System.Drawing.Size(176, 20)
        Me.ckShowHorizontalGridlines.TabIndex = 31
        Me.ckShowHorizontalGridlines.Text = "Show horizontal gridlines"
        Me.ckShowHorizontalGridlines.UseVisualStyleBackColor = True
        '
        'ckShowVerticalGridlines
        '
        Me.ckShowVerticalGridlines.AutoSize = True
        Me.ckShowVerticalGridlines.Checked = True
        Me.ckShowVerticalGridlines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowVerticalGridlines.Location = New System.Drawing.Point(11, 131)
        Me.ckShowVerticalGridlines.Name = "ckShowVerticalGridlines"
        Me.ckShowVerticalGridlines.Size = New System.Drawing.Size(162, 20)
        Me.ckShowVerticalGridlines.TabIndex = 30
        Me.ckShowVerticalGridlines.Text = "Show vertical gridlines"
        Me.ckShowVerticalGridlines.UseVisualStyleBackColor = True
        '
        'ckShowEndpointValueLabels
        '
        Me.ckShowEndpointValueLabels.AutoSize = True
        Me.ckShowEndpointValueLabels.Location = New System.Drawing.Point(227, 105)
        Me.ckShowEndpointValueLabels.Name = "ckShowEndpointValueLabels"
        Me.ckShowEndpointValueLabels.Size = New System.Drawing.Size(193, 20)
        Me.ckShowEndpointValueLabels.TabIndex = 29
        Me.ckShowEndpointValueLabels.Text = "Show endpoint value labels"
        Me.ckShowEndpointValueLabels.UseVisualStyleBackColor = True
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
        'txtXAxisNumberFormat
        '
        Me.txtXAxisNumberFormat.Location = New System.Drawing.Point(323, 52)
        Me.txtXAxisNumberFormat.Name = "txtXAxisNumberFormat"
        Me.txtXAxisNumberFormat.Size = New System.Drawing.Size(104, 22)
        Me.txtXAxisNumberFormat.TabIndex = 23
        '
        'lblXAxisNumberFormat
        '
        Me.lblXAxisNumberFormat.AutoSize = True
        Me.lblXAxisNumberFormat.Location = New System.Drawing.Point(224, 55)
        Me.lblXAxisNumberFormat.Name = "lblXAxisNumberFormat"
        Me.lblXAxisNumberFormat.Size = New System.Drawing.Size(98, 16)
        Me.lblXAxisNumberFormat.TabIndex = 22
        Me.lblXAxisNumberFormat.Text = "Number format:"
        '
        'txtXAxisMajorUnit
        '
        Me.txtXAxisMajorUnit.Location = New System.Drawing.Point(105, 49)
        Me.txtXAxisMajorUnit.Name = "txtXAxisMajorUnit"
        Me.txtXAxisMajorUnit.Size = New System.Drawing.Size(104, 22)
        Me.txtXAxisMajorUnit.TabIndex = 21
        '
        'lblXAxisMajorUnit
        '
        Me.lblXAxisMajorUnit.AutoSize = True
        Me.lblXAxisMajorUnit.Location = New System.Drawing.Point(6, 52)
        Me.lblXAxisMajorUnit.Name = "lblXAxisMajorUnit"
        Me.lblXAxisMajorUnit.Size = New System.Drawing.Size(90, 16)
        Me.lblXAxisMajorUnit.TabIndex = 20
        Me.lblXAxisMajorUnit.Text = "Major interval:"
        '
        'txtXAxisMaximum
        '
        Me.txtXAxisMaximum.Location = New System.Drawing.Point(323, 21)
        Me.txtXAxisMaximum.Name = "txtXAxisMaximum"
        Me.txtXAxisMaximum.Size = New System.Drawing.Size(104, 22)
        Me.txtXAxisMaximum.TabIndex = 19
        '
        'lblXAxisMaximum
        '
        Me.lblXAxisMaximum.AutoSize = True
        Me.lblXAxisMaximum.Location = New System.Drawing.Point(226, 24)
        Me.lblXAxisMaximum.Name = "lblXAxisMaximum"
        Me.lblXAxisMaximum.Size = New System.Drawing.Size(78, 16)
        Me.lblXAxisMaximum.TabIndex = 18
        Me.lblXAxisMaximum.Text = "X maximum:"
        '
        'txtXAxisMinimum
        '
        Me.txtXAxisMinimum.Location = New System.Drawing.Point(105, 21)
        Me.txtXAxisMinimum.Name = "txtXAxisMinimum"
        Me.txtXAxisMinimum.Size = New System.Drawing.Size(104, 22)
        Me.txtXAxisMinimum.TabIndex = 17
        '
        'lblXAxisMinimum
        '
        Me.lblXAxisMinimum.AutoSize = True
        Me.lblXAxisMinimum.Location = New System.Drawing.Point(6, 24)
        Me.lblXAxisMinimum.Name = "lblXAxisMinimum"
        Me.lblXAxisMinimum.Size = New System.Drawing.Size(74, 16)
        Me.lblXAxisMinimum.TabIndex = 2
        Me.lblXAxisMinimum.Text = "X minimum:"
        '
        'grpTitlesSeries
        '
        Me.grpTitlesSeries.Controls.Add(Me.txtAdditionalSeriesName)
        Me.grpTitlesSeries.Controls.Add(Me.lblAdditionalSeriesName)
        Me.grpTitlesSeries.Controls.Add(Me.txtSecondSeriesName)
        Me.grpTitlesSeries.Controls.Add(Me.lblSecondSeriesName)
        Me.grpTitlesSeries.Controls.Add(Me.txtFirstSeriesName)
        Me.grpTitlesSeries.Controls.Add(Me.lblFirstSeriesName)
        Me.grpTitlesSeries.Controls.Add(Me.txtXAxisTitle)
        Me.grpTitlesSeries.Controls.Add(Me.lblXAxisTitle)
        Me.grpTitlesSeries.Controls.Add(Me.txtChartTitle)
        Me.grpTitlesSeries.Controls.Add(Me.lblChartTitle)
        Me.grpTitlesSeries.Location = New System.Drawing.Point(3, 88)
        Me.grpTitlesSeries.Name = "grpTitlesSeries"
        Me.grpTitlesSeries.Size = New System.Drawing.Size(439, 161)
        Me.grpTitlesSeries.TabIndex = 15
        Me.grpTitlesSeries.TabStop = False
        Me.grpTitlesSeries.Text = "Titles and series"
        '
        'txtAdditionalSeriesName
        '
        Me.txtAdditionalSeriesName.Location = New System.Drawing.Point(144, 133)
        Me.txtAdditionalSeriesName.Name = "txtAdditionalSeriesName"
        Me.txtAdditionalSeriesName.Size = New System.Drawing.Size(283, 22)
        Me.txtAdditionalSeriesName.TabIndex = 25
        Me.txtAdditionalSeriesName.Text = "Observations"
        '
        'lblAdditionalSeriesName
        '
        Me.lblAdditionalSeriesName.AutoSize = True
        Me.lblAdditionalSeriesName.Location = New System.Drawing.Point(6, 136)
        Me.lblAdditionalSeriesName.Name = "lblAdditionalSeriesName"
        Me.lblAdditionalSeriesName.Size = New System.Drawing.Size(123, 16)
        Me.lblAdditionalSeriesName.TabIndex = 24
        Me.lblAdditionalSeriesName.Text = "Observation series:"
        '
        'txtSecondSeriesName
        '
        Me.txtSecondSeriesName.Location = New System.Drawing.Point(144, 105)
        Me.txtSecondSeriesName.Name = "txtSecondSeriesName"
        Me.txtSecondSeriesName.Size = New System.Drawing.Size(283, 22)
        Me.txtSecondSeriesName.TabIndex = 23
        Me.txtSecondSeriesName.Text = "Second"
        '
        'lblSecondSeriesName
        '
        Me.lblSecondSeriesName.AutoSize = True
        Me.lblSecondSeriesName.Location = New System.Drawing.Point(6, 108)
        Me.lblSecondSeriesName.Name = "lblSecondSeriesName"
        Me.lblSecondSeriesName.Size = New System.Drawing.Size(134, 16)
        Me.lblSecondSeriesName.TabIndex = 22
        Me.lblSecondSeriesName.Text = "Second series name:"
        '
        'txtFirstSeriesName
        '
        Me.txtFirstSeriesName.Location = New System.Drawing.Point(144, 77)
        Me.txtFirstSeriesName.Name = "txtFirstSeriesName"
        Me.txtFirstSeriesName.Size = New System.Drawing.Size(283, 22)
        Me.txtFirstSeriesName.TabIndex = 21
        Me.txtFirstSeriesName.Text = "First"
        '
        'lblFirstSeriesName
        '
        Me.lblFirstSeriesName.AutoSize = True
        Me.lblFirstSeriesName.Location = New System.Drawing.Point(6, 80)
        Me.lblFirstSeriesName.Name = "lblFirstSeriesName"
        Me.lblFirstSeriesName.Size = New System.Drawing.Size(112, 16)
        Me.lblFirstSeriesName.TabIndex = 20
        Me.lblFirstSeriesName.Text = "First series name:"
        '
        'txtXAxisTitle
        '
        Me.txtXAxisTitle.Location = New System.Drawing.Point(144, 49)
        Me.txtXAxisTitle.Name = "txtXAxisTitle"
        Me.txtXAxisTitle.Size = New System.Drawing.Size(283, 22)
        Me.txtXAxisTitle.TabIndex = 19
        '
        'lblXAxisTitle
        '
        Me.lblXAxisTitle.AutoSize = True
        Me.lblXAxisTitle.Location = New System.Drawing.Point(6, 52)
        Me.lblXAxisTitle.Name = "lblXAxisTitle"
        Me.lblXAxisTitle.Size = New System.Drawing.Size(69, 16)
        Me.lblXAxisTitle.TabIndex = 18
        Me.lblXAxisTitle.Text = "X-axis title:"
        '
        'txtChartTitle
        '
        Me.txtChartTitle.Location = New System.Drawing.Point(144, 21)
        Me.txtChartTitle.Name = "txtChartTitle"
        Me.txtChartTitle.Size = New System.Drawing.Size(283, 22)
        Me.txtChartTitle.TabIndex = 17
        Me.txtChartTitle.Text = "Dumbbell plot"
        '
        'lblChartTitle
        '
        Me.lblChartTitle.AutoSize = True
        Me.lblChartTitle.Location = New System.Drawing.Point(6, 24)
        Me.lblChartTitle.Name = "lblChartTitle"
        Me.lblChartTitle.Size = New System.Drawing.Size(64, 16)
        Me.lblChartTitle.TabIndex = 2
        Me.lblChartTitle.Text = "Chart title:"
        '
        'grpOrdering
        '
        Me.grpOrdering.Controls.Add(Me.ckOmitIncompleteRows)
        Me.grpOrdering.Controls.Add(Me.ckReverseCategoryOrder)
        Me.grpOrdering.Controls.Add(Me.cmbSortMode)
        Me.grpOrdering.Controls.Add(Me.lblSortMode)
        Me.grpOrdering.Location = New System.Drawing.Point(3, 3)
        Me.grpOrdering.Name = "grpOrdering"
        Me.grpOrdering.Size = New System.Drawing.Size(439, 79)
        Me.grpOrdering.TabIndex = 1
        Me.grpOrdering.TabStop = False
        Me.grpOrdering.Text = "Ordering"
        '
        'ckOmitIncompleteRows
        '
        Me.ckOmitIncompleteRows.AutoSize = True
        Me.ckOmitIncompleteRows.Location = New System.Drawing.Point(227, 51)
        Me.ckOmitIncompleteRows.Name = "ckOmitIncompleteRows"
        Me.ckOmitIncompleteRows.Size = New System.Drawing.Size(156, 20)
        Me.ckOmitIncompleteRows.TabIndex = 14
        Me.ckOmitIncompleteRows.Text = "Omit incomplete rows"
        Me.ckOmitIncompleteRows.UseVisualStyleBackColor = True
        '
        'ckReverseCategoryOrder
        '
        Me.ckReverseCategoryOrder.AutoSize = True
        Me.ckReverseCategoryOrder.Location = New System.Drawing.Point(24, 51)
        Me.ckReverseCategoryOrder.Name = "ckReverseCategoryOrder"
        Me.ckReverseCategoryOrder.Size = New System.Drawing.Size(172, 20)
        Me.ckReverseCategoryOrder.TabIndex = 13
        Me.ckReverseCategoryOrder.Text = "Reverse category order"
        Me.ckReverseCategoryOrder.UseVisualStyleBackColor = True
        '
        'cmbSortMode
        '
        Me.cmbSortMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbSortMode.FormattingEnabled = True
        Me.cmbSortMode.Location = New System.Drawing.Point(144, 21)
        Me.cmbSortMode.Name = "cmbSortMode"
        Me.cmbSortMode.Size = New System.Drawing.Size(283, 24)
        Me.cmbSortMode.TabIndex = 3
        '
        'lblSortMode
        '
        Me.lblSortMode.AutoSize = True
        Me.lblSortMode.Location = New System.Drawing.Point(6, 24)
        Me.lblSortMode.Name = "lblSortMode"
        Me.lblSortMode.Size = New System.Drawing.Size(101, 16)
        Me.lblSortMode.TabIndex = 2
        Me.lblSortMode.Text = "Sort categories:"
        '
        'TabPage3_Appearance
        '
        Me.TabPage3_Appearance.Controls.Add(Me.grpConnector)
        Me.TabPage3_Appearance.Controls.Add(Me.grpAdditionalObs)
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
        Me.grpConnector.Controls.Add(Me.cmbConnectorLineStyle)
        Me.grpConnector.Controls.Add(Me.nudConnectorWidth)
        Me.grpConnector.Controls.Add(Me.pnlConnectorColor)
        Me.grpConnector.Controls.Add(Me.btnConnectorColor)
        Me.grpConnector.Controls.Add(Me.nudConnectorTransparency)
        Me.grpConnector.Controls.Add(Me.lblTransparency)
        Me.grpConnector.Controls.Add(Me.lblLineStyle)
        Me.grpConnector.Controls.Add(Me.lblLineWidth)
        Me.grpConnector.Controls.Add(Me.lblConnectorColor)
        Me.grpConnector.Location = New System.Drawing.Point(5, 123)
        Me.grpConnector.Name = "grpConnector"
        Me.grpConnector.Size = New System.Drawing.Size(439, 113)
        Me.grpConnector.TabIndex = 14
        Me.grpConnector.TabStop = False
        Me.grpConnector.Text = "Connector"
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
        Me.lblConnectorColor.Location = New System.Drawing.Point(6, 24)
        Me.lblConnectorColor.Name = "lblConnectorColor"
        Me.lblConnectorColor.Size = New System.Drawing.Size(39, 16)
        Me.lblConnectorColor.TabIndex = 2
        Me.lblConnectorColor.Text = "Color"
        '
        'grpAdditionalObs
        '
        Me.grpAdditionalObs.Controls.Add(Me.pnlAddObsColor)
        Me.grpAdditionalObs.Controls.Add(Me.btnAddObsColor)
        Me.grpAdditionalObs.Controls.Add(Me.nudAddObsJitter)
        Me.grpAdditionalObs.Controls.Add(Me.lblAddObsJitter)
        Me.grpAdditionalObs.Controls.Add(Me.nudAddObsTransparency)
        Me.grpAdditionalObs.Controls.Add(Me.nudAddObsMarkerSize)
        Me.grpAdditionalObs.Controls.Add(Me.lblAddObsTransparency)
        Me.grpAdditionalObs.Controls.Add(Me.lblAddObsMarkerSize)
        Me.grpAdditionalObs.Controls.Add(Me.lblAddObsColor)
        Me.grpAdditionalObs.Location = New System.Drawing.Point(6, 242)
        Me.grpAdditionalObs.Name = "grpAdditionalObs"
        Me.grpAdditionalObs.Size = New System.Drawing.Size(439, 116)
        Me.grpAdditionalObs.TabIndex = 13
        Me.grpAdditionalObs.TabStop = False
        Me.grpAdditionalObs.Text = "Additional observations"
        '
        'pnlAddObsColor
        '
        Me.pnlAddObsColor.Location = New System.Drawing.Point(164, 21)
        Me.pnlAddObsColor.Name = "pnlAddObsColor"
        Me.pnlAddObsColor.Size = New System.Drawing.Size(54, 24)
        Me.pnlAddObsColor.TabIndex = 23
        '
        'btnAddObsColor
        '
        Me.btnAddObsColor.Location = New System.Drawing.Point(83, 21)
        Me.btnAddObsColor.Name = "btnAddObsColor"
        Me.btnAddObsColor.Size = New System.Drawing.Size(75, 23)
        Me.btnAddObsColor.TabIndex = 22
        Me.btnAddObsColor.Text = "Select..."
        Me.btnAddObsColor.UseVisualStyleBackColor = True
        '
        'nudAddObsJitter
        '
        Me.nudAddObsJitter.DecimalPlaces = 2
        Me.nudAddObsJitter.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        Me.nudAddObsJitter.Location = New System.Drawing.Point(141, 76)
        Me.nudAddObsJitter.Maximum = New Decimal(New Integer() {45, 0, 0, 131072})
        Me.nudAddObsJitter.Name = "nudAddObsJitter"
        Me.nudAddObsJitter.Size = New System.Drawing.Size(74, 22)
        Me.nudAddObsJitter.TabIndex = 17
        Me.nudAddObsJitter.Value = New Decimal(New Integer() {15, 0, 0, 131072})
        '
        'lblAddObsJitter
        '
        Me.lblAddObsJitter.AutoSize = True
        Me.lblAddObsJitter.Location = New System.Drawing.Point(6, 78)
        Me.lblAddObsJitter.Name = "lblAddObsJitter"
        Me.lblAddObsJitter.Size = New System.Drawing.Size(79, 16)
        Me.lblAddObsJitter.TabIndex = 16
        Me.lblAddObsJitter.Text = "Vertical jitter"
        '
        'nudAddObsTransparency
        '
        Me.nudAddObsTransparency.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudAddObsTransparency.Location = New System.Drawing.Point(362, 48)
        Me.nudAddObsTransparency.Name = "nudAddObsTransparency"
        Me.nudAddObsTransparency.Size = New System.Drawing.Size(74, 22)
        Me.nudAddObsTransparency.TabIndex = 15
        Me.nudAddObsTransparency.Value = New Decimal(New Integer() {45, 0, 0, 0})
        '
        'nudAddObsMarkerSize
        '
        Me.nudAddObsMarkerSize.Location = New System.Drawing.Point(141, 48)
        Me.nudAddObsMarkerSize.Maximum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.nudAddObsMarkerSize.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudAddObsMarkerSize.Name = "nudAddObsMarkerSize"
        Me.nudAddObsMarkerSize.Size = New System.Drawing.Size(74, 22)
        Me.nudAddObsMarkerSize.TabIndex = 13
        Me.nudAddObsMarkerSize.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'lblAddObsTransparency
        '
        Me.lblAddObsTransparency.AutoSize = True
        Me.lblAddObsTransparency.Location = New System.Drawing.Point(221, 50)
        Me.lblAddObsTransparency.Name = "lblAddObsTransparency"
        Me.lblAddObsTransparency.Size = New System.Drawing.Size(117, 16)
        Me.lblAddObsTransparency.TabIndex = 12
        Me.lblAddObsTransparency.Text = "Transparency (%):"
        '
        'lblAddObsMarkerSize
        '
        Me.lblAddObsMarkerSize.AutoSize = True
        Me.lblAddObsMarkerSize.Location = New System.Drawing.Point(6, 54)
        Me.lblAddObsMarkerSize.Name = "lblAddObsMarkerSize"
        Me.lblAddObsMarkerSize.Size = New System.Drawing.Size(76, 16)
        Me.lblAddObsMarkerSize.TabIndex = 5
        Me.lblAddObsMarkerSize.Text = "Marker size"
        '
        'lblAddObsColor
        '
        Me.lblAddObsColor.AutoSize = True
        Me.lblAddObsColor.Location = New System.Drawing.Point(6, 24)
        Me.lblAddObsColor.Name = "lblAddObsColor"
        Me.lblAddObsColor.Size = New System.Drawing.Size(39, 16)
        Me.lblAddObsColor.TabIndex = 3
        Me.lblAddObsColor.Text = "Color"
        '
        'grpMarkers
        '
        Me.grpMarkers.Controls.Add(Me.cmdMarkerStyle)
        Me.grpMarkers.Controls.Add(Me.pnlSecondMarkerColor)
        Me.grpMarkers.Controls.Add(Me.btnSecondMarkerColor)
        Me.grpMarkers.Controls.Add(Me.pnlFirstMarkerColor)
        Me.grpMarkers.Controls.Add(Me.btnFirstMarkerColor)
        Me.grpMarkers.Controls.Add(Me.nudMarkerSize)
        Me.grpMarkers.Controls.Add(Me.lblMarkerSize)
        Me.grpMarkers.Controls.Add(Me.lblMarkerStyle)
        Me.grpMarkers.Controls.Add(Me.lblSecondMarkerColor)
        Me.grpMarkers.Controls.Add(Me.lblFirstMarkerColor)
        Me.grpMarkers.Location = New System.Drawing.Point(5, 3)
        Me.grpMarkers.Name = "grpMarkers"
        Me.grpMarkers.Size = New System.Drawing.Size(439, 114)
        Me.grpMarkers.TabIndex = 0
        Me.grpMarkers.TabStop = False
        Me.grpMarkers.Text = "Markers"
        '
        'cmdMarkerStyle
        '
        Me.cmdMarkerStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmdMarkerStyle.FormattingEnabled = True
        Me.cmdMarkerStyle.Location = New System.Drawing.Point(83, 80)
        Me.cmdMarkerStyle.Name = "cmdMarkerStyle"
        Me.cmdMarkerStyle.Size = New System.Drawing.Size(193, 24)
        Me.cmdMarkerStyle.TabIndex = 16
        '
        'pnlSecondMarkerColor
        '
        Me.pnlSecondMarkerColor.Location = New System.Drawing.Point(222, 47)
        Me.pnlSecondMarkerColor.Name = "pnlSecondMarkerColor"
        Me.pnlSecondMarkerColor.Size = New System.Drawing.Size(54, 24)
        Me.pnlSecondMarkerColor.TabIndex = 15
        '
        'btnSecondMarkerColor
        '
        Me.btnSecondMarkerColor.Location = New System.Drawing.Point(141, 46)
        Me.btnSecondMarkerColor.Name = "btnSecondMarkerColor"
        Me.btnSecondMarkerColor.Size = New System.Drawing.Size(75, 23)
        Me.btnSecondMarkerColor.TabIndex = 14
        Me.btnSecondMarkerColor.Text = "Select..."
        Me.btnSecondMarkerColor.UseVisualStyleBackColor = True
        '
        'pnlFirstMarkerColor
        '
        Me.pnlFirstMarkerColor.Location = New System.Drawing.Point(222, 17)
        Me.pnlFirstMarkerColor.Name = "pnlFirstMarkerColor"
        Me.pnlFirstMarkerColor.Size = New System.Drawing.Size(54, 24)
        Me.pnlFirstMarkerColor.TabIndex = 13
        '
        'btnFirstMarkerColor
        '
        Me.btnFirstMarkerColor.Location = New System.Drawing.Point(141, 17)
        Me.btnFirstMarkerColor.Name = "btnFirstMarkerColor"
        Me.btnFirstMarkerColor.Size = New System.Drawing.Size(75, 23)
        Me.btnFirstMarkerColor.TabIndex = 12
        Me.btnFirstMarkerColor.Text = "Select..."
        Me.btnFirstMarkerColor.UseVisualStyleBackColor = True
        '
        'nudMarkerSize
        '
        Me.nudMarkerSize.Location = New System.Drawing.Point(359, 83)
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
        Me.lblMarkerSize.Location = New System.Drawing.Point(277, 85)
        Me.lblMarkerSize.Name = "lblMarkerSize"
        Me.lblMarkerSize.Size = New System.Drawing.Size(76, 16)
        Me.lblMarkerSize.TabIndex = 10
        Me.lblMarkerSize.Text = "Marker size"
        '
        'lblMarkerStyle
        '
        Me.lblMarkerStyle.AutoSize = True
        Me.lblMarkerStyle.Location = New System.Drawing.Point(6, 83)
        Me.lblMarkerStyle.Name = "lblMarkerStyle"
        Me.lblMarkerStyle.Size = New System.Drawing.Size(80, 16)
        Me.lblMarkerStyle.TabIndex = 6
        Me.lblMarkerStyle.Text = "Marker style"
        '
        'lblSecondMarkerColor
        '
        Me.lblSecondMarkerColor.AutoSize = True
        Me.lblSecondMarkerColor.Location = New System.Drawing.Point(6, 54)
        Me.lblSecondMarkerColor.Name = "lblSecondMarkerColor"
        Me.lblSecondMarkerColor.Size = New System.Drawing.Size(132, 16)
        Me.lblSecondMarkerColor.TabIndex = 4
        Me.lblSecondMarkerColor.Text = "Second marker color"
        '
        'lblFirstMarkerColor
        '
        Me.lblFirstMarkerColor.AutoSize = True
        Me.lblFirstMarkerColor.Location = New System.Drawing.Point(6, 24)
        Me.lblFirstMarkerColor.Name = "lblFirstMarkerColor"
        Me.lblFirstMarkerColor.Size = New System.Drawing.Size(110, 16)
        Me.lblFirstMarkerColor.TabIndex = 2
        Me.lblFirstMarkerColor.Text = "First marker color"
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(302, 449)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 15
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(383, 449)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 14
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'Ui01DumbbellPlot
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(460, 478)
        Me.Controls.Add(Me.TabMultipage)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(478, 525)
        Me.Name = "Ui01DumbbellPlot"
        Me.ShowIcon = False
        Me.Text = "Dumbbell Plot"
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1_Input.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPage2_Options.ResumeLayout(False)
        Me.grpAxisChart.ResumeLayout(False)
        Me.grpAxisChart.PerformLayout()
        CType(Me.nudChartHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudChartWidth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpTitlesSeries.ResumeLayout(False)
        Me.grpTitlesSeries.PerformLayout()
        Me.grpOrdering.ResumeLayout(False)
        Me.grpOrdering.PerformLayout()
        Me.TabPage3_Appearance.ResumeLayout(False)
        Me.grpConnector.ResumeLayout(False)
        Me.grpConnector.PerformLayout()
        CType(Me.nudConnectorWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudConnectorTransparency, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpAdditionalObs.ResumeLayout(False)
        Me.grpAdditionalObs.PerformLayout()
        CType(Me.nudAddObsJitter, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudAddObsTransparency, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudAddObsMarkerSize, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpMarkers.ResumeLayout(False)
        Me.grpMarkers.PerformLayout()
        CType(Me.nudMarkerSize, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage3_Appearance As Windows.Forms.TabPage
    Friend WithEvents grpAdditionalObs As Windows.Forms.GroupBox
    Friend WithEvents lblAddObsTransparency As Windows.Forms.Label
    Friend WithEvents lblAddObsMarkerSize As Windows.Forms.Label
    Friend WithEvents lblAddObsColor As Windows.Forms.Label
    Friend WithEvents grpMarkers As Windows.Forms.GroupBox
    Friend WithEvents lblMarkerSize As Windows.Forms.Label
    Friend WithEvents lblMarkerStyle As Windows.Forms.Label
    Friend WithEvents lblSecondMarkerColor As Windows.Forms.Label
    Friend WithEvents lblFirstMarkerColor As Windows.Forms.Label
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents TabPage1_Input As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents RefEdit_FirstValues As Excel2007RefEdit
    Friend WithEvents lblFirstValues As Windows.Forms.Label
    Friend WithEvents RefEdit_CategoryLabels As Excel2007RefEdit
    Friend WithEvents lblCategoryLabels As Windows.Forms.Label
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents RefEdit_AdditionalObservationCat As Excel2007RefEdit
    Friend WithEvents lblAdditionalObservationCat As Windows.Forms.Label
    Friend WithEvents lblAdditionalObsLabel As Windows.Forms.Label
    Friend WithEvents RefEdit_AdditionalObservationValues As Excel2007RefEdit
    Friend WithEvents lblAdditionalObservationValues As Windows.Forms.Label
    Friend WithEvents grpConnector As Windows.Forms.GroupBox
    Friend WithEvents lblTransparency As Windows.Forms.Label
    Friend WithEvents lblLineStyle As Windows.Forms.Label
    Friend WithEvents lblLineWidth As Windows.Forms.Label
    Friend WithEvents lblConnectorColor As Windows.Forms.Label
    Friend WithEvents nudConnectorTransparency As Windows.Forms.NumericUpDown
    Friend WithEvents nudMarkerSize As Windows.Forms.NumericUpDown
    Friend WithEvents nudAddObsMarkerSize As Windows.Forms.NumericUpDown
    Friend WithEvents nudAddObsTransparency As Windows.Forms.NumericUpDown
    Friend WithEvents nudAddObsJitter As Windows.Forms.NumericUpDown
    Friend WithEvents lblAddObsJitter As Windows.Forms.Label
    Friend WithEvents RefEdit_SecondValues As Excel2007RefEdit
    Friend WithEvents lblSecondValues As Windows.Forms.Label
    Friend WithEvents TabPage2_Options As Windows.Forms.TabPage
    Friend WithEvents grpOrdering As Windows.Forms.GroupBox
    Friend WithEvents cmbSortMode As Windows.Forms.ComboBox
    Friend WithEvents lblSortMode As Windows.Forms.Label
    Friend WithEvents ckOmitIncompleteRows As Windows.Forms.CheckBox
    Friend WithEvents ckReverseCategoryOrder As Windows.Forms.CheckBox
    Friend WithEvents grpTitlesSeries As Windows.Forms.GroupBox
    Friend WithEvents lblChartTitle As Windows.Forms.Label
    Friend WithEvents txtChartTitle As Windows.Forms.TextBox
    Friend WithEvents txtFirstSeriesName As Windows.Forms.TextBox
    Friend WithEvents lblFirstSeriesName As Windows.Forms.Label
    Friend WithEvents txtXAxisTitle As Windows.Forms.TextBox
    Friend WithEvents lblXAxisTitle As Windows.Forms.Label
    Friend WithEvents txtAdditionalSeriesName As Windows.Forms.TextBox
    Friend WithEvents lblAdditionalSeriesName As Windows.Forms.Label
    Friend WithEvents txtSecondSeriesName As Windows.Forms.TextBox
    Friend WithEvents lblSecondSeriesName As Windows.Forms.Label
    Friend WithEvents grpAxisChart As Windows.Forms.GroupBox
    Friend WithEvents txtXAxisNumberFormat As Windows.Forms.TextBox
    Friend WithEvents lblXAxisNumberFormat As Windows.Forms.Label
    Friend WithEvents txtXAxisMajorUnit As Windows.Forms.TextBox
    Friend WithEvents lblXAxisMajorUnit As Windows.Forms.Label
    Friend WithEvents txtXAxisMaximum As Windows.Forms.TextBox
    Friend WithEvents lblXAxisMaximum As Windows.Forms.Label
    Friend WithEvents txtXAxisMinimum As Windows.Forms.TextBox
    Friend WithEvents lblXAxisMinimum As Windows.Forms.Label
    Friend WithEvents pnlSecondMarkerColor As Windows.Forms.Panel
    Friend WithEvents btnSecondMarkerColor As Windows.Forms.Button
    Friend WithEvents pnlFirstMarkerColor As Windows.Forms.Panel
    Friend WithEvents btnFirstMarkerColor As Windows.Forms.Button
    Friend WithEvents cmdMarkerStyle As Windows.Forms.ComboBox
    Friend WithEvents nudConnectorWidth As Windows.Forms.NumericUpDown
    Friend WithEvents pnlConnectorColor As Windows.Forms.Panel
    Friend WithEvents btnConnectorColor As Windows.Forms.Button
    Friend WithEvents cmbConnectorLineStyle As Windows.Forms.ComboBox
    Friend WithEvents pnlAddObsColor As Windows.Forms.Panel
    Friend WithEvents btnAddObsColor As Windows.Forms.Button
    Friend WithEvents nudChartWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartWidth As Windows.Forms.Label
    Friend WithEvents nudChartHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartHeight As Windows.Forms.Label
    Friend WithEvents ckShowVerticalGridlines As Windows.Forms.CheckBox
    Friend WithEvents ckShowEndpointValueLabels As Windows.Forms.CheckBox
    Friend WithEvents ckShowLegend As Windows.Forms.CheckBox
    Friend WithEvents ckShowHorizontalGridlines As Windows.Forms.CheckBox
End Class
