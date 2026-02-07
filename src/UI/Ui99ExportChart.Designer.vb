<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Ui99ExportChart
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
        Me.btExport = New System.Windows.Forms.Button()
        Me.lblDPI = New System.Windows.Forms.Label()
        Me.spinBtnDPI = New System.Windows.Forms.NumericUpDown()
        Me.cbFormat = New System.Windows.Forms.ComboBox()
        Me.lblFormat = New System.Windows.Forms.Label()
        Me.cbAspectRatio = New System.Windows.Forms.CheckBox()
        Me.spinBtnHeight = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblHeight = New System.Windows.Forms.Label()
        Me.lblWidth = New System.Windows.Forms.Label()
        Me.cbSheets = New System.Windows.Forms.ComboBox()
        Me.cbCharts = New System.Windows.Forms.ComboBox()
        Me.lblSheets = New System.Windows.Forms.Label()
        Me.lblCharts = New System.Windows.Forms.Label()
        Me.spinBtnJPGquality = New System.Windows.Forms.NumericUpDown()
        Me.lblJPGquality = New System.Windows.Forms.Label()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.grpExportFormat = New System.Windows.Forms.GroupBox()
        CType(Me.spinBtnDPI, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnJPGquality, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpInput.SuspendLayout()
        Me.grpExportFormat.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(415, 230)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 8
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btExport
        '
        Me.btExport.Location = New System.Drawing.Point(496, 230)
        Me.btExport.Name = "btExport"
        Me.btExport.Size = New System.Drawing.Size(75, 23)
        Me.btExport.TabIndex = 7
        Me.btExport.Text = "Export"
        Me.btExport.UseVisualStyleBackColor = True
        '
        'lblDPI
        '
        Me.lblDPI.AutoSize = True
        Me.lblDPI.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDPI.Location = New System.Drawing.Point(73, 63)
        Me.lblDPI.Name = "lblDPI"
        Me.lblDPI.Size = New System.Drawing.Size(29, 16)
        Me.lblDPI.TabIndex = 11
        Me.lblDPI.Text = "DPI"
        '
        'spinBtnDPI
        '
        Me.spinBtnDPI.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnDPI.Location = New System.Drawing.Point(113, 57)
        Me.spinBtnDPI.Maximum = New Decimal(New Integer() {1200, 0, 0, 0})
        Me.spinBtnDPI.Minimum = New Decimal(New Integer() {72, 0, 0, 0})
        Me.spinBtnDPI.Name = "spinBtnDPI"
        Me.spinBtnDPI.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnDPI.TabIndex = 10
        Me.spinBtnDPI.Value = New Decimal(New Integer() {300, 0, 0, 0})
        '
        'cbFormat
        '
        Me.cbFormat.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFormat.FormattingEnabled = True
        Me.cbFormat.Location = New System.Drawing.Point(113, 25)
        Me.cbFormat.Name = "cbFormat"
        Me.cbFormat.Size = New System.Drawing.Size(121, 24)
        Me.cbFormat.TabIndex = 12
        '
        'lblFormat
        '
        Me.lblFormat.AutoSize = True
        Me.lblFormat.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFormat.Location = New System.Drawing.Point(17, 33)
        Me.lblFormat.Name = "lblFormat"
        Me.lblFormat.Size = New System.Drawing.Size(90, 16)
        Me.lblFormat.TabIndex = 13
        Me.lblFormat.Text = "Export Format"
        '
        'cbAspectRatio
        '
        Me.cbAspectRatio.AutoSize = True
        Me.cbAspectRatio.Checked = True
        Me.cbAspectRatio.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbAspectRatio.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbAspectRatio.Location = New System.Drawing.Point(193, 86)
        Me.cbAspectRatio.Name = "cbAspectRatio"
        Me.cbAspectRatio.Size = New System.Drawing.Size(164, 20)
        Me.cbAspectRatio.TabIndex = 14
        Me.cbAspectRatio.Text = "Preserve Aspect Ratio"
        Me.cbAspectRatio.UseVisualStyleBackColor = True
        '
        'spinBtnHeight
        '
        Me.spinBtnHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnHeight.Location = New System.Drawing.Point(113, 85)
        Me.spinBtnHeight.Maximum = New Decimal(New Integer() {20000, 0, 0, 0})
        Me.spinBtnHeight.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnHeight.Name = "spinBtnHeight"
        Me.spinBtnHeight.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnHeight.TabIndex = 15
        Me.spinBtnHeight.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'spinBtnWidth
        '
        Me.spinBtnWidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnWidth.Location = New System.Drawing.Point(113, 113)
        Me.spinBtnWidth.Maximum = New Decimal(New Integer() {20000, 0, 0, 0})
        Me.spinBtnWidth.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnWidth.Name = "spinBtnWidth"
        Me.spinBtnWidth.Size = New System.Drawing.Size(67, 22)
        Me.spinBtnWidth.TabIndex = 16
        Me.spinBtnWidth.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblHeight
        '
        Me.lblHeight.AutoSize = True
        Me.lblHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHeight.Location = New System.Drawing.Point(15, 91)
        Me.lblHeight.Name = "lblHeight"
        Me.lblHeight.Size = New System.Drawing.Size(92, 16)
        Me.lblHeight.TabIndex = 17
        Me.lblHeight.Text = "Height (pixels)"
        '
        'lblWidth
        '
        Me.lblWidth.AutoSize = True
        Me.lblWidth.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblWidth.Location = New System.Drawing.Point(20, 119)
        Me.lblWidth.Name = "lblWidth"
        Me.lblWidth.Size = New System.Drawing.Size(87, 16)
        Me.lblWidth.TabIndex = 18
        Me.lblWidth.Text = "Width (pixels)"
        '
        'cbSheets
        '
        Me.cbSheets.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbSheets.FormattingEnabled = True
        Me.cbSheets.Location = New System.Drawing.Point(96, 27)
        Me.cbSheets.Name = "cbSheets"
        Me.cbSheets.Size = New System.Drawing.Size(213, 24)
        Me.cbSheets.TabIndex = 20
        '
        'cbCharts
        '
        Me.cbCharts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbCharts.FormattingEnabled = True
        Me.cbCharts.Location = New System.Drawing.Point(359, 27)
        Me.cbCharts.Name = "cbCharts"
        Me.cbCharts.Size = New System.Drawing.Size(178, 24)
        Me.cbCharts.TabIndex = 21
        '
        'lblSheets
        '
        Me.lblSheets.AutoSize = True
        Me.lblSheets.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSheets.Location = New System.Drawing.Point(18, 35)
        Me.lblSheets.Name = "lblSheets"
        Me.lblSheets.Size = New System.Drawing.Size(72, 16)
        Me.lblSheets.TabIndex = 22
        Me.lblSheets.Text = "Worksheet"
        '
        'lblCharts
        '
        Me.lblCharts.AutoSize = True
        Me.lblCharts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCharts.Location = New System.Drawing.Point(315, 35)
        Me.lblCharts.Name = "lblCharts"
        Me.lblCharts.Size = New System.Drawing.Size(38, 16)
        Me.lblCharts.TabIndex = 23
        Me.lblCharts.Text = "Chart"
        '
        'spinBtnJPGquality
        '
        Me.spinBtnJPGquality.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnJPGquality.Location = New System.Drawing.Point(325, 26)
        Me.spinBtnJPGquality.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.spinBtnJPGquality.Name = "spinBtnJPGquality"
        Me.spinBtnJPGquality.Size = New System.Drawing.Size(52, 22)
        Me.spinBtnJPGquality.TabIndex = 24
        Me.spinBtnJPGquality.Value = New Decimal(New Integer() {92, 0, 0, 0})
        '
        'lblJPGquality
        '
        Me.lblJPGquality.AutoSize = True
        Me.lblJPGquality.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblJPGquality.Location = New System.Drawing.Point(244, 28)
        Me.lblJPGquality.Name = "lblJPGquality"
        Me.lblJPGquality.Size = New System.Drawing.Size(75, 16)
        Me.lblJPGquality.TabIndex = 25
        Me.lblJPGquality.Text = "JPG quality"
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.lblSheets)
        Me.grpInput.Controls.Add(Me.cbSheets)
        Me.grpInput.Controls.Add(Me.cbCharts)
        Me.grpInput.Controls.Add(Me.lblCharts)
        Me.grpInput.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpInput.Location = New System.Drawing.Point(15, 12)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(556, 87)
        Me.grpInput.TabIndex = 26
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Select Chart"
        '
        'grpExportFormat
        '
        Me.grpExportFormat.Controls.Add(Me.lblFormat)
        Me.grpExportFormat.Controls.Add(Me.spinBtnDPI)
        Me.grpExportFormat.Controls.Add(Me.lblJPGquality)
        Me.grpExportFormat.Controls.Add(Me.lblDPI)
        Me.grpExportFormat.Controls.Add(Me.spinBtnJPGquality)
        Me.grpExportFormat.Controls.Add(Me.cbFormat)
        Me.grpExportFormat.Controls.Add(Me.lblWidth)
        Me.grpExportFormat.Controls.Add(Me.cbAspectRatio)
        Me.grpExportFormat.Controls.Add(Me.lblHeight)
        Me.grpExportFormat.Controls.Add(Me.spinBtnHeight)
        Me.grpExportFormat.Controls.Add(Me.spinBtnWidth)
        Me.grpExportFormat.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpExportFormat.Location = New System.Drawing.Point(15, 105)
        Me.grpExportFormat.Name = "grpExportFormat"
        Me.grpExportFormat.Size = New System.Drawing.Size(391, 148)
        Me.grpExportFormat.TabIndex = 27
        Me.grpExportFormat.TabStop = False
        Me.grpExportFormat.Text = "Chart Export Settings"
        '
        'Ui99ExportChart
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(577, 262)
        Me.Controls.Add(Me.grpExportFormat)
        Me.Controls.Add(Me.grpInput)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btExport)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(595, 309)
        Me.MinimumSize = New System.Drawing.Size(595, 309)
        Me.Name = "Ui99ExportChart"
        Me.ShowIcon = False
        Me.Text = "Export Chart"
        CType(Me.spinBtnDPI, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnJPGquality, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.grpExportFormat.ResumeLayout(False)
        Me.grpExportFormat.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btExport As Windows.Forms.Button
    Friend WithEvents lblDPI As Windows.Forms.Label
    Friend WithEvents spinBtnDPI As Windows.Forms.NumericUpDown
    Friend WithEvents cbFormat As Windows.Forms.ComboBox
    Friend WithEvents lblFormat As Windows.Forms.Label
    Friend WithEvents cbAspectRatio As Windows.Forms.CheckBox
    Friend WithEvents spinBtnHeight As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnWidth As Windows.Forms.NumericUpDown
    Friend WithEvents lblHeight As Windows.Forms.Label
    Friend WithEvents lblWidth As Windows.Forms.Label
    Friend WithEvents cbSheets As Windows.Forms.ComboBox
    Friend WithEvents cbCharts As Windows.Forms.ComboBox
    Friend WithEvents lblSheets As Windows.Forms.Label
    Friend WithEvents lblCharts As Windows.Forms.Label
    Friend WithEvents spinBtnJPGquality As Windows.Forms.NumericUpDown
    Friend WithEvents lblJPGquality As Windows.Forms.Label
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents grpExportFormat As Windows.Forms.GroupBox
End Class
