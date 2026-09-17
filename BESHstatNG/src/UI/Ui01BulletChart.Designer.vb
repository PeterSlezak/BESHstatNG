<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Ui01BulletChart
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui01BulletChart))
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1_Input = New System.Windows.Forms.TabPage()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.lblSubtitles = New System.Windows.Forms.Label()
        Me.lblActualValues = New System.Windows.Forms.Label()
        Me.lblMeasureLabels = New System.Windows.Forms.Label()
        Me.lblTargetValues = New System.Windows.Forms.Label()
        Me.TabPage2_Options = New System.Windows.Forms.TabPage()
        Me.grpDataHandling = New System.Windows.Forms.GroupBox()
        Me.ckOmitIncompleteRows = New System.Windows.Forms.CheckBox()
        Me.grpLabels = New System.Windows.Forms.GroupBox()
        Me.ckShowSubtitles = New System.Windows.Forms.CheckBox()
        Me.ckShowScaleLabels = New System.Windows.Forms.CheckBox()
        Me.ckShowMeasureLabels = New System.Windows.Forms.CheckBox()
        Me.txtValueNumberFormat = New System.Windows.Forms.TextBox()
        Me.lblValueNumberFormat = New System.Windows.Forms.Label()
        Me.txtScaleNumberFormat = New System.Windows.Forms.TextBox()
        Me.lblScaleNumberFormat = New System.Windows.Forms.Label()
        Me.grpScale = New System.Windows.Forms.GroupBox()
        Me.cmbDefaultDirection = New System.Windows.Forms.ComboBox()
        Me.lblDefaultDirection = New System.Windows.Forms.Label()
        Me.lblScaleMaximums = New System.Windows.Forms.Label()
        Me.TabPage3_Appearance = New System.Windows.Forms.TabPage()
        Me.grpRanges = New System.Windows.Forms.GroupBox()
        Me.ckShowBandOutlines = New System.Windows.Forms.CheckBox()
        Me.grpActualValue = New System.Windows.Forms.GroupBox()
        Me.pnlActualBarColor = New System.Windows.Forms.Panel()
        Me.btnActualBarColor = New System.Windows.Forms.Button()
        Me.lblActualBarColor = New System.Windows.Forms.Label()
        Me.ckActualBarOutline = New System.Windows.Forms.CheckBox()
        Me.nudActualBarHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblActualBarHeight = New System.Windows.Forms.Label()
        Me.lblQualitativeRanges = New System.Windows.Forms.Label()
        Me.lblDirections = New System.Windows.Forms.Label()
        Me.lblRangeNote = New System.Windows.Forms.Label()
        Me.lblMajorIntervals = New System.Windows.Forms.Label()
        Me.nudTargetMajorTickCount = New System.Windows.Forms.NumericUpDown()
        Me.lblTargetMajorTickCount = New System.Windows.Forms.Label()
        Me.ckShowScaleTickMarks = New System.Windows.Forms.CheckBox()
        Me.ckShowTargetValueLabels = New System.Windows.Forms.CheckBox()
        Me.ckShowActualValueLabels = New System.Windows.Forms.CheckBox()
        Me.grpChart = New System.Windows.Forms.GroupBox()
        Me.nudChartHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblChartHeight = New System.Windows.Forms.Label()
        Me.nudChartWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblChartWidth = New System.Windows.Forms.Label()
        Me.grpTarget = New System.Windows.Forms.GroupBox()
        Me.pnlTargetColor = New System.Windows.Forms.Panel()
        Me.btnTargetColor = New System.Windows.Forms.Button()
        Me.lblTargetColor = New System.Windows.Forms.Label()
        Me.nudTargetMarkerWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblTargetMarkerWidth = New System.Windows.Forms.Label()
        Me.nudTargetMarkerHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblTargetMarkerHeight = New System.Windows.Forms.Label()
        Me.cmbBandPalette = New System.Windows.Forms.ComboBox()
        Me.lblBandPalette = New System.Windows.Forms.Label()
        Me.RefEditOutput = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_Directions = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_QualitativeRanges = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_Subtitles = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_ActualValues = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_MeasureLabels = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_TargetValues = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_MajorIntervals = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_ScaleMaximums = New BESHStatNG.Excel2007RefEdit()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1_Input.SuspendLayout()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPage2_Options.SuspendLayout()
        Me.grpDataHandling.SuspendLayout()
        Me.grpLabels.SuspendLayout()
        Me.grpScale.SuspendLayout()
        Me.TabPage3_Appearance.SuspendLayout()
        Me.grpRanges.SuspendLayout()
        Me.grpActualValue.SuspendLayout()
        CType(Me.nudActualBarHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTargetMajorTickCount, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpChart.SuspendLayout()
        CType(Me.nudChartHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudChartWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpTarget.SuspendLayout()
        CType(Me.nudTargetMarkerWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTargetMarkerHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(290, 454)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 20
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(371, 454)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 19
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1_Input)
        Me.TabMultipage.Controls.Add(Me.TabPage2_Options)
        Me.TabMultipage.Controls.Add(Me.TabPage3_Appearance)
        Me.TabMultipage.Location = New System.Drawing.Point(3, 1)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(456, 447)
        Me.TabMultipage.TabIndex = 21
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
        Me.grpOutput.Location = New System.Drawing.Point(3, 302)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(442, 113)
        Me.grpOutput.TabIndex = 8
        Me.grpOutput.TabStop = False
        Me.grpOutput.Text = "Output"
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
        Me.grpInput.Controls.Add(Me.lblRangeNote)
        Me.grpInput.Controls.Add(Me.RefEdit_Directions)
        Me.grpInput.Controls.Add(Me.lblDirections)
        Me.grpInput.Controls.Add(Me.RefEdit_QualitativeRanges)
        Me.grpInput.Controls.Add(Me.lblQualitativeRanges)
        Me.grpInput.Controls.Add(Me.RefEdit_Subtitles)
        Me.grpInput.Controls.Add(Me.lblSubtitles)
        Me.grpInput.Controls.Add(Me.RefEdit_ActualValues)
        Me.grpInput.Controls.Add(Me.lblActualValues)
        Me.grpInput.Controls.Add(Me.RefEdit_MeasureLabels)
        Me.grpInput.Controls.Add(Me.lblMeasureLabels)
        Me.grpInput.Controls.Add(Me.RefEdit_TargetValues)
        Me.grpInput.Controls.Add(Me.lblTargetValues)
        Me.grpInput.Location = New System.Drawing.Point(3, 3)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(442, 293)
        Me.grpInput.TabIndex = 3
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'lblSubtitles
        '
        Me.lblSubtitles.Location = New System.Drawing.Point(11, 62)
        Me.lblSubtitles.Name = "lblSubtitles"
        Me.lblSubtitles.Size = New System.Drawing.Size(128, 32)
        Me.lblSubtitles.TabIndex = 14
        Me.lblSubtitles.Text = "Subtitle / unit (optional):"
        '
        'lblActualValues
        '
        Me.lblActualValues.AutoSize = True
        Me.lblActualValues.Location = New System.Drawing.Point(11, 102)
        Me.lblActualValues.Name = "lblActualValues"
        Me.lblActualValues.Size = New System.Drawing.Size(90, 16)
        Me.lblActualValues.TabIndex = 9
        Me.lblActualValues.Text = "Actual values:"
        '
        'lblMeasureLabels
        '
        Me.lblMeasureLabels.AutoSize = True
        Me.lblMeasureLabels.Location = New System.Drawing.Point(11, 22)
        Me.lblMeasureLabels.Name = "lblMeasureLabels"
        Me.lblMeasureLabels.Size = New System.Drawing.Size(103, 16)
        Me.lblMeasureLabels.TabIndex = 7
        Me.lblMeasureLabels.Text = "Measure labels:"
        '
        'lblTargetValues
        '
        Me.lblTargetValues.AutoSize = True
        Me.lblTargetValues.Location = New System.Drawing.Point(14, 142)
        Me.lblTargetValues.Name = "lblTargetValues"
        Me.lblTargetValues.Size = New System.Drawing.Size(93, 16)
        Me.lblTargetValues.TabIndex = 5
        Me.lblTargetValues.Text = "Target values:"
        '
        'TabPage2_Options
        '
        Me.TabPage2_Options.Controls.Add(Me.grpDataHandling)
        Me.TabPage2_Options.Controls.Add(Me.grpLabels)
        Me.TabPage2_Options.Controls.Add(Me.grpScale)
        Me.TabPage2_Options.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2_Options.Name = "TabPage2_Options"
        Me.TabPage2_Options.Size = New System.Drawing.Size(448, 418)
        Me.TabPage2_Options.TabIndex = 3
        Me.TabPage2_Options.Text = "Options"
        Me.TabPage2_Options.UseVisualStyleBackColor = True
        '
        'grpDataHandling
        '
        Me.grpDataHandling.Controls.Add(Me.ckOmitIncompleteRows)
        Me.grpDataHandling.Location = New System.Drawing.Point(3, 181)
        Me.grpDataHandling.Name = "grpDataHandling"
        Me.grpDataHandling.Size = New System.Drawing.Size(439, 53)
        Me.grpDataHandling.TabIndex = 27
        Me.grpDataHandling.TabStop = False
        Me.grpDataHandling.Text = "Data handling"
        '
        'ckOmitIncompleteRows
        '
        Me.ckOmitIncompleteRows.AutoSize = True
        Me.ckOmitIncompleteRows.Location = New System.Drawing.Point(11, 21)
        Me.ckOmitIncompleteRows.Name = "ckOmitIncompleteRows"
        Me.ckOmitIncompleteRows.Size = New System.Drawing.Size(156, 20)
        Me.ckOmitIncompleteRows.TabIndex = 29
        Me.ckOmitIncompleteRows.Text = "Omit incomplete rows"
        Me.ckOmitIncompleteRows.UseVisualStyleBackColor = True
        '
        'grpLabels
        '
        Me.grpLabels.Controls.Add(Me.ckShowTargetValueLabels)
        Me.grpLabels.Controls.Add(Me.ckShowActualValueLabels)
        Me.grpLabels.Controls.Add(Me.ckShowScaleTickMarks)
        Me.grpLabels.Controls.Add(Me.ckShowSubtitles)
        Me.grpLabels.Controls.Add(Me.ckShowScaleLabels)
        Me.grpLabels.Controls.Add(Me.ckShowMeasureLabels)
        Me.grpLabels.Controls.Add(Me.txtValueNumberFormat)
        Me.grpLabels.Controls.Add(Me.lblValueNumberFormat)
        Me.grpLabels.Controls.Add(Me.txtScaleNumberFormat)
        Me.grpLabels.Controls.Add(Me.lblScaleNumberFormat)
        Me.grpLabels.Location = New System.Drawing.Point(3, 240)
        Me.grpLabels.Name = "grpLabels"
        Me.grpLabels.Size = New System.Drawing.Size(439, 175)
        Me.grpLabels.TabIndex = 26
        Me.grpLabels.TabStop = False
        Me.grpLabels.Text = "Labels"
        '
        'ckShowSubtitles
        '
        Me.ckShowSubtitles.AutoSize = True
        Me.ckShowSubtitles.Checked = True
        Me.ckShowSubtitles.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowSubtitles.Location = New System.Drawing.Point(222, 21)
        Me.ckShowSubtitles.Name = "ckShowSubtitles"
        Me.ckShowSubtitles.Size = New System.Drawing.Size(151, 20)
        Me.ckShowSubtitles.TabIndex = 32
        Me.ckShowSubtitles.Text = "Show subtitles / units"
        Me.ckShowSubtitles.UseVisualStyleBackColor = True
        '
        'ckShowScaleLabels
        '
        Me.ckShowScaleLabels.AutoSize = True
        Me.ckShowScaleLabels.Checked = True
        Me.ckShowScaleLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowScaleLabels.Location = New System.Drawing.Point(8, 47)
        Me.ckShowScaleLabels.Name = "ckShowScaleLabels"
        Me.ckShowScaleLabels.Size = New System.Drawing.Size(138, 20)
        Me.ckShowScaleLabels.TabIndex = 31
        Me.ckShowScaleLabels.Text = "Show scale labels"
        Me.ckShowScaleLabels.UseVisualStyleBackColor = True
        '
        'ckShowMeasureLabels
        '
        Me.ckShowMeasureLabels.AutoSize = True
        Me.ckShowMeasureLabels.Checked = True
        Me.ckShowMeasureLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowMeasureLabels.Location = New System.Drawing.Point(8, 21)
        Me.ckShowMeasureLabels.Name = "ckShowMeasureLabels"
        Me.ckShowMeasureLabels.Size = New System.Drawing.Size(158, 20)
        Me.ckShowMeasureLabels.TabIndex = 28
        Me.ckShowMeasureLabels.Text = "Show measure labels"
        Me.ckShowMeasureLabels.UseVisualStyleBackColor = True
        '
        'txtValueNumberFormat
        '
        Me.txtValueNumberFormat.Location = New System.Drawing.Point(157, 134)
        Me.txtValueNumberFormat.Name = "txtValueNumberFormat"
        Me.txtValueNumberFormat.Size = New System.Drawing.Size(216, 22)
        Me.txtValueNumberFormat.TabIndex = 23
        '
        'lblValueNumberFormat
        '
        Me.lblValueNumberFormat.AutoSize = True
        Me.lblValueNumberFormat.Location = New System.Drawing.Point(8, 137)
        Me.lblValueNumberFormat.Name = "lblValueNumberFormat"
        Me.lblValueNumberFormat.Size = New System.Drawing.Size(133, 16)
        Me.lblValueNumberFormat.TabIndex = 22
        Me.lblValueNumberFormat.Text = "Value number format:"
        '
        'txtScaleNumberFormat
        '
        Me.txtScaleNumberFormat.Location = New System.Drawing.Point(157, 106)
        Me.txtScaleNumberFormat.Name = "txtScaleNumberFormat"
        Me.txtScaleNumberFormat.Size = New System.Drawing.Size(216, 22)
        Me.txtScaleNumberFormat.TabIndex = 21
        '
        'lblScaleNumberFormat
        '
        Me.lblScaleNumberFormat.AutoSize = True
        Me.lblScaleNumberFormat.Location = New System.Drawing.Point(6, 109)
        Me.lblScaleNumberFormat.Name = "lblScaleNumberFormat"
        Me.lblScaleNumberFormat.Size = New System.Drawing.Size(133, 16)
        Me.lblScaleNumberFormat.TabIndex = 20
        Me.lblScaleNumberFormat.Text = "Scale number format:"
        '
        'grpScale
        '
        Me.grpScale.Controls.Add(Me.nudTargetMajorTickCount)
        Me.grpScale.Controls.Add(Me.lblTargetMajorTickCount)
        Me.grpScale.Controls.Add(Me.RefEdit_MajorIntervals)
        Me.grpScale.Controls.Add(Me.lblMajorIntervals)
        Me.grpScale.Controls.Add(Me.RefEdit_ScaleMaximums)
        Me.grpScale.Controls.Add(Me.cmbDefaultDirection)
        Me.grpScale.Controls.Add(Me.lblDefaultDirection)
        Me.grpScale.Controls.Add(Me.lblScaleMaximums)
        Me.grpScale.Location = New System.Drawing.Point(3, 3)
        Me.grpScale.Name = "grpScale"
        Me.grpScale.Size = New System.Drawing.Size(439, 173)
        Me.grpScale.TabIndex = 15
        Me.grpScale.TabStop = False
        Me.grpScale.Text = "Scale"
        '
        'cmbDefaultDirection
        '
        Me.cmbDefaultDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDefaultDirection.FormattingEnabled = True
        Me.cmbDefaultDirection.Location = New System.Drawing.Point(157, 107)
        Me.cmbDefaultDirection.Name = "cmbDefaultDirection"
        Me.cmbDefaultDirection.Size = New System.Drawing.Size(275, 24)
        Me.cmbDefaultDirection.TabIndex = 41
        '
        'lblDefaultDirection
        '
        Me.lblDefaultDirection.AutoSize = True
        Me.lblDefaultDirection.Location = New System.Drawing.Point(6, 110)
        Me.lblDefaultDirection.Name = "lblDefaultDirection"
        Me.lblDefaultDirection.Size = New System.Drawing.Size(106, 16)
        Me.lblDefaultDirection.TabIndex = 34
        Me.lblDefaultDirection.Text = "Default direction:"
        '
        'lblScaleMaximums
        '
        Me.lblScaleMaximums.Location = New System.Drawing.Point(6, 19)
        Me.lblScaleMaximums.Name = "lblScaleMaximums"
        Me.lblScaleMaximums.Size = New System.Drawing.Size(142, 32)
        Me.lblScaleMaximums.TabIndex = 18
        Me.lblScaleMaximums.Text = "Scale maximum (optional):"
        '
        'TabPage3_Appearance
        '
        Me.TabPage3_Appearance.Controls.Add(Me.grpTarget)
        Me.TabPage3_Appearance.Controls.Add(Me.grpChart)
        Me.TabPage3_Appearance.Controls.Add(Me.grpRanges)
        Me.TabPage3_Appearance.Controls.Add(Me.grpActualValue)
        Me.TabPage3_Appearance.Location = New System.Drawing.Point(4, 25)
        Me.TabPage3_Appearance.Name = "TabPage3_Appearance"
        Me.TabPage3_Appearance.Size = New System.Drawing.Size(448, 418)
        Me.TabPage3_Appearance.TabIndex = 1
        Me.TabPage3_Appearance.Text = "Appearance"
        Me.TabPage3_Appearance.UseVisualStyleBackColor = True
        '
        'grpRanges
        '
        Me.grpRanges.Controls.Add(Me.cmbBandPalette)
        Me.grpRanges.Controls.Add(Me.lblBandPalette)
        Me.grpRanges.Controls.Add(Me.ckShowBandOutlines)
        Me.grpRanges.Location = New System.Drawing.Point(3, 265)
        Me.grpRanges.Name = "grpRanges"
        Me.grpRanges.Size = New System.Drawing.Size(439, 98)
        Me.grpRanges.TabIndex = 14
        Me.grpRanges.TabStop = False
        Me.grpRanges.Text = "Ranges"
        '
        'ckShowBandOutlines
        '
        Me.ckShowBandOutlines.AutoSize = True
        Me.ckShowBandOutlines.Location = New System.Drawing.Point(11, 21)
        Me.ckShowBandOutlines.Name = "ckShowBandOutlines"
        Me.ckShowBandOutlines.Size = New System.Drawing.Size(149, 20)
        Me.ckShowBandOutlines.TabIndex = 30
        Me.ckShowBandOutlines.Text = "Show range outlines"
        Me.ckShowBandOutlines.UseVisualStyleBackColor = True
        '
        'grpActualValue
        '
        Me.grpActualValue.Controls.Add(Me.pnlActualBarColor)
        Me.grpActualValue.Controls.Add(Me.btnActualBarColor)
        Me.grpActualValue.Controls.Add(Me.lblActualBarColor)
        Me.grpActualValue.Controls.Add(Me.ckActualBarOutline)
        Me.grpActualValue.Controls.Add(Me.nudActualBarHeight)
        Me.grpActualValue.Controls.Add(Me.lblActualBarHeight)
        Me.grpActualValue.Location = New System.Drawing.Point(3, 63)
        Me.grpActualValue.Name = "grpActualValue"
        Me.grpActualValue.Size = New System.Drawing.Size(439, 95)
        Me.grpActualValue.TabIndex = 0
        Me.grpActualValue.TabStop = False
        Me.grpActualValue.Text = "Actual value"
        '
        'pnlActualBarColor
        '
        Me.pnlActualBarColor.Location = New System.Drawing.Point(224, 26)
        Me.pnlActualBarColor.Name = "pnlActualBarColor"
        Me.pnlActualBarColor.Size = New System.Drawing.Size(54, 24)
        Me.pnlActualBarColor.TabIndex = 32
        '
        'btnActualBarColor
        '
        Me.btnActualBarColor.Location = New System.Drawing.Point(143, 26)
        Me.btnActualBarColor.Name = "btnActualBarColor"
        Me.btnActualBarColor.Size = New System.Drawing.Size(75, 23)
        Me.btnActualBarColor.TabIndex = 31
        Me.btnActualBarColor.Text = "Select..."
        Me.btnActualBarColor.UseVisualStyleBackColor = True
        '
        'lblActualBarColor
        '
        Me.lblActualBarColor.AutoSize = True
        Me.lblActualBarColor.Location = New System.Drawing.Point(8, 29)
        Me.lblActualBarColor.Name = "lblActualBarColor"
        Me.lblActualBarColor.Size = New System.Drawing.Size(100, 16)
        Me.lblActualBarColor.TabIndex = 30
        Me.lblActualBarColor.Text = "Actual bar color"
        '
        'ckActualBarOutline
        '
        Me.ckActualBarOutline.AutoSize = True
        Me.ckActualBarOutline.Location = New System.Drawing.Point(241, 56)
        Me.ckActualBarOutline.Name = "ckActualBarOutline"
        Me.ckActualBarOutline.Size = New System.Drawing.Size(127, 20)
        Me.ckActualBarOutline.TabIndex = 29
        Me.ckActualBarOutline.Text = "Show bar outline"
        Me.ckActualBarOutline.UseVisualStyleBackColor = True
        '
        'nudActualBarHeight
        '
        Me.nudActualBarHeight.Location = New System.Drawing.Point(143, 55)
        Me.nudActualBarHeight.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudActualBarHeight.Name = "nudActualBarHeight"
        Me.nudActualBarHeight.Size = New System.Drawing.Size(74, 22)
        Me.nudActualBarHeight.TabIndex = 11
        Me.nudActualBarHeight.Value = New Decimal(New Integer() {36, 0, 0, 0})
        '
        'lblActualBarHeight
        '
        Me.lblActualBarHeight.AutoSize = True
        Me.lblActualBarHeight.Location = New System.Drawing.Point(8, 57)
        Me.lblActualBarHeight.Name = "lblActualBarHeight"
        Me.lblActualBarHeight.Size = New System.Drawing.Size(93, 16)
        Me.lblActualBarHeight.TabIndex = 10
        Me.lblActualBarHeight.Text = "Bar height (%):"
        '
        'lblQualitativeRanges
        '
        Me.lblQualitativeRanges.AutoSize = True
        Me.lblQualitativeRanges.Location = New System.Drawing.Point(14, 178)
        Me.lblQualitativeRanges.Name = "lblQualitativeRanges"
        Me.lblQualitativeRanges.Size = New System.Drawing.Size(118, 16)
        Me.lblQualitativeRanges.TabIndex = 16
        Me.lblQualitativeRanges.Text = "Qualitative ranges:"
        '
        'lblDirections
        '
        Me.lblDirections.AutoSize = True
        Me.lblDirections.Location = New System.Drawing.Point(14, 214)
        Me.lblDirections.Name = "lblDirections"
        Me.lblDirections.Size = New System.Drawing.Size(122, 16)
        Me.lblDirections.TabIndex = 18
        Me.lblDirections.Text = "Direction (optional):"
        '
        'lblRangeNote
        '
        Me.lblRangeNote.Location = New System.Drawing.Point(146, 250)
        Me.lblRangeNote.Name = "lblRangeNote"
        Me.lblRangeNote.Size = New System.Drawing.Size(283, 32)
        Me.lblRangeNote.TabIndex = 20
        Me.lblRangeNote.Text = "Select 2–5 columns containing cumulative upper boundaries."
        '
        'lblMajorIntervals
        '
        Me.lblMajorIntervals.Location = New System.Drawing.Point(6, 59)
        Me.lblMajorIntervals.Name = "lblMajorIntervals"
        Me.lblMajorIntervals.Size = New System.Drawing.Size(142, 32)
        Me.lblMajorIntervals.TabIndex = 43
        Me.lblMajorIntervals.Text = "Major interval (optional):"
        '
        'nudTargetMajorTickCount
        '
        Me.nudTargetMajorTickCount.Location = New System.Drawing.Point(157, 137)
        Me.nudTargetMajorTickCount.Maximum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.nudTargetMajorTickCount.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.nudTargetMajorTickCount.Name = "nudTargetMajorTickCount"
        Me.nudTargetMajorTickCount.Size = New System.Drawing.Size(74, 22)
        Me.nudTargetMajorTickCount.TabIndex = 46
        Me.nudTargetMajorTickCount.Value = New Decimal(New Integer() {6, 0, 0, 0})
        '
        'lblTargetMajorTickCount
        '
        Me.lblTargetMajorTickCount.AutoSize = True
        Me.lblTargetMajorTickCount.Location = New System.Drawing.Point(6, 139)
        Me.lblTargetMajorTickCount.Name = "lblTargetMajorTickCount"
        Me.lblTargetMajorTickCount.Size = New System.Drawing.Size(145, 16)
        Me.lblTargetMajorTickCount.TabIndex = 45
        Me.lblTargetMajorTickCount.Text = "Automatic tick intervals:"
        '
        'ckShowScaleTickMarks
        '
        Me.ckShowScaleTickMarks.AutoSize = True
        Me.ckShowScaleTickMarks.Location = New System.Drawing.Point(222, 47)
        Me.ckShowScaleTickMarks.Name = "ckShowScaleTickMarks"
        Me.ckShowScaleTickMarks.Size = New System.Drawing.Size(161, 20)
        Me.ckShowScaleTickMarks.TabIndex = 33
        Me.ckShowScaleTickMarks.Text = "Show scale tick marks"
        Me.ckShowScaleTickMarks.UseVisualStyleBackColor = True
        '
        'ckShowTargetValueLabels
        '
        Me.ckShowTargetValueLabels.AutoSize = True
        Me.ckShowTargetValueLabels.Location = New System.Drawing.Point(222, 73)
        Me.ckShowTargetValueLabels.Name = "ckShowTargetValueLabels"
        Me.ckShowTargetValueLabels.Size = New System.Drawing.Size(142, 20)
        Me.ckShowTargetValueLabels.TabIndex = 35
        Me.ckShowTargetValueLabels.Text = "Show target values"
        Me.ckShowTargetValueLabels.UseVisualStyleBackColor = True
        '
        'ckShowActualValueLabels
        '
        Me.ckShowActualValueLabels.AutoSize = True
        Me.ckShowActualValueLabels.Location = New System.Drawing.Point(8, 73)
        Me.ckShowActualValueLabels.Name = "ckShowActualValueLabels"
        Me.ckShowActualValueLabels.Size = New System.Drawing.Size(144, 20)
        Me.ckShowActualValueLabels.TabIndex = 34
        Me.ckShowActualValueLabels.Text = "Show actual values"
        Me.ckShowActualValueLabels.UseVisualStyleBackColor = True
        '
        'grpChart
        '
        Me.grpChart.Controls.Add(Me.nudChartHeight)
        Me.grpChart.Controls.Add(Me.lblChartHeight)
        Me.grpChart.Controls.Add(Me.nudChartWidth)
        Me.grpChart.Controls.Add(Me.lblChartWidth)
        Me.grpChart.Location = New System.Drawing.Point(3, 3)
        Me.grpChart.Name = "grpChart"
        Me.grpChart.Size = New System.Drawing.Size(439, 54)
        Me.grpChart.TabIndex = 15
        Me.grpChart.TabStop = False
        Me.grpChart.Text = "Chart"
        '
        'nudChartHeight
        '
        Me.nudChartHeight.Location = New System.Drawing.Point(265, 21)
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
        Me.lblChartHeight.Location = New System.Drawing.Point(200, 23)
        Me.lblChartHeight.Name = "lblChartHeight"
        Me.lblChartHeight.Size = New System.Drawing.Size(46, 16)
        Me.lblChartHeight.TabIndex = 12
        Me.lblChartHeight.Text = "Height"
        '
        'nudChartWidth
        '
        Me.nudChartWidth.Location = New System.Drawing.Point(73, 21)
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
        Me.lblChartWidth.Location = New System.Drawing.Point(8, 23)
        Me.lblChartWidth.Name = "lblChartWidth"
        Me.lblChartWidth.Size = New System.Drawing.Size(41, 16)
        Me.lblChartWidth.TabIndex = 10
        Me.lblChartWidth.Text = "Width"
        '
        'grpTarget
        '
        Me.grpTarget.Controls.Add(Me.nudTargetMarkerHeight)
        Me.grpTarget.Controls.Add(Me.lblTargetMarkerHeight)
        Me.grpTarget.Controls.Add(Me.pnlTargetColor)
        Me.grpTarget.Controls.Add(Me.btnTargetColor)
        Me.grpTarget.Controls.Add(Me.lblTargetColor)
        Me.grpTarget.Controls.Add(Me.nudTargetMarkerWidth)
        Me.grpTarget.Controls.Add(Me.lblTargetMarkerWidth)
        Me.grpTarget.Location = New System.Drawing.Point(3, 164)
        Me.grpTarget.Name = "grpTarget"
        Me.grpTarget.Size = New System.Drawing.Size(439, 95)
        Me.grpTarget.TabIndex = 33
        Me.grpTarget.TabStop = False
        Me.grpTarget.Text = "Target value"
        '
        'pnlTargetColor
        '
        Me.pnlTargetColor.Location = New System.Drawing.Point(224, 26)
        Me.pnlTargetColor.Name = "pnlTargetColor"
        Me.pnlTargetColor.Size = New System.Drawing.Size(54, 24)
        Me.pnlTargetColor.TabIndex = 32
        '
        'btnTargetColor
        '
        Me.btnTargetColor.Location = New System.Drawing.Point(143, 26)
        Me.btnTargetColor.Name = "btnTargetColor"
        Me.btnTargetColor.Size = New System.Drawing.Size(75, 23)
        Me.btnTargetColor.TabIndex = 31
        Me.btnTargetColor.Text = "Select..."
        Me.btnTargetColor.UseVisualStyleBackColor = True
        '
        'lblTargetColor
        '
        Me.lblTargetColor.AutoSize = True
        Me.lblTargetColor.Location = New System.Drawing.Point(8, 29)
        Me.lblTargetColor.Name = "lblTargetColor"
        Me.lblTargetColor.Size = New System.Drawing.Size(125, 16)
        Me.lblTargetColor.TabIndex = 30
        Me.lblTargetColor.Text = "Target marker color"
        '
        'nudTargetMarkerWidth
        '
        Me.nudTargetMarkerWidth.DecimalPlaces = 2
        Me.nudTargetMarkerWidth.Increment = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.nudTargetMarkerWidth.Location = New System.Drawing.Point(143, 55)
        Me.nudTargetMarkerWidth.Maximum = New Decimal(New Integer() {32, 0, 0, 0})
        Me.nudTargetMarkerWidth.Minimum = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.nudTargetMarkerWidth.Name = "nudTargetMarkerWidth"
        Me.nudTargetMarkerWidth.Size = New System.Drawing.Size(74, 22)
        Me.nudTargetMarkerWidth.TabIndex = 11
        Me.nudTargetMarkerWidth.Value = New Decimal(New Integer() {225, 0, 0, 131072})
        '
        'lblTargetMarkerWidth
        '
        Me.lblTargetMarkerWidth.AutoSize = True
        Me.lblTargetMarkerWidth.Location = New System.Drawing.Point(8, 57)
        Me.lblTargetMarkerWidth.Name = "lblTargetMarkerWidth"
        Me.lblTargetMarkerWidth.Size = New System.Drawing.Size(85, 16)
        Me.lblTargetMarkerWidth.TabIndex = 10
        Me.lblTargetMarkerWidth.Text = "Marker width:"
        '
        'nudTargetMarkerHeight
        '
        Me.nudTargetMarkerHeight.Location = New System.Drawing.Point(359, 55)
        Me.nudTargetMarkerHeight.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudTargetMarkerHeight.Name = "nudTargetMarkerHeight"
        Me.nudTargetMarkerHeight.Size = New System.Drawing.Size(74, 22)
        Me.nudTargetMarkerHeight.TabIndex = 34
        Me.nudTargetMarkerHeight.Value = New Decimal(New Integer() {78, 0, 0, 0})
        '
        'lblTargetMarkerHeight
        '
        Me.lblTargetMarkerHeight.AutoSize = True
        Me.lblTargetMarkerHeight.Location = New System.Drawing.Point(231, 57)
        Me.lblTargetMarkerHeight.Name = "lblTargetMarkerHeight"
        Me.lblTargetMarkerHeight.Size = New System.Drawing.Size(114, 16)
        Me.lblTargetMarkerHeight.TabIndex = 33
        Me.lblTargetMarkerHeight.Text = "Marker height (%):"
        '
        'cmbBandPalette
        '
        Me.cmbBandPalette.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBandPalette.FormattingEnabled = True
        Me.cmbBandPalette.Location = New System.Drawing.Point(159, 52)
        Me.cmbBandPalette.Name = "cmbBandPalette"
        Me.cmbBandPalette.Size = New System.Drawing.Size(275, 24)
        Me.cmbBandPalette.TabIndex = 43
        '
        'lblBandPalette
        '
        Me.lblBandPalette.AutoSize = True
        Me.lblBandPalette.Location = New System.Drawing.Point(8, 55)
        Me.lblBandPalette.Name = "lblBandPalette"
        Me.lblBandPalette.Size = New System.Drawing.Size(95, 16)
        Me.lblBandPalette.TabIndex = 42
        Me.lblBandPalette.Text = "Range palette:"
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
        'RefEdit_Directions
        '
        Me.RefEdit_Directions.Address = ""
        Me.RefEdit_Directions.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_Directions.ExcelConnector = Nothing
        Me.RefEdit_Directions.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_Directions.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_Directions.Location = New System.Drawing.Point(146, 214)
        Me.RefEdit_Directions.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_Directions.Name = "RefEdit_Directions"
        Me.RefEdit_Directions.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_Directions.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_Directions.TabIndex = 19
        '
        'RefEdit_QualitativeRanges
        '
        Me.RefEdit_QualitativeRanges.Address = ""
        Me.RefEdit_QualitativeRanges.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_QualitativeRanges.ExcelConnector = Nothing
        Me.RefEdit_QualitativeRanges.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_QualitativeRanges.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_QualitativeRanges.Location = New System.Drawing.Point(146, 178)
        Me.RefEdit_QualitativeRanges.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_QualitativeRanges.Name = "RefEdit_QualitativeRanges"
        Me.RefEdit_QualitativeRanges.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_QualitativeRanges.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_QualitativeRanges.TabIndex = 17
        '
        'RefEdit_Subtitles
        '
        Me.RefEdit_Subtitles.Address = ""
        Me.RefEdit_Subtitles.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_Subtitles.ExcelConnector = Nothing
        Me.RefEdit_Subtitles.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_Subtitles.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_Subtitles.Location = New System.Drawing.Point(146, 62)
        Me.RefEdit_Subtitles.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_Subtitles.Name = "RefEdit_Subtitles"
        Me.RefEdit_Subtitles.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_Subtitles.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_Subtitles.TabIndex = 15
        '
        'RefEdit_ActualValues
        '
        Me.RefEdit_ActualValues.Address = ""
        Me.RefEdit_ActualValues.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_ActualValues.ExcelConnector = Nothing
        Me.RefEdit_ActualValues.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_ActualValues.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_ActualValues.Location = New System.Drawing.Point(146, 102)
        Me.RefEdit_ActualValues.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_ActualValues.Name = "RefEdit_ActualValues"
        Me.RefEdit_ActualValues.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_ActualValues.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_ActualValues.TabIndex = 10
        '
        'RefEdit_MeasureLabels
        '
        Me.RefEdit_MeasureLabels.Address = ""
        Me.RefEdit_MeasureLabels.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_MeasureLabels.ExcelConnector = Nothing
        Me.RefEdit_MeasureLabels.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_MeasureLabels.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_MeasureLabels.Location = New System.Drawing.Point(146, 22)
        Me.RefEdit_MeasureLabels.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_MeasureLabels.Name = "RefEdit_MeasureLabels"
        Me.RefEdit_MeasureLabels.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_MeasureLabels.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_MeasureLabels.TabIndex = 8
        '
        'RefEdit_TargetValues
        '
        Me.RefEdit_TargetValues.Address = ""
        Me.RefEdit_TargetValues.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_TargetValues.ExcelConnector = Nothing
        Me.RefEdit_TargetValues.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_TargetValues.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_TargetValues.Location = New System.Drawing.Point(146, 142)
        Me.RefEdit_TargetValues.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_TargetValues.Name = "RefEdit_TargetValues"
        Me.RefEdit_TargetValues.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_TargetValues.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_TargetValues.TabIndex = 6
        '
        'RefEdit_MajorIntervals
        '
        Me.RefEdit_MajorIntervals.Address = ""
        Me.RefEdit_MajorIntervals.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_MajorIntervals.ExcelConnector = Nothing
        Me.RefEdit_MajorIntervals.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_MajorIntervals.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_MajorIntervals.Location = New System.Drawing.Point(149, 59)
        Me.RefEdit_MajorIntervals.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_MajorIntervals.Name = "RefEdit_MajorIntervals"
        Me.RefEdit_MajorIntervals.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_MajorIntervals.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_MajorIntervals.TabIndex = 44
        '
        'RefEdit_ScaleMaximums
        '
        Me.RefEdit_ScaleMaximums.Address = ""
        Me.RefEdit_ScaleMaximums.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_ScaleMaximums.ExcelConnector = Nothing
        Me.RefEdit_ScaleMaximums.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_ScaleMaximums.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_ScaleMaximums.Location = New System.Drawing.Point(149, 19)
        Me.RefEdit_ScaleMaximums.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_ScaleMaximums.Name = "RefEdit_ScaleMaximums"
        Me.RefEdit_ScaleMaximums.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_ScaleMaximums.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_ScaleMaximums.TabIndex = 42
        '
        'Ui01BulletChart
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(459, 484)
        Me.Controls.Add(Me.TabMultipage)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(477, 531)
        Me.Name = "Ui01BulletChart"
        Me.ShowIcon = False
        Me.Text = "Bullet Chart"
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1_Input.ResumeLayout(False)
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPage2_Options.ResumeLayout(False)
        Me.grpDataHandling.ResumeLayout(False)
        Me.grpDataHandling.PerformLayout()
        Me.grpLabels.ResumeLayout(False)
        Me.grpLabels.PerformLayout()
        Me.grpScale.ResumeLayout(False)
        Me.grpScale.PerformLayout()
        Me.TabPage3_Appearance.ResumeLayout(False)
        Me.grpRanges.ResumeLayout(False)
        Me.grpRanges.PerformLayout()
        Me.grpActualValue.ResumeLayout(False)
        Me.grpActualValue.PerformLayout()
        CType(Me.nudActualBarHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTargetMajorTickCount, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpChart.ResumeLayout(False)
        Me.grpChart.PerformLayout()
        CType(Me.nudChartHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudChartWidth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpTarget.ResumeLayout(False)
        Me.grpTarget.PerformLayout()
        CType(Me.nudTargetMarkerWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTargetMarkerHeight, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1_Input As Windows.Forms.TabPage
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents RefEdit_Subtitles As Excel2007RefEdit
    Friend WithEvents lblSubtitles As Windows.Forms.Label
    Friend WithEvents RefEdit_ActualValues As Excel2007RefEdit
    Friend WithEvents lblActualValues As Windows.Forms.Label
    Friend WithEvents RefEdit_MeasureLabels As Excel2007RefEdit
    Friend WithEvents lblMeasureLabels As Windows.Forms.Label
    Friend WithEvents RefEdit_TargetValues As Excel2007RefEdit
    Friend WithEvents lblTargetValues As Windows.Forms.Label
    Friend WithEvents TabPage2_Options As Windows.Forms.TabPage
    Friend WithEvents grpDataHandling As Windows.Forms.GroupBox
    Friend WithEvents ckOmitIncompleteRows As Windows.Forms.CheckBox
    Friend WithEvents grpLabels As Windows.Forms.GroupBox
    Friend WithEvents ckShowSubtitles As Windows.Forms.CheckBox
    Friend WithEvents ckShowScaleLabels As Windows.Forms.CheckBox
    Friend WithEvents ckShowMeasureLabels As Windows.Forms.CheckBox
    Friend WithEvents txtValueNumberFormat As Windows.Forms.TextBox
    Friend WithEvents lblValueNumberFormat As Windows.Forms.Label
    Friend WithEvents txtScaleNumberFormat As Windows.Forms.TextBox
    Friend WithEvents lblScaleNumberFormat As Windows.Forms.Label
    Friend WithEvents grpScale As Windows.Forms.GroupBox
    Friend WithEvents cmbDefaultDirection As Windows.Forms.ComboBox
    Friend WithEvents lblDefaultDirection As Windows.Forms.Label
    Friend WithEvents lblScaleMaximums As Windows.Forms.Label
    Friend WithEvents TabPage3_Appearance As Windows.Forms.TabPage
    Friend WithEvents grpRanges As Windows.Forms.GroupBox
    Friend WithEvents ckShowBandOutlines As Windows.Forms.CheckBox
    Friend WithEvents grpActualValue As Windows.Forms.GroupBox
    Friend WithEvents pnlActualBarColor As Windows.Forms.Panel
    Friend WithEvents btnActualBarColor As Windows.Forms.Button
    Friend WithEvents lblActualBarColor As Windows.Forms.Label
    Friend WithEvents ckActualBarOutline As Windows.Forms.CheckBox
    Friend WithEvents nudActualBarHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblActualBarHeight As Windows.Forms.Label
    Friend WithEvents RefEdit_Directions As Excel2007RefEdit
    Friend WithEvents lblDirections As Windows.Forms.Label
    Friend WithEvents RefEdit_QualitativeRanges As Excel2007RefEdit
    Friend WithEvents lblQualitativeRanges As Windows.Forms.Label
    Friend WithEvents lblRangeNote As Windows.Forms.Label
    Friend WithEvents RefEdit_MajorIntervals As Excel2007RefEdit
    Friend WithEvents lblMajorIntervals As Windows.Forms.Label
    Friend WithEvents RefEdit_ScaleMaximums As Excel2007RefEdit
    Friend WithEvents nudTargetMajorTickCount As Windows.Forms.NumericUpDown
    Friend WithEvents lblTargetMajorTickCount As Windows.Forms.Label
    Friend WithEvents ckShowTargetValueLabels As Windows.Forms.CheckBox
    Friend WithEvents ckShowActualValueLabels As Windows.Forms.CheckBox
    Friend WithEvents ckShowScaleTickMarks As Windows.Forms.CheckBox
    Friend WithEvents grpChart As Windows.Forms.GroupBox
    Friend WithEvents nudChartHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartHeight As Windows.Forms.Label
    Friend WithEvents nudChartWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblChartWidth As Windows.Forms.Label
    Friend WithEvents grpTarget As Windows.Forms.GroupBox
    Friend WithEvents pnlTargetColor As Windows.Forms.Panel
    Friend WithEvents btnTargetColor As Windows.Forms.Button
    Friend WithEvents lblTargetColor As Windows.Forms.Label
    Friend WithEvents nudTargetMarkerWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblTargetMarkerWidth As Windows.Forms.Label
    Friend WithEvents nudTargetMarkerHeight As Windows.Forms.NumericUpDown
    Friend WithEvents lblTargetMarkerHeight As Windows.Forms.Label
    Friend WithEvents cmbBandPalette As Windows.Forms.ComboBox
    Friend WithEvents lblBandPalette As Windows.Forms.Label
End Class
