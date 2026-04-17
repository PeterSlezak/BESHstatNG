<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Ui11PCA
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Ui11PCA))
        Me.TabPageOptionsSPM = New System.Windows.Forms.TabPage()
        Me.ckShowRegressionLines = New System.Windows.Forms.CheckBox()
        Me.ckDisplayCorrelCoef = New System.Windows.Forms.CheckBox()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.cbGruppingVar = New System.Windows.Forms.ComboBox()
        Me.lblGruppingVar = New System.Windows.Forms.Label()
        Me.cbKmeansRowLabel = New System.Windows.Forms.ComboBox()
        Me.lblKmeansRowLabel = New System.Windows.Forms.Label()
        Me.ckFirstRow = New System.Windows.Forms.CheckBox()
        Me.lbXs = New System.Windows.Forms.ListBox()
        Me.cbSheetsList = New System.Windows.Forms.ComboBox()
        Me.btReload = New System.Windows.Forms.Button()
        Me.btRemoveX = New System.Windows.Forms.Button()
        Me.btAddX = New System.Windows.Forms.Button()
        Me.lbAllColumns = New System.Windows.Forms.ListBox()
        Me.lblAllColumns = New System.Windows.Forms.Label()
        Me.lblX = New System.Windows.Forms.Label()
        Me.lblSelectedSheet = New System.Windows.Forms.Label()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPageOptionsPCA = New System.Windows.Forms.TabPage()
        Me.grpExtract = New System.Windows.Forms.GroupBox()
        Me.spinBtnExtractEigen = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnExtractComp = New System.Windows.Forms.NumericUpDown()
        Me.spinBtnExtractVariance = New System.Windows.Forms.NumericUpDown()
        Me.optExtractVariance = New System.Windows.Forms.RadioButton()
        Me.optExtractFixed = New System.Windows.Forms.RadioButton()
        Me.optExtractEigen = New System.Windows.Forms.RadioButton()
        Me.grpAnalyzeType = New System.Windows.Forms.GroupBox()
        Me.optCovar = New System.Windows.Forms.RadioButton()
        Me.optCorr = New System.Windows.Forms.RadioButton()
        Me.grpIterOptions = New System.Windows.Forms.GroupBox()
        Me.tbMaxIter = New System.Windows.Forms.TextBox()
        Me.lblMaxIter = New System.Windows.Forms.Label()
        Me.lblEps = New System.Windows.Forms.Label()
        Me.tbEps = New System.Windows.Forms.TextBox()
        Me.TabPageOptionsKmeans = New System.Windows.Forms.TabPage()
        Me.lblCenterHint = New System.Windows.Forms.Label()
        Me.lblCenterRef = New System.Windows.Forms.Label()
        Me.grpKmeansFit = New System.Windows.Forms.GroupBox()
        Me.tbKmeansSeed = New System.Windows.Forms.TextBox()
        Me.lblSeed = New System.Windows.Forms.Label()
        Me.tbKmeansTolerance = New System.Windows.Forms.TextBox()
        Me.nudKmeansMaxIterations = New System.Windows.Forms.NumericUpDown()
        Me.nudKmeansStarts = New System.Windows.Forms.NumericUpDown()
        Me.lblTol = New System.Windows.Forms.Label()
        Me.lblKmeanMaxIter = New System.Windows.Forms.Label()
        Me.lblStarts = New System.Windows.Forms.Label()
        Me.grpKmeansPreprocess = New System.Windows.Forms.GroupBox()
        Me.cbKmeansEmptyCluster = New System.Windows.Forms.ComboBox()
        Me.lblEmpty = New System.Windows.Forms.Label()
        Me.cbKmeansMissingPolicy = New System.Windows.Forms.ComboBox()
        Me.lblMissing = New System.Windows.Forms.Label()
        Me.cbKmeansStandardization = New System.Windows.Forms.ComboBox()
        Me.lblStd = New System.Windows.Forms.Label()
        Me.grpKmeansBasic = New System.Windows.Forms.GroupBox()
        Me.cbKmeansDistance = New System.Windows.Forms.ComboBox()
        Me.lblDist = New System.Windows.Forms.Label()
        Me.cbKmeansInitialization = New System.Windows.Forms.ComboBox()
        Me.lblInit = New System.Windows.Forms.Label()
        Me.lblK = New System.Windows.Forms.Label()
        Me.nudKmeansClusters = New System.Windows.Forms.NumericUpDown()
        Me.TabPageOptionsHierarchicalClustering = New System.Windows.Forms.TabPage()
        Me.grpHierarchicalDendrogram = New System.Windows.Forms.GroupBox()
        Me.ckHierarchicalCreateDendrogram = New System.Windows.Forms.CheckBox()
        Me.cbHierarchicalLabelMode = New System.Windows.Forms.ComboBox()
        Me.cbHierarchicalOrientation = New System.Windows.Forms.ComboBox()
        Me.lblOrientation = New System.Windows.Forms.Label()
        Me.cbHierarchicalHeightMode = New System.Windows.Forms.ComboBox()
        Me.lblLabelMode = New System.Windows.Forms.Label()
        Me.lblHeightMode = New System.Windows.Forms.Label()
        Me.grpHierarchicalMembership = New System.Windows.Forms.GroupBox()
        Me.nudHierarchicalClusters = New System.Windows.Forms.NumericUpDown()
        Me.tbHierarchicalCutHeight = New System.Windows.Forms.TextBox()
        Me.optHierarchicalCutByHeight = New System.Windows.Forms.RadioButton()
        Me.optHierarchicalCutByClusters = New System.Windows.Forms.RadioButton()
        Me.lblMembershipHint = New System.Windows.Forms.Label()
        Me.grpHierarchicalPreprocess = New System.Windows.Forms.GroupBox()
        Me.cbHierarchicalMissingPolicy = New System.Windows.Forms.ComboBox()
        Me.lblMissingHierarchicalClustering = New System.Windows.Forms.Label()
        Me.cbHierarchicalStandardization = New System.Windows.Forms.ComboBox()
        Me.lblStdHierarchicalClustering = New System.Windows.Forms.Label()
        Me.grpHierarchicalBasic = New System.Windows.Forms.GroupBox()
        Me.tbHierarchicalMinkowskiPower = New System.Windows.Forms.TextBox()
        Me.cbHierarchicalDistance = New System.Windows.Forms.ComboBox()
        Me.lblDistance = New System.Windows.Forms.Label()
        Me.cbHierarchicalLinkage = New System.Windows.Forms.ComboBox()
        Me.lblHierarchicalMinkowskiPower = New System.Windows.Forms.Label()
        Me.lblLinkage = New System.Windows.Forms.Label()
        Me.TabPageOptionsFA = New System.Windows.Forms.TabPage()
        Me.grpFAIterations = New System.Windows.Forms.GroupBox()
        Me.tbFAEps = New System.Windows.Forms.TextBox()
        Me.nudFAMaxIter = New System.Windows.Forms.NumericUpDown()
        Me.lblFAEps = New System.Windows.Forms.Label()
        Me.lblFAMaxIter = New System.Windows.Forms.Label()
        Me.grpFAScoring = New System.Windows.Forms.GroupBox()
        Me.cbFAScoreMethod = New System.Windows.Forms.ComboBox()
        Me.lblFaScoreMethod = New System.Windows.Forms.Label()
        Me.grpFARotation = New System.Windows.Forms.GroupBox()
        Me.lblFAPromaxPower = New System.Windows.Forms.Label()
        Me.ckFAKaiserNormalization = New System.Windows.Forms.CheckBox()
        Me.cbFARotation = New System.Windows.Forms.ComboBox()
        Me.lblFaRotationMethod = New System.Windows.Forms.Label()
        Me.nudFAPromaxPower = New System.Windows.Forms.NumericUpDown()
        Me.grpFARetention = New System.Windows.Forms.GroupBox()
        Me.nudFAVariance = New System.Windows.Forms.NumericUpDown()
        Me.nudFAEigen = New System.Windows.Forms.NumericUpDown()
        Me.nudFAFactors = New System.Windows.Forms.NumericUpDown()
        Me.optFAExtractVariance = New System.Windows.Forms.RadioButton()
        Me.optFAExtractEigen = New System.Windows.Forms.RadioButton()
        Me.optFAExtractFixed = New System.Windows.Forms.RadioButton()
        Me.grpFABasic = New System.Windows.Forms.GroupBox()
        Me.cbFAMissingPolicy = New System.Windows.Forms.ComboBox()
        Me.lblMissingFa = New System.Windows.Forms.Label()
        Me.cbFACommunalityInit = New System.Windows.Forms.ComboBox()
        Me.lblFaStartingCommunalities = New System.Windows.Forms.Label()
        Me.cbFAExtraction = New System.Windows.Forms.ComboBox()
        Me.lblFaExtractionMethod = New System.Windows.Forms.Label()
        Me.optFACovariance = New System.Windows.Forms.RadioButton()
        Me.optFACorrelation = New System.Windows.Forms.RadioButton()
        Me.TabPageOptionsDA = New System.Windows.Forms.TabPage()
        Me.grpDAPrior = New System.Windows.Forms.GroupBox()
        Me.tbDAUserPriors = New System.Windows.Forms.TextBox()
        Me.lblDAUserPriors = New System.Windows.Forms.Label()
        Me.cbDAPriors = New System.Windows.Forms.ComboBox()
        Me.lblDAPriors = New System.Windows.Forms.Label()
        Me.grpDAValidation = New System.Windows.Forms.GroupBox()
        Me.tbDASeed = New System.Windows.Forms.TextBox()
        Me.tbDAHoldoutFraction = New System.Windows.Forms.NumericUpDown()
        Me.lblDASeed = New System.Windows.Forms.Label()
        Me.ckDAStratified = New System.Windows.Forms.CheckBox()
        Me.lblDAHoldoutFraction = New System.Windows.Forms.Label()
        Me.nudDAFolds = New System.Windows.Forms.NumericUpDown()
        Me.lblDAFolds = New System.Windows.Forms.Label()
        Me.cbDAValidation = New System.Windows.Forms.ComboBox()
        Me.lblDAValidation = New System.Windows.Forms.Label()
        Me.grpDABasic = New System.Windows.Forms.GroupBox()
        Me.tbDARegularization = New System.Windows.Forms.TextBox()
        Me.lblDARegularization = New System.Windows.Forms.Label()
        Me.cbDAMethod = New System.Windows.Forms.ComboBox()
        Me.cbDAStandardization = New System.Windows.Forms.ComboBox()
        Me.lblDAStandardization = New System.Windows.Forms.Label()
        Me.cbDAMissingPolicy = New System.Windows.Forms.ComboBox()
        Me.lblMissingDA = New System.Windows.Forms.Label()
        Me.lblDAMethod = New System.Windows.Forms.Label()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btCalculate = New System.Windows.Forms.Button()
        Me.refKmeansCenters = New Global.BESHStatNG.Excel2007RefEdit()
        Me.TabPageOptionsSPM.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPageOptionsPCA.SuspendLayout()
        Me.grpExtract.SuspendLayout()
        CType(Me.spinBtnExtractEigen, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnExtractComp, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinBtnExtractVariance, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpAnalyzeType.SuspendLayout()
        Me.grpIterOptions.SuspendLayout()
        Me.TabPageOptionsKmeans.SuspendLayout()
        Me.grpKmeansFit.SuspendLayout()
        CType(Me.nudKmeansMaxIterations, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudKmeansStarts, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpKmeansPreprocess.SuspendLayout()
        Me.grpKmeansBasic.SuspendLayout()
        CType(Me.nudKmeansClusters, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageOptionsHierarchicalClustering.SuspendLayout()
        Me.grpHierarchicalDendrogram.SuspendLayout()
        Me.grpHierarchicalMembership.SuspendLayout()
        CType(Me.nudHierarchicalClusters, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpHierarchicalPreprocess.SuspendLayout()
        Me.grpHierarchicalBasic.SuspendLayout()
        Me.TabPageOptionsFA.SuspendLayout()
        Me.grpFAIterations.SuspendLayout()
        CType(Me.nudFAMaxIter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpFAScoring.SuspendLayout()
        Me.grpFARotation.SuspendLayout()
        CType(Me.nudFAPromaxPower, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpFARetention.SuspendLayout()
        CType(Me.nudFAVariance, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudFAEigen, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudFAFactors, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpFABasic.SuspendLayout()
        Me.TabPageOptionsDA.SuspendLayout()
        Me.grpDAPrior.SuspendLayout()
        Me.grpDAValidation.SuspendLayout()
        CType(Me.tbDAHoldoutFraction, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudDAFolds, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpDABasic.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabPageOptionsSPM
        '
        Me.TabPageOptionsSPM.Controls.Add(Me.ckShowRegressionLines)
        Me.TabPageOptionsSPM.Controls.Add(Me.ckDisplayCorrelCoef)
        Me.TabPageOptionsSPM.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsSPM.Name = "TabPageOptionsSPM"
        Me.TabPageOptionsSPM.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptionsSPM.TabIndex = 2
        Me.TabPageOptionsSPM.Text = "Options"
        Me.TabPageOptionsSPM.UseVisualStyleBackColor = True
        '
        'ckShowRegressionLines
        '
        Me.ckShowRegressionLines.AutoSize = True
        Me.ckShowRegressionLines.Checked = True
        Me.ckShowRegressionLines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckShowRegressionLines.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckShowRegressionLines.Location = New System.Drawing.Point(19, 45)
        Me.ckShowRegressionLines.Name = "ckShowRegressionLines"
        Me.ckShowRegressionLines.Size = New System.Drawing.Size(170, 20)
        Me.ckShowRegressionLines.TabIndex = 6
        Me.ckShowRegressionLines.Text = "Show Regression Lines"
        Me.ckShowRegressionLines.UseVisualStyleBackColor = True
        '
        'ckDisplayCorrelCoef
        '
        Me.ckDisplayCorrelCoef.AutoSize = True
        Me.ckDisplayCorrelCoef.Checked = True
        Me.ckDisplayCorrelCoef.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckDisplayCorrelCoef.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckDisplayCorrelCoef.Location = New System.Drawing.Point(19, 19)
        Me.ckDisplayCorrelCoef.Name = "ckDisplayCorrelCoef"
        Me.ckDisplayCorrelCoef.Size = New System.Drawing.Size(215, 20)
        Me.ckDisplayCorrelCoef.TabIndex = 5
        Me.ckDisplayCorrelCoef.Text = "Display Correlation Coefficients"
        Me.ckDisplayCorrelCoef.UseVisualStyleBackColor = True
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.cbGruppingVar)
        Me.TabPage1.Controls.Add(Me.lblGruppingVar)
        Me.TabPage1.Controls.Add(Me.cbKmeansRowLabel)
        Me.TabPage1.Controls.Add(Me.lblKmeansRowLabel)
        Me.TabPage1.Controls.Add(Me.ckFirstRow)
        Me.TabPage1.Controls.Add(Me.lbXs)
        Me.TabPage1.Controls.Add(Me.cbSheetsList)
        Me.TabPage1.Controls.Add(Me.btReload)
        Me.TabPage1.Controls.Add(Me.btRemoveX)
        Me.TabPage1.Controls.Add(Me.btAddX)
        Me.TabPage1.Controls.Add(Me.lbAllColumns)
        Me.TabPage1.Controls.Add(Me.lblAllColumns)
        Me.TabPage1.Controls.Add(Me.lblX)
        Me.TabPage1.Controls.Add(Me.lblSelectedSheet)
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(836, 465)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Select Variables"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'cbGruppingVar
        '
        Me.cbGruppingVar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbGruppingVar.FormattingEnabled = True
        Me.cbGruppingVar.Location = New System.Drawing.Point(579, 107)
        Me.cbGruppingVar.Name = "cbGruppingVar"
        Me.cbGruppingVar.Size = New System.Drawing.Size(240, 24)
        Me.cbGruppingVar.TabIndex = 29
        Me.cbGruppingVar.Visible = False
        '
        'lblGruppingVar
        '
        Me.lblGruppingVar.AutoSize = True
        Me.lblGruppingVar.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblGruppingVar.Location = New System.Drawing.Point(576, 88)
        Me.lblGruppingVar.Name = "lblGruppingVar"
        Me.lblGruppingVar.Size = New System.Drawing.Size(137, 16)
        Me.lblGruppingVar.TabIndex = 28
        Me.lblGruppingVar.Text = "Grouping Variable:"
        Me.lblGruppingVar.Visible = False
        '
        'cbKmeansRowLabel
        '
        Me.cbKmeansRowLabel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbKmeansRowLabel.FormattingEnabled = True
        Me.cbKmeansRowLabel.Location = New System.Drawing.Point(579, 154)
        Me.cbKmeansRowLabel.Name = "cbKmeansRowLabel"
        Me.cbKmeansRowLabel.Size = New System.Drawing.Size(240, 24)
        Me.cbKmeansRowLabel.TabIndex = 27
        Me.cbKmeansRowLabel.Visible = False
        '
        'lblKmeansRowLabel
        '
        Me.lblKmeansRowLabel.AutoSize = True
        Me.lblKmeansRowLabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblKmeansRowLabel.Location = New System.Drawing.Point(576, 135)
        Me.lblKmeansRowLabel.Name = "lblKmeansRowLabel"
        Me.lblKmeansRowLabel.Size = New System.Drawing.Size(209, 16)
        Me.lblKmeansRowLabel.TabIndex = 26
        Me.lblKmeansRowLabel.Text = "Optional Row Label Variable:"
        Me.lblKmeansRowLabel.Visible = False
        '
        'ckFirstRow
        '
        Me.ckFirstRow.Checked = True
        Me.ckFirstRow.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckFirstRow.Location = New System.Drawing.Point(232, 74)
        Me.ckFirstRow.Name = "ckFirstRow"
        Me.ckFirstRow.Size = New System.Drawing.Size(84, 94)
        Me.ckFirstRow.TabIndex = 25
        Me.ckFirstRow.Text = "1st Row Contains Variable Names"
        Me.ckFirstRow.UseVisualStyleBackColor = True
        Me.ckFirstRow.Visible = False
        '
        'lbXs
        '
        Me.lbXs.FormattingEnabled = True
        Me.lbXs.ItemHeight = 16
        Me.lbXs.Location = New System.Drawing.Point(322, 23)
        Me.lbXs.Name = "lbXs"
        Me.lbXs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbXs.Size = New System.Drawing.Size(240, 420)
        Me.lbXs.TabIndex = 17
        '
        'cbSheetsList
        '
        Me.cbSheetsList.FormattingEnabled = True
        Me.cbSheetsList.Location = New System.Drawing.Point(579, 23)
        Me.cbSheetsList.Name = "cbSheetsList"
        Me.cbSheetsList.Size = New System.Drawing.Size(240, 24)
        Me.cbSheetsList.TabIndex = 21
        Me.cbSheetsList.Text = "Select Sheet"
        '
        'btReload
        '
        Me.btReload.Location = New System.Drawing.Point(579, 53)
        Me.btReload.Name = "btReload"
        Me.btReload.Size = New System.Drawing.Size(130, 23)
        Me.btReload.TabIndex = 20
        Me.btReload.Text = "Reload Sheet Data"
        Me.btReload.UseVisualStyleBackColor = True
        '
        'btRemoveX
        '
        Me.btRemoveX.Location = New System.Drawing.Point(277, 22)
        Me.btRemoveX.Name = "btRemoveX"
        Me.btRemoveX.Size = New System.Drawing.Size(39, 23)
        Me.btRemoveX.TabIndex = 16
        Me.btRemoveX.Text = "<<"
        Me.btRemoveX.UseVisualStyleBackColor = True
        '
        'btAddX
        '
        Me.btAddX.Location = New System.Drawing.Point(232, 22)
        Me.btAddX.Name = "btAddX"
        Me.btAddX.Size = New System.Drawing.Size(39, 23)
        Me.btAddX.TabIndex = 15
        Me.btAddX.Text = ">>"
        Me.btAddX.UseVisualStyleBackColor = True
        '
        'lbAllColumns
        '
        Me.lbAllColumns.FormattingEnabled = True
        Me.lbAllColumns.ItemHeight = 16
        Me.lbAllColumns.Location = New System.Drawing.Point(5, 22)
        Me.lbAllColumns.Name = "lbAllColumns"
        Me.lbAllColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.lbAllColumns.Size = New System.Drawing.Size(221, 420)
        Me.lbAllColumns.TabIndex = 0
        '
        'lblAllColumns
        '
        Me.lblAllColumns.AutoSize = True
        Me.lblAllColumns.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAllColumns.Location = New System.Drawing.Point(15, 3)
        Me.lblAllColumns.Name = "lblAllColumns"
        Me.lblAllColumns.Size = New System.Drawing.Size(144, 16)
        Me.lblAllColumns.TabIndex = 1
        Me.lblAllColumns.Text = "Worksheet Columns"
        '
        'lblX
        '
        Me.lblX.AutoSize = True
        Me.lblX.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblX.Location = New System.Drawing.Point(322, 3)
        Me.lblX.Name = "lblX"
        Me.lblX.Size = New System.Drawing.Size(150, 16)
        Me.lblX.TabIndex = 18
        Me.lblX.Text = "Selected Variable(s)"
        '
        'lblSelectedSheet
        '
        Me.lblSelectedSheet.AutoSize = True
        Me.lblSelectedSheet.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelectedSheet.Location = New System.Drawing.Point(576, 3)
        Me.lblSelectedSheet.Name = "lblSelectedSheet"
        Me.lblSelectedSheet.Size = New System.Drawing.Size(132, 16)
        Me.lblSelectedSheet.TabIndex = 24
        Me.lblSelectedSheet.Text = "Active Worksheet:"
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsSPM)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsPCA)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsKmeans)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsHierarchicalClustering)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsFA)
        Me.TabControl1.Controls.Add(Me.TabPageOptionsDA)
        Me.TabControl1.Location = New System.Drawing.Point(3, 2)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(844, 494)
        Me.TabControl1.TabIndex = 3
        '
        'TabPageOptionsPCA
        '
        Me.TabPageOptionsPCA.Controls.Add(Me.grpExtract)
        Me.TabPageOptionsPCA.Controls.Add(Me.grpAnalyzeType)
        Me.TabPageOptionsPCA.Controls.Add(Me.grpIterOptions)
        Me.TabPageOptionsPCA.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsPCA.Name = "TabPageOptionsPCA"
        Me.TabPageOptionsPCA.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptionsPCA.TabIndex = 3
        Me.TabPageOptionsPCA.Text = "Options"
        Me.TabPageOptionsPCA.UseVisualStyleBackColor = True
        '
        'grpExtract
        '
        Me.grpExtract.Controls.Add(Me.spinBtnExtractEigen)
        Me.grpExtract.Controls.Add(Me.spinBtnExtractComp)
        Me.grpExtract.Controls.Add(Me.spinBtnExtractVariance)
        Me.grpExtract.Controls.Add(Me.optExtractVariance)
        Me.grpExtract.Controls.Add(Me.optExtractFixed)
        Me.grpExtract.Controls.Add(Me.optExtractEigen)
        Me.grpExtract.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpExtract.Location = New System.Drawing.Point(14, 134)
        Me.grpExtract.Name = "grpExtract"
        Me.grpExtract.Size = New System.Drawing.Size(312, 143)
        Me.grpExtract.TabIndex = 5
        Me.grpExtract.TabStop = False
        Me.grpExtract.Text = "Extract"
        '
        'spinBtnExtractEigen
        '
        Me.spinBtnExtractEigen.DecimalPlaces = 2
        Me.spinBtnExtractEigen.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnExtractEigen.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.spinBtnExtractEigen.Location = New System.Drawing.Point(237, 29)
        Me.spinBtnExtractEigen.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnExtractEigen.Name = "spinBtnExtractEigen"
        Me.spinBtnExtractEigen.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnExtractEigen.TabIndex = 7
        Me.spinBtnExtractEigen.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'spinBtnExtractComp
        '
        Me.spinBtnExtractComp.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnExtractComp.Location = New System.Drawing.Point(237, 61)
        Me.spinBtnExtractComp.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnExtractComp.Name = "spinBtnExtractComp"
        Me.spinBtnExtractComp.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnExtractComp.TabIndex = 6
        Me.spinBtnExtractComp.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'spinBtnExtractVariance
        '
        Me.spinBtnExtractVariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.spinBtnExtractVariance.Location = New System.Drawing.Point(237, 96)
        Me.spinBtnExtractVariance.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        Me.spinBtnExtractVariance.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinBtnExtractVariance.Name = "spinBtnExtractVariance"
        Me.spinBtnExtractVariance.Size = New System.Drawing.Size(59, 22)
        Me.spinBtnExtractVariance.TabIndex = 5
        Me.spinBtnExtractVariance.Value = New Decimal(New Integer() {80, 0, 0, 0})
        '
        'optExtractVariance
        '
        Me.optExtractVariance.AutoSize = True
        Me.optExtractVariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optExtractVariance.Location = New System.Drawing.Point(16, 98)
        Me.optExtractVariance.Name = "optExtractVariance"
        Me.optExtractVariance.Size = New System.Drawing.Size(168, 20)
        Me.optExtractVariance.TabIndex = 2
        Me.optExtractVariance.Text = "Variance Explained [%]"
        Me.optExtractVariance.UseVisualStyleBackColor = True
        '
        'optExtractFixed
        '
        Me.optExtractFixed.AutoSize = True
        Me.optExtractFixed.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optExtractFixed.Location = New System.Drawing.Point(16, 63)
        Me.optExtractFixed.Name = "optExtractFixed"
        Me.optExtractFixed.Size = New System.Drawing.Size(205, 20)
        Me.optExtractFixed.TabIndex = 1
        Me.optExtractFixed.Text = "Fixed Number of Components"
        Me.optExtractFixed.UseVisualStyleBackColor = True
        '
        'optExtractEigen
        '
        Me.optExtractEigen.AutoSize = True
        Me.optExtractEigen.Checked = True
        Me.optExtractEigen.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optExtractEigen.Location = New System.Drawing.Point(16, 31)
        Me.optExtractEigen.Name = "optExtractEigen"
        Me.optExtractEigen.Size = New System.Drawing.Size(185, 20)
        Me.optExtractEigen.TabIndex = 0
        Me.optExtractEigen.TabStop = True
        Me.optExtractEigen.Text = "Based on Eigenvalue  (>=)"
        Me.optExtractEigen.UseVisualStyleBackColor = True
        '
        'grpAnalyzeType
        '
        Me.grpAnalyzeType.Controls.Add(Me.optCovar)
        Me.grpAnalyzeType.Controls.Add(Me.optCorr)
        Me.grpAnalyzeType.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpAnalyzeType.Location = New System.Drawing.Point(332, 18)
        Me.grpAnalyzeType.Name = "grpAnalyzeType"
        Me.grpAnalyzeType.Size = New System.Drawing.Size(312, 110)
        Me.grpAnalyzeType.TabIndex = 4
        Me.grpAnalyzeType.TabStop = False
        Me.grpAnalyzeType.Text = "Analyze"
        '
        'optCovar
        '
        Me.optCovar.AutoSize = True
        Me.optCovar.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCovar.Location = New System.Drawing.Point(16, 63)
        Me.optCovar.Name = "optCovar"
        Me.optCovar.Size = New System.Drawing.Size(167, 20)
        Me.optCovar.TabIndex = 1
        Me.optCovar.Text = "Covariance MatrixType"
        Me.optCovar.UseVisualStyleBackColor = True
        '
        'optCorr
        '
        Me.optCorr.AutoSize = True
        Me.optCorr.Checked = True
        Me.optCorr.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optCorr.Location = New System.Drawing.Point(16, 31)
        Me.optCorr.Name = "optCorr"
        Me.optCorr.Size = New System.Drawing.Size(163, 20)
        Me.optCorr.TabIndex = 0
        Me.optCorr.TabStop = True
        Me.optCorr.Text = "Correlation MatrixType"
        Me.optCorr.UseVisualStyleBackColor = True
        '
        'grpIterOptions
        '
        Me.grpIterOptions.Controls.Add(Me.tbMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblMaxIter)
        Me.grpIterOptions.Controls.Add(Me.lblEps)
        Me.grpIterOptions.Controls.Add(Me.tbEps)
        Me.grpIterOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpIterOptions.Location = New System.Drawing.Point(14, 18)
        Me.grpIterOptions.Name = "grpIterOptions"
        Me.grpIterOptions.Size = New System.Drawing.Size(312, 110)
        Me.grpIterOptions.TabIndex = 1
        Me.grpIterOptions.TabStop = False
        Me.grpIterOptions.Text = "Convergence Options"
        '
        'tbMaxIter
        '
        Me.tbMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMaxIter.Location = New System.Drawing.Point(171, 63)
        Me.tbMaxIter.Name = "tbMaxIter"
        Me.tbMaxIter.Size = New System.Drawing.Size(125, 22)
        Me.tbMaxIter.TabIndex = 3
        Me.tbMaxIter.Text = "50"
        '
        'lblMaxIter
        '
        Me.lblMaxIter.AutoSize = True
        Me.lblMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMaxIter.Location = New System.Drawing.Point(15, 69)
        Me.lblMaxIter.Name = "lblMaxIter"
        Me.lblMaxIter.Size = New System.Drawing.Size(92, 16)
        Me.lblMaxIter.TabIndex = 2
        Me.lblMaxIter.Text = "Max. Iterations"
        '
        'lblEps
        '
        Me.lblEps.AutoSize = True
        Me.lblEps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEps.Location = New System.Drawing.Point(15, 37)
        Me.lblEps.Name = "lblEps"
        Me.lblEps.Size = New System.Drawing.Size(140, 16)
        Me.lblEps.TabIndex = 1
        Me.lblEps.Text = "Convergence Criterion"
        '
        'tbEps
        '
        Me.tbEps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbEps.Location = New System.Drawing.Point(171, 31)
        Me.tbEps.Name = "tbEps"
        Me.tbEps.Size = New System.Drawing.Size(125, 22)
        Me.tbEps.TabIndex = 1
        Me.tbEps.Text = "0.000001"
        '
        'TabPageOptionsKmeans
        '
        Me.TabPageOptionsKmeans.Controls.Add(Me.lblCenterHint)
        Me.TabPageOptionsKmeans.Controls.Add(Me.lblCenterRef)
        Me.TabPageOptionsKmeans.Controls.Add(Me.grpKmeansFit)
        Me.TabPageOptionsKmeans.Controls.Add(Me.grpKmeansPreprocess)
        Me.TabPageOptionsKmeans.Controls.Add(Me.grpKmeansBasic)
        Me.TabPageOptionsKmeans.Controls.Add(Me.refKmeansCenters)
        Me.TabPageOptionsKmeans.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsKmeans.Name = "TabPageOptionsKmeans"
        Me.TabPageOptionsKmeans.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptionsKmeans.TabIndex = 4
        Me.TabPageOptionsKmeans.Text = "Options"
        Me.TabPageOptionsKmeans.UseVisualStyleBackColor = True
        '
        'lblCenterHint
        '
        Me.lblCenterHint.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCenterHint.Location = New System.Drawing.Point(12, 348)
        Me.lblCenterHint.Name = "lblCenterHint"
        Me.lblCenterHint.Size = New System.Drawing.Size(321, 53)
        Me.lblCenterHint.TabIndex = 17
        Me.lblCenterHint.Text = "Provide a contiguous numeric range with k rows and one column per selected analys" &
    "is variable. Do not include a header row."
        '
        'lblCenterRef
        '
        Me.lblCenterRef.AutoSize = True
        Me.lblCenterRef.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCenterRef.Location = New System.Drawing.Point(12, 292)
        Me.lblCenterRef.Name = "lblCenterRef"
        Me.lblCenterRef.Size = New System.Drawing.Size(241, 16)
        Me.lblCenterRef.TabIndex = 15
        Me.lblCenterRef.Text = "User-Specified Starting Centers Range:"
        '
        'grpKmeansFit
        '
        Me.grpKmeansFit.Controls.Add(Me.tbKmeansSeed)
        Me.grpKmeansFit.Controls.Add(Me.lblSeed)
        Me.grpKmeansFit.Controls.Add(Me.tbKmeansTolerance)
        Me.grpKmeansFit.Controls.Add(Me.nudKmeansMaxIterations)
        Me.grpKmeansFit.Controls.Add(Me.nudKmeansStarts)
        Me.grpKmeansFit.Controls.Add(Me.lblTol)
        Me.grpKmeansFit.Controls.Add(Me.lblKmeanMaxIter)
        Me.grpKmeansFit.Controls.Add(Me.lblStarts)
        Me.grpKmeansFit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpKmeansFit.Location = New System.Drawing.Point(357, 17)
        Me.grpKmeansFit.Name = "grpKmeansFit"
        Me.grpKmeansFit.Size = New System.Drawing.Size(336, 139)
        Me.grpKmeansFit.TabIndex = 9
        Me.grpKmeansFit.TabStop = False
        Me.grpKmeansFit.Text = "Fitting Controls"
        '
        'tbKmeansSeed
        '
        Me.tbKmeansSeed.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbKmeansSeed.Location = New System.Drawing.Point(159, 109)
        Me.tbKmeansSeed.Name = "tbKmeansSeed"
        Me.tbKmeansSeed.Size = New System.Drawing.Size(168, 22)
        Me.tbKmeansSeed.TabIndex = 13
        '
        'lblSeed
        '
        Me.lblSeed.AutoSize = True
        Me.lblSeed.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSeed.Location = New System.Drawing.Point(6, 111)
        Me.lblSeed.Name = "lblSeed"
        Me.lblSeed.Size = New System.Drawing.Size(155, 16)
        Me.lblSeed.TabIndex = 12
        Me.lblSeed.Text = "Random seed (optional):"
        '
        'tbKmeansTolerance
        '
        Me.tbKmeansTolerance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbKmeansTolerance.Location = New System.Drawing.Point(159, 81)
        Me.tbKmeansTolerance.Name = "tbKmeansTolerance"
        Me.tbKmeansTolerance.Size = New System.Drawing.Size(168, 22)
        Me.tbKmeansTolerance.TabIndex = 11
        '
        'nudKmeansMaxIterations
        '
        Me.nudKmeansMaxIterations.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudKmeansMaxIterations.Location = New System.Drawing.Point(159, 55)
        Me.nudKmeansMaxIterations.Maximum = New Decimal(New Integer() {100000, 0, 0, 0})
        Me.nudKmeansMaxIterations.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudKmeansMaxIterations.Name = "nudKmeansMaxIterations"
        Me.nudKmeansMaxIterations.Size = New System.Drawing.Size(56, 22)
        Me.nudKmeansMaxIterations.TabIndex = 10
        Me.nudKmeansMaxIterations.Value = New Decimal(New Integer() {100, 0, 0, 0})
        '
        'nudKmeansStarts
        '
        Me.nudKmeansStarts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudKmeansStarts.Location = New System.Drawing.Point(159, 30)
        Me.nudKmeansStarts.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        Me.nudKmeansStarts.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudKmeansStarts.Name = "nudKmeansStarts"
        Me.nudKmeansStarts.Size = New System.Drawing.Size(56, 22)
        Me.nudKmeansStarts.TabIndex = 9
        Me.nudKmeansStarts.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'lblTol
        '
        Me.lblTol.AutoSize = True
        Me.lblTol.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTol.Location = New System.Drawing.Point(6, 84)
        Me.lblTol.Name = "lblTol"
        Me.lblTol.Size = New System.Drawing.Size(150, 16)
        Me.lblTol.TabIndex = 7
        Me.lblTol.Text = "Convergence tolerance:"
        '
        'lblKmeanMaxIter
        '
        Me.lblKmeanMaxIter.AutoSize = True
        Me.lblKmeanMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblKmeanMaxIter.Location = New System.Drawing.Point(6, 57)
        Me.lblKmeanMaxIter.Name = "lblKmeanMaxIter"
        Me.lblKmeanMaxIter.Size = New System.Drawing.Size(124, 16)
        Me.lblKmeanMaxIter.TabIndex = 5
        Me.lblKmeanMaxIter.Text = "Maximum iterations:"
        '
        'lblStarts
        '
        Me.lblStarts.AutoSize = True
        Me.lblStarts.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStarts.Location = New System.Drawing.Point(6, 30)
        Me.lblStarts.Name = "lblStarts"
        Me.lblStarts.Size = New System.Drawing.Size(97, 16)
        Me.lblStarts.TabIndex = 2
        Me.lblStarts.Text = "Random starts:"
        '
        'grpKmeansPreprocess
        '
        Me.grpKmeansPreprocess.Controls.Add(Me.cbKmeansEmptyCluster)
        Me.grpKmeansPreprocess.Controls.Add(Me.lblEmpty)
        Me.grpKmeansPreprocess.Controls.Add(Me.cbKmeansMissingPolicy)
        Me.grpKmeansPreprocess.Controls.Add(Me.lblMissing)
        Me.grpKmeansPreprocess.Controls.Add(Me.cbKmeansStandardization)
        Me.grpKmeansPreprocess.Controls.Add(Me.lblStd)
        Me.grpKmeansPreprocess.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpKmeansPreprocess.Location = New System.Drawing.Point(15, 150)
        Me.grpKmeansPreprocess.Name = "grpKmeansPreprocess"
        Me.grpKmeansPreprocess.Size = New System.Drawing.Size(336, 118)
        Me.grpKmeansPreprocess.TabIndex = 1
        Me.grpKmeansPreprocess.TabStop = False
        Me.grpKmeansPreprocess.Text = "Preprocessing"
        '
        'cbKmeansEmptyCluster
        '
        Me.cbKmeansEmptyCluster.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbKmeansEmptyCluster.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbKmeansEmptyCluster.Location = New System.Drawing.Point(159, 81)
        Me.cbKmeansEmptyCluster.Name = "cbKmeansEmptyCluster"
        Me.cbKmeansEmptyCluster.Size = New System.Drawing.Size(168, 24)
        Me.cbKmeansEmptyCluster.TabIndex = 8
        '
        'lblEmpty
        '
        Me.lblEmpty.AutoSize = True
        Me.lblEmpty.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEmpty.Location = New System.Drawing.Point(6, 84)
        Me.lblEmpty.Name = "lblEmpty"
        Me.lblEmpty.Size = New System.Drawing.Size(144, 16)
        Me.lblEmpty.TabIndex = 7
        Me.lblEmpty.Text = "Empty cluster handling:"
        '
        'cbKmeansMissingPolicy
        '
        Me.cbKmeansMissingPolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbKmeansMissingPolicy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbKmeansMissingPolicy.Location = New System.Drawing.Point(159, 54)
        Me.cbKmeansMissingPolicy.Name = "cbKmeansMissingPolicy"
        Me.cbKmeansMissingPolicy.Size = New System.Drawing.Size(168, 24)
        Me.cbKmeansMissingPolicy.TabIndex = 6
        '
        'lblMissing
        '
        Me.lblMissing.AutoSize = True
        Me.lblMissing.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMissing.Location = New System.Drawing.Point(6, 57)
        Me.lblMissing.Name = "lblMissing"
        Me.lblMissing.Size = New System.Drawing.Size(99, 16)
        Me.lblMissing.TabIndex = 5
        Me.lblMissing.Text = "Missing values:"
        '
        'cbKmeansStandardization
        '
        Me.cbKmeansStandardization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbKmeansStandardization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbKmeansStandardization.Location = New System.Drawing.Point(159, 27)
        Me.cbKmeansStandardization.Name = "cbKmeansStandardization"
        Me.cbKmeansStandardization.Size = New System.Drawing.Size(168, 24)
        Me.cbKmeansStandardization.TabIndex = 4
        '
        'lblStd
        '
        Me.lblStd.AutoSize = True
        Me.lblStd.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStd.Location = New System.Drawing.Point(6, 30)
        Me.lblStd.Name = "lblStd"
        Me.lblStd.Size = New System.Drawing.Size(103, 16)
        Me.lblStd.TabIndex = 2
        Me.lblStd.Text = "Standardization:"
        '
        'grpKmeansBasic
        '
        Me.grpKmeansBasic.Controls.Add(Me.cbKmeansDistance)
        Me.grpKmeansBasic.Controls.Add(Me.lblDist)
        Me.grpKmeansBasic.Controls.Add(Me.cbKmeansInitialization)
        Me.grpKmeansBasic.Controls.Add(Me.lblInit)
        Me.grpKmeansBasic.Controls.Add(Me.lblK)
        Me.grpKmeansBasic.Controls.Add(Me.nudKmeansClusters)
        Me.grpKmeansBasic.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpKmeansBasic.Location = New System.Drawing.Point(15, 17)
        Me.grpKmeansBasic.Name = "grpKmeansBasic"
        Me.grpKmeansBasic.Size = New System.Drawing.Size(336, 127)
        Me.grpKmeansBasic.TabIndex = 0
        Me.grpKmeansBasic.TabStop = False
        Me.grpKmeansBasic.Text = "Partition Options"
        '
        'cbKmeansDistance
        '
        Me.cbKmeansDistance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbKmeansDistance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbKmeansDistance.Location = New System.Drawing.Point(159, 88)
        Me.cbKmeansDistance.Name = "cbKmeansDistance"
        Me.cbKmeansDistance.Size = New System.Drawing.Size(168, 24)
        Me.cbKmeansDistance.TabIndex = 5
        '
        'lblDist
        '
        Me.lblDist.AutoSize = True
        Me.lblDist.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDist.Location = New System.Drawing.Point(6, 91)
        Me.lblDist.Name = "lblDist"
        Me.lblDist.Size = New System.Drawing.Size(121, 16)
        Me.lblDist.TabIndex = 4
        Me.lblDist.Text = "Reported distance:"
        '
        'cbKmeansInitialization
        '
        Me.cbKmeansInitialization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbKmeansInitialization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbKmeansInitialization.Location = New System.Drawing.Point(159, 57)
        Me.cbKmeansInitialization.Name = "cbKmeansInitialization"
        Me.cbKmeansInitialization.Size = New System.Drawing.Size(168, 24)
        Me.cbKmeansInitialization.TabIndex = 3
        '
        'lblInit
        '
        Me.lblInit.AutoSize = True
        Me.lblInit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblInit.Location = New System.Drawing.Point(6, 60)
        Me.lblInit.Name = "lblInit"
        Me.lblInit.Size = New System.Drawing.Size(78, 16)
        Me.lblInit.TabIndex = 2
        Me.lblInit.Text = "Initialization:"
        '
        'lblK
        '
        Me.lblK.AutoSize = True
        Me.lblK.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblK.Location = New System.Drawing.Point(6, 29)
        Me.lblK.Name = "lblK"
        Me.lblK.Size = New System.Drawing.Size(139, 16)
        Me.lblK.TabIndex = 1
        Me.lblK.Text = "Number of clusters (k):"
        '
        'nudKmeansClusters
        '
        Me.nudKmeansClusters.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudKmeansClusters.Location = New System.Drawing.Point(159, 29)
        Me.nudKmeansClusters.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudKmeansClusters.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudKmeansClusters.Name = "nudKmeansClusters"
        Me.nudKmeansClusters.Size = New System.Drawing.Size(56, 22)
        Me.nudKmeansClusters.TabIndex = 0
        Me.nudKmeansClusters.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        'TabPageOptionsHierarchicalClustering
        '
        Me.TabPageOptionsHierarchicalClustering.Controls.Add(Me.grpHierarchicalDendrogram)
        Me.TabPageOptionsHierarchicalClustering.Controls.Add(Me.grpHierarchicalMembership)
        Me.TabPageOptionsHierarchicalClustering.Controls.Add(Me.grpHierarchicalPreprocess)
        Me.TabPageOptionsHierarchicalClustering.Controls.Add(Me.grpHierarchicalBasic)
        Me.TabPageOptionsHierarchicalClustering.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsHierarchicalClustering.Name = "TabPageOptionsHierarchicalClustering"
        Me.TabPageOptionsHierarchicalClustering.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptionsHierarchicalClustering.TabIndex = 5
        Me.TabPageOptionsHierarchicalClustering.Text = "Options"
        Me.TabPageOptionsHierarchicalClustering.UseVisualStyleBackColor = True
        '
        'grpHierarchicalDendrogram
        '
        Me.grpHierarchicalDendrogram.Controls.Add(Me.ckHierarchicalCreateDendrogram)
        Me.grpHierarchicalDendrogram.Controls.Add(Me.cbHierarchicalLabelMode)
        Me.grpHierarchicalDendrogram.Controls.Add(Me.cbHierarchicalOrientation)
        Me.grpHierarchicalDendrogram.Controls.Add(Me.lblOrientation)
        Me.grpHierarchicalDendrogram.Controls.Add(Me.cbHierarchicalHeightMode)
        Me.grpHierarchicalDendrogram.Controls.Add(Me.lblLabelMode)
        Me.grpHierarchicalDendrogram.Controls.Add(Me.lblHeightMode)
        Me.grpHierarchicalDendrogram.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHierarchicalDendrogram.Location = New System.Drawing.Point(357, 15)
        Me.grpHierarchicalDendrogram.Name = "grpHierarchicalDendrogram"
        Me.grpHierarchicalDendrogram.Size = New System.Drawing.Size(336, 169)
        Me.grpHierarchicalDendrogram.TabIndex = 13
        Me.grpHierarchicalDendrogram.TabStop = False
        Me.grpHierarchicalDendrogram.Text = "Dendrogram"
        '
        'ckHierarchicalCreateDendrogram
        '
        Me.ckHierarchicalCreateDendrogram.AutoSize = True
        Me.ckHierarchicalCreateDendrogram.Checked = True
        Me.ckHierarchicalCreateDendrogram.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckHierarchicalCreateDendrogram.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckHierarchicalCreateDendrogram.Location = New System.Drawing.Point(9, 34)
        Me.ckHierarchicalCreateDendrogram.Name = "ckHierarchicalCreateDendrogram"
        Me.ckHierarchicalCreateDendrogram.Size = New System.Drawing.Size(178, 20)
        Me.ckHierarchicalCreateDendrogram.TabIndex = 7
        Me.ckHierarchicalCreateDendrogram.Text = "Create dendrogram chart"
        Me.ckHierarchicalCreateDendrogram.UseVisualStyleBackColor = True
        '
        'cbHierarchicalLabelMode
        '
        Me.cbHierarchicalLabelMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHierarchicalLabelMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHierarchicalLabelMode.Location = New System.Drawing.Point(162, 132)
        Me.cbHierarchicalLabelMode.Name = "cbHierarchicalLabelMode"
        Me.cbHierarchicalLabelMode.Size = New System.Drawing.Size(168, 24)
        Me.cbHierarchicalLabelMode.TabIndex = 6
        '
        'cbHierarchicalOrientation
        '
        Me.cbHierarchicalOrientation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHierarchicalOrientation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHierarchicalOrientation.Location = New System.Drawing.Point(162, 99)
        Me.cbHierarchicalOrientation.Name = "cbHierarchicalOrientation"
        Me.cbHierarchicalOrientation.Size = New System.Drawing.Size(168, 24)
        Me.cbHierarchicalOrientation.TabIndex = 5
        '
        'lblOrientation
        '
        Me.lblOrientation.AutoSize = True
        Me.lblOrientation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOrientation.Location = New System.Drawing.Point(6, 102)
        Me.lblOrientation.Name = "lblOrientation"
        Me.lblOrientation.Size = New System.Drawing.Size(74, 16)
        Me.lblOrientation.TabIndex = 4
        Me.lblOrientation.Text = "Orientation:"
        '
        'cbHierarchicalHeightMode
        '
        Me.cbHierarchicalHeightMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHierarchicalHeightMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHierarchicalHeightMode.Location = New System.Drawing.Point(162, 67)
        Me.cbHierarchicalHeightMode.Name = "cbHierarchicalHeightMode"
        Me.cbHierarchicalHeightMode.Size = New System.Drawing.Size(168, 24)
        Me.cbHierarchicalHeightMode.TabIndex = 3
        '
        'lblLabelMode
        '
        Me.lblLabelMode.AutoSize = True
        Me.lblLabelMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLabelMode.Location = New System.Drawing.Point(6, 135)
        Me.lblLabelMode.Name = "lblLabelMode"
        Me.lblLabelMode.Size = New System.Drawing.Size(82, 16)
        Me.lblLabelMode.TabIndex = 2
        Me.lblLabelMode.Text = "Label mode:"
        '
        'lblHeightMode
        '
        Me.lblHeightMode.AutoSize = True
        Me.lblHeightMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHeightMode.Location = New System.Drawing.Point(6, 70)
        Me.lblHeightMode.Name = "lblHeightMode"
        Me.lblHeightMode.Size = New System.Drawing.Size(87, 16)
        Me.lblHeightMode.TabIndex = 1
        Me.lblHeightMode.Text = "Height mode:"
        '
        'grpHierarchicalMembership
        '
        Me.grpHierarchicalMembership.Controls.Add(Me.nudHierarchicalClusters)
        Me.grpHierarchicalMembership.Controls.Add(Me.tbHierarchicalCutHeight)
        Me.grpHierarchicalMembership.Controls.Add(Me.optHierarchicalCutByHeight)
        Me.grpHierarchicalMembership.Controls.Add(Me.optHierarchicalCutByClusters)
        Me.grpHierarchicalMembership.Controls.Add(Me.lblMembershipHint)
        Me.grpHierarchicalMembership.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHierarchicalMembership.Location = New System.Drawing.Point(15, 253)
        Me.grpHierarchicalMembership.Name = "grpHierarchicalMembership"
        Me.grpHierarchicalMembership.Size = New System.Drawing.Size(336, 144)
        Me.grpHierarchicalMembership.TabIndex = 13
        Me.grpHierarchicalMembership.TabStop = False
        Me.grpHierarchicalMembership.Text = "Membership Table"
        '
        'nudHierarchicalClusters
        '
        Me.nudHierarchicalClusters.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudHierarchicalClusters.Location = New System.Drawing.Point(220, 33)
        Me.nudHierarchicalClusters.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudHierarchicalClusters.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudHierarchicalClusters.Name = "nudHierarchicalClusters"
        Me.nudHierarchicalClusters.Size = New System.Drawing.Size(110, 22)
        Me.nudHierarchicalClusters.TabIndex = 14
        Me.nudHierarchicalClusters.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        'tbHierarchicalCutHeight
        '
        Me.tbHierarchicalCutHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbHierarchicalCutHeight.Location = New System.Drawing.Point(186, 59)
        Me.tbHierarchicalCutHeight.Name = "tbHierarchicalCutHeight"
        Me.tbHierarchicalCutHeight.Size = New System.Drawing.Size(144, 22)
        Me.tbHierarchicalCutHeight.TabIndex = 13
        '
        'optHierarchicalCutByHeight
        '
        Me.optHierarchicalCutByHeight.AutoSize = True
        Me.optHierarchicalCutByHeight.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optHierarchicalCutByHeight.Location = New System.Drawing.Point(9, 59)
        Me.optHierarchicalCutByHeight.Name = "optHierarchicalCutByHeight"
        Me.optHierarchicalCutByHeight.Size = New System.Drawing.Size(171, 20)
        Me.optHierarchicalCutByHeight.TabIndex = 4
        Me.optHierarchicalCutByHeight.Text = "Cut tree at merge height:"
        Me.optHierarchicalCutByHeight.UseVisualStyleBackColor = True
        '
        'optHierarchicalCutByClusters
        '
        Me.optHierarchicalCutByClusters.AutoSize = True
        Me.optHierarchicalCutByClusters.Checked = True
        Me.optHierarchicalCutByClusters.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optHierarchicalCutByClusters.Location = New System.Drawing.Point(9, 33)
        Me.optHierarchicalCutByClusters.Name = "optHierarchicalCutByClusters"
        Me.optHierarchicalCutByClusters.Size = New System.Drawing.Size(205, 20)
        Me.optHierarchicalCutByClusters.TabIndex = 3
        Me.optHierarchicalCutByClusters.TabStop = True
        Me.optHierarchicalCutByClusters.Text = "Cut tree by number of clusters:"
        Me.optHierarchicalCutByClusters.UseVisualStyleBackColor = True
        '
        'lblMembershipHint
        '
        Me.lblMembershipHint.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMembershipHint.Location = New System.Drawing.Point(6, 96)
        Me.lblMembershipHint.Name = "lblMembershipHint"
        Me.lblMembershipHint.Size = New System.Drawing.Size(324, 40)
        Me.lblMembershipHint.TabIndex = 2
        Me.lblMembershipHint.Text = "The selected rule controls the cluster-membership table returned in the formatted" &
    " results output."
        '
        'grpHierarchicalPreprocess
        '
        Me.grpHierarchicalPreprocess.Controls.Add(Me.cbHierarchicalMissingPolicy)
        Me.grpHierarchicalPreprocess.Controls.Add(Me.lblMissingHierarchicalClustering)
        Me.grpHierarchicalPreprocess.Controls.Add(Me.cbHierarchicalStandardization)
        Me.grpHierarchicalPreprocess.Controls.Add(Me.lblStdHierarchicalClustering)
        Me.grpHierarchicalPreprocess.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHierarchicalPreprocess.Location = New System.Drawing.Point(15, 148)
        Me.grpHierarchicalPreprocess.Name = "grpHierarchicalPreprocess"
        Me.grpHierarchicalPreprocess.Size = New System.Drawing.Size(336, 99)
        Me.grpHierarchicalPreprocess.TabIndex = 13
        Me.grpHierarchicalPreprocess.TabStop = False
        Me.grpHierarchicalPreprocess.Text = "Preprocessing"
        '
        'cbHierarchicalMissingPolicy
        '
        Me.cbHierarchicalMissingPolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHierarchicalMissingPolicy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHierarchicalMissingPolicy.Location = New System.Drawing.Point(162, 58)
        Me.cbHierarchicalMissingPolicy.Name = "cbHierarchicalMissingPolicy"
        Me.cbHierarchicalMissingPolicy.Size = New System.Drawing.Size(168, 24)
        Me.cbHierarchicalMissingPolicy.TabIndex = 5
        '
        'lblMissingHierarchicalClustering
        '
        Me.lblMissingHierarchicalClustering.AutoSize = True
        Me.lblMissingHierarchicalClustering.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMissingHierarchicalClustering.Location = New System.Drawing.Point(6, 61)
        Me.lblMissingHierarchicalClustering.Name = "lblMissingHierarchicalClustering"
        Me.lblMissingHierarchicalClustering.Size = New System.Drawing.Size(99, 16)
        Me.lblMissingHierarchicalClustering.TabIndex = 4
        Me.lblMissingHierarchicalClustering.Text = "Missing values:"
        '
        'cbHierarchicalStandardization
        '
        Me.cbHierarchicalStandardization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHierarchicalStandardization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHierarchicalStandardization.Location = New System.Drawing.Point(162, 26)
        Me.cbHierarchicalStandardization.Name = "cbHierarchicalStandardization"
        Me.cbHierarchicalStandardization.Size = New System.Drawing.Size(168, 24)
        Me.cbHierarchicalStandardization.TabIndex = 3
        '
        'lblStdHierarchicalClustering
        '
        Me.lblStdHierarchicalClustering.AutoSize = True
        Me.lblStdHierarchicalClustering.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStdHierarchicalClustering.Location = New System.Drawing.Point(6, 29)
        Me.lblStdHierarchicalClustering.Name = "lblStdHierarchicalClustering"
        Me.lblStdHierarchicalClustering.Size = New System.Drawing.Size(103, 16)
        Me.lblStdHierarchicalClustering.TabIndex = 1
        Me.lblStdHierarchicalClustering.Text = "Standardization:"
        '
        'grpHierarchicalBasic
        '
        Me.grpHierarchicalBasic.Controls.Add(Me.tbHierarchicalMinkowskiPower)
        Me.grpHierarchicalBasic.Controls.Add(Me.cbHierarchicalDistance)
        Me.grpHierarchicalBasic.Controls.Add(Me.lblDistance)
        Me.grpHierarchicalBasic.Controls.Add(Me.cbHierarchicalLinkage)
        Me.grpHierarchicalBasic.Controls.Add(Me.lblHierarchicalMinkowskiPower)
        Me.grpHierarchicalBasic.Controls.Add(Me.lblLinkage)
        Me.grpHierarchicalBasic.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpHierarchicalBasic.Location = New System.Drawing.Point(15, 15)
        Me.grpHierarchicalBasic.Name = "grpHierarchicalBasic"
        Me.grpHierarchicalBasic.Size = New System.Drawing.Size(336, 127)
        Me.grpHierarchicalBasic.TabIndex = 1
        Me.grpHierarchicalBasic.TabStop = False
        Me.grpHierarchicalBasic.Text = "Method and Distance"
        '
        'tbHierarchicalMinkowskiPower
        '
        Me.tbHierarchicalMinkowskiPower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbHierarchicalMinkowskiPower.Location = New System.Drawing.Point(162, 91)
        Me.tbHierarchicalMinkowskiPower.Name = "tbHierarchicalMinkowskiPower"
        Me.tbHierarchicalMinkowskiPower.Size = New System.Drawing.Size(168, 22)
        Me.tbHierarchicalMinkowskiPower.TabIndex = 12
        '
        'cbHierarchicalDistance
        '
        Me.cbHierarchicalDistance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHierarchicalDistance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHierarchicalDistance.Location = New System.Drawing.Point(162, 58)
        Me.cbHierarchicalDistance.Name = "cbHierarchicalDistance"
        Me.cbHierarchicalDistance.Size = New System.Drawing.Size(168, 24)
        Me.cbHierarchicalDistance.TabIndex = 5
        '
        'lblDistance
        '
        Me.lblDistance.AutoSize = True
        Me.lblDistance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDistance.Location = New System.Drawing.Point(6, 61)
        Me.lblDistance.Name = "lblDistance"
        Me.lblDistance.Size = New System.Drawing.Size(102, 16)
        Me.lblDistance.TabIndex = 4
        Me.lblDistance.Text = "Distance metric:"
        '
        'cbHierarchicalLinkage
        '
        Me.cbHierarchicalLinkage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbHierarchicalLinkage.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbHierarchicalLinkage.Location = New System.Drawing.Point(162, 26)
        Me.cbHierarchicalLinkage.Name = "cbHierarchicalLinkage"
        Me.cbHierarchicalLinkage.Size = New System.Drawing.Size(168, 24)
        Me.cbHierarchicalLinkage.TabIndex = 3
        '
        'lblHierarchicalMinkowskiPower
        '
        Me.lblHierarchicalMinkowskiPower.AutoSize = True
        Me.lblHierarchicalMinkowskiPower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHierarchicalMinkowskiPower.Location = New System.Drawing.Point(6, 94)
        Me.lblHierarchicalMinkowskiPower.Name = "lblHierarchicalMinkowskiPower"
        Me.lblHierarchicalMinkowskiPower.Size = New System.Drawing.Size(112, 16)
        Me.lblHierarchicalMinkowskiPower.TabIndex = 2
        Me.lblHierarchicalMinkowskiPower.Text = "Minkowski power:"
        '
        'lblLinkage
        '
        Me.lblLinkage.AutoSize = True
        Me.lblLinkage.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLinkage.Location = New System.Drawing.Point(6, 29)
        Me.lblLinkage.Name = "lblLinkage"
        Me.lblLinkage.Size = New System.Drawing.Size(58, 16)
        Me.lblLinkage.TabIndex = 1
        Me.lblLinkage.Text = "Linkage:"
        '
        'TabPageOptionsFA
        '
        Me.TabPageOptionsFA.Controls.Add(Me.grpFAIterations)
        Me.TabPageOptionsFA.Controls.Add(Me.grpFAScoring)
        Me.TabPageOptionsFA.Controls.Add(Me.grpFARotation)
        Me.TabPageOptionsFA.Controls.Add(Me.grpFARetention)
        Me.TabPageOptionsFA.Controls.Add(Me.grpFABasic)
        Me.TabPageOptionsFA.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsFA.Name = "TabPageOptionsFA"
        Me.TabPageOptionsFA.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptionsFA.TabIndex = 6
        Me.TabPageOptionsFA.Text = "Options"
        Me.TabPageOptionsFA.UseVisualStyleBackColor = True
        '
        'grpFAIterations
        '
        Me.grpFAIterations.Controls.Add(Me.tbFAEps)
        Me.grpFAIterations.Controls.Add(Me.nudFAMaxIter)
        Me.grpFAIterations.Controls.Add(Me.lblFAEps)
        Me.grpFAIterations.Controls.Add(Me.lblFAMaxIter)
        Me.grpFAIterations.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpFAIterations.Location = New System.Drawing.Point(356, 222)
        Me.grpFAIterations.Name = "grpFAIterations"
        Me.grpFAIterations.Size = New System.Drawing.Size(336, 93)
        Me.grpFAIterations.TabIndex = 26
        Me.grpFAIterations.TabStop = False
        Me.grpFAIterations.Text = "Iteration Options"
        '
        'tbFAEps
        '
        Me.tbFAEps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbFAEps.Location = New System.Drawing.Point(159, 58)
        Me.tbFAEps.Name = "tbFAEps"
        Me.tbFAEps.Size = New System.Drawing.Size(168, 22)
        Me.tbFAEps.TabIndex = 11
        '
        'nudFAMaxIter
        '
        Me.nudFAMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudFAMaxIter.Location = New System.Drawing.Point(159, 32)
        Me.nudFAMaxIter.Maximum = New Decimal(New Integer() {100000, 0, 0, 0})
        Me.nudFAMaxIter.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudFAMaxIter.Name = "nudFAMaxIter"
        Me.nudFAMaxIter.Size = New System.Drawing.Size(76, 22)
        Me.nudFAMaxIter.TabIndex = 10
        Me.nudFAMaxIter.Value = New Decimal(New Integer() {250, 0, 0, 0})
        '
        'lblFAEps
        '
        Me.lblFAEps.AutoSize = True
        Me.lblFAEps.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFAEps.Location = New System.Drawing.Point(6, 61)
        Me.lblFAEps.Name = "lblFAEps"
        Me.lblFAEps.Size = New System.Drawing.Size(150, 16)
        Me.lblFAEps.TabIndex = 7
        Me.lblFAEps.Text = "Convergence tolerance:"
        '
        'lblFAMaxIter
        '
        Me.lblFAMaxIter.AutoSize = True
        Me.lblFAMaxIter.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFAMaxIter.Location = New System.Drawing.Point(6, 34)
        Me.lblFAMaxIter.Name = "lblFAMaxIter"
        Me.lblFAMaxIter.Size = New System.Drawing.Size(124, 16)
        Me.lblFAMaxIter.TabIndex = 5
        Me.lblFAMaxIter.Text = "Maximum iterations:"
        '
        'grpFAScoring
        '
        Me.grpFAScoring.Controls.Add(Me.cbFAScoreMethod)
        Me.grpFAScoring.Controls.Add(Me.lblFaScoreMethod)
        Me.grpFAScoring.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpFAScoring.Location = New System.Drawing.Point(356, 148)
        Me.grpFAScoring.Name = "grpFAScoring"
        Me.grpFAScoring.Size = New System.Drawing.Size(336, 65)
        Me.grpFAScoring.TabIndex = 25
        Me.grpFAScoring.TabStop = False
        Me.grpFAScoring.Text = "Scores"
        '
        'cbFAScoreMethod
        '
        Me.cbFAScoreMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbFAScoreMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFAScoreMethod.Location = New System.Drawing.Point(162, 28)
        Me.cbFAScoreMethod.Name = "cbFAScoreMethod"
        Me.cbFAScoreMethod.Size = New System.Drawing.Size(168, 24)
        Me.cbFAScoreMethod.TabIndex = 22
        '
        'lblFaScoreMethod
        '
        Me.lblFaScoreMethod.AutoSize = True
        Me.lblFaScoreMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFaScoreMethod.Location = New System.Drawing.Point(6, 31)
        Me.lblFaScoreMethod.Name = "lblFaScoreMethod"
        Me.lblFaScoreMethod.Size = New System.Drawing.Size(94, 16)
        Me.lblFaScoreMethod.TabIndex = 21
        Me.lblFaScoreMethod.Text = "Score method:"
        '
        'grpFARotation
        '
        Me.grpFARotation.Controls.Add(Me.lblFAPromaxPower)
        Me.grpFARotation.Controls.Add(Me.ckFAKaiserNormalization)
        Me.grpFARotation.Controls.Add(Me.cbFARotation)
        Me.grpFARotation.Controls.Add(Me.lblFaRotationMethod)
        Me.grpFARotation.Controls.Add(Me.nudFAPromaxPower)
        Me.grpFARotation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpFARotation.Location = New System.Drawing.Point(356, 12)
        Me.grpFARotation.Name = "grpFARotation"
        Me.grpFARotation.Size = New System.Drawing.Size(336, 130)
        Me.grpFARotation.TabIndex = 21
        Me.grpFARotation.TabStop = False
        Me.grpFARotation.Text = "Rotation"
        '
        'lblFAPromaxPower
        '
        Me.lblFAPromaxPower.AutoSize = True
        Me.lblFAPromaxPower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFAPromaxPower.Location = New System.Drawing.Point(6, 92)
        Me.lblFAPromaxPower.Name = "lblFAPromaxPower"
        Me.lblFAPromaxPower.Size = New System.Drawing.Size(96, 16)
        Me.lblFAPromaxPower.TabIndex = 24
        Me.lblFAPromaxPower.Text = "Promax power:"
        '
        'ckFAKaiserNormalization
        '
        Me.ckFAKaiserNormalization.AutoSize = True
        Me.ckFAKaiserNormalization.Checked = True
        Me.ckFAKaiserNormalization.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckFAKaiserNormalization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckFAKaiserNormalization.Location = New System.Drawing.Point(9, 58)
        Me.ckFAKaiserNormalization.Name = "ckFAKaiserNormalization"
        Me.ckFAKaiserNormalization.Size = New System.Drawing.Size(177, 20)
        Me.ckFAKaiserNormalization.TabIndex = 23
        Me.ckFAKaiserNormalization.Text = "Use Kaiser normalization"
        Me.ckFAKaiserNormalization.UseVisualStyleBackColor = True
        '
        'cbFARotation
        '
        Me.cbFARotation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbFARotation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFARotation.Location = New System.Drawing.Point(162, 28)
        Me.cbFARotation.Name = "cbFARotation"
        Me.cbFARotation.Size = New System.Drawing.Size(168, 24)
        Me.cbFARotation.TabIndex = 22
        '
        'lblFaRotationMethod
        '
        Me.lblFaRotationMethod.AutoSize = True
        Me.lblFaRotationMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFaRotationMethod.Location = New System.Drawing.Point(6, 31)
        Me.lblFaRotationMethod.Name = "lblFaRotationMethod"
        Me.lblFaRotationMethod.Size = New System.Drawing.Size(108, 16)
        Me.lblFaRotationMethod.TabIndex = 21
        Me.lblFaRotationMethod.Text = "Rotation method:"
        '
        'nudFAPromaxPower
        '
        Me.nudFAPromaxPower.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudFAPromaxPower.Location = New System.Drawing.Point(162, 90)
        Me.nudFAPromaxPower.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudFAPromaxPower.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudFAPromaxPower.Name = "nudFAPromaxPower"
        Me.nudFAPromaxPower.Size = New System.Drawing.Size(110, 22)
        Me.nudFAPromaxPower.TabIndex = 20
        Me.nudFAPromaxPower.Value = New Decimal(New Integer() {4, 0, 0, 0})
        '
        'grpFARetention
        '
        Me.grpFARetention.Controls.Add(Me.nudFAVariance)
        Me.grpFARetention.Controls.Add(Me.nudFAEigen)
        Me.grpFARetention.Controls.Add(Me.nudFAFactors)
        Me.grpFARetention.Controls.Add(Me.optFAExtractVariance)
        Me.grpFARetention.Controls.Add(Me.optFAExtractEigen)
        Me.grpFARetention.Controls.Add(Me.optFAExtractFixed)
        Me.grpFARetention.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpFARetention.Location = New System.Drawing.Point(14, 222)
        Me.grpFARetention.Name = "grpFARetention"
        Me.grpFARetention.Size = New System.Drawing.Size(336, 130)
        Me.grpFARetention.TabIndex = 13
        Me.grpFARetention.TabStop = False
        Me.grpFARetention.Text = "Retention"
        '
        'nudFAVariance
        '
        Me.nudFAVariance.DecimalPlaces = 1
        Me.nudFAVariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudFAVariance.Location = New System.Drawing.Point(220, 90)
        Me.nudFAVariance.Name = "nudFAVariance"
        Me.nudFAVariance.Size = New System.Drawing.Size(110, 22)
        Me.nudFAVariance.TabIndex = 20
        Me.nudFAVariance.Value = New Decimal(New Integer() {70, 0, 0, 0})
        '
        'nudFAEigen
        '
        Me.nudFAEigen.DecimalPlaces = 2
        Me.nudFAEigen.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudFAEigen.Location = New System.Drawing.Point(220, 62)
        Me.nudFAEigen.Name = "nudFAEigen"
        Me.nudFAEigen.Size = New System.Drawing.Size(110, 22)
        Me.nudFAEigen.TabIndex = 19
        Me.nudFAEigen.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'nudFAFactors
        '
        Me.nudFAFactors.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudFAFactors.Location = New System.Drawing.Point(220, 34)
        Me.nudFAFactors.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudFAFactors.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudFAFactors.Name = "nudFAFactors"
        Me.nudFAFactors.Size = New System.Drawing.Size(110, 22)
        Me.nudFAFactors.TabIndex = 18
        Me.nudFAFactors.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'optFAExtractVariance
        '
        Me.optFAExtractVariance.AutoSize = True
        Me.optFAExtractVariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optFAExtractVariance.Location = New System.Drawing.Point(9, 88)
        Me.optFAExtractVariance.Name = "optFAExtractVariance"
        Me.optFAExtractVariance.Size = New System.Drawing.Size(175, 20)
        Me.optFAExtractVariance.TabIndex = 17
        Me.optFAExtractVariance.Text = "Cumulative variance (%):"
        Me.optFAExtractVariance.UseVisualStyleBackColor = True
        '
        'optFAExtractEigen
        '
        Me.optFAExtractEigen.AutoSize = True
        Me.optFAExtractEigen.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optFAExtractEigen.Location = New System.Drawing.Point(9, 62)
        Me.optFAExtractEigen.Name = "optFAExtractEigen"
        Me.optFAExtractEigen.Size = New System.Drawing.Size(133, 20)
        Me.optFAExtractEigen.TabIndex = 16
        Me.optFAExtractEigen.Text = "Eigenvalue cutoff:"
        Me.optFAExtractEigen.UseVisualStyleBackColor = True
        '
        'optFAExtractFixed
        '
        Me.optFAExtractFixed.AutoSize = True
        Me.optFAExtractFixed.Checked = True
        Me.optFAExtractFixed.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optFAExtractFixed.Location = New System.Drawing.Point(9, 36)
        Me.optFAExtractFixed.Name = "optFAExtractFixed"
        Me.optFAExtractFixed.Size = New System.Drawing.Size(169, 20)
        Me.optFAExtractFixed.TabIndex = 15
        Me.optFAExtractFixed.TabStop = True
        Me.optFAExtractFixed.Text = "Fixed number of factors:"
        Me.optFAExtractFixed.UseVisualStyleBackColor = True
        '
        'grpFABasic
        '
        Me.grpFABasic.Controls.Add(Me.cbFAMissingPolicy)
        Me.grpFABasic.Controls.Add(Me.lblMissingFa)
        Me.grpFABasic.Controls.Add(Me.cbFACommunalityInit)
        Me.grpFABasic.Controls.Add(Me.lblFaStartingCommunalities)
        Me.grpFABasic.Controls.Add(Me.cbFAExtraction)
        Me.grpFABasic.Controls.Add(Me.lblFaExtractionMethod)
        Me.grpFABasic.Controls.Add(Me.optFACovariance)
        Me.grpFABasic.Controls.Add(Me.optFACorrelation)
        Me.grpFABasic.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpFABasic.Location = New System.Drawing.Point(14, 12)
        Me.grpFABasic.Name = "grpFABasic"
        Me.grpFABasic.Size = New System.Drawing.Size(336, 194)
        Me.grpFABasic.TabIndex = 2
        Me.grpFABasic.TabStop = False
        Me.grpFABasic.Text = "Model Setup"
        '
        'cbFAMissingPolicy
        '
        Me.cbFAMissingPolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbFAMissingPolicy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFAMissingPolicy.Location = New System.Drawing.Point(162, 157)
        Me.cbFAMissingPolicy.Name = "cbFAMissingPolicy"
        Me.cbFAMissingPolicy.Size = New System.Drawing.Size(168, 24)
        Me.cbFAMissingPolicy.TabIndex = 20
        '
        'lblMissingFa
        '
        Me.lblMissingFa.AutoSize = True
        Me.lblMissingFa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMissingFa.Location = New System.Drawing.Point(6, 160)
        Me.lblMissingFa.Name = "lblMissingFa"
        Me.lblMissingFa.Size = New System.Drawing.Size(99, 16)
        Me.lblMissingFa.TabIndex = 19
        Me.lblMissingFa.Text = "Missing values:"
        '
        'cbFACommunalityInit
        '
        Me.cbFACommunalityInit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbFACommunalityInit.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFACommunalityInit.Location = New System.Drawing.Point(162, 127)
        Me.cbFACommunalityInit.Name = "cbFACommunalityInit"
        Me.cbFACommunalityInit.Size = New System.Drawing.Size(168, 24)
        Me.cbFACommunalityInit.TabIndex = 18
        '
        'lblFaStartingCommunalities
        '
        Me.lblFaStartingCommunalities.AutoSize = True
        Me.lblFaStartingCommunalities.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFaStartingCommunalities.Location = New System.Drawing.Point(6, 130)
        Me.lblFaStartingCommunalities.Name = "lblFaStartingCommunalities"
        Me.lblFaStartingCommunalities.Size = New System.Drawing.Size(144, 16)
        Me.lblFaStartingCommunalities.TabIndex = 17
        Me.lblFaStartingCommunalities.Text = "Starting communalities:"
        '
        'cbFAExtraction
        '
        Me.cbFAExtraction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbFAExtraction.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFAExtraction.Location = New System.Drawing.Point(162, 95)
        Me.cbFAExtraction.Name = "cbFAExtraction"
        Me.cbFAExtraction.Size = New System.Drawing.Size(168, 24)
        Me.cbFAExtraction.TabIndex = 16
        '
        'lblFaExtractionMethod
        '
        Me.lblFaExtractionMethod.AutoSize = True
        Me.lblFaExtractionMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFaExtractionMethod.Location = New System.Drawing.Point(6, 98)
        Me.lblFaExtractionMethod.Name = "lblFaExtractionMethod"
        Me.lblFaExtractionMethod.Size = New System.Drawing.Size(116, 16)
        Me.lblFaExtractionMethod.TabIndex = 15
        Me.lblFaExtractionMethod.Text = "Extraction method:"
        '
        'optFACovariance
        '
        Me.optFACovariance.AutoSize = True
        Me.optFACovariance.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optFACovariance.Location = New System.Drawing.Point(18, 63)
        Me.optFACovariance.Name = "optFACovariance"
        Me.optFACovariance.Size = New System.Drawing.Size(135, 20)
        Me.optFACovariance.TabIndex = 14
        Me.optFACovariance.Text = "Covariance matrix"
        Me.optFACovariance.UseVisualStyleBackColor = True
        '
        'optFACorrelation
        '
        Me.optFACorrelation.AutoSize = True
        Me.optFACorrelation.Checked = True
        Me.optFACorrelation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optFACorrelation.Location = New System.Drawing.Point(18, 31)
        Me.optFACorrelation.Name = "optFACorrelation"
        Me.optFACorrelation.Size = New System.Drawing.Size(131, 20)
        Me.optFACorrelation.TabIndex = 13
        Me.optFACorrelation.TabStop = True
        Me.optFACorrelation.Text = "Correlation matrix"
        Me.optFACorrelation.UseVisualStyleBackColor = True
        '
        'TabPageOptionsDA
        '
        Me.TabPageOptionsDA.Controls.Add(Me.grpDAPrior)
        Me.TabPageOptionsDA.Controls.Add(Me.grpDAValidation)
        Me.TabPageOptionsDA.Controls.Add(Me.grpDABasic)
        Me.TabPageOptionsDA.Location = New System.Drawing.Point(4, 25)
        Me.TabPageOptionsDA.Name = "TabPageOptionsDA"
        Me.TabPageOptionsDA.Size = New System.Drawing.Size(836, 465)
        Me.TabPageOptionsDA.TabIndex = 7
        Me.TabPageOptionsDA.Text = "Options"
        Me.TabPageOptionsDA.UseVisualStyleBackColor = True
        '
        'grpDAPrior
        '
        Me.grpDAPrior.Controls.Add(Me.tbDAUserPriors)
        Me.grpDAPrior.Controls.Add(Me.lblDAUserPriors)
        Me.grpDAPrior.Controls.Add(Me.cbDAPriors)
        Me.grpDAPrior.Controls.Add(Me.lblDAPriors)
        Me.grpDAPrior.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpDAPrior.Location = New System.Drawing.Point(393, 3)
        Me.grpDAPrior.Name = "grpDAPrior"
        Me.grpDAPrior.Size = New System.Drawing.Size(410, 183)
        Me.grpDAPrior.TabIndex = 35
        Me.grpDAPrior.TabStop = False
        Me.grpDAPrior.Text = "Group Prior Probabilities"
        '
        'tbDAUserPriors
        '
        Me.tbDAUserPriors.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbDAUserPriors.Location = New System.Drawing.Point(88, 62)
        Me.tbDAUserPriors.Multiline = True
        Me.tbDAUserPriors.Name = "tbDAUserPriors"
        Me.tbDAUserPriors.Size = New System.Drawing.Size(316, 109)
        Me.tbDAUserPriors.TabIndex = 42
        '
        'lblDAUserPriors
        '
        Me.lblDAUserPriors.AutoSize = True
        Me.lblDAUserPriors.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDAUserPriors.Location = New System.Drawing.Point(6, 62)
        Me.lblDAUserPriors.Name = "lblDAUserPriors"
        Me.lblDAUserPriors.Size = New System.Drawing.Size(77, 16)
        Me.lblDAUserPriors.TabIndex = 41
        Me.lblDAUserPriors.Text = "User Priors:"
        '
        'cbDAPriors
        '
        Me.cbDAPriors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDAPriors.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDAPriors.Location = New System.Drawing.Point(88, 28)
        Me.cbDAPriors.Name = "cbDAPriors"
        Me.cbDAPriors.Size = New System.Drawing.Size(316, 24)
        Me.cbDAPriors.TabIndex = 40
        '
        'lblDAPriors
        '
        Me.lblDAPriors.AutoSize = True
        Me.lblDAPriors.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDAPriors.Location = New System.Drawing.Point(6, 29)
        Me.lblDAPriors.Name = "lblDAPriors"
        Me.lblDAPriors.Size = New System.Drawing.Size(45, 16)
        Me.lblDAPriors.TabIndex = 39
        Me.lblDAPriors.Text = "Priors:"
        '
        'grpDAValidation
        '
        Me.grpDAValidation.Controls.Add(Me.tbDASeed)
        Me.grpDAValidation.Controls.Add(Me.tbDAHoldoutFraction)
        Me.grpDAValidation.Controls.Add(Me.lblDASeed)
        Me.grpDAValidation.Controls.Add(Me.ckDAStratified)
        Me.grpDAValidation.Controls.Add(Me.lblDAHoldoutFraction)
        Me.grpDAValidation.Controls.Add(Me.nudDAFolds)
        Me.grpDAValidation.Controls.Add(Me.lblDAFolds)
        Me.grpDAValidation.Controls.Add(Me.cbDAValidation)
        Me.grpDAValidation.Controls.Add(Me.lblDAValidation)
        Me.grpDAValidation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpDAValidation.Location = New System.Drawing.Point(5, 165)
        Me.grpDAValidation.Name = "grpDAValidation"
        Me.grpDAValidation.Size = New System.Drawing.Size(382, 164)
        Me.grpDAValidation.TabIndex = 28
        Me.grpDAValidation.TabStop = False
        Me.grpDAValidation.Text = "Validation / Resampling"
        '
        'tbDASeed
        '
        Me.tbDASeed.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbDASeed.Location = New System.Drawing.Point(177, 135)
        Me.tbDASeed.Name = "tbDASeed"
        Me.tbDASeed.Size = New System.Drawing.Size(188, 22)
        Me.tbDASeed.TabIndex = 39
        '
        'tbDAHoldoutFraction
        '
        Me.tbDAHoldoutFraction.DecimalPlaces = 2
        Me.tbDAHoldoutFraction.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbDAHoldoutFraction.Location = New System.Drawing.Point(289, 106)
        Me.tbDAHoldoutFraction.Maximum = New Decimal(New Integer() {99, 0, 0, 131072})
        Me.tbDAHoldoutFraction.Minimum = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.tbDAHoldoutFraction.Name = "tbDAHoldoutFraction"
        Me.tbDAHoldoutFraction.Size = New System.Drawing.Size(76, 22)
        Me.tbDAHoldoutFraction.TabIndex = 39
        Me.tbDAHoldoutFraction.Value = New Decimal(New Integer() {50, 0, 0, 131072})
        '
        'lblDASeed
        '
        Me.lblDASeed.AutoSize = True
        Me.lblDASeed.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDASeed.Location = New System.Drawing.Point(16, 138)
        Me.lblDASeed.Name = "lblDASeed"
        Me.lblDASeed.Size = New System.Drawing.Size(155, 16)
        Me.lblDASeed.TabIndex = 38
        Me.lblDASeed.Text = "Random seed (optional):"
        '
        'ckDAStratified
        '
        Me.ckDAStratified.AutoSize = True
        Me.ckDAStratified.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ckDAStratified.Location = New System.Drawing.Point(178, 21)
        Me.ckDAStratified.Name = "ckDAStratified"
        Me.ckDAStratified.Size = New System.Drawing.Size(81, 20)
        Me.ckDAStratified.TabIndex = 37
        Me.ckDAStratified.Text = "Stratified"
        Me.ckDAStratified.UseVisualStyleBackColor = True
        '
        'lblDAHoldoutFraction
        '
        Me.lblDAHoldoutFraction.AutoSize = True
        Me.lblDAHoldoutFraction.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDAHoldoutFraction.Location = New System.Drawing.Point(151, 108)
        Me.lblDAHoldoutFraction.Name = "lblDAHoldoutFraction"
        Me.lblDAHoldoutFraction.Size = New System.Drawing.Size(108, 16)
        Me.lblDAHoldoutFraction.TabIndex = 38
        Me.lblDAHoldoutFraction.Text = "Holdout Fraction:"
        '
        'nudDAFolds
        '
        Me.nudDAFolds.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.nudDAFolds.Location = New System.Drawing.Point(289, 78)
        Me.nudDAFolds.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudDAFolds.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudDAFolds.Name = "nudDAFolds"
        Me.nudDAFolds.Size = New System.Drawing.Size(76, 22)
        Me.nudDAFolds.TabIndex = 34
        Me.nudDAFolds.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'lblDAFolds
        '
        Me.lblDAFolds.AutoSize = True
        Me.lblDAFolds.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDAFolds.Location = New System.Drawing.Point(151, 80)
        Me.lblDAFolds.Name = "lblDAFolds"
        Me.lblDAFolds.Size = New System.Drawing.Size(107, 16)
        Me.lblDAFolds.TabIndex = 33
        Me.lblDAFolds.Text = "Validation Folds:"
        '
        'cbDAValidation
        '
        Me.cbDAValidation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDAValidation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDAValidation.Location = New System.Drawing.Point(177, 48)
        Me.cbDAValidation.Name = "cbDAValidation"
        Me.cbDAValidation.Size = New System.Drawing.Size(188, 24)
        Me.cbDAValidation.TabIndex = 32
        '
        'lblDAValidation
        '
        Me.lblDAValidation.AutoSize = True
        Me.lblDAValidation.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDAValidation.Location = New System.Drawing.Point(16, 51)
        Me.lblDAValidation.Name = "lblDAValidation"
        Me.lblDAValidation.Size = New System.Drawing.Size(108, 16)
        Me.lblDAValidation.TabIndex = 31
        Me.lblDAValidation.Text = "Validation Mode:"
        '
        'grpDABasic
        '
        Me.grpDABasic.Controls.Add(Me.tbDARegularization)
        Me.grpDABasic.Controls.Add(Me.lblDARegularization)
        Me.grpDABasic.Controls.Add(Me.cbDAMethod)
        Me.grpDABasic.Controls.Add(Me.cbDAStandardization)
        Me.grpDABasic.Controls.Add(Me.lblDAStandardization)
        Me.grpDABasic.Controls.Add(Me.cbDAMissingPolicy)
        Me.grpDABasic.Controls.Add(Me.lblMissingDA)
        Me.grpDABasic.Controls.Add(Me.lblDAMethod)
        Me.grpDABasic.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpDABasic.Location = New System.Drawing.Point(5, 3)
        Me.grpDABasic.Name = "grpDABasic"
        Me.grpDABasic.Size = New System.Drawing.Size(382, 156)
        Me.grpDABasic.TabIndex = 27
        Me.grpDABasic.TabStop = False
        Me.grpDABasic.Text = "Model Specification"
        '
        'tbDARegularization
        '
        Me.tbDARegularization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbDARegularization.Location = New System.Drawing.Point(167, 122)
        Me.tbDARegularization.Name = "tbDARegularization"
        Me.tbDARegularization.Size = New System.Drawing.Size(198, 22)
        Me.tbDARegularization.TabIndex = 34
        '
        'lblDARegularization
        '
        Me.lblDARegularization.AutoSize = True
        Me.lblDARegularization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDARegularization.Location = New System.Drawing.Point(6, 124)
        Me.lblDARegularization.Name = "lblDARegularization"
        Me.lblDARegularization.Size = New System.Drawing.Size(162, 16)
        Me.lblDARegularization.TabIndex = 33
        Me.lblDARegularization.Text = "Covariance regularization:"
        '
        'cbDAMethod
        '
        Me.cbDAMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDAMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDAMethod.Location = New System.Drawing.Point(167, 28)
        Me.cbDAMethod.Name = "cbDAMethod"
        Me.cbDAMethod.Size = New System.Drawing.Size(198, 24)
        Me.cbDAMethod.TabIndex = 23
        '
        'cbDAStandardization
        '
        Me.cbDAStandardization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDAStandardization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDAStandardization.Location = New System.Drawing.Point(167, 59)
        Me.cbDAStandardization.Name = "cbDAStandardization"
        Me.cbDAStandardization.Size = New System.Drawing.Size(198, 24)
        Me.cbDAStandardization.TabIndex = 22
        '
        'lblDAStandardization
        '
        Me.lblDAStandardization.AutoSize = True
        Me.lblDAStandardization.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDAStandardization.Location = New System.Drawing.Point(6, 62)
        Me.lblDAStandardization.Name = "lblDAStandardization"
        Me.lblDAStandardization.Size = New System.Drawing.Size(103, 16)
        Me.lblDAStandardization.TabIndex = 21
        Me.lblDAStandardization.Text = "Standardization:"
        '
        'cbDAMissingPolicy
        '
        Me.cbDAMissingPolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDAMissingPolicy.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDAMissingPolicy.Location = New System.Drawing.Point(167, 90)
        Me.cbDAMissingPolicy.Name = "cbDAMissingPolicy"
        Me.cbDAMissingPolicy.Size = New System.Drawing.Size(198, 24)
        Me.cbDAMissingPolicy.TabIndex = 20
        '
        'lblMissingDA
        '
        Me.lblMissingDA.AutoSize = True
        Me.lblMissingDA.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMissingDA.Location = New System.Drawing.Point(6, 93)
        Me.lblMissingDA.Name = "lblMissingDA"
        Me.lblMissingDA.Size = New System.Drawing.Size(99, 16)
        Me.lblMissingDA.TabIndex = 19
        Me.lblMissingDA.Text = "Missing values:"
        '
        'lblDAMethod
        '
        Me.lblDAMethod.AutoSize = True
        Me.lblDAMethod.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDAMethod.Location = New System.Drawing.Point(6, 31)
        Me.lblDAMethod.Name = "lblDAMethod"
        Me.lblDAMethod.Size = New System.Drawing.Size(55, 16)
        Me.lblDAMethod.TabIndex = 15
        Me.lblDAMethod.Text = "Method:"
        '
        'btnHelp
        '
        Me.btnHelp.Location = New System.Drawing.Point(687, 502)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(75, 23)
        Me.btnHelp.TabIndex = 6
        Me.btnHelp.Text = "Help"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btCalculate
        '
        Me.btCalculate.Location = New System.Drawing.Point(768, 502)
        Me.btCalculate.Name = "btCalculate"
        Me.btCalculate.Size = New System.Drawing.Size(75, 23)
        Me.btCalculate.TabIndex = 5
        Me.btCalculate.Text = "Fit"
        Me.btCalculate.UseVisualStyleBackColor = True
        '
        'refKmeansCenters
        '
        Me.refKmeansCenters.Address = ""
        Me.refKmeansCenters.BackColor = System.Drawing.Color.Transparent
        Me.refKmeansCenters.ExcelConnector = Nothing
        Me.refKmeansCenters.ImageMaximized = Global.BESHStatNG.My.Resources.Resources.imgMaximized
        Me.refKmeansCenters.ImageMinimized = CType(resources.GetObject("refKmeansCenters.ImageMinimized"), System.Drawing.Image)
        Me.refKmeansCenters.Location = New System.Drawing.Point(15, 312)
        Me.refKmeansCenters.Margin = New System.Windows.Forms.Padding(4)
        Me.refKmeansCenters.Name = "refKmeansCenters"
        Me.refKmeansCenters.RefEditFont = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.refKmeansCenters.Size = New System.Drawing.Size(300, 32)
        Me.refKmeansCenters.TabIndex = 16
        '
        'Ui11PCA
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(856, 539)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btCalculate)
        Me.Controls.Add(Me.TabControl1)
        Me.Name = "Ui11PCA"
        Me.ShowIcon = False
        Me.Text = "Form1"
        Me.TabPageOptionsSPM.ResumeLayout(False)
        Me.TabPageOptionsSPM.PerformLayout()
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPageOptionsPCA.ResumeLayout(False)
        Me.grpExtract.ResumeLayout(False)
        Me.grpExtract.PerformLayout()
        CType(Me.spinBtnExtractEigen, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnExtractComp, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinBtnExtractVariance, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpAnalyzeType.ResumeLayout(False)
        Me.grpAnalyzeType.PerformLayout()
        Me.grpIterOptions.ResumeLayout(False)
        Me.grpIterOptions.PerformLayout()
        Me.TabPageOptionsKmeans.ResumeLayout(False)
        Me.TabPageOptionsKmeans.PerformLayout()
        Me.grpKmeansFit.ResumeLayout(False)
        Me.grpKmeansFit.PerformLayout()
        CType(Me.nudKmeansMaxIterations, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudKmeansStarts, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpKmeansPreprocess.ResumeLayout(False)
        Me.grpKmeansPreprocess.PerformLayout()
        Me.grpKmeansBasic.ResumeLayout(False)
        Me.grpKmeansBasic.PerformLayout()
        CType(Me.nudKmeansClusters, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageOptionsHierarchicalClustering.ResumeLayout(False)
        Me.grpHierarchicalDendrogram.ResumeLayout(False)
        Me.grpHierarchicalDendrogram.PerformLayout()
        Me.grpHierarchicalMembership.ResumeLayout(False)
        Me.grpHierarchicalMembership.PerformLayout()
        CType(Me.nudHierarchicalClusters, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpHierarchicalPreprocess.ResumeLayout(False)
        Me.grpHierarchicalPreprocess.PerformLayout()
        Me.grpHierarchicalBasic.ResumeLayout(False)
        Me.grpHierarchicalBasic.PerformLayout()
        Me.TabPageOptionsFA.ResumeLayout(False)
        Me.grpFAIterations.ResumeLayout(False)
        Me.grpFAIterations.PerformLayout()
        CType(Me.nudFAMaxIter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpFAScoring.ResumeLayout(False)
        Me.grpFAScoring.PerformLayout()
        Me.grpFARotation.ResumeLayout(False)
        Me.grpFARotation.PerformLayout()
        CType(Me.nudFAPromaxPower, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpFARetention.ResumeLayout(False)
        Me.grpFARetention.PerformLayout()
        CType(Me.nudFAVariance, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudFAEigen, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudFAFactors, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpFABasic.ResumeLayout(False)
        Me.grpFABasic.PerformLayout()
        Me.TabPageOptionsDA.ResumeLayout(False)
        Me.grpDAPrior.ResumeLayout(False)
        Me.grpDAPrior.PerformLayout()
        Me.grpDAValidation.ResumeLayout(False)
        Me.grpDAValidation.PerformLayout()
        CType(Me.tbDAHoldoutFraction, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudDAFolds, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpDABasic.ResumeLayout(False)
        Me.grpDABasic.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabPageOptionsSPM As Windows.Forms.TabPage
    Friend WithEvents TabPage1 As Windows.Forms.TabPage
    Friend WithEvents lbXs As Windows.Forms.ListBox
    Friend WithEvents cbSheetsList As Windows.Forms.ComboBox
    Friend WithEvents btReload As Windows.Forms.Button
    Friend WithEvents btRemoveX As Windows.Forms.Button
    Friend WithEvents btAddX As Windows.Forms.Button
    Friend WithEvents lbAllColumns As Windows.Forms.ListBox
    Friend WithEvents lblAllColumns As Windows.Forms.Label
    Friend WithEvents lblX As Windows.Forms.Label
    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents ckShowRegressionLines As Windows.Forms.CheckBox
    Friend WithEvents ckDisplayCorrelCoef As Windows.Forms.CheckBox
    Friend WithEvents btnHelp As Windows.Forms.Button
    Friend WithEvents btCalculate As Windows.Forms.Button
    Friend WithEvents lblSelectedSheet As Windows.Forms.Label
    Friend WithEvents TabPageOptionsPCA As Windows.Forms.TabPage
    Friend WithEvents grpAnalyzeType As Windows.Forms.GroupBox
    Friend WithEvents optCovar As Windows.Forms.RadioButton
    Friend WithEvents optCorr As Windows.Forms.RadioButton
    Friend WithEvents grpIterOptions As Windows.Forms.GroupBox
    Friend WithEvents tbMaxIter As Windows.Forms.TextBox
    Friend WithEvents lblMaxIter As Windows.Forms.Label
    Friend WithEvents lblEps As Windows.Forms.Label
    Friend WithEvents tbEps As Windows.Forms.TextBox
    Friend WithEvents grpExtract As Windows.Forms.GroupBox
    Friend WithEvents optExtractVariance As Windows.Forms.RadioButton
    Friend WithEvents optExtractFixed As Windows.Forms.RadioButton
    Friend WithEvents optExtractEigen As Windows.Forms.RadioButton
    Friend WithEvents spinBtnExtractVariance As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnExtractComp As Windows.Forms.NumericUpDown
    Friend WithEvents spinBtnExtractEigen As Windows.Forms.NumericUpDown
    Friend WithEvents ckFirstRow As Windows.Forms.CheckBox
    Friend WithEvents TabPageOptionsKmeans As Windows.Forms.TabPage
    Friend WithEvents grpKmeansBasic As Windows.Forms.GroupBox
    Friend WithEvents cbKmeansInitialization As Windows.Forms.ComboBox
    Friend WithEvents lblInit As Windows.Forms.Label
    Friend WithEvents lblK As Windows.Forms.Label
    Friend WithEvents nudKmeansClusters As Windows.Forms.NumericUpDown
    Friend WithEvents lblDist As Windows.Forms.Label
    Friend WithEvents cbKmeansDistance As Windows.Forms.ComboBox
    Friend WithEvents grpKmeansPreprocess As Windows.Forms.GroupBox
    Friend WithEvents cbKmeansStandardization As Windows.Forms.ComboBox
    Friend WithEvents lblStd As Windows.Forms.Label
    Friend WithEvents cbKmeansMissingPolicy As Windows.Forms.ComboBox
    Friend WithEvents lblMissing As Windows.Forms.Label
    Friend WithEvents cbKmeansEmptyCluster As Windows.Forms.ComboBox
    Friend WithEvents lblEmpty As Windows.Forms.Label
    Friend WithEvents grpKmeansFit As Windows.Forms.GroupBox
    Friend WithEvents nudKmeansStarts As Windows.Forms.NumericUpDown
    Friend WithEvents lblTol As Windows.Forms.Label
    Friend WithEvents lblKmeanMaxIter As Windows.Forms.Label
    Friend WithEvents lblStarts As Windows.Forms.Label
    Friend WithEvents nudKmeansMaxIterations As Windows.Forms.NumericUpDown
    Friend WithEvents tbKmeansTolerance As Windows.Forms.TextBox
    Friend WithEvents lblSeed As Windows.Forms.Label
    Friend WithEvents tbKmeansSeed As Windows.Forms.TextBox
    Friend WithEvents cbKmeansRowLabel As Windows.Forms.ComboBox
    Friend WithEvents lblKmeansRowLabel As Windows.Forms.Label
    Friend WithEvents lblCenterHint As Windows.Forms.Label
    Friend WithEvents refKmeansCenters As Global.BESHStatNG.Excel2007RefEdit
    Friend WithEvents lblCenterRef As Windows.Forms.Label
    Friend WithEvents TabPageOptionsHierarchicalClustering As Windows.Forms.TabPage
    Friend WithEvents grpHierarchicalBasic As Windows.Forms.GroupBox
    Friend WithEvents cbHierarchicalDistance As Windows.Forms.ComboBox
    Friend WithEvents lblDistance As Windows.Forms.Label
    Friend WithEvents cbHierarchicalLinkage As Windows.Forms.ComboBox
    Friend WithEvents lblHierarchicalMinkowskiPower As Windows.Forms.Label
    Friend WithEvents lblLinkage As Windows.Forms.Label
    Friend WithEvents tbHierarchicalMinkowskiPower As Windows.Forms.TextBox
    Friend WithEvents grpHierarchicalPreprocess As Windows.Forms.GroupBox
    Friend WithEvents cbHierarchicalMissingPolicy As Windows.Forms.ComboBox
    Friend WithEvents lblMissingHierarchicalClustering As Windows.Forms.Label
    Friend WithEvents cbHierarchicalStandardization As Windows.Forms.ComboBox
    Friend WithEvents lblStdHierarchicalClustering As Windows.Forms.Label
    Friend WithEvents grpHierarchicalMembership As Windows.Forms.GroupBox
    Friend WithEvents optHierarchicalCutByClusters As Windows.Forms.RadioButton
    Friend WithEvents lblMembershipHint As Windows.Forms.Label
    Friend WithEvents tbHierarchicalCutHeight As Windows.Forms.TextBox
    Friend WithEvents optHierarchicalCutByHeight As Windows.Forms.RadioButton
    Friend WithEvents grpHierarchicalDendrogram As Windows.Forms.GroupBox
    Friend WithEvents cbHierarchicalOrientation As Windows.Forms.ComboBox
    Friend WithEvents lblOrientation As Windows.Forms.Label
    Friend WithEvents cbHierarchicalHeightMode As Windows.Forms.ComboBox
    Friend WithEvents lblLabelMode As Windows.Forms.Label
    Friend WithEvents lblHeightMode As Windows.Forms.Label
    Friend WithEvents nudHierarchicalClusters As Windows.Forms.NumericUpDown
    Friend WithEvents cbHierarchicalLabelMode As Windows.Forms.ComboBox
    Friend WithEvents ckHierarchicalCreateDendrogram As Windows.Forms.CheckBox
    Friend WithEvents TabPageOptionsFA As Windows.Forms.TabPage
    Friend WithEvents grpFABasic As Windows.Forms.GroupBox
    Friend WithEvents grpFARetention As Windows.Forms.GroupBox
    Friend WithEvents cbFAMissingPolicy As Windows.Forms.ComboBox
    Friend WithEvents lblMissingFa As Windows.Forms.Label
    Friend WithEvents cbFACommunalityInit As Windows.Forms.ComboBox
    Friend WithEvents lblFaStartingCommunalities As Windows.Forms.Label
    Friend WithEvents cbFAExtraction As Windows.Forms.ComboBox
    Friend WithEvents lblFaExtractionMethod As Windows.Forms.Label
    Friend WithEvents optFACovariance As Windows.Forms.RadioButton
    Friend WithEvents optFACorrelation As Windows.Forms.RadioButton
    Friend WithEvents optFAExtractEigen As Windows.Forms.RadioButton
    Friend WithEvents optFAExtractFixed As Windows.Forms.RadioButton
    Friend WithEvents optFAExtractVariance As Windows.Forms.RadioButton
    Friend WithEvents grpFARotation As Windows.Forms.GroupBox
    Friend WithEvents nudFAPromaxPower As Windows.Forms.NumericUpDown
    Friend WithEvents nudFAVariance As Windows.Forms.NumericUpDown
    Friend WithEvents nudFAEigen As Windows.Forms.NumericUpDown
    Friend WithEvents nudFAFactors As Windows.Forms.NumericUpDown
    Friend WithEvents lblFAPromaxPower As Windows.Forms.Label
    Friend WithEvents ckFAKaiserNormalization As Windows.Forms.CheckBox
    Friend WithEvents cbFARotation As Windows.Forms.ComboBox
    Friend WithEvents lblFaRotationMethod As Windows.Forms.Label
    Friend WithEvents grpFAScoring As Windows.Forms.GroupBox
    Friend WithEvents cbFAScoreMethod As Windows.Forms.ComboBox
    Friend WithEvents lblFaScoreMethod As Windows.Forms.Label
    Friend WithEvents grpFAIterations As Windows.Forms.GroupBox
    Friend WithEvents tbFAEps As Windows.Forms.TextBox
    Friend WithEvents nudFAMaxIter As Windows.Forms.NumericUpDown
    Friend WithEvents lblFAEps As Windows.Forms.Label
    Friend WithEvents lblFAMaxIter As Windows.Forms.Label
    Friend WithEvents cbGruppingVar As Windows.Forms.ComboBox
    Friend WithEvents lblGruppingVar As Windows.Forms.Label
    Friend WithEvents TabPageOptionsDA As Windows.Forms.TabPage
    Friend WithEvents grpDABasic As Windows.Forms.GroupBox
    Friend WithEvents cbDAMissingPolicy As Windows.Forms.ComboBox
    Friend WithEvents lblMissingDA As Windows.Forms.Label
    Friend WithEvents lblDAMethod As Windows.Forms.Label
    Friend WithEvents cbDAMethod As Windows.Forms.ComboBox
    Friend WithEvents cbDAStandardization As Windows.Forms.ComboBox
    Friend WithEvents lblDAStandardization As Windows.Forms.Label
    Friend WithEvents tbDARegularization As Windows.Forms.TextBox
    Friend WithEvents lblDARegularization As Windows.Forms.Label
    Friend WithEvents grpDAPrior As Windows.Forms.GroupBox
    Friend WithEvents grpDAValidation As Windows.Forms.GroupBox
    Friend WithEvents nudDAFolds As Windows.Forms.NumericUpDown
    Friend WithEvents lblDAFolds As Windows.Forms.Label
    Friend WithEvents cbDAValidation As Windows.Forms.ComboBox
    Friend WithEvents lblDAValidation As Windows.Forms.Label
    Friend WithEvents tbDAUserPriors As Windows.Forms.TextBox
    Friend WithEvents lblDAUserPriors As Windows.Forms.Label
    Friend WithEvents cbDAPriors As Windows.Forms.ComboBox
    Friend WithEvents lblDAPriors As Windows.Forms.Label
    Friend WithEvents tbDASeed As Windows.Forms.TextBox
    Friend WithEvents tbDAHoldoutFraction As Windows.Forms.NumericUpDown
    Friend WithEvents lblDASeed As Windows.Forms.Label
    Friend WithEvents ckDAStratified As Windows.Forms.CheckBox
    Friend WithEvents lblDAHoldoutFraction As Windows.Forms.Label
End Class
