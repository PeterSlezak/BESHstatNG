<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Ui3XYZplot
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui3XYZplot))
        Me.lblLabels = New System.Windows.Forms.Label()
        Me.grpChartSettings = New System.Windows.Forms.GroupBox()
        Me.cbPointLabelPosition = New System.Windows.Forms.ComboBox()
        Me.lblPointLabelPosition = New System.Windows.Forms.Label()
        Me.spinBtnLabelFontSize = New System.Windows.Forms.NumericUpDown()
        Me.ckDataPointLabels = New System.Windows.Forms.CheckBox()
        Me.ckZdropLines = New System.Windows.Forms.CheckBox()
        Me.lblMarkerSize = New System.Windows.Forms.Label()
        Me.spinBtnMarkerSize = New System.Windows.Forms.NumericUpDown()
        Me.ckGridlines = New System.Windows.Forms.CheckBox()
        Me.ckScaleAxes = New System.Windows.Forms.CheckBox()
        Me.spinBtnXZPlanePointSize = New System.Windows.Forms.NumericUpDown()
        Me.ckXZplanePoints = New System.Windows.Forms.CheckBox()
        Me.spinBtnYZPlanePointSize = New System.Windows.Forms.NumericUpDown()
        Me.ckYZplanePoints = New System.Windows.Forms.CheckBox()
        Me.spinBtnXYPlanePointSize = New System.Windows.Forms.NumericUpDown()
        Me.lblPointSize = New System.Windows.Forms.Label()
        Me.ckXYplanePoints = New System.Windows.Forms.CheckBox()
        Me.grpViewSettings = New System.Windows.Forms.GroupBox()
        Me.btResetView = New System.Windows.Forms.Button()
        Me.spinBtnShiftY = New System.Windows.Forms.NumericUpDown()
        Me.lblShiftY = New System.Windows.Forms.Label()
        Me.spinBtnShiftX = New System.Windows.Forms.NumericUpDown()
        Me.lblShiftX = New System.Windows.Forms.Label()
        Me.spinBtnZoom = New System.Windows.Forms.NumericUpDown()
        Me.lblZoom = New System.Windows.Forms.Label()
        Me.spinBtnRotationZ = New System.Windows.Forms.NumericUpDown()
        Me.lblRotationZ = New System.Windows.Forms.Label()
        Me.spinBtnRotationX = New System.Windows.Forms.NumericUpDown()
        Me.lblRotationX = New System.Windows.Forms.Label()
        Me.lblX = New System.Windows.Forms.Label()
        Me.lblY = New System.Windows.Forms.Label()
        Me.lblZ = New System.Windows.Forms.Label()
        Me.lblGroup = New System.Windows.Forms.Label()
        Me.btOK = New System.Windows.Forms.Button()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.RefEdit5_Labels = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit4_Group = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit3_Z = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit2_Y = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit1_X = New BESHStatNG.Excel2007RefEdit()
        Me.grpChartSettings.SuspendLayout()
        CType(Me.spinBtnLabelFontSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnMarkerSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnXZPlanePointSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnYZPlanePointSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnXYPlanePointSize, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpViewSettings.SuspendLayout()
        CType(Me.spinBtnShiftY, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnShiftX, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnZoom, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnRotationZ, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnRotationX, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblLabels
        '
        Me.lblLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLabels.Location = New System.Drawing.Point(12, 163)
        Me.lblLabels.Name = "lblLabels"
        Me.lblLabels.Size = New System.Drawing.Size(119, 32)
        Me.lblLabels.TabIndex = 4
        Me.lblLabels.Text = "Datapoint Labels (optional)"
        '
        'grpChartSettings
        '
        Me.grpChartSettings.Controls.Add(Me.cbPointLabelPosition)
        Me.grpChartSettings.Controls.Add(Me.lblPointLabelPosition)
        Me.grpChartSettings.Controls.Add(Me.spinBtnLabelFontSize)
        Me.grpChartSettings.Controls.Add(Me.ckDataPointLabels)
        Me.grpChartSettings.Controls.Add(Me.ckZdropLines)
        Me.grpChartSettings.Controls.Add(Me.lblMarkerSize)
        Me.grpChartSettings.Controls.Add(Me.spinBtnMarkerSize)
        Me.grpChartSettings.Controls.Add(Me.ckGridlines)
        Me.grpChartSettings.Controls.Add(Me.ckScaleAxes)
        Me.grpChartSettings.Controls.Add(Me.spinBtnXZPlanePointSize)
        Me.grpChartSettings.Controls.Add(Me.ckXZplanePoints)
        Me.grpChartSettings.Controls.Add(Me.spinBtnYZPlanePointSize)
        Me.grpChartSettings.Controls.Add(Me.ckYZplanePoints)
        Me.grpChartSettings.Controls.Add(Me.spinBtnXYPlanePointSize)
        Me.grpChartSettings.Controls.Add(Me.lblPointSize)
        Me.grpChartSettings.Controls.Add(Me.ckXYplanePoints)
        Me.grpChartSettings.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpChartSettings.Location = New System.Drawing.Point(383, 12)
        Me.grpChartSettings.Name = "grpChartSettings"
        Me.grpChartSettings.Size = New System.Drawing.Size(372, 310)
        Me.grpChartSettings.TabIndex = 1
        Me.grpChartSettings.TabStop = False
        Me.grpChartSettings.Text = "Chart Settings"
        '
        'cbPointLabelPosition
        '
        Me.cbPointLabelPosition.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbPointLabelPosition.FormattingEnabled = True
        Me.cbPointLabelPosition.Location = New System.Drawing.Point(205, 266)
        Me.cbPointLabelPosition.Name = "cbPointLabelPosition"
        Me.cbPointLabelPosition.Size = New System.Drawing.Size(156, 24)
        Me.cbPointLabelPosition.TabIndex = 22
        '
        'lblPointLabelPosition
        '
        Me.lblPointLabelPosition.AutoSize = True
        Me.lblPointLabelPosition.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPointLabelPosition.Location = New System.Drawing.Point(42, 269)
        Me.lblPointLabelPosition.Name = "lblPointLabelPosition"
        Me.lblPointLabelPosition.Size = New System.Drawing.Size(157, 16)
        Me.lblPointLabelPosition.TabIndex = 16
        Me.lblPointLabelPosition.Text = "Data Point Label Position"
        '
        'spinBtnLabelFontSize
        '
        Me.spinBtnLabelFontSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnLabelFontSize.Location = New System.Drawing.Point(209, 236)
        Me.spinBtnLabelFontSize.Maximum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.spinBtnLabelFontSize.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinBtnLabelFontSize.Name = "spinBtnLabelFontSize"
        Me.spinBtnLabelFontSize.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnLabelFontSize.TabIndex = 15
        Me.spinBtnLabelFontSize.Value = New Decimal(New Integer() {9, 0, 0, 0})
        '
        'ckDataPointLabels
        '
        Me.ckDataPointLabels.AutoSize = True
        Me.ckDataPointLabels.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckDataPointLabels.Location = New System.Drawing.Point(6, 238)
        Me.ckDataPointLabels.Name = "ckDataPointLabels"
        Me.ckDataPointLabels.Size = New System.Drawing.Size(171, 20)
        Me.ckDataPointLabels.TabIndex = 14
        Me.ckDataPointLabels.Text = "Show Data Point Labels"
        Me.ckDataPointLabels.UseVisualStyleBackColor = True
        '
        'ckZdropLines
        '
        Me.ckZdropLines.AutoSize = True
        Me.ckZdropLines.Checked = True
        Me.ckZdropLines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckZdropLines.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckZdropLines.Location = New System.Drawing.Point(6, 212)
        Me.ckZdropLines.Name = "ckZdropLines"
        Me.ckZdropLines.Size = New System.Drawing.Size(104, 20)
        Me.ckZdropLines.TabIndex = 13
        Me.ckZdropLines.Text = "Z-drop Lines"
        Me.ckZdropLines.UseVisualStyleBackColor = True
        '
        'lblMarkerSize
        '
        Me.lblMarkerSize.AutoSize = True
        Me.lblMarkerSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMarkerSize.Location = New System.Drawing.Point(106, 181)
        Me.lblMarkerSize.Name = "lblMarkerSize"
        Me.lblMarkerSize.Size = New System.Drawing.Size(78, 16)
        Me.lblMarkerSize.TabIndex = 12
        Me.lblMarkerSize.Text = "Marker Size"
        '
        'spinBtnMarkerSize
        '
        Me.spinBtnMarkerSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnMarkerSize.Location = New System.Drawing.Point(209, 179)
        Me.spinBtnMarkerSize.Maximum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.spinBtnMarkerSize.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinBtnMarkerSize.Name = "spinBtnMarkerSize"
        Me.spinBtnMarkerSize.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnMarkerSize.TabIndex = 11
        Me.spinBtnMarkerSize.Value = New Decimal(New Integer() {6, 0, 0, 0})
        '
        'ckGridlines
        '
        Me.ckGridlines.AutoSize = True
        Me.ckGridlines.Checked = True
        Me.ckGridlines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckGridlines.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckGridlines.Location = New System.Drawing.Point(6, 156)
        Me.ckGridlines.Name = "ckGridlines"
        Me.ckGridlines.Size = New System.Drawing.Size(82, 20)
        Me.ckGridlines.TabIndex = 10
        Me.ckGridlines.Text = "Gridlines"
        Me.ckGridlines.UseVisualStyleBackColor = True
        '
        'ckScaleAxes
        '
        Me.ckScaleAxes.AutoSize = True
        Me.ckScaleAxes.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckScaleAxes.Location = New System.Drawing.Point(6, 130)
        Me.ckScaleAxes.Name = "ckScaleAxes"
        Me.ckScaleAxes.Size = New System.Drawing.Size(97, 20)
        Me.ckScaleAxes.TabIndex = 9
        Me.ckScaleAxes.Text = "Scale Axes"
        Me.ckScaleAxes.UseVisualStyleBackColor = True
        '
        'spinBtnXZPlanePointSize
        '
        Me.spinBtnXZPlanePointSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnXZPlanePointSize.Location = New System.Drawing.Point(205, 89)
        Me.spinBtnXZPlanePointSize.Maximum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.spinBtnXZPlanePointSize.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinBtnXZPlanePointSize.Name = "spinBtnXZPlanePointSize"
        Me.spinBtnXZPlanePointSize.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnXZPlanePointSize.TabIndex = 8
        Me.spinBtnXZPlanePointSize.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'ckXZplanePoints
        '
        Me.ckXZplanePoints.AutoSize = True
        Me.ckXZplanePoints.Checked = True
        Me.ckXZplanePoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckXZplanePoints.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckXZplanePoints.Location = New System.Drawing.Point(6, 89)
        Me.ckXZplanePoints.Name = "ckXZplanePoints"
        Me.ckXZplanePoints.Size = New System.Drawing.Size(177, 20)
        Me.ckXZplanePoints.TabIndex = 7
        Me.ckXZplanePoints.Text = "Show Points on XZ Plane"
        Me.ckXZplanePoints.UseVisualStyleBackColor = True
        '
        'spinBtnYZPlanePointSize
        '
        Me.spinBtnYZPlanePointSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnYZPlanePointSize.Location = New System.Drawing.Point(205, 63)
        Me.spinBtnYZPlanePointSize.Maximum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.spinBtnYZPlanePointSize.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinBtnYZPlanePointSize.Name = "spinBtnYZPlanePointSize"
        Me.spinBtnYZPlanePointSize.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnYZPlanePointSize.TabIndex = 6
        Me.spinBtnYZPlanePointSize.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'ckYZplanePoints
        '
        Me.ckYZplanePoints.AutoSize = True
        Me.ckYZplanePoints.Checked = True
        Me.ckYZplanePoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckYZplanePoints.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckYZplanePoints.Location = New System.Drawing.Point(6, 63)
        Me.ckYZplanePoints.Name = "ckYZplanePoints"
        Me.ckYZplanePoints.Size = New System.Drawing.Size(178, 20)
        Me.ckYZplanePoints.TabIndex = 5
        Me.ckYZplanePoints.Text = "Show Points on YZ Plane"
        Me.ckYZplanePoints.UseVisualStyleBackColor = True
        '
        'spinBtnXYPlanePointSize
        '
        Me.spinBtnXYPlanePointSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnXYPlanePointSize.Location = New System.Drawing.Point(205, 37)
        Me.spinBtnXYPlanePointSize.Maximum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.spinBtnXYPlanePointSize.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.spinBtnXYPlanePointSize.Name = "spinBtnXYPlanePointSize"
        Me.spinBtnXYPlanePointSize.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnXYPlanePointSize.TabIndex = 4
        Me.spinBtnXYPlanePointSize.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'lblPointSize
        '
        Me.lblPointSize.AutoSize = True
        Me.lblPointSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPointSize.Location = New System.Drawing.Point(202, 18)
        Me.lblPointSize.Name = "lblPointSize"
        Me.lblPointSize.Size = New System.Drawing.Size(66, 16)
        Me.lblPointSize.TabIndex = 1
        Me.lblPointSize.Text = "Point Size"
        '
        'ckXYplanePoints
        '
        Me.ckXYplanePoints.AutoSize = True
        Me.ckXYplanePoints.Checked = True
        Me.ckXYplanePoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckXYplanePoints.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckXYplanePoints.Location = New System.Drawing.Point(6, 37)
        Me.ckXYplanePoints.Name = "ckXYplanePoints"
        Me.ckXYplanePoints.Size = New System.Drawing.Size(178, 20)
        Me.ckXYplanePoints.TabIndex = 0
        Me.ckXYplanePoints.Text = "Show Points on XY Plane"
        Me.ckXYplanePoints.UseVisualStyleBackColor = True
        '
        'grpViewSettings
        '
        Me.grpViewSettings.Controls.Add(Me.btResetView)
        Me.grpViewSettings.Controls.Add(Me.spinBtnShiftY)
        Me.grpViewSettings.Controls.Add(Me.lblShiftY)
        Me.grpViewSettings.Controls.Add(Me.spinBtnShiftX)
        Me.grpViewSettings.Controls.Add(Me.lblShiftX)
        Me.grpViewSettings.Controls.Add(Me.spinBtnZoom)
        Me.grpViewSettings.Controls.Add(Me.lblZoom)
        Me.grpViewSettings.Controls.Add(Me.spinBtnRotationZ)
        Me.grpViewSettings.Controls.Add(Me.lblRotationZ)
        Me.grpViewSettings.Controls.Add(Me.spinBtnRotationX)
        Me.grpViewSettings.Controls.Add(Me.lblRotationX)
        Me.grpViewSettings.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpViewSettings.Location = New System.Drawing.Point(15, 219)
        Me.grpViewSettings.Name = "grpViewSettings"
        Me.grpViewSettings.Size = New System.Drawing.Size(331, 179)
        Me.grpViewSettings.TabIndex = 2
        Me.grpViewSettings.TabStop = False
        Me.grpViewSettings.Text = "View Settings"
        '
        'btResetView
        '
        Me.btResetView.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btResetView.Location = New System.Drawing.Point(227, 145)
        Me.btResetView.Name = "btResetView"
        Me.btResetView.Size = New System.Drawing.Size(91, 23)
        Me.btResetView.TabIndex = 12
        Me.btResetView.Text = "Reset View"
        Me.btResetView.UseVisualStyleBackColor = True
        '
        'spinBtnShiftY
        '
        Me.spinBtnShiftY.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnShiftY.Location = New System.Drawing.Point(162, 146)
        Me.spinBtnShiftY.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.spinBtnShiftY.Name = "spinBtnShiftY"
        Me.spinBtnShiftY.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnShiftY.TabIndex = 11
        Me.spinBtnShiftY.Value = New Decimal(New Integer() {50, 0, 0, 0})
        '
        'lblShiftY
        '
        Me.lblShiftY.AutoSize = True
        Me.lblShiftY.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblShiftY.Location = New System.Drawing.Point(43, 148)
        Me.lblShiftY.Name = "lblShiftY"
        Me.lblShiftY.Size = New System.Drawing.Size(113, 16)
        Me.lblShiftY.TabIndex = 10
        Me.lblShiftY.Text = "Shift in Y Direction"
        '
        'spinBtnShiftX
        '
        Me.spinBtnShiftX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnShiftX.Location = New System.Drawing.Point(162, 118)
        Me.spinBtnShiftX.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.spinBtnShiftX.Name = "spinBtnShiftX"
        Me.spinBtnShiftX.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnShiftX.TabIndex = 9
        Me.spinBtnShiftX.Value = New Decimal(New Integer() {50, 0, 0, 0})
        '
        'lblShiftX
        '
        Me.lblShiftX.AutoSize = True
        Me.lblShiftX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblShiftX.Location = New System.Drawing.Point(44, 120)
        Me.lblShiftX.Name = "lblShiftX"
        Me.lblShiftX.Size = New System.Drawing.Size(112, 16)
        Me.lblShiftX.TabIndex = 8
        Me.lblShiftX.Text = "Shift in X Direction"
        '
        'spinBtnZoom
        '
        Me.spinBtnZoom.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnZoom.Location = New System.Drawing.Point(162, 90)
        Me.spinBtnZoom.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.spinBtnZoom.Name = "spinBtnZoom"
        Me.spinBtnZoom.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnZoom.TabIndex = 7
        '
        'lblZoom
        '
        Me.lblZoom.AutoSize = True
        Me.lblZoom.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblZoom.Location = New System.Drawing.Point(114, 92)
        Me.lblZoom.Name = "lblZoom"
        Me.lblZoom.Size = New System.Drawing.Size(42, 16)
        Me.lblZoom.TabIndex = 6
        Me.lblZoom.Text = "Zoom"
        '
        'spinBtnRotationZ
        '
        Me.spinBtnRotationZ.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnRotationZ.Location = New System.Drawing.Point(162, 62)
        Me.spinBtnRotationZ.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.spinBtnRotationZ.Name = "spinBtnRotationZ"
        Me.spinBtnRotationZ.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnRotationZ.TabIndex = 5
        Me.spinBtnRotationZ.Value = New Decimal(New Integer() {60, 0, 0, 0})
        '
        'lblRotationZ
        '
        Me.lblRotationZ.AutoSize = True
        Me.lblRotationZ.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRotationZ.Location = New System.Drawing.Point(6, 62)
        Me.lblRotationZ.Name = "lblRotationZ"
        Me.lblRotationZ.Size = New System.Drawing.Size(150, 16)
        Me.lblRotationZ.TabIndex = 4
        Me.lblRotationZ.Text = "Rotation Z axis [degree]"
        '
        'spinBtnRotationX
        '
        Me.spinBtnRotationX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnRotationX.Location = New System.Drawing.Point(162, 37)
        Me.spinBtnRotationX.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.spinBtnRotationX.Name = "spinBtnRotationX"
        Me.spinBtnRotationX.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnRotationX.TabIndex = 3
        Me.spinBtnRotationX.Value = New Decimal(New Integer() {120, 0, 0, 0})
        '
        'lblRotationX
        '
        Me.lblRotationX.AutoSize = True
        Me.lblRotationX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRotationX.Location = New System.Drawing.Point(6, 37)
        Me.lblRotationX.Name = "lblRotationX"
        Me.lblRotationX.Size = New System.Drawing.Size(150, 16)
        Me.lblRotationX.TabIndex = 0
        Me.lblRotationX.Text = "Rotation X axis [degree]"
        '
        'lblX
        '
        Me.lblX.AutoSize = True
        Me.lblX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblX.Location = New System.Drawing.Point(109, 13)
        Me.lblX.Name = "lblX"
        Me.lblX.Size = New System.Drawing.Size(15, 16)
        Me.lblX.TabIndex = 0
        Me.lblX.Text = "X"
        '
        'lblY
        '
        Me.lblY.AutoSize = True
        Me.lblY.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblY.Location = New System.Drawing.Point(108, 48)
        Me.lblY.Name = "lblY"
        Me.lblY.Size = New System.Drawing.Size(16, 16)
        Me.lblY.TabIndex = 1
        Me.lblY.Text = "Y"
        '
        'lblZ
        '
        Me.lblZ.AutoSize = True
        Me.lblZ.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblZ.Location = New System.Drawing.Point(108, 88)
        Me.lblZ.Name = "lblZ"
        Me.lblZ.Size = New System.Drawing.Size(15, 16)
        Me.lblZ.TabIndex = 2
        Me.lblZ.Text = "Z"
        '
        'lblGroup
        '
        Me.lblGroup.AutoSize = True
        Me.lblGroup.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblGroup.Location = New System.Drawing.Point(5, 125)
        Me.lblGroup.Name = "lblGroup"
        Me.lblGroup.Size = New System.Drawing.Size(119, 16)
        Me.lblGroup.TabIndex = 3
        Me.lblGroup.Text = "Group ID (optional)"
        '
        'btOK
        '
        Me.btOK.Location = New System.Drawing.Point(673, 360)
        Me.btOK.Name = "btOK"
        Me.btOK.Size = New System.Drawing.Size(75, 23)
        Me.btOK.TabIndex = 6
        Me.btOK.Text = "OK"
        Me.btOK.UseVisualStyleBackColor = True
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(592, 360)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 7
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'RefEdit5_Labels
        '
        Me.RefEdit5_Labels.Address = ""
        Me.RefEdit5_Labels.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit5_Labels.ExcelConnector = Nothing
        Me.RefEdit5_Labels.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit5_Labels.ImageMinimized = CType(resources.GetObject("RefEdit5_Labels.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit5_Labels.Location = New System.Drawing.Point(130, 163)
        Me.RefEdit5_Labels.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit5_Labels.Name = "RefEdit5_Labels"
        Me.RefEdit5_Labels.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit5_Labels.Size = New System.Drawing.Size(246, 32)
        Me.RefEdit5_Labels.TabIndex = 13
        '
        'RefEdit4_Group
        '
        Me.RefEdit4_Group.Address = ""
        Me.RefEdit4_Group.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit4_Group.ExcelConnector = Nothing
        Me.RefEdit4_Group.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit4_Group.ImageMinimized = CType(resources.GetObject("RefEdit4_Group.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit4_Group.Location = New System.Drawing.Point(130, 125)
        Me.RefEdit4_Group.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit4_Group.Name = "RefEdit4_Group"
        Me.RefEdit4_Group.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit4_Group.Size = New System.Drawing.Size(246, 32)
        Me.RefEdit4_Group.TabIndex = 12
        '
        'RefEdit3_Z
        '
        Me.RefEdit3_Z.Address = ""
        Me.RefEdit3_Z.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit3_Z.ExcelConnector = Nothing
        Me.RefEdit3_Z.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit3_Z.ImageMinimized = CType(resources.GetObject("RefEdit3_Z.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit3_Z.Location = New System.Drawing.Point(130, 88)
        Me.RefEdit3_Z.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit3_Z.Name = "RefEdit3_Z"
        Me.RefEdit3_Z.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit3_Z.Size = New System.Drawing.Size(246, 32)
        Me.RefEdit3_Z.TabIndex = 11
        '
        'RefEdit2_Y
        '
        Me.RefEdit2_Y.Address = ""
        Me.RefEdit2_Y.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit2_Y.ExcelConnector = Nothing
        Me.RefEdit2_Y.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit2_Y.ImageMinimized = CType(resources.GetObject("RefEdit2_Y.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit2_Y.Location = New System.Drawing.Point(130, 48)
        Me.RefEdit2_Y.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit2_Y.Name = "RefEdit2_Y"
        Me.RefEdit2_Y.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit2_Y.Size = New System.Drawing.Size(246, 32)
        Me.RefEdit2_Y.TabIndex = 10
        '
        'RefEdit1_X
        '
        Me.RefEdit1_X.Address = ""
        Me.RefEdit1_X.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit1_X.ExcelConnector = Nothing
        Me.RefEdit1_X.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit1_X.ImageMinimized = CType(resources.GetObject("RefEdit1_X.ImageMinimized"), System.Drawing.Image)
        Me.RefEdit1_X.Location = New System.Drawing.Point(130, 13)
        Me.RefEdit1_X.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit1_X.Name = "RefEdit1_X"
        Me.RefEdit1_X.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit1_X.Size = New System.Drawing.Size(246, 32)
        Me.RefEdit1_X.TabIndex = 9
        '
        'Ui3XYZplot
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(767, 403)
        Me.Controls.Add(Me.lblLabels)
        Me.Controls.Add(Me.RefEdit5_Labels)
        Me.Controls.Add(Me.RefEdit4_Group)
        Me.Controls.Add(Me.lblGroup)
        Me.Controls.Add(Me.RefEdit3_Z)
        Me.Controls.Add(Me.RefEdit2_Y)
        Me.Controls.Add(Me.RefEdit1_X)
        Me.Controls.Add(Me.lblZ)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.lblY)
        Me.Controls.Add(Me.btOK)
        Me.Controls.Add(Me.lblX)
        Me.Controls.Add(Me.grpViewSettings)
        Me.Controls.Add(Me.grpChartSettings)
        Me.Name = "Ui3XYZplot"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "3D XYZ Scatter Plot"
        Me.grpChartSettings.ResumeLayout(False)
        Me.grpChartSettings.PerformLayout()
        CType(Me.spinBtnLabelFontSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnMarkerSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnXZPlanePointSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnYZPlanePointSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnXYPlanePointSize, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpViewSettings.ResumeLayout(False)
        Me.grpViewSettings.PerformLayout()
        CType(Me.spinBtnShiftY, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnShiftX, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnZoom, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnRotationZ, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnRotationX, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents grpChartSettings As Windows.Forms.GroupBox
    Friend WithEvents grpViewSettings As Windows.Forms.GroupBox
    Friend WithEvents lblY As Windows.Forms.Label
    Friend WithEvents lblX As Windows.Forms.Label
    Friend WithEvents lblLabels As Windows.Forms.Label
    Friend WithEvents lblGroup As Windows.Forms.Label
    Friend WithEvents lblZ As Windows.Forms.Label
    Friend WithEvents btOK As Windows.Forms.Button
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents RefEdit1_X As Excel2007RefEdit
    Friend WithEvents RefEdit2_Y As Excel2007RefEdit
    Friend WithEvents RefEdit3_Z As Excel2007RefEdit
    Friend WithEvents RefEdit4_Group As Excel2007RefEdit
    Friend WithEvents RefEdit5_Labels As Excel2007RefEdit
    Friend WithEvents lblRotationX As Windows.Forms.Label
    Friend WithEvents spinBtnRotationX As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnRotationZ As Windows.Forms.NumericUpDown
    Friend WithEvents lblRotationZ As Windows.Forms.Label
    Friend WithEvents spinBtnShiftY As Windows.Forms.NumericUpDown
    Friend WithEvents lblShiftY As Windows.Forms.Label
    Friend WithEvents spinBtnShiftX As Windows.Forms.NumericUpDown
    Friend WithEvents lblShiftX As Windows.Forms.Label
    Friend WithEvents spinBtnZoom As Windows.Forms.NumericUpDown
    Friend WithEvents lblZoom As Windows.Forms.Label
    Friend WithEvents spinBtnXYPlanePointSize As Windows.Forms.NumericUpDown
    Friend WithEvents lblPointSize As Windows.Forms.Label
    Friend WithEvents ckXYplanePoints As Windows.Forms.CheckBox
    Friend WithEvents btResetView As Windows.Forms.Button
    Friend WithEvents spinBtnXZPlanePointSize As Windows.Forms.NumericUpDown
    Friend WithEvents ckXZplanePoints As Windows.Forms.CheckBox
    Friend WithEvents spinBtnYZPlanePointSize As Windows.Forms.NumericUpDown
    Friend WithEvents ckYZplanePoints As Windows.Forms.CheckBox
    Friend WithEvents lblMarkerSize As Windows.Forms.Label
    Friend WithEvents spinBtnMarkerSize As Windows.Forms.NumericUpDown
    Friend WithEvents ckGridlines As Windows.Forms.CheckBox
    Friend WithEvents ckScaleAxes As Windows.Forms.CheckBox
    Friend WithEvents spinBtnLabelFontSize As Windows.Forms.NumericUpDown
    Friend WithEvents ckDataPointLabels As Windows.Forms.CheckBox
    Friend WithEvents ckZdropLines As Windows.Forms.CheckBox
    Friend WithEvents lblPointLabelPosition As Windows.Forms.Label
    Friend WithEvents cbPointLabelPosition As Windows.Forms.ComboBox
End Class
