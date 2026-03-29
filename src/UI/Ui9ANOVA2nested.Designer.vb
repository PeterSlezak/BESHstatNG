<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Ui9ANOVA2nested
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui9ANOVA2nested))
        Me.grpOutput = New System.Windows.Forms.GroupBox()
        Me.RefEditOutput = New Global.BESHStatNG.Excel2007RefEdit()
        Me.optWorkbook = New System.Windows.Forms.RadioButton()
        Me.optWorksheet = New System.Windows.Forms.RadioButton()
        Me.optOutputRange = New System.Windows.Forms.RadioButton()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCompute = New System.Windows.Forms.Button()
        Me.grpInput = New System.Windows.Forms.GroupBox()
        Me.lblRefedit2_Nested = New System.Windows.Forms.Label()
        Me.RefEdit2_Nested = New Global.BESHStatNG.Excel2007RefEdit()
        Me.lblRefedit3_Data = New System.Windows.Forms.Label()
        Me.lblRefedit1_Group = New System.Windows.Forms.Label()
        Me.RefEdit1_Group = New Global.BESHStatNG.Excel2007RefEdit()
        Me.RefEdit3_Data = New Global.BESHStatNG.Excel2007RefEdit()
        Me.grpOutput.SuspendLayout()
        Me.grpInput.SuspendLayout()
        Me.SuspendLayout()
        '
        'grpOutput
        '
        Me.grpOutput.Controls.Add(Me.RefEditOutput)
        Me.grpOutput.Controls.Add(Me.optWorkbook)
        Me.grpOutput.Controls.Add(Me.optWorksheet)
        Me.grpOutput.Controls.Add(Me.optOutputRange)
        Me.grpOutput.Location = New System.Drawing.Point(12, 183)
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
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(298, 319)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 7
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCompute
        '
        Me.btCompute.Location = New System.Drawing.Point(379, 319)
        Me.btCompute.Name = "btCompute"
        Me.btCompute.Size = New System.Drawing.Size(75, 23)
        Me.btCompute.TabIndex = 6
        Me.btCompute.Text = "Compute"
        Me.btCompute.UseVisualStyleBackColor = True
        '
        'grpInput
        '
        Me.grpInput.Controls.Add(Me.lblRefedit2_Nested)
        Me.grpInput.Controls.Add(Me.RefEdit2_Nested)
        Me.grpInput.Controls.Add(Me.lblRefedit3_Data)
        Me.grpInput.Controls.Add(Me.lblRefedit1_Group)
        Me.grpInput.Controls.Add(Me.RefEdit1_Group)
        Me.grpInput.Controls.Add(Me.RefEdit3_Data)
        Me.grpInput.Location = New System.Drawing.Point(11, 12)
        Me.grpInput.Name = "grpInput"
        Me.grpInput.Size = New System.Drawing.Size(442, 165)
        Me.grpInput.TabIndex = 8
        Me.grpInput.TabStop = False
        Me.grpInput.Text = "Input"
        '
        'lblRefedit2_Nested
        '
        Me.lblRefedit2_Nested.AutoSize = True
        Me.lblRefedit2_Nested.Location = New System.Drawing.Point(9, 77)
        Me.lblRefedit2_Nested.Name = "lblRefedit2_Nested"
        Me.lblRefedit2_Nested.Size = New System.Drawing.Size(92, 16)
        Me.lblRefedit2_Nested.TabIndex = 7
        Me.lblRefedit2_Nested.Text = "Nested Factor"
        '
        'RefEdit2_Nested
        '
        Me.RefEdit2_Nested.Address = ""
        Me.RefEdit2_Nested.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit2_Nested.ExcelConnector = Nothing
        Me.RefEdit2_Nested.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit2_Nested.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit2_Nested.Location = New System.Drawing.Point(152, 77)
        Me.RefEdit2_Nested.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit2_Nested.Name = "RefEdit2_Nested"
        Me.RefEdit2_Nested.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit2_Nested.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit2_Nested.TabIndex = 6
        '
        'lblRefedit3_Data
        '
        Me.lblRefedit3_Data.AutoSize = True
        Me.lblRefedit3_Data.Location = New System.Drawing.Point(9, 117)
        Me.lblRefedit3_Data.Name = "lblRefedit3_Data"
        Me.lblRefedit3_Data.Size = New System.Drawing.Size(39, 16)
        Me.lblRefedit3_Data.TabIndex = 3
        Me.lblRefedit3_Data.Text = "Data:"
        '
        'lblRefedit1_Group
        '
        Me.lblRefedit1_Group.AutoSize = True
        Me.lblRefedit1_Group.Location = New System.Drawing.Point(9, 37)
        Me.lblRefedit1_Group.Name = "lblRefedit1_Group"
        Me.lblRefedit1_Group.Size = New System.Drawing.Size(85, 16)
        Me.lblRefedit1_Group.TabIndex = 2
        Me.lblRefedit1_Group.Text = "Group Factor"
        '
        'RefEdit1_Group
        '
        Me.RefEdit1_Group.Address = ""
        Me.RefEdit1_Group.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit1_Group.ExcelConnector = Nothing
        Me.RefEdit1_Group.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit1_Group.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit1_Group.Location = New System.Drawing.Point(152, 37)
        Me.RefEdit1_Group.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit1_Group.Name = "RefEdit1_Group"
        Me.RefEdit1_Group.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit1_Group.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit1_Group.TabIndex = 4
        '
        'RefEdit3_Data
        '
        Me.RefEdit3_Data.Address = ""
        Me.RefEdit3_Data.BackColor = System.Drawing.Color.Transparent
        Me.RefEdit3_Data.ExcelConnector = Nothing
        Me.RefEdit3_Data.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.RefEdit3_Data.ImageMinimized = Global.BESHStatNG.My.Resources.Resources.imgMinimized
        Me.RefEdit3_Data.Location = New System.Drawing.Point(152, 117)
        Me.RefEdit3_Data.Margin = New System.Windows.Forms.Padding(4)
        Me.RefEdit3_Data.Name = "RefEdit3_Data"
        Me.RefEdit3_Data.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RefEdit3_Data.Size = New System.Drawing.Size(283, 32)
        Me.RefEdit3_Data.TabIndex = 5
        '
        'Ui9ANOVA2nested
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(465, 351)
        Me.Controls.Add(Me.grpInput)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCompute)
        Me.Controls.Add(Me.grpOutput)
        Me.MaximizeBox = False
        Me.MinimumSize = New System.Drawing.Size(483, 398)
        Me.Name = "Ui9ANOVA2nested"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Ui9ANOVA2nested"
        Me.grpOutput.ResumeLayout(False)
        Me.grpOutput.PerformLayout()
        Me.grpInput.ResumeLayout(False)
        Me.grpInput.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents grpOutput As Windows.Forms.GroupBox
    Friend WithEvents RefEditOutput As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents optWorkbook As Windows.Forms.RadioButton
    Friend WithEvents optWorksheet As Windows.Forms.RadioButton
    Friend WithEvents optOutputRange As Windows.Forms.RadioButton
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCompute As Windows.Forms.Button
    Friend WithEvents grpInput As Windows.Forms.GroupBox
    Friend WithEvents lblRefedit3_Data As Windows.Forms.Label
    Friend WithEvents lblRefedit1_Group As Windows.Forms.Label
    Friend WithEvents RefEdit1_Group As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents RefEdit3_Data As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents lblRefedit2_Nested As Windows.Forms.Label
    Friend WithEvents RefEdit2_Nested As Global.BESHStatNG.Excel2007RefEdit
End Class
