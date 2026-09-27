<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui01ConvexHullPlot
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui01ConvexHullPlot))
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.RefEdit_GroupID = New BESHStatNG.Excel2007RefEdit()
        Me.lblGroup = New System.Windows.Forms.Label()
        Me.RefEdit_Y = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_X = New BESHStatNG.Excel2007RefEdit()
        Me.lblY = New System.Windows.Forms.Label()
        Me.lblX = New System.Windows.Forms.Label()
        Me.TabPage_Appearance = New System.Windows.Forms.TabPage()
        Me.grpMarkerLineAppearance = New System.Windows.Forms.GroupBox()
        Me.pnlHullLineColor = New System.Windows.Forms.Panel()
        Me.btnHullLineColor = New System.Windows.Forms.Button()
        Me.NumericUpDown2 = New System.Windows.Forms.NumericUpDown()
        Me.lblHullLineWeight = New System.Windows.Forms.Label()
        Me.cbHullLineStyle = New System.Windows.Forms.ComboBox()
        Me.lblHullLineStyle = New System.Windows.Forms.Label()
        Me.pnlMarkerColor = New System.Windows.Forms.Panel()
        Me.btnMarkerColor = New System.Windows.Forms.Button()
        Me.NumericUpDown1 = New System.Windows.Forms.NumericUpDown()
        Me.lblMarkerSize = New System.Windows.Forms.Label()
        Me.cbMarkerStyle = New System.Windows.Forms.ComboBox()
        Me.lblMarkerStyle = New System.Windows.Forms.Label()
        Me.grpGroupAppearance = New System.Windows.Forms.GroupBox()
        Me.cbGroupStyleMode = New System.Windows.Forms.ComboBox()
        Me.lblGroupStyleMode = New System.Windows.Forms.Label()
        Me.grpDisplay = New System.Windows.Forms.GroupBox()
        Me.ckShowMajorGridlines = New System.Windows.Forms.CheckBox()
        Me.ckShowLegend = New System.Windows.Forms.CheckBox()
        Me.TabPage_Options = New System.Windows.Forms.TabPage()
        Me.grpOptions = New System.Windows.Forms.GroupBox()
        Me.lblToleranceHint = New System.Windows.Forms.Label()
        Me.tbCollinearityTolerance = New System.Windows.Forms.TextBox()
        Me.lblCollinearityTolerance = New System.Windows.Forms.Label()
        Me.nudPaddingPercentY = New System.Windows.Forms.NumericUpDown()
        Me.lblPaddingPercentY = New System.Windows.Forms.Label()
        Me.nudPaddingPercentX = New System.Windows.Forms.NumericUpDown()
        Me.lblPaddingPercentX = New System.Windows.Forms.Label()
        Me.ckIncludeCollinearBoundaryPoints = New System.Windows.Forms.CheckBox()
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.TabPage_Appearance.SuspendLayout()
        Me.grpMarkerLineAppearance.SuspendLayout()
        CType(Me.NumericUpDown2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpGroupAppearance.SuspendLayout()
        Me.grpDisplay.SuspendLayout()
        Me.TabPage_Options.SuspendLayout()
        Me.grpOptions.SuspendLayout()
        CType(Me.nudPaddingPercentY, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudPaddingPercentX, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpOutput.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(298, 444)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 10
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(379, 444)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 9
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1)
        Me.TabMultipage.Controls.Add(Me.TabPage_Options)
        Me.TabMultipage.Controls.Add(Me.TabPage_Appearance)
        Me.TabMultipage.Location = New System.Drawing.Point(-2, 6)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(456, 432)
        Me.TabMultipage.TabIndex = 8
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.grpOutput)
        Me.TabPage1.Controls.Add(Me.grpInput)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(448, 403)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Input"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.RefEdit_GroupID)
        Me.grpInput.Controls.Add(Me.lblGroup)
        Me.grpInput.Controls.Add(Me.RefEdit_Y)
        Me.grpInput.Controls.Add(Me.RefEdit_X)
        Me.grpInput.Controls.Add(Me.lblY)
        Me.grpInput.Controls.Add(Me.lblX)
        Me.grpInput.Location = New System.Drawing.Point(3, 19)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(442, 140)
        Me.grpInput.TabIndex = 2
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'RefEdit_GroupID
        '
        Me.RefEdit_GroupID.Address = ""
        Me.RefEdit_GroupID.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_GroupID.ExcelConnector = Nothing
        Me.RefEdit_GroupID.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_GroupID.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_GroupID.Location = New System.Drawing.Point(152, 101)
        Me.RefEdit_GroupID.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_GroupID.Name = "RefEdit_GroupID"
        Me.RefEdit_GroupID.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_GroupID.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_GroupID.TabIndex = 7
        '
        'lblGroup
        '
        Me.lblGroup.AutoSize = True
        Me.lblGroup.Location = New System.Drawing.Point(13, 101)
        Me.lblGroup.Name = "lblGroup"
        Me.lblGroup.Size = New System.Drawing.Size(119, 16)
        Me.lblGroup.TabIndex = 6
        Me.lblGroup.Text = "Group ID (optional)"
        '
        'RefEdit_Y
        '
        Me.RefEdit_Y.Address = ""
        Me.RefEdit_Y.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_Y.ExcelConnector = Nothing
        Me.RefEdit_Y.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_Y.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_Y.Location = New System.Drawing.Point(152, 62)
        Me.RefEdit_Y.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_Y.Name = "RefEdit_Y"
        Me.RefEdit_Y.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_Y.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_Y.TabIndex = 5
        '
        'RefEdit_X
        '
        Me.RefEdit_X.Address = ""
        Me.RefEdit_X.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_X.ExcelConnector = Nothing
        Me.RefEdit_X.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_X.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_X.Location = New System.Drawing.Point(152, 22)
        Me.RefEdit_X.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_X.Name = "RefEdit_X"
        Me.RefEdit_X.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_X.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_X.TabIndex = 4
        '
        'lblY
        '
        Me.lblY.AutoSize = True
        Me.lblY.Location = New System.Drawing.Point(13, 62)
        Me.lblY.Name = "lblY"
        Me.lblY.Size = New System.Drawing.Size(16, 16)
        Me.lblY.TabIndex = 3
        Me.lblY.Text = "Y"
        '
        'lblX
        '
        Me.lblX.AutoSize = True
        Me.lblX.Location = New System.Drawing.Point(13, 22)
        Me.lblX.Name = "lblX"
        Me.lblX.Size = New System.Drawing.Size(15, 16)
        Me.lblX.TabIndex = 2
        Me.lblX.Text = "X"
        '
        'TabPage_Appearance
        '
        Me.TabPage_Appearance.Controls.Add(Me.grpMarkerLineAppearance)
        Me.TabPage_Appearance.Controls.Add(Me.grpGroupAppearance)
        Me.TabPage_Appearance.Controls.Add(Me.grpDisplay)
        Me.TabPage_Appearance.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_Appearance.Name = "TabPage_Appearance"
        Me.TabPage_Appearance.Size = New System.Drawing.Size(448, 403)
        Me.TabPage_Appearance.TabIndex = 1
        Me.TabPage_Appearance.Text = "Appearance"
        Me.TabPage_Appearance.UseVisualStyleBackColor = True
        '
        'grpMarkerLineAppearance
        '
        Me.grpMarkerLineAppearance.Controls.Add(Me.pnlHullLineColor)
        Me.grpMarkerLineAppearance.Controls.Add(Me.btnHullLineColor)
        Me.grpMarkerLineAppearance.Controls.Add(Me.NumericUpDown2)
        Me.grpMarkerLineAppearance.Controls.Add(Me.lblHullLineWeight)
        Me.grpMarkerLineAppearance.Controls.Add(Me.cbHullLineStyle)
        Me.grpMarkerLineAppearance.Controls.Add(Me.lblHullLineStyle)
        Me.grpMarkerLineAppearance.Controls.Add(Me.pnlMarkerColor)
        Me.grpMarkerLineAppearance.Controls.Add(Me.btnMarkerColor)
        Me.grpMarkerLineAppearance.Controls.Add(Me.NumericUpDown1)
        Me.grpMarkerLineAppearance.Controls.Add(Me.lblMarkerSize)
        Me.grpMarkerLineAppearance.Controls.Add(Me.cbMarkerStyle)
        Me.grpMarkerLineAppearance.Controls.Add(Me.lblMarkerStyle)
        Me.grpMarkerLineAppearance.Location = New System.Drawing.Point(18, 174)
        Me.grpMarkerLineAppearance.Name = "grpMarkerLineAppearance"
        Me.grpMarkerLineAppearance.Size = New System.Drawing.Size(423, 211)
        Me.grpMarkerLineAppearance.TabIndex = 4
        Me.grpMarkerLineAppearance.TabStop = False
        Me.grpMarkerLineAppearance.Text = "Markers and Lines"
        '
        'pnlHullLineColor
        '
        Me.pnlHullLineColor.Location = New System.Drawing.Point(153, 175)
        Me.pnlHullLineColor.Name = "pnlHullLineColor"
        Me.pnlHullLineColor.Size = New System.Drawing.Size(59, 30)
        Me.pnlHullLineColor.TabIndex = 13
        '
        'btnHullLineColor
        '
        Me.btnHullLineColor.Location = New System.Drawing.Point(33, 182)
        Me.btnHullLineColor.Name = "btnHullLineColor"
        Me.btnHullLineColor.Size = New System.Drawing.Size(114, 23)
        Me.btnHullLineColor.TabIndex = 12
        Me.btnHullLineColor.Text = "Line color..."
        Me.btnHullLineColor.UseVisualStyleBackColor = True
        '
        'NumericUpDown2
        '
        Me.NumericUpDown2.DecimalPlaces = 2
        Me.NumericUpDown2.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.NumericUpDown2.Increment = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.NumericUpDown2.Location = New System.Drawing.Point(153, 148)
        Me.NumericUpDown2.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.NumericUpDown2.Minimum = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.NumericUpDown2.Name = "NumericUpDown2"
        Me.NumericUpDown2.Size = New System.Drawing.Size(59, 22)
        Me.NumericUpDown2.TabIndex = 11
        Me.NumericUpDown2.Value = New Decimal(New Integer() {15, 0, 0, 65536})
        '
        'lblHullLineWeight
        '
        Me.lblHullLineWeight.AutoSize = True
        Me.lblHullLineWeight.Location = New System.Drawing.Point(6, 150)
        Me.lblHullLineWeight.Name = "lblHullLineWeight"
        Me.lblHullLineWeight.Size = New System.Drawing.Size(69, 16)
        Me.lblHullLineWeight.TabIndex = 10
        Me.lblHullLineWeight.Text = "Line Width"
        '
        'cbHullLineStyle
        '
        Me.cbHullLineStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHullLineStyle.FormattingEnabled = True
        Me.cbHullLineStyle.Location = New System.Drawing.Point(153, 118)
        Me.cbHullLineStyle.Name = "cbHullLineStyle"
        Me.cbHullLineStyle.Size = New System.Drawing.Size(224, 24)
        Me.cbHullLineStyle.TabIndex = 9
        '
        'lblHullLineStyle
        '
        Me.lblHullLineStyle.AutoSize = True
        Me.lblHullLineStyle.Location = New System.Drawing.Point(6, 121)
        Me.lblHullLineStyle.Name = "lblHullLineStyle"
        Me.lblHullLineStyle.Size = New System.Drawing.Size(65, 16)
        Me.lblHullLineStyle.TabIndex = 8
        Me.lblHullLineStyle.Text = "Line Style"
        '
        'pnlMarkerColor
        '
        Me.pnlMarkerColor.Location = New System.Drawing.Point(153, 82)
        Me.pnlMarkerColor.Name = "pnlMarkerColor"
        Me.pnlMarkerColor.Size = New System.Drawing.Size(59, 30)
        Me.pnlMarkerColor.TabIndex = 7
        '
        'btnMarkerColor
        '
        Me.btnMarkerColor.Location = New System.Drawing.Point(33, 89)
        Me.btnMarkerColor.Name = "btnMarkerColor"
        Me.btnMarkerColor.Size = New System.Drawing.Size(114, 23)
        Me.btnMarkerColor.TabIndex = 6
        Me.btnMarkerColor.Text = "Marker color..."
        Me.btnMarkerColor.UseVisualStyleBackColor = True
        '
        'NumericUpDown1
        '
        Me.NumericUpDown1.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.NumericUpDown1.Location = New System.Drawing.Point(153, 54)
        Me.NumericUpDown1.Maximum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.NumericUpDown1.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.NumericUpDown1.Name = "NumericUpDown1"
        Me.NumericUpDown1.Size = New System.Drawing.Size(59, 22)
        Me.NumericUpDown1.TabIndex = 5
        Me.NumericUpDown1.Value = New Decimal(New Integer() {6, 0, 0, 0})
        '
        'lblMarkerSize
        '
        Me.lblMarkerSize.AutoSize = True
        Me.lblMarkerSize.Location = New System.Drawing.Point(6, 56)
        Me.lblMarkerSize.Name = "lblMarkerSize"
        Me.lblMarkerSize.Size = New System.Drawing.Size(33, 16)
        Me.lblMarkerSize.TabIndex = 4
        Me.lblMarkerSize.Text = "Size"
        '
        'cbMarkerStyle
        '
        Me.cbMarkerStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbMarkerStyle.FormattingEnabled = True
        Me.cbMarkerStyle.Location = New System.Drawing.Point(153, 24)
        Me.cbMarkerStyle.Name = "cbMarkerStyle"
        Me.cbMarkerStyle.Size = New System.Drawing.Size(224, 24)
        Me.cbMarkerStyle.TabIndex = 3
        '
        'lblMarkerStyle
        '
        Me.lblMarkerStyle.AutoSize = True
        Me.lblMarkerStyle.Location = New System.Drawing.Point(6, 27)
        Me.lblMarkerStyle.Name = "lblMarkerStyle"
        Me.lblMarkerStyle.Size = New System.Drawing.Size(53, 16)
        Me.lblMarkerStyle.TabIndex = 2
        Me.lblMarkerStyle.Text = "Symbol"
        '
        'grpGroupAppearance
        '
        Me.grpGroupAppearance.Controls.Add(Me.cbGroupStyleMode)
        Me.grpGroupAppearance.Controls.Add(Me.lblGroupStyleMode)
        Me.grpGroupAppearance.Location = New System.Drawing.Point(18, 98)
        Me.grpGroupAppearance.Name = "grpGroupAppearance"
        Me.grpGroupAppearance.Size = New System.Drawing.Size(423, 70)
        Me.grpGroupAppearance.TabIndex = 3
        Me.grpGroupAppearance.TabStop = False
        Me.grpGroupAppearance.Text = "Grouped data"
        '
        'cbGroupStyleMode
        '
        Me.cbGroupStyleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbGroupStyleMode.FormattingEnabled = True
        Me.cbGroupStyleMode.Location = New System.Drawing.Point(153, 24)
        Me.cbGroupStyleMode.Name = "cbGroupStyleMode"
        Me.cbGroupStyleMode.Size = New System.Drawing.Size(224, 24)
        Me.cbGroupStyleMode.TabIndex = 3
        '
        'lblGroupStyleMode
        '
        Me.lblGroupStyleMode.AutoSize = True
        Me.lblGroupStyleMode.Location = New System.Drawing.Point(6, 27)
        Me.lblGroupStyleMode.Name = "lblGroupStyleMode"
        Me.lblGroupStyleMode.Size = New System.Drawing.Size(141, 16)
        Me.lblGroupStyleMode.TabIndex = 2
        Me.lblGroupStyleMode.Text = "Differentiate groups by"
        '
        'grpDisplay
        '
        Me.grpDisplay.Controls.Add(Me.ckShowMajorGridlines)
        Me.grpDisplay.Controls.Add(Me.ckShowLegend)
        Me.grpDisplay.Location = New System.Drawing.Point(18, 14)
        Me.grpDisplay.Name = "grpDisplay"
        Me.grpDisplay.Size = New System.Drawing.Size(423, 78)
        Me.grpDisplay.TabIndex = 0
        Me.grpDisplay.TabStop = False
        Me.grpDisplay.Text = "Display"
        '
        'ckShowMajorGridlines
        '
        Me.ckShowMajorGridlines.AutoSize = True
        Me.ckShowMajorGridlines.Checked = True
        Me.ckShowMajorGridlines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowMajorGridlines.Location = New System.Drawing.Point(6, 47)
        Me.ckShowMajorGridlines.Name = "ckShowMajorGridlines"
        Me.ckShowMajorGridlines.Size = New System.Drawing.Size(153, 20)
        Me.ckShowMajorGridlines.TabIndex = 2
        Me.ckShowMajorGridlines.Text = "Show major gridlines"
        Me.ckShowMajorGridlines.UseVisualStyleBackColor = True
        '
        'ckShowLegend
        '
        Me.ckShowLegend.AutoSize = True
        Me.ckShowLegend.Checked = True
        Me.ckShowLegend.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowLegend.Location = New System.Drawing.Point(6, 21)
        Me.ckShowLegend.Name = "ckShowLegend"
        Me.ckShowLegend.Size = New System.Drawing.Size(107, 20)
        Me.ckShowLegend.TabIndex = 1
        Me.ckShowLegend.Text = "Show legend"
        Me.ckShowLegend.UseVisualStyleBackColor = True
        '
        'TabPage_Options
        '
        Me.TabPage_Options.Controls.Add(Me.grpOptions)
        Me.TabPage_Options.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_Options.Name = "TabPage_Options"
        Me.TabPage_Options.Size = New System.Drawing.Size(448, 403)
        Me.TabPage_Options.TabIndex = 2
        Me.TabPage_Options.Text = "Options"
        Me.TabPage_Options.UseVisualStyleBackColor = True
        '
        'grpOptions
        '
        Me.grpOptions.Controls.Add(Me.lblToleranceHint)
        Me.grpOptions.Controls.Add(Me.tbCollinearityTolerance)
        Me.grpOptions.Controls.Add(Me.lblCollinearityTolerance)
        Me.grpOptions.Controls.Add(Me.nudPaddingPercentY)
        Me.grpOptions.Controls.Add(Me.lblPaddingPercentY)
        Me.grpOptions.Controls.Add(Me.nudPaddingPercentX)
        Me.grpOptions.Controls.Add(Me.lblPaddingPercentX)
        Me.grpOptions.Controls.Add(Me.ckIncludeCollinearBoundaryPoints)
        Me.grpOptions.Location = New System.Drawing.Point(3, 18)
        Me.grpOptions.Name = "grpOptions"
        Me.grpOptions.Size = New System.Drawing.Size(436, 151)
        Me.grpOptions.TabIndex = 7
        Me.grpOptions.TabStop = False
        Me.grpOptions.Text = "Options"
        '
        'lblToleranceHint
        '
        Me.lblToleranceHint.AutoSize = True
        Me.lblToleranceHint.Location = New System.Drawing.Point(225, 120)
        Me.lblToleranceHint.Name = "lblToleranceHint"
        Me.lblToleranceHint.Size = New System.Drawing.Size(133, 16)
        Me.lblToleranceHint.TabIndex = 9
        Me.lblToleranceHint.Text = "0 = exact comparison"
        '
        'tbCollinearityTolerance
        '
        Me.tbCollinearityTolerance.Location = New System.Drawing.Point(155, 117)
        Me.tbCollinearityTolerance.Name = "tbCollinearityTolerance"
        Me.tbCollinearityTolerance.Size = New System.Drawing.Size(61, 22)
        Me.tbCollinearityTolerance.TabIndex = 8
        Me.tbCollinearityTolerance.Text = "0"
        '
        'lblCollinearityTolerance
        '
        Me.lblCollinearityTolerance.AutoSize = True
        Me.lblCollinearityTolerance.Location = New System.Drawing.Point(17, 120)
        Me.lblCollinearityTolerance.Name = "lblCollinearityTolerance"
        Me.lblCollinearityTolerance.Size = New System.Drawing.Size(132, 16)
        Me.lblCollinearityTolerance.TabIndex = 7
        Me.lblCollinearityTolerance.Text = "Collinearity tolerance"
        '
        'nudPaddingPercentY
        '
        Me.nudPaddingPercentY.DecimalPlaces = 2
        Me.nudPaddingPercentY.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudPaddingPercentY.Location = New System.Drawing.Point(155, 85)
        Me.nudPaddingPercentY.Name = "nudPaddingPercentY"
        Me.nudPaddingPercentY.Size = New System.Drawing.Size(59, 22)
        Me.nudPaddingPercentY.TabIndex = 6
        '
        'lblPaddingPercentY
        '
        Me.lblPaddingPercentY.AutoSize = True
        Me.lblPaddingPercentY.Location = New System.Drawing.Point(58, 87)
        Me.lblPaddingPercentY.Name = "lblPaddingPercentY"
        Me.lblPaddingPercentY.Size = New System.Drawing.Size(92, 16)
        Me.lblPaddingPercentY.TabIndex = 5
        Me.lblPaddingPercentY.Text = "Y padding (%)"
        '
        'nudPaddingPercentX
        '
        Me.nudPaddingPercentX.DecimalPlaces = 2
        Me.nudPaddingPercentX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudPaddingPercentX.Location = New System.Drawing.Point(155, 58)
        Me.nudPaddingPercentX.Name = "nudPaddingPercentX"
        Me.nudPaddingPercentX.Size = New System.Drawing.Size(59, 22)
        Me.nudPaddingPercentX.TabIndex = 4
        '
        'lblPaddingPercentX
        '
        Me.lblPaddingPercentX.AutoSize = True
        Me.lblPaddingPercentX.Location = New System.Drawing.Point(58, 60)
        Me.lblPaddingPercentX.Name = "lblPaddingPercentX"
        Me.lblPaddingPercentX.Size = New System.Drawing.Size(91, 16)
        Me.lblPaddingPercentX.TabIndex = 1
        Me.lblPaddingPercentX.Text = "X padding (%)"
        '
        'ckIncludeCollinearBoundaryPoints
        '
        Me.ckIncludeCollinearBoundaryPoints.AutoSize = True
        Me.ckIncludeCollinearBoundaryPoints.Checked = True
        Me.ckIncludeCollinearBoundaryPoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckIncludeCollinearBoundaryPoints.Location = New System.Drawing.Point(155, 32)
        Me.ckIncludeCollinearBoundaryPoints.Name = "ckIncludeCollinearBoundaryPoints"
        Me.ckIncludeCollinearBoundaryPoints.Size = New System.Drawing.Size(225, 20)
        Me.ckIncludeCollinearBoundaryPoints.TabIndex = 0
        Me.ckIncludeCollinearBoundaryPoints.Text = "Include collinear boundary points"
        Me.ckIncludeCollinearBoundaryPoints.UseVisualStyleBackColor = True
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(3, 187)
        Me.grpOutput.Name = "grpOutput"
        Me.grpOutput.Size = New System.Drawing.Size(442, 130)
        Me.grpOutput.TabIndex = 5
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
        'Ui01ConvexHullPlot
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(460, 472)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.TabMultipage)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(478, 519)
        Me.MinimumSize = New System.Drawing.Size(478, 519)
        Me.Name = "Ui01ConvexHullPlot"
        Me.ShowIcon = False
        Me.Text = "Convex Hull Plot"
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.TabPage_Appearance.ResumeLayout(False)
        Me.grpMarkerLineAppearance.ResumeLayout(False)
        Me.grpMarkerLineAppearance.PerformLayout()
        CType(Me.NumericUpDown2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpGroupAppearance.ResumeLayout(False)
        Me.grpGroupAppearance.PerformLayout()
        Me.grpDisplay.ResumeLayout(False)
        Me.grpDisplay.PerformLayout()
        Me.TabPage_Options.ResumeLayout(False)
        Me.grpOptions.ResumeLayout(False)
        Me.grpOptions.PerformLayout()
        CType(Me.nudPaddingPercentY, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudPaddingPercentX, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents RefEdit_GroupID As Excel2007RefEdit
    Friend WithEvents lblGroup As Windows.Forms.Label
    Friend WithEvents RefEdit_Y As Excel2007RefEdit
    Friend WithEvents RefEdit_X As Excel2007RefEdit
    Friend WithEvents lblY As Windows.Forms.Label
    Friend WithEvents lblX As Windows.Forms.Label
    Friend WithEvents TabPage_Appearance As Windows.Forms.TabPage
    Friend WithEvents grpDisplay As Windows.Forms.GroupBox
    Friend WithEvents ckShowLegend As Windows.Forms.CheckBox
    Friend WithEvents grpGroupAppearance As Windows.Forms.GroupBox
    Friend WithEvents cbGroupStyleMode As Windows.Forms.ComboBox
    Friend WithEvents lblGroupStyleMode As Windows.Forms.Label
    Friend WithEvents ckShowMajorGridlines As Windows.Forms.CheckBox
    Friend WithEvents grpMarkerLineAppearance As Windows.Forms.GroupBox
    Friend WithEvents NumericUpDown1 As Windows.Forms.NumericUpDown
    Friend WithEvents lblMarkerSize As Windows.Forms.Label
    Friend WithEvents cbMarkerStyle As Windows.Forms.ComboBox
    Friend WithEvents lblMarkerStyle As Windows.Forms.Label
    Friend WithEvents pnlMarkerColor As Windows.Forms.Panel
    Friend WithEvents btnMarkerColor As Windows.Forms.Button
    Friend WithEvents pnlHullLineColor As Windows.Forms.Panel
    Friend WithEvents btnHullLineColor As Windows.Forms.Button
    Friend WithEvents NumericUpDown2 As Windows.Forms.NumericUpDown
    Friend WithEvents lblHullLineWeight As Windows.Forms.Label
    Friend WithEvents cbHullLineStyle As Windows.Forms.ComboBox
    Friend WithEvents lblHullLineStyle As Windows.Forms.Label
    Friend WithEvents TabPage_Options As Windows.Forms.TabPage
    Friend WithEvents grpOptions As Windows.Forms.GroupBox
    Friend WithEvents lblToleranceHint As Windows.Forms.Label
    Friend WithEvents tbCollinearityTolerance As Windows.Forms.TextBox
    Friend WithEvents lblCollinearityTolerance As Windows.Forms.Label
    Friend WithEvents nudPaddingPercentY As Windows.Forms.NumericUpDown
    Friend WithEvents lblPaddingPercentY As Windows.Forms.Label
    Friend WithEvents nudPaddingPercentX As Windows.Forms.NumericUpDown
    Friend WithEvents lblPaddingPercentX As Windows.Forms.Label
    Friend WithEvents ckIncludeCollinearBoundaryPoints As Windows.Forms.CheckBox
    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
End Class
