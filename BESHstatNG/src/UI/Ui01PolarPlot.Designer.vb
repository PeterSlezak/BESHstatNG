<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui01PolarPlot
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
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.TabMultipage = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.grpOptions = New System.Windows.Forms.GroupBox()
        Me.grpRotation = New System.Windows.Forms.GroupBox()
        Me.optClockwise = New System.Windows.Forms.RadioButton()
        Me.optCounterClockwise = New System.Windows.Forms.RadioButton()
        Me.grpAngleUnit = New System.Windows.Forms.GroupBox()
        Me.optRadians = New System.Windows.Forms.RadioButton()
        Me.optPercentage = New System.Windows.Forms.RadioButton()
        Me.optDegrees = New System.Windows.Forms.RadioButton()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.lblRefeditAngle = New System.Windows.Forms.Label()
        Me.lblRefeditRadius = New System.Windows.Forms.Label()
        Me.grpZeroAngle = New System.Windows.Forms.GroupBox()
        Me.optNorth = New System.Windows.Forms.RadioButton()
        Me.optSouth = New System.Windows.Forms.RadioButton()
        Me.optEast = New System.Windows.Forms.RadioButton()
        Me.optWest = New System.Windows.Forms.RadioButton()
        Me.ckConnectPoints = New System.Windows.Forms.CheckBox()
        Me.lblGroup = New System.Windows.Forms.Label()
        Me.TabPage_Options = New System.Windows.Forms.TabPage()
        Me.lblAngularTickInterval = New System.Windows.Forms.Label()
        Me.lblRadialTickInterval = New System.Windows.Forms.Label()
        Me.RefEdit_GroupID = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_Angle = New BESHStatNG.Excel2007RefEdit()
        Me.RefEdit_Radius = New BESHStatNG.Excel2007RefEdit()
        Me.tbAngularTickInterval = New System.Windows.Forms.TextBox()
        Me.tbRadialTickInterval = New System.Windows.Forms.TextBox()
        Me.TabMultipage.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.grpOptions.SuspendLayout()
        Me.grpRotation.SuspendLayout()
        Me.grpAngleUnit.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.grpZeroAngle.SuspendLayout()
        Me.TabPage_Options.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(295, 444)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 7
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(376, 444)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 6
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'TabMultipage
        '
        Me.TabMultipage.Controls.Add(Me.TabPage1)
        Me.TabMultipage.Controls.Add(Me.TabPage_Options)
        Me.TabMultipage.Location = New System.Drawing.Point(0, 6)
        Me.TabMultipage.Name = "TabMultipage"
        Me.TabMultipage.SelectedIndex = 0
        Me.TabMultipage.Size = New System.Drawing.Size(456, 432)
        Me.TabMultipage.TabIndex = 5
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.grpOptions)
        Me.TabPage1.Controls.Add(Me.grpInput)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(448, 403)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Input"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'grpOptions
        '
        Me.grpOptions.Controls.Add(Me.ckConnectPoints)
        Me.grpOptions.Controls.Add(Me.grpZeroAngle)
        Me.grpOptions.Controls.Add(Me.grpRotation)
        Me.grpOptions.Controls.Add(Me.grpAngleUnit)
        Me.grpOptions.Location = New System.Drawing.Point(9, 165)
        Me.grpOptions.Name = "grpOptions"
        Me.grpOptions.Size = New System.Drawing.Size(436, 232)
        Me.grpOptions.TabIndex = 6
        Me.grpOptions.TabStop = False
        Me.grpOptions.Text = "Options"
        '
        'grpRotation
        '
        Me.grpRotation.Controls.Add(Me.optClockwise)
        Me.grpRotation.Controls.Add(Me.optCounterClockwise)
        Me.grpRotation.Location = New System.Drawing.Point(224, 21)
        Me.grpRotation.Name = "grpRotation"
        Me.grpRotation.Size = New System.Drawing.Size(202, 105)
        Me.grpRotation.TabIndex = 8
        Me.grpRotation.TabStop = False
        Me.grpRotation.Text = "Rotation"
        '
        'optClockwise
        '
        Me.optClockwise.AutoSize = True
        Me.optClockwise.Checked = True
        Me.optClockwise.Location = New System.Drawing.Point(6, 21)
        Me.optClockwise.Name = "optClockwise"
        Me.optClockwise.Size = New System.Drawing.Size(89, 20)
        Me.optClockwise.TabIndex = 4
        Me.optClockwise.TabStop = True
        Me.optClockwise.Text = "Clockwise"
        Me.optClockwise.UseVisualStyleBackColor = True
        '
        'optCounterClockwise
        '
        Me.optCounterClockwise.AutoSize = True
        Me.optCounterClockwise.Location = New System.Drawing.Point(5, 47)
        Me.optCounterClockwise.Name = "optCounterClockwise"
        Me.optCounterClockwise.Size = New System.Drawing.Size(133, 20)
        Me.optCounterClockwise.TabIndex = 5
        Me.optCounterClockwise.Text = "Counterclockwise"
        Me.optCounterClockwise.UseVisualStyleBackColor = True
        '
        'grpAngleUnit
        '
        Me.grpAngleUnit.Controls.Add(Me.optRadians)
        Me.grpAngleUnit.Controls.Add(Me.optPercentage)
        Me.grpAngleUnit.Controls.Add(Me.optDegrees)
        Me.grpAngleUnit.Location = New System.Drawing.Point(16, 21)
        Me.grpAngleUnit.Name = "grpAngleUnit"
        Me.grpAngleUnit.Size = New System.Drawing.Size(202, 105)
        Me.grpAngleUnit.TabIndex = 7
        Me.grpAngleUnit.TabStop = False
        Me.grpAngleUnit.Text = "Angle Unit"
        '
        'optRadians
        '
        Me.optRadians.AutoSize = True
        Me.optRadians.Location = New System.Drawing.Point(6, 21)
        Me.optRadians.Name = "optRadians"
        Me.optRadians.Size = New System.Drawing.Size(164, 20)
        Me.optRadians.TabIndex = 4
        Me.optRadians.Text = "Radians (1 circle = 2Pi)"
        Me.optRadians.UseVisualStyleBackColor = True
        '
        'optPercentage
        '
        Me.optPercentage.AutoSize = True
        Me.optPercentage.Location = New System.Drawing.Point(5, 73)
        Me.optPercentage.Name = "optPercentage"
        Me.optPercentage.Size = New System.Drawing.Size(185, 20)
        Me.optPercentage.TabIndex = 6
        Me.optPercentage.Text = "Percentage (1 circle = 100)"
        Me.optPercentage.UseVisualStyleBackColor = True
        '
        'optDegrees
        '
        Me.optDegrees.AutoSize = True
        Me.optDegrees.Checked = True
        Me.optDegrees.Location = New System.Drawing.Point(5, 47)
        Me.optDegrees.Name = "optDegrees"
        Me.optDegrees.Size = New System.Drawing.Size(172, 20)
        Me.optDegrees.TabIndex = 5
        Me.optDegrees.TabStop = True
        Me.optDegrees.Text = "Degrees (1 circle = 360°)"
        Me.optDegrees.UseVisualStyleBackColor = True
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.RefEdit_GroupID)
        Me.grpInput.Controls.Add(Me.lblGroup)
        Me.grpInput.Controls.Add(Me.RefEdit_Angle)
        Me.grpInput.Controls.Add(Me.RefEdit_Radius)
        Me.grpInput.Controls.Add(Me.lblRefeditAngle)
        Me.grpInput.Controls.Add(Me.lblRefeditRadius)
        Me.grpInput.Location = New System.Drawing.Point(9, 19)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(436, 140)
        Me.grpInput.TabIndex = 2
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'lblRefeditAngle
        '
        Me.lblRefeditAngle.AutoSize = True
        Me.lblRefeditAngle.Location = New System.Drawing.Point(13, 62)
        Me.lblRefeditAngle.Name = "lblRefeditAngle"
        Me.lblRefeditAngle.Size = New System.Drawing.Size(42, 16)
        Me.lblRefeditAngle.TabIndex = 3
        Me.lblRefeditAngle.Text = "Angle"
        '
        'lblRefeditRadius
        '
        Me.lblRefeditRadius.AutoSize = True
        Me.lblRefeditRadius.Location = New System.Drawing.Point(13, 22)
        Me.lblRefeditRadius.Name = "lblRefeditRadius"
        Me.lblRefeditRadius.Size = New System.Drawing.Size(50, 16)
        Me.lblRefeditRadius.TabIndex = 2
        Me.lblRefeditRadius.Text = "Radius"
        '
        'grpZeroAngle
        '
        Me.grpZeroAngle.Controls.Add(Me.optWest)
        Me.grpZeroAngle.Controls.Add(Me.optEast)
        Me.grpZeroAngle.Controls.Add(Me.optNorth)
        Me.grpZeroAngle.Controls.Add(Me.optSouth)
        Me.grpZeroAngle.Location = New System.Drawing.Point(16, 127)
        Me.grpZeroAngle.Name = "grpZeroAngle"
        Me.grpZeroAngle.Size = New System.Drawing.Size(202, 89)
        Me.grpZeroAngle.TabIndex = 9
        Me.grpZeroAngle.TabStop = False
        Me.grpZeroAngle.Text = "Zero Angle"
        '
        'optNorth
        '
        Me.optNorth.AutoSize = True
        Me.optNorth.Location = New System.Drawing.Point(6, 21)
        Me.optNorth.Name = "optNorth"
        Me.optNorth.Size = New System.Drawing.Size(60, 20)
        Me.optNorth.TabIndex = 4
        Me.optNorth.Text = "North"
        Me.optNorth.UseVisualStyleBackColor = True
        '
        'optSouth
        '
        Me.optSouth.AutoSize = True
        Me.optSouth.Location = New System.Drawing.Point(5, 47)
        Me.optSouth.Name = "optSouth"
        Me.optSouth.Size = New System.Drawing.Size(62, 20)
        Me.optSouth.TabIndex = 5
        Me.optSouth.Text = "South"
        Me.optSouth.UseVisualStyleBackColor = True
        '
        'optEast
        '
        Me.optEast.AutoSize = True
        Me.optEast.Checked = True
        Me.optEast.Location = New System.Drawing.Point(88, 21)
        Me.optEast.Name = "optEast"
        Me.optEast.Size = New System.Drawing.Size(55, 20)
        Me.optEast.TabIndex = 6
        Me.optEast.TabStop = True
        Me.optEast.Text = "East"
        Me.optEast.UseVisualStyleBackColor = True
        '
        'optWest
        '
        Me.optWest.AutoSize = True
        Me.optWest.Location = New System.Drawing.Point(88, 47)
        Me.optWest.Name = "optWest"
        Me.optWest.Size = New System.Drawing.Size(59, 20)
        Me.optWest.TabIndex = 7
        Me.optWest.Text = "West"
        Me.optWest.UseVisualStyleBackColor = True
        '
        'ckConnectPoints
        '
        Me.ckConnectPoints.AutoSize = True
        Me.ckConnectPoints.Checked = True
        Me.ckConnectPoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckConnectPoints.Location = New System.Drawing.Point(229, 133)
        Me.ckConnectPoints.Name = "ckConnectPoints"
        Me.ckConnectPoints.Size = New System.Drawing.Size(118, 20)
        Me.ckConnectPoints.TabIndex = 10
        Me.ckConnectPoints.Text = "Connect Points"
        Me.ckConnectPoints.UseVisualStyleBackColor = True
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
        'TabPage_Options
        '
        Me.TabPage_Options.Controls.Add(Me.tbRadialTickInterval)
        Me.TabPage_Options.Controls.Add(Me.tbAngularTickInterval)
        Me.TabPage_Options.Controls.Add(Me.lblRadialTickInterval)
        Me.TabPage_Options.Controls.Add(Me.lblAngularTickInterval)
        Me.TabPage_Options.Location = New System.Drawing.Point(4, 25)
        Me.TabPage_Options.Name = "TabPage_Options"
        Me.TabPage_Options.Size = New System.Drawing.Size(448, 403)
        Me.TabPage_Options.TabIndex = 1
        Me.TabPage_Options.Text = "Options"
        Me.TabPage_Options.UseVisualStyleBackColor = True
        '
        'lblAngularTickInterval
        '
        Me.lblAngularTickInterval.AutoSize = True
        Me.lblAngularTickInterval.Location = New System.Drawing.Point(20, 28)
        Me.lblAngularTickInterval.Name = "lblAngularTickInterval"
        Me.lblAngularTickInterval.Size = New System.Drawing.Size(128, 16)
        Me.lblAngularTickInterval.TabIndex = 0
        Me.lblAngularTickInterval.Text = "Angular Tick Interval"
        '
        'lblRadialTickInterval
        '
        Me.lblRadialTickInterval.AutoSize = True
        Me.lblRadialTickInterval.Location = New System.Drawing.Point(20, 59)
        Me.lblRadialTickInterval.Name = "lblRadialTickInterval"
        Me.lblRadialTickInterval.Size = New System.Drawing.Size(122, 16)
        Me.lblRadialTickInterval.TabIndex = 1
        Me.lblRadialTickInterval.Text = "Radial Tick Interval"
        '
        'RefEdit_GroupID
        '
        Me.RefEdit_GroupID.Address = ""
        Me.RefEdit_GroupID.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_GroupID.ExcelConnector = Nothing
        Me.RefEdit_GroupID.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_GroupID.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_GroupID.Location = New System.Drawing.Point(136, 101)
        Me.RefEdit_GroupID.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_GroupID.Name = "RefEdit_GroupID"
        Me.RefEdit_GroupID.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_GroupID.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_GroupID.TabIndex = 7
        '
        'RefEdit_Angle
        '
        Me.RefEdit_Angle.Address = ""
        Me.RefEdit_Angle.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_Angle.ExcelConnector = Nothing
        Me.RefEdit_Angle.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_Angle.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_Angle.Location = New System.Drawing.Point(136, 62)
        Me.RefEdit_Angle.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_Angle.Name = "RefEdit_Angle"
        Me.RefEdit_Angle.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_Angle.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_Angle.TabIndex = 5
        '
        'RefEdit_Radius
        '
        Me.RefEdit_Radius.Address = ""
        Me.RefEdit_Radius.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit_Radius.ExcelConnector = Nothing
        Me.RefEdit_Radius.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit_Radius.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit_Radius.Location = New System.Drawing.Point(136, 22)
        Me.RefEdit_Radius.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit_Radius.Name = "RefEdit_Radius"
        Me.RefEdit_Radius.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit_Radius.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit_Radius.TabIndex = 4
        '
        'tbAngularTickInterval
        '
        Me.tbAngularTickInterval.Location = New System.Drawing.Point(154, 25)
        Me.tbAngularTickInterval.Name = "tbAngularTickInterval"
        Me.tbAngularTickInterval.Size = New System.Drawing.Size(100, 22)
        Me.tbAngularTickInterval.TabIndex = 2
        '
        'tbRadialTickInterval
        '
        Me.tbRadialTickInterval.Location = New System.Drawing.Point(154, 56)
        Me.tbRadialTickInterval.Name = "tbRadialTickInterval"
        Me.tbRadialTickInterval.Size = New System.Drawing.Size(100, 22)
        Me.tbRadialTickInterval.TabIndex = 3
        '
        'Ui01PolarPlot
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(457, 471)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.TabMultipage)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(475, 518)
        Me.MinimumSize = New System.Drawing.Size(475, 518)
        Me.Name = "Ui01PolarPlot"
        Me.ShowIcon = False
        Me.Text = "Polar Plot"
        Me.TabMultipage.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.grpOptions.ResumeLayout(False)
        Me.grpOptions.PerformLayout()
        Me.grpRotation.ResumeLayout(False)
        Me.grpRotation.PerformLayout()
        Me.grpAngleUnit.ResumeLayout(False)
        Me.grpAngleUnit.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.grpZeroAngle.ResumeLayout(False)
        Me.grpZeroAngle.PerformLayout()
        Me.TabPage_Options.ResumeLayout(False)
        Me.TabPage_Options.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents TabMultipage As Windows.Forms.TabControl
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents RefEdit_Angle As Excel2007RefEdit
    Friend WithEvents RefEdit_Radius As Excel2007RefEdit
    Friend WithEvents lblRefeditAngle As Windows.Forms.Label
    Friend WithEvents lblRefeditRadius As Windows.Forms.Label
    Friend WithEvents grpOptions As Windows.Forms.GroupBox
    Friend WithEvents optPercentage As Windows.Forms.RadioButton
    Friend WithEvents optDegrees As Windows.Forms.RadioButton
    Friend WithEvents optRadians As Windows.Forms.RadioButton
    Friend WithEvents grpRotation As Windows.Forms.GroupBox
    Friend WithEvents optClockwise As Windows.Forms.RadioButton
    Friend WithEvents optCounterClockwise As Windows.Forms.RadioButton
    Friend WithEvents grpAngleUnit As Windows.Forms.GroupBox
    Friend WithEvents ckConnectPoints As Windows.Forms.CheckBox
    Friend WithEvents grpZeroAngle As Windows.Forms.GroupBox
    Friend WithEvents optWest As Windows.Forms.RadioButton
    Friend WithEvents optEast As Windows.Forms.RadioButton
    Friend WithEvents optNorth As Windows.Forms.RadioButton
    Friend WithEvents optSouth As Windows.Forms.RadioButton
    Friend WithEvents RefEdit_GroupID As Excel2007RefEdit
    Friend WithEvents lblGroup As Windows.Forms.Label
    Friend WithEvents TabPage_Options As Windows.Forms.TabPage
    Friend WithEvents lblAngularTickInterval As Windows.Forms.Label
    Friend WithEvents lblRadialTickInterval As Windows.Forms.Label
    Friend WithEvents tbRadialTickInterval As Windows.Forms.TextBox
    Friend WithEvents tbAngularTickInterval As Windows.Forms.TextBox
End Class
