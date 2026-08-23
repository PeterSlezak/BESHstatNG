Option Explicit On
Option Strict On
Option Infer On
Option Compare Binary

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.StatisticalProcessControl
Imports Excel = Microsoft.Office.Interop.Excel

Public Class Ui22MultivariateControlCharts

    Private Const MinimumFormWidth As Integer = 900
    Private Const MinimumFormHeight As Integer = 740
    Private Const NoneColumnText As String = "(none)"

    Private pWorksheet As Excel.Worksheet
    Private pWorkbook As Excel.Workbook
    Private pColumnInfo As New Dictionary(Of String, VarColumnInfo)(StringComparer.Ordinal)
    Private pCancelRequested As Boolean
    Private pBusy As Boolean
    Private pSuppressUiEvents As Boolean
    Private pAllowPseudoInverseBeforeGeneralizedVariance As Nullable(Of Boolean)
    Private ReadOnly pToolTip As New ToolTip()

    Private Enum ObservationStructure
        IndividualObservations = 0
        RationalSubgroups = 1
    End Enum

    Private Enum DiagnosticsScope
        AllPoints = 0
        SignalledPointsOnly = 1
    End Enum

    Private NotInheritable Class ComboItem(Of T)
        Public Sub New(displayText As String, value As T)
            Me.DisplayText = displayText
            Me.Value = value
        End Sub

        Public ReadOnly Property DisplayText As String
        Public ReadOnly Property Value As T

        Public Overrides Function ToString() As String
            Return DisplayText
        End Function
    End Class

    Private NotInheritable Class ChartChoice
        Public Sub New(displayText As String,
                       chartType As SpcChartType,
                       description As String,
                       requirements As String,
                       allowsSubgroups As Boolean,
                       requiresSubgroups As Boolean)
            Me.DisplayText = displayText
            Me.ChartType = chartType
            Me.Description = description
            Me.Requirements = requirements
            Me.AllowsSubgroups = allowsSubgroups
            Me.RequiresSubgroups = requiresSubgroups
        End Sub

        Public ReadOnly Property DisplayText As String
        Public ReadOnly Property ChartType As SpcChartType
        Public ReadOnly Property Description As String
        Public ReadOnly Property Requirements As String
        Public ReadOnly Property AllowsSubgroups As Boolean
        Public ReadOnly Property RequiresSubgroups As Boolean

        Public Overrides Function ToString() As String
            Return DisplayText
        End Function
    End Class

    Private NotInheritable Class InputRowContext
        Public Property FirstRow As Integer
        Public Property LastRow As Integer
        Public Property PointIndexByRow As Integer()
        Public Property PointCount As Integer
        Public Property IsGrouped As Boolean

        Public ReadOnly Property RowCount As Integer
            Get
                Return LastRow - FirstRow + 1
            End Get
        End Property
    End Class

    Private NotInheritable Class PointAssignments
        Public Property StageIds As String()
        Public Property Phases As SpcPhase()
    End Class

    Private NotInheritable Class PointExclusions
        Public Property Scopes As SpcExclusionScope()
        Public Property Reasons As String()
    End Class

    Private Shared ReadOnly ChartChoices As ChartChoice() = {
        New ChartChoice(
            "Hotelling T-squared",
            SpcChartType.HotellingT2,
            "Monitors the joint location of correlated process variables using their covariance structure.",
            "Select at least two numeric measurement variables. Choose individual observations or rational subgroups; grouped Hotelling limits require equal complete subgroup sizes.",
            allowsSubgroups:=True,
            requiresSubgroups:=False),
        New ChartChoice(
            "Generalized variance",
            SpcChartType.GeneralizedVariance,
            "Monitors changes in overall multivariate dispersion through the determinant of each subgroup covariance matrix.",
            "Select at least two numeric measurement variables and one subgroup-ID column. Every subgroup must contain more complete observations than variables.",
            allowsSubgroups:=True,
            requiresSubgroups:=True),
        New ChartChoice(
            "PCA T-squared",
            SpcChartType.PcaT2,
            "Monitors variation within the retained principal-component model space.",
            "Select at least two numeric measurement variables. Observations must be ordered individual rows.",
            allowsSubgroups:=False,
            requiresSubgroups:=False),
        New ChartChoice(
            "PCA Q",
            SpcChartType.PcaQ,
            "Monitors residual variation not explained by the retained principal components.",
            "Select at least two numeric measurement variables. At least one nonzero component must remain in the residual subspace.",
            allowsSubgroups:=False,
            requiresSubgroups:=False),
        New ChartChoice(
            "MEWMA",
            SpcChartType.Mewma,
            "Uses an exponentially weighted multivariate state to detect sustained small joint shifts.",
            "Select at least two ordered numeric measurement variables. Lambda must be in (0, 1].",
            allowsSubgroups:=False,
            requiresSubgroups:=False),
        New ChartChoice(
            "MCUSUM (Crosier)",
            SpcChartType.Mcusum,
            "Uses the Crosier multivariate cumulative-sum statistic to detect persistent joint mean shifts.",
            "Select at least two ordered numeric measurement variables and specify design parameters k and h.",
            allowsSubgroups:=False,
            requiresSubgroups:=False)
    }

    Public Sub New(analysis As String, tagn As Integer)
        InitializeComponent()
        Me.Text = analysis
        Me.Tag = tagn

        ConfigureDesignerCorrections()
        ConfigureResponsiveLayout()
        ConfigureGridColumns()
        InitializeOptionControls()
        WireRoleButtons()
        Me.WireHelp(Me.btnHelp)
    End Sub

#Region "Initialization and layout"

    Private Sub ConfigureDesignerCorrections()
        'These controls are remnants of Ui21 and are not part of the multivariate request.
        Dim obsoleteControls As Control() = {
            lblCount, lbCounts, btAddCounts, btRemoveCounts,
            lblSampleSize, lbSampleSize, btAddSampleSize, btRemoveSampleSize,
            lblExposure, lbExposure, btAddExposure, btRemoveExposure
        }
        For Each control As Control In obsoleteControls
            control.Visible = False
            control.Enabled = False
        Next

        splitHistoricalModel.Orientation = Orientation.Vertical
        splitHistoricalModel.SplitterDistance = Math.Min(285, Math.Max(200, splitHistoricalModel.Width \ 3))
        dgvHistoricalMean.Dock = DockStyle.None
        dgvHistoricalCovariance.Dock = DockStyle.None
        lblHistoricalMeanGrid.BringToFront()
        lblHistoricalCovarianceGrid.BringToFront()

        grpPcaOptions.Font = New Font(grpPcaOptions.Font, FontStyle.Bold)
        grpGeneralizedVarianceOptions.Font = New Font(grpGeneralizedVarianceOptions.Font, FontStyle.Bold)
        grpMewmaOptions.Font = New Font(grpMewmaOptions.Font, FontStyle.Bold)
        grpMcusumOptions.Font = New Font(grpMcusumOptions.Font, FontStyle.Bold)
        grpSequentialResetOptions.Font = New Font(grpSequentialResetOptions.Font, FontStyle.Bold)
        For Each group As GroupBox In {grpPcaOptions, grpGeneralizedVarianceOptions, grpMewmaOptions,
                                       grpMcusumOptions, grpSequentialResetOptions}
            For Each child As Control In group.Controls
                child.Font = New Font(child.Font, FontStyle.Regular)
            Next
        Next
    End Sub

    Private Sub InitializeOptionControls()
        pSuppressUiEvents = True
        Try
            cbChartType.BeginUpdate()
            cbChartType.Items.Clear()
            cbChartType.Items.AddRange(ChartChoices)
            cbChartType.DropDownStyle = ComboBoxStyle.DropDownList
            cbChartType.SelectedIndex = 0
            cbChartType.EndUpdate()

            FillCombo(cbMissingValuePolicy,
                      New ComboItem(Of SpcMissingValuePolicy)("Reject", SpcMissingValuePolicy.Reject),
                      New ComboItem(Of SpcMissingValuePolicy)("Omit incomplete observation", SpcMissingValuePolicy.OmitPoint))

            FillCombo(cbModelSource,
                      New ComboItem(Of SpcMultivariateModelSource)("Estimate from Phase I", SpcMultivariateModelSource.EstimateFromPhaseI),
                      New ComboItem(Of SpcMultivariateModelSource)("Use historical parameters", SpcMultivariateModelSource.UseHistoricalParameters))

            FillCombo(cbImportedExclusionScope,
                      New ComboItem(Of SpcExclusionScope)("Parameter estimation and signal evaluation", SpcExclusionScope.EstimationAndRules),
                      New ComboItem(Of SpcExclusionScope)("Parameter estimation", SpcExclusionScope.ParameterEstimation),
                      New ComboItem(Of SpcExclusionScope)("Signal evaluation", SpcExclusionScope.RuleEvaluation))

            FillCombo(cbHorizontalTickOrientation,
                      New ComboItem(Of Integer)("0°", 0),
                      New ComboItem(Of Integer)("45°", 45),
                      New ComboItem(Of Integer)("90°", 90))

            FillCombo(cbDiagnosticsScope,
                      New ComboItem(Of DiagnosticsScope)("All points", DiagnosticsScope.AllPoints),
                      New ComboItem(Of DiagnosticsScope)("Signalled points only", DiagnosticsScope.SignalledPointsOnly))

            FillCombo(cbPcaMatrix,
                      New ComboItem(Of Boolean)("Covariance matrix", False),
                      New ComboItem(Of Boolean)("Correlation matrix", True))

            FillCombo(cbSequenceGapBehavior,
                      New ComboItem(Of SpcSequenceGapBehavior)("Break sequence", SpcSequenceGapBehavior.BreakSequence),
                      New ComboItem(Of SpcSequenceGapBehavior)("Skip point and continue", SpcSequenceGapBehavior.SkipPointAndContinue))

            spinControlLimitAlpha.DecimalPlaces = 4
            spinControlLimitAlpha.Increment = 0.0001D
            spinControlLimitAlpha.Minimum = 0.0001D
            spinControlLimitAlpha.Maximum = 0.9999D
            spinControlLimitAlpha.Value = 0.0027D

            spinCovarianceRegularization.DecimalPlaces = 6
            spinCovarianceRegularization.Increment = 0.0001D
            spinCovarianceRegularization.Minimum = 0D
            spinCovarianceRegularization.Maximum = 25D
            spinCovarianceRegularization.Value = 0D

            spinPcaCumulativeVariance.DecimalPlaces = 1
            spinPcaCumulativeVariance.Increment = 1D
            spinPcaCumulativeVariance.Minimum = 1D
            spinPcaCumulativeVariance.Maximum = 100D
            spinPcaCumulativeVariance.Value = 90D
            spinPcaComponentCount.Minimum = 1D
            spinPcaComponentCount.Maximum = 1000D
            spinPcaComponentCount.Value = 1D

            spinGvSigmaMultiplier.DecimalPlaces = 2
            spinGvSigmaMultiplier.Increment = 0.1D
            spinGvSigmaMultiplier.Minimum = 0.01D
            spinGvSigmaMultiplier.Maximum = 100D
            spinGvSigmaMultiplier.Value = 3D

            spinMewmaLambda.DecimalPlaces = 2
            spinMewmaLambda.Increment = 0.05D
            spinMewmaLambda.Minimum = 0.01D
            spinMewmaLambda.Maximum = 1D
            spinMewmaLambda.Value = 0.2D
            spinMewmaControlLimit.DecimalPlaces = 4
            spinMewmaControlLimit.Increment = 0.1D
            spinMewmaControlLimit.Minimum = 0.0001D
            spinMewmaControlLimit.Maximum = 1000000D
            spinMewmaControlLimit.Value = 1D

            spinMcusumReferenceValue.DecimalPlaces = 3
            spinMcusumReferenceValue.Increment = 0.1D
            spinMcusumReferenceValue.Minimum = 0D
            spinMcusumReferenceValue.Maximum = 100D
            spinMcusumReferenceValue.Value = 0.5D
            spinMcusumDecisionInterval.DecimalPlaces = 3
            spinMcusumDecisionInterval.Increment = 0.1D
            spinMcusumDecisionInterval.Minimum = 0.001D
            spinMcusumDecisionInterval.Maximum = 1000D
            spinMcusumDecisionInterval.Value = 5.5D

            spinLastPhaseIPoint.Minimum = 1D
            spinLastPhaseIPoint.Maximum = 1000000D
            chkUseLowerHotellingLimit.Checked = False
            chkAllowPseudoInverse.Checked = True
            rbSinglePhaseI.Checked = True
            rbPcaVarianceSelection.Checked = True
            chkSpecifyGvSigmaMultiplier.Checked = False
            chkSpecifyMewmaControlLimit.Checked = False
            chkResetAtStageBoundary.Checked = True
            chkResetAtPhaseBoundary.Checked = True
            chkResetAfterSignal.Checked = False
            chkUseSequenceValuesForHorizontalAxis.Checked = False
            tbHorizontalAxisTitle.Text = "Sample"
            tbValueNumberFormat.Text = "0.####"
            btnInterrupt.Enabled = False
            ProgressBar.Minimum = 0
            ProgressBar.Maximum = 100
            ProgressBar.Value = 0

            pToolTip.SetToolTip(cbSequenceGapBehavior,
                "Controls whether an omitted or rule-excluded point resets MEWMA/MCUSUM recursion or is skipped without changing the carried state.")
        Finally
            pSuppressUiEvents = False
        End Try

        PopulateObservationStructures(SpcChartType.HotellingT2)
        UpdatePcaSelectionControls()
        UpdateChartDependentControls()
        UpdateQuickPhaseControls()
        UpdateDiagnosticsControls()
        LayoutHistoricalGrids()
    End Sub

    Private Sub ConfigureGridColumns()
        dgvStages.AllowUserToAddRows = True
        dgvStages.AllowUserToDeleteRows = True
        dgvStages.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        colStageID.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colStagePhase.Items.Clear()
        colStagePhase.Items.AddRange("Phase I", "Phase II")

        dgvExclusions.AllowUserToAddRows = False
        dgvExclusions.AllowUserToDeleteRows = True
        colExclusionReason.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colExclusionScope.Items.Clear()
        colExclusionScope.Items.AddRange("Parameter estimation", "Signal evaluation",
                                         "Parameter estimation and signal evaluation")

        dgvHistoricalMean.AllowUserToAddRows = False
        dgvHistoricalMean.AllowUserToDeleteRows = False
        dgvHistoricalMean.RowHeadersVisible = False
        colHistoricalMeanVariable.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colHistoricalMeanValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

        dgvHistoricalCovariance.AllowUserToAddRows = False
        dgvHistoricalCovariance.AllowUserToDeleteRows = False
        dgvHistoricalCovariance.RowHeadersVisible = False
    End Sub

    Private Sub ConfigureResponsiveLayout()
        Me.MinimumSize = New Size(MinimumFormWidth, MinimumFormHeight)
        Me.MaximizeBox = True
        Me.KeyPreview = True
        Me.AcceptButton = btCompute

        TabControl1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        ProgressBar.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        btnHelp.Anchor = AnchorStyles.Right Or AnchorStyles.Bottom
        btnInterrupt.Anchor = AnchorStyles.Right Or AnchorStyles.Bottom
        btCompute.Anchor = AnchorStyles.Right Or AnchorStyles.Bottom

        AddHandler Me.Resize, AddressOf ResponsiveLayoutChanged
        AddHandler TabControl1.SelectedIndexChanged, AddressOf ResponsiveLayoutChanged
        AddHandler splitHistoricalModel.Resize, AddressOf HistoricalLayoutChanged
        AddHandler splitHistoricalModel.SplitterMoved, AddressOf HistoricalLayoutChanged
        ApplyResponsiveLayout()
    End Sub

    Private Sub ResponsiveLayoutChanged(sender As Object, e As System.EventArgs)
        ApplyResponsiveLayout()
    End Sub

    Private Sub HistoricalLayoutChanged(sender As Object, e As System.EventArgs)
        LayoutHistoricalGrids()
    End Sub

    Private Shared Function InnerWidth(page As TabPage) As Integer
        Return Math.Max(763, page.ClientSize.Width - 14)
    End Function

    Private Sub ApplyResponsiveLayout()
        If TabControl1 Is Nothing Then Return
        LayoutChartDataTab()
        LayoutModelTab()
        LayoutPhasesTab()
        LayoutMethodTab()
        LayoutOutputTab()
    End Sub

    Private Sub LayoutChartDataTab()
        Dim width As Integer = InnerWidth(TabPage1_ChartData)
        grpChartSelection.Width = width
        grpWorksheet.Width = width

        Dim descriptionX As Integer = Math.Max(395, CInt(Math.Floor(width * 0.51R)))
        cbChartType.Width = Math.Max(239, descriptionX - cbChartType.Left - 3)
        cbObservationStructure.Width = cbChartType.Width
        lblChartDescription.Left = descriptionX
        lblChartDescription.Width = Math.Max(250, width - descriptionX - 6)
        lblDataRequirements.Left = descriptionX
        lblDataRequirements.Width = lblChartDescription.Width

        Dim rightX As Integer = Math.Max(495, CInt(Math.Floor(width * 0.62R)))
        Dim addX As Integer = rightX - 90
        Dim removeX As Integer = rightX - 45
        Dim leftWidth As Integer = Math.Max(180, addX - lbAllColumns.Left - 12)
        Dim rightWidth As Integer = Math.Max(220, width - rightX - 5)

        lbAllColumns.Width = leftWidth
        btReload.Left = lbAllColumns.Right + 5
        btClearDataRoles.Left = width - btClearDataRoles.Width - 6

        Dim lists As ListBox() = {lbVariables, lbSubgroupID, lbLabels, lbSequence}
        Dim labels As Label() = {lblVariables, lblSubgroupID, lblLabels, lblSequence}
        Dim adds As Button() = {btAddVariables, btAddSubgroupID, btAddLabels, btAddSequence}
        Dim removes As Button() = {btRemoveVariables, btRemoveSubgroupID, btRemoveLabels, btRemoveSequence}
        For i As Integer = 0 To lists.Length - 1
            lists(i).Left = rightX
            lists(i).Width = rightWidth
            labels(i).Left = rightX
            adds(i).Left = addX
            removes(i).Left = removeX
        Next

        Dim labelGap As Integer = 4
        Dim rowGap As Integer = 8
        lbVariables.Top = lblVariables.Bottom + labelGap
        Dim availableGroupHeight As Integer = TabPage1_ChartData.ClientSize.Height - grpWorksheet.Top - 8
        grpWorksheet.Height = Math.Max(500, availableGroupHeight)
        Dim lowerHeight As Integer = 3 * (lblSubgroupID.Height + labelGap + lbSubgroupID.Height + rowGap)
        lbVariables.Height = Math.Max(120, grpWorksheet.ClientSize.Height - lbVariables.Top - lowerHeight - 14)
        AlignRoleButtons(lbVariables, btAddVariables, btRemoveVariables)

        Dim top As Integer = lbVariables.Bottom + rowGap
        For i As Integer = 1 To labels.Length - 1
            labels(i).Top = top
            lists(i).Top = labels(i).Bottom + labelGap
            AlignRoleButtons(lists(i), adds(i), removes(i))
            top = lists(i).Bottom + rowGap
        Next
        lbAllColumns.Height = Math.Max(100, grpWorksheet.ClientSize.Height - lbAllColumns.Top - 12)
        TabPage1_ChartData.AutoScrollMinSize = New Size(790, grpWorksheet.Bottom + 8)
    End Sub

    Private Shared Sub AlignRoleButtons(list As ListBox, addButton As Button, removeButton As Button)
        Dim top As Integer = list.Top + CInt(Math.Floor((list.Height - addButton.Height) / 2.0R))
        addButton.Top = top
        removeButton.Top = top
    End Sub

    Private Sub LayoutModelTab()
        Dim width As Integer = InnerWidth(TabPage2_ModelLimits)
        grpModelOptions.Width = width
        grpHistoricalModel.Width = width
        grpHistoricalModel.Height = Math.Max(300, TabPage2_ModelLimits.ClientSize.Height - grpHistoricalModel.Top - 8)
        splitHistoricalModel.Width = grpHistoricalModel.ClientSize.Width - 12
        splitHistoricalModel.Height = Math.Max(220, grpHistoricalModel.ClientSize.Height - splitHistoricalModel.Top - 8)
        btClearHistoricalModel.Left = width - btClearHistoricalModel.Width - 18
        btImportHistoricalCovariance.Left = btClearHistoricalModel.Left - btImportHistoricalCovariance.Width - 6
        LayoutHistoricalGrids()
        TabPage2_ModelLimits.AutoScrollMinSize = New Size(790, grpHistoricalModel.Bottom + 8)
    End Sub

    Private Sub LayoutHistoricalGrids()
        If splitHistoricalModel Is Nothing Then Return
        LayoutHistoricalPanel(splitHistoricalModel.Panel1, lblHistoricalMeanGrid, dgvHistoricalMean)
        LayoutHistoricalPanel(splitHistoricalModel.Panel2, lblHistoricalCovarianceGrid, dgvHistoricalCovariance)
    End Sub

    Private Shared Sub LayoutHistoricalPanel(panel As SplitterPanel, label As Label, grid As DataGridView)
        label.Left = 3
        label.Top = 3
        grid.Left = 0
        grid.Top = label.Bottom + 4
        grid.Width = Math.Max(1, panel.ClientSize.Width)
        grid.Height = Math.Max(1, panel.ClientSize.Height - grid.Top)
        grid.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
    End Sub

    Private Sub LayoutPhasesTab()
        Dim width As Integer = InnerWidth(TabPage3_PhasesExclusions)
        grpPhaseColumns.Width = width
        grpQuickPhaseSetup.Width = width
        grpStages.Width = width
        grpExclusions.Width = width
        dgvStages.Width = width - 12
        dgvExclusions.Width = width - 15

        Dim sourceCombos As ComboBox() = {cbStageColumn, cbPhaseColumn, cbExclusionColumn,
                                         cbExclusionReasonColumn, cbImportedExclusionScope}
        For Each combo As ComboBox In sourceCombos
            combo.Width = Math.Max(239, width - combo.Left - 317)
        Next
        btImportStages.Left = width - btImportStages.Width - 79
        btImportExclusions.Left = width - btImportExclusions.Width - 79

        Dim remaining As Integer = Math.Max(350, TabPage3_PhasesExclusions.ClientSize.Height - grpStages.Top - 12)
        grpStages.Height = Math.Max(148, CInt(Math.Floor(remaining * 0.45R)))
        dgvStages.Height = Math.Max(90, grpStages.ClientSize.Height - dgvStages.Top)
        grpExclusions.Top = grpStages.Bottom + 6
        grpExclusions.Height = Math.Max(180, TabPage3_PhasesExclusions.ClientSize.Height - grpExclusions.Top - 8)
        dgvExclusions.Height = Math.Max(120, grpExclusions.ClientSize.Height - dgvExclusions.Top - 6)
        TabPage3_PhasesExclusions.AutoScrollMinSize = New Size(790, grpExclusions.Bottom + 8)
    End Sub

    Private Sub LayoutMethodTab()
        Dim width As Integer = InnerWidth(TabPage4_MethodOptions)
        lblMethodDescription.Width = width - 10
        flpMethodOptions.Width = width
        flpMethodOptions.Height = Math.Max(300, TabPage4_MethodOptions.ClientSize.Height - flpMethodOptions.Top - 8)
        For Each group As GroupBox In {grpPcaOptions, grpGeneralizedVarianceOptions, grpMewmaOptions,
                                       grpMcusumOptions, grpSequentialResetOptions}
            group.Width = Math.Max(760, flpMethodOptions.ClientSize.Width - 10)
        Next
        TabPage4_MethodOptions.AutoScrollMinSize = New Size(790, flpMethodOptions.Bottom + 8)
    End Sub

    Private Sub LayoutOutputTab()
        Dim width As Integer = InnerWidth(TabPage5_OutputAppearance)
        For Each group As GroupBox In {grpOutputs, grpTitleAxes, grpChartDisplay, grpChartDimensions}
            group.Width = width
        Next
        tbChartTitle.Width = Math.Max(240, CInt(Math.Floor(width * 0.42R)) - tbChartTitle.Left)
        Dim rightX As Integer = Math.Max(494, CInt(Math.Floor(width * 0.62R)))
        For Each textBox As TextBox In {tbValueAxisTitle, tbHorizontalAxisTitle, tbValueNumberFormat}
            textBox.Left = rightX
            textBox.Width = Math.Max(200, width - rightX - 29)
        Next
        lblValueAxisTitle.Left = rightX - 124
        lblHorizontalAxisTitle.Left = rightX - 124
        lblValueNumberFormat.Left = rightX - 124
        TabPage5_OutputAppearance.AutoScrollMinSize = New Size(790, grpChartDimensions.Bottom + 8)
    End Sub

    Private Shared Sub FillCombo(Of T)(combo As ComboBox, ParamArray items As ComboItem(Of T)())
        combo.BeginUpdate()
        Try
            combo.Items.Clear()
            combo.DropDownStyle = ComboBoxStyle.DropDownList
            combo.Items.AddRange(items)
            If combo.Items.Count > 0 Then combo.SelectedIndex = 0
        Finally
            combo.EndUpdate()
        End Try
    End Sub

    Private Shared Function SelectedComboValue(Of T)(combo As ComboBox) As T
        Dim item As ComboItem(Of T) = TryCast(combo.SelectedItem, ComboItem(Of T))
        If item Is Nothing Then Throw New InvalidOperationException("No value is selected for " & combo.Name & ".")
        Return item.Value
    End Function

    Private Shared Sub SelectComboValue(Of T)(combo As ComboBox, value As T)
        For i As Integer = 0 To combo.Items.Count - 1
            Dim item As ComboItem(Of T) = TryCast(combo.Items(i), ComboItem(Of T))
            If item IsNot Nothing AndAlso EqualityComparer(Of T).Default.Equals(item.Value, value) Then
                combo.SelectedIndex = i
                Return
            End If
        Next
    End Sub

#End Region

#Region "Worksheet and data roles"

    Public Sub Populate(ws As Object)
        Dim worksheet As Excel.Worksheet = TryCast(ws, Excel.Worksheet)
        If worksheet Is Nothing Then Throw New ArgumentException("An Excel worksheet is required.", NameOf(ws))
        pWorksheet = worksheet
        pWorkbook = DirectCast(worksheet.Parent, Excel.Workbook)
        ReloadColumnLists(clearAssignments:=False)
    End Sub

    Private Sub ReloadColumnLists(clearAssignments As Boolean)
        If pWorksheet Is Nothing Then Return
        If clearAssignments Then ClearDataRoles()
        lbAllColumns.Items.Clear()
        pColumnInfo.Clear()

        Dim finalColumn As Integer = LastColumnInSheet(pWorksheet)
        Dim maxRows As Integer = MaxRowsInSheet(pWorksheet)
        Dim headerRange As Excel.Range = pWorksheet.Range(pWorksheet.Cells(1, 1), pWorksheet.Cells(1, finalColumn))
        pColumnInfo = VarNamesToLBox(headerRange, maxRows, lbAllColumns, bNumeric_only:=False)

        cbSheetsList.BeginUpdate()
        Try
            cbSheetsList.Items.Clear()
            For Each sheetObject As Object In pWorkbook.Worksheets
                Dim sheet As Excel.Worksheet = TryCast(sheetObject, Excel.Worksheet)
                If sheet IsNot Nothing Then cbSheetsList.Items.Add(sheet.Name)
            Next
            cbSheetsList.SelectedIndex = cbSheetsList.FindStringExact(pWorksheet.Name)
        Finally
            cbSheetsList.EndUpdate()
        End Try
        PopulateSourceColumnCombos()
    End Sub

    Private Sub PopulateSourceColumnCombos()
        For Each combo As ComboBox In {cbStageColumn, cbPhaseColumn, cbExclusionColumn, cbExclusionReasonColumn}
            Dim previous As String = If(combo.SelectedItem Is Nothing, NoneColumnText, combo.SelectedItem.ToString())
            combo.BeginUpdate()
            Try
                combo.Items.Clear()
                combo.Items.Add(NoneColumnText)
                For Each item As Object In lbAllColumns.Items
                    combo.Items.Add(item.ToString())
                Next
                Dim index As Integer = combo.FindStringExact(previous)
                combo.SelectedIndex = If(index >= 0, index, 0)
                combo.DropDownStyle = ComboBoxStyle.DropDownList
            Finally
                combo.EndUpdate()
            End Try
        Next
    End Sub

    Private Sub WireRoleButtons()
        AddHandler btAddVariables.Click, Sub() MoveSelectedColumns(lbVariables, allowMany:=True)
        AddHandler btAddSubgroupID.Click, Sub() MoveSelectedColumns(lbSubgroupID, allowMany:=False)
        AddHandler btAddLabels.Click, Sub() MoveSelectedColumns(lbLabels, allowMany:=False)
        AddHandler btAddSequence.Click, Sub() MoveSelectedColumns(lbSequence, allowMany:=False)
        AddHandler btRemoveVariables.Click, Sub() RemoveSelectedItems(lbVariables)
        AddHandler btRemoveSubgroupID.Click, Sub() RemoveSelectedItems(lbSubgroupID)
        AddHandler btRemoveLabels.Click, Sub() RemoveSelectedItems(lbLabels)
        AddHandler btRemoveSequence.Click, Sub() RemoveSelectedItems(lbSequence)
    End Sub

    Private Sub MoveSelectedColumns(target As ListBox, allowMany As Boolean)
        If lbAllColumns.SelectedItems.Count = 0 Then Return
        Dim selected As New List(Of String)()
        For Each item As Object In lbAllColumns.SelectedItems
            selected.Add(item.ToString())
        Next
        If Not allowMany AndAlso selected.Count > 1 Then
            ShowInputError("Select exactly one available column for this role.")
            Return
        End If

        For Each columnName As String In selected
            Dim assignedRole As String = FindAssignedRole(columnName, target)
            If assignedRole.Length > 0 Then
                ShowInputError("Column '" & columnName & "' is already assigned to " & assignedRole & ".")
                Continue For
            End If
            If Not allowMany Then target.Items.Clear()
            If Not target.Items.Contains(columnName) Then target.Items.Add(columnName)
        Next
        RoleAssignmentsChanged()
    End Sub

    Private Function FindAssignedRole(columnName As String, target As ListBox) As String
        Dim roles As New List(Of KeyValuePair(Of String, ListBox)) From {
            New KeyValuePair(Of String, ListBox)("measurement variables", lbVariables),
            New KeyValuePair(Of String, ListBox)("subgroup ID", lbSubgroupID),
            New KeyValuePair(Of String, ListBox)("point label", lbLabels),
            New KeyValuePair(Of String, ListBox)("sequence/date/time", lbSequence)
        }
        For Each role As KeyValuePair(Of String, ListBox) In roles
            If role.Value IsNot target AndAlso role.Value.Items.Contains(columnName) Then Return role.Key
        Next
        Return String.Empty
    End Function

    Private Sub RemoveSelectedItems(list As ListBox)
        Dim selected As New List(Of Object)()
        For Each item As Object In list.SelectedItems
            selected.Add(item)
        Next
        For Each item As Object In selected
            list.Items.Remove(item)
        Next
        RoleAssignmentsChanged()
    End Sub

    Private Sub RoleAssignmentsChanged()
        UpdatePcaComponentMaximum()
        RefreshHistoricalVariables(preserveValues:=True)
        chkUseSequenceValuesForHorizontalAxis.Enabled = lbSequence.Items.Count = 1
        If lbSequence.Items.Count = 0 Then chkUseSequenceValuesForHorizontalAxis.Checked = False
    End Sub

    Private Sub ClearDataRoles()
        For Each list As ListBox In {lbVariables, lbSubgroupID, lbLabels, lbSequence}
            list.Items.Clear()
        Next
        RoleAssignmentsChanged()
    End Sub

    Private Sub btReload_Click(sender As Object, e As System.EventArgs) Handles btReload.Click
        If pWorkbook Is Nothing Then Return
        If cbSheetsList.SelectedItem Is Nothing Then
            ReloadColumnLists(clearAssignments:=False)
            Return
        End If
        Dim selectedName As String = cbSheetsList.SelectedItem.ToString()
        Dim changed As Boolean = pWorksheet Is Nothing OrElse
            Not String.Equals(pWorksheet.Name, selectedName, StringComparison.Ordinal)
        pWorksheet = DirectCast(pWorkbook.Worksheets(selectedName), Excel.Worksheet)
        ReloadColumnLists(clearAssignments:=changed)
    End Sub

    Private Sub btClearDataRoles_Click(sender As Object, e As System.EventArgs) Handles btClearDataRoles.Click
        ClearDataRoles()
    End Sub

#End Region

#Region "Dynamic chart options"

    Private Function SelectedChartChoice() As ChartChoice
        Dim choice As ChartChoice = TryCast(cbChartType.SelectedItem, ChartChoice)
        If choice Is Nothing Then Throw New InvalidOperationException("Select a multivariate control-chart type.")
        Return choice
    End Function

    Private Function SelectedChartType() As SpcChartType
        Return SelectedChartChoice().ChartType
    End Function

    Private Function IsGroupedStructure() As Boolean
        Return SelectedComboValue(Of ObservationStructure)(cbObservationStructure) = ObservationStructure.RationalSubgroups
    End Function

    Private Sub cbChartType_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbChartType.SelectedIndexChanged
        If pSuppressUiEvents Then Return
        UpdateChartDependentControls()
    End Sub

    Private Sub cbObservationStructure_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbObservationStructure.SelectedIndexChanged
        If pSuppressUiEvents Then Return
        UpdateRoleAvailability()
        UpdateChartDependentControls(preserveStructure:=True)
    End Sub

    Private Sub PopulateObservationStructures(chartType As SpcChartType)
        Dim choice As ChartChoice = ChartChoices.First(Function(item) item.ChartType = chartType)
        Dim previous As Nullable(Of ObservationStructure) = Nothing
        Dim selected As ComboItem(Of ObservationStructure) = TryCast(cbObservationStructure.SelectedItem, ComboItem(Of ObservationStructure))
        If selected IsNot Nothing Then previous = selected.Value

        pSuppressUiEvents = True
        Try
            cbObservationStructure.BeginUpdate()
            cbObservationStructure.Items.Clear()
            If Not choice.RequiresSubgroups Then
                cbObservationStructure.Items.Add(New ComboItem(Of ObservationStructure)(
                    "Individual observations", ObservationStructure.IndividualObservations))
            End If
            If choice.AllowsSubgroups Then
                cbObservationStructure.Items.Add(New ComboItem(Of ObservationStructure)(
                    "Rational subgroups", ObservationStructure.RationalSubgroups))
            End If
            cbObservationStructure.DropDownStyle = ComboBoxStyle.DropDownList
            If previous.HasValue Then SelectComboValue(cbObservationStructure, previous.Value)
            If cbObservationStructure.SelectedIndex < 0 Then cbObservationStructure.SelectedIndex = 0
        Finally
            cbObservationStructure.EndUpdate()
            pSuppressUiEvents = False
        End Try
    End Sub

    Private Sub UpdateChartDependentControls(Optional preserveStructure As Boolean = False)
        If cbChartType.SelectedItem Is Nothing Then Return
        Dim choice As ChartChoice = SelectedChartChoice()
        If Not preserveStructure Then PopulateObservationStructures(choice.ChartType)
        lblChartDescription.Text = choice.Description
        lblDataRequirements.Text = choice.Requirements
        UpdateRoleAvailability()

        Dim isPca As Boolean = choice.ChartType = SpcChartType.PcaT2 OrElse choice.ChartType = SpcChartType.PcaQ
        Dim isGv As Boolean = choice.ChartType = SpcChartType.GeneralizedVariance
        Dim isMewma As Boolean = choice.ChartType = SpcChartType.Mewma
        Dim isMcusum As Boolean = choice.ChartType = SpcChartType.Mcusum
        Dim isSequential As Boolean = isMewma OrElse isMcusum
        grpPcaOptions.Visible = isPca
        grpGeneralizedVarianceOptions.Visible = isGv
        grpMewmaOptions.Visible = isMewma
        grpMcusumOptions.Visible = isMcusum
        grpSequentialResetOptions.Visible = isSequential
        cbSequenceGapBehavior.Enabled = isSequential
        lblSequenceGapBehavior.Enabled = isSequential

        spinMewmaLambda.Enabled = isMewma
        lblMewmaLambda.Enabled = isMewma
        spinMewmaControlLimit.Enabled = isMewma AndAlso chkSpecifyMewmaControlLimit.Checked
        spinMcusumReferenceValue.Enabled = isMcusum
        lblMcusumReferenceValue.Enabled = isMcusum
        spinMcusumDecisionInterval.Enabled = isMcusum
        lblMcusumDecisionInterval.Enabled = isMcusum

        Select Case choice.ChartType
            Case SpcChartType.HotellingT2
                lblMethodDescription.Text = "Hotelling T-squared uses the selected covariance model and has no additional chart-specific design parameters."
            Case SpcChartType.GeneralizedVariance
                lblMethodDescription.Text = "Generalized variance monitors |S|. Supply a sigma multiplier only when you intentionally want to override the alpha-derived limits."
            Case SpcChartType.PcaT2
                lblMethodDescription.Text = "PCA T-squared monitors scores in the retained principal-component space."
            Case SpcChartType.PcaQ
                lblMethodDescription.Text = "PCA Q monitors residual variation; at least one nonzero component must remain outside the retained model."
            Case SpcChartType.Mewma
                lblMethodDescription.Text = "MEWMA uses lambda to weight recent multivariate observations. A supplied UCL should be calibrated for the intended in-control ARL."
            Case SpcChartType.Mcusum
                lblMethodDescription.Text = "MCUSUM uses Crosier design parameters k and h; validate their in-control ARL for the process dimension."
        End Select

        lblPcaNote.Text = If(choice.ChartType = SpcChartType.PcaQ,
            "PCA Q requires a residual subspace. If the variance rule retains every nonzero component, the backend leaves one component for Q monitoring and reports a warning.",
            "PCA T-squared monitors the retained component space. Covariance PCA preserves measurement units; correlation PCA standardizes every variable.")
        lblMewmaNote.Text = "Without a supplied UCL, the backend uses a chi-square pointwise approximation and reports that an ARL-calibrated limit is preferred for production monitoring."
        lblMcusumNote.Text = "The in-control ARL depends on k, h, dimension and covariance estimation. Validate the selected design for the intended process."

        Dim individualHotelling As Boolean = choice.ChartType = SpcChartType.HotellingT2 AndAlso Not IsGroupedStructure()
        chkUseLowerHotellingLimit.Enabled = individualHotelling
        If Not individualHotelling Then chkUseLowerHotellingLimit.Checked = False
        If isGv Then
            If Not pAllowPseudoInverseBeforeGeneralizedVariance.HasValue Then
                pAllowPseudoInverseBeforeGeneralizedVariance = chkAllowPseudoInverse.Checked
            End If
            chkAllowPseudoInverse.Checked = False
            chkAllowPseudoInverse.Enabled = False
        Else
            chkAllowPseudoInverse.Enabled = True
            If pAllowPseudoInverseBeforeGeneralizedVariance.HasValue Then
                chkAllowPseudoInverse.Checked = pAllowPseudoInverseBeforeGeneralizedVariance.Value
                pAllowPseudoInverseBeforeGeneralizedVariance = Nothing
            End If
        End If

        Dim alphaEnabled As Boolean = Not isMcusum AndAlso
            Not (isMewma AndAlso chkSpecifyMewmaControlLimit.Checked) AndAlso
            Not (isGv AndAlso chkSpecifyGvSigmaMultiplier.Checked)
        spinControlLimitAlpha.Enabled = alphaEnabled
        lblControlLimitAlpha.Enabled = alphaEnabled
        UpdateHistoricalControls()
        UpdatePcaSelectionControls()
        UpdatePcaComponentMaximum()
        ApplyResponsiveLayout()
    End Sub

    Private Sub UpdateRoleAvailability()
        If cbObservationStructure.SelectedItem Is Nothing Then Return
        Dim grouped As Boolean = IsGroupedStructure()
        SetRoleEnabled(lbVariables, lblVariables, btAddVariables, btRemoveVariables, True)
        SetRoleEnabled(lbSubgroupID, lblSubgroupID, btAddSubgroupID, btRemoveSubgroupID, grouped)
        SetRoleEnabled(lbLabels, lblLabels, btAddLabels, btRemoveLabels, True)
        SetRoleEnabled(lbSequence, lblSequence, btAddSequence, btRemoveSequence, True)
        lbVariables.SelectionMode = SelectionMode.MultiExtended
        colExclusionPoint.HeaderText = If(grouped, "Subgroup point", "Point")
    End Sub

    Private Shared Sub SetRoleEnabled(list As ListBox, label As Label,
                                      addButton As Button, removeButton As Button,
                                      enabled As Boolean)
        If Not enabled Then list.Items.Clear()
        list.Enabled = enabled
        label.Enabled = enabled
        addButton.Enabled = enabled
        removeButton.Enabled = enabled
    End Sub

    Private Sub cbModelSource_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbModelSource.SelectedIndexChanged
        If pSuppressUiEvents Then Return
        UpdateHistoricalControls()
        UpdateQuickPhaseControls()
    End Sub

    Private Sub UpdateHistoricalControls()
        If cbModelSource.SelectedItem Is Nothing OrElse cbChartType.SelectedItem Is Nothing Then Return
        Dim historical As Boolean = SelectedComboValue(Of SpcMultivariateModelSource)(cbModelSource) =
                                    SpcMultivariateModelSource.UseHistoricalParameters
        grpHistoricalModel.Enabled = historical
        Dim meanEnabled As Boolean = historical AndAlso SelectedChartType() <> SpcChartType.GeneralizedVariance
        dgvHistoricalMean.Enabled = meanEnabled
        lblHistoricalMeanGrid.Enabled = meanEnabled
        btImportHistoricalMean.Enabled = meanEnabled
        rbSinglePhaseII.Enabled = historical
        If Not historical AndAlso rbSinglePhaseII.Checked Then rbSinglePhaseI.Checked = True
        lblModelNote.Text = If(historical,
            If(SelectedChartType() = SpcChartType.GeneralizedVariance,
               "Historical generalized-variance monitoring requires a positive-definite covariance matrix. A mean vector is not used.",
               "Historical monitoring requires a finite mean vector and a symmetric positive-definite covariance matrix matching the selected variables."),
            "The in-control mean/covariance model is estimated from eligible Phase-I observations. Exclude known special causes from parameter estimation.")
    End Sub

    Private Sub rbPca_CheckedChanged(sender As Object, e As System.EventArgs) Handles rbPcaVarianceSelection.CheckedChanged, rbPcaFixedComponents.CheckedChanged
        UpdatePcaSelectionControls()
    End Sub

    Private Sub UpdatePcaSelectionControls()
        spinPcaCumulativeVariance.Enabled = rbPcaVarianceSelection.Checked
        spinPcaComponentCount.Enabled = rbPcaFixedComponents.Checked
    End Sub

    Private Sub UpdatePcaComponentMaximum()
        Dim variableCount As Integer = Math.Max(1, lbVariables.Items.Count)
        Dim maximum As Integer = variableCount
        If cbChartType.SelectedItem IsNot Nothing AndAlso SelectedChartType() = SpcChartType.PcaQ Then
            maximum = Math.Max(1, variableCount - 1)
        End If
        spinPcaComponentCount.Maximum = CDec(maximum)
        If spinPcaComponentCount.Value > spinPcaComponentCount.Maximum Then
            spinPcaComponentCount.Value = spinPcaComponentCount.Maximum
        End If
    End Sub

    Private Sub chkSpecifyGvSigmaMultiplier_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkSpecifyGvSigmaMultiplier.CheckedChanged
        spinGvSigmaMultiplier.Enabled = chkSpecifyGvSigmaMultiplier.Checked
        If Not pSuppressUiEvents Then UpdateChartDependentControls(preserveStructure:=True)
    End Sub

    Private Sub chkSpecifyMewmaControlLimit_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkSpecifyMewmaControlLimit.CheckedChanged
        Dim isMewma As Boolean = cbChartType.SelectedItem IsNot Nothing AndAlso
            SelectedChartType() = SpcChartType.Mewma
        spinMewmaControlLimit.Enabled = isMewma AndAlso chkSpecifyMewmaControlLimit.Checked
        If Not pSuppressUiEvents Then UpdateChartDependentControls(preserveStructure:=True)
    End Sub

    Private Sub chkWriteDiagnostics_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkWriteDiagnostics.CheckedChanged
        UpdateDiagnosticsControls()
    End Sub

    Private Sub UpdateDiagnosticsControls()
        cbDiagnosticsScope.Enabled = chkWriteDiagnostics.Checked
        lblDiagnosticsScope.Enabled = chkWriteDiagnostics.Checked
    End Sub

#End Region

#Region "Historical model, stages, and exclusions"

    Private Sub btRefreshHistoricalVariables_Click(sender As Object, e As System.EventArgs) Handles btRefreshHistoricalVariables.Click
        RefreshHistoricalVariables(preserveValues:=True)
    End Sub

    Private Sub RefreshHistoricalVariables(preserveValues As Boolean)
        Dim names As String() = lbVariables.Items.Cast(Of Object)().Select(Function(item) item.ToString()).ToArray()
        Dim oldMeans As New Dictionary(Of String, Object)(StringComparer.Ordinal)
        Dim oldCovariance As New Dictionary(Of String, Object)(StringComparer.Ordinal)
        If preserveValues Then
            For Each row As DataGridViewRow In dgvHistoricalMean.Rows
                If row.IsNewRow Then Continue For
                Dim name As String = CellText(row, colHistoricalMeanVariable.Index)
                If name.Length > 0 Then oldMeans(name) = row.Cells(colHistoricalMeanValue.Index).Value
            Next
            If dgvHistoricalCovariance.Columns.Count > 1 Then
                For Each row As DataGridViewRow In dgvHistoricalCovariance.Rows
                    If row.IsNewRow Then Continue For
                    Dim rowName As String = Convert.ToString(row.Cells(0).Value, CultureInfo.CurrentCulture)
                    For column As Integer = 1 To dgvHistoricalCovariance.Columns.Count - 1
                        oldCovariance(rowName & ChrW(31) & dgvHistoricalCovariance.Columns(column).HeaderText) = row.Cells(column).Value
                    Next
                Next
            End If
        End If

        dgvHistoricalMean.Rows.Clear()
        dgvHistoricalCovariance.Rows.Clear()
        dgvHistoricalCovariance.Columns.Clear()
        If names.Length = 0 Then Return

        For Each name As String In names
            Dim index As Integer = dgvHistoricalMean.Rows.Add(name, Nothing)
            If oldMeans.ContainsKey(name) Then dgvHistoricalMean.Rows(index).Cells(1).Value = oldMeans(name)
        Next

        Dim nameColumn As New DataGridViewTextBoxColumn With {
            .Name = "colHistoricalCovarianceVariable",
            .HeaderText = "Variable",
            .ReadOnly = True,
            .Frozen = True,
            .Width = 125
        }
        dgvHistoricalCovariance.Columns.Add(nameColumn)
        For i As Integer = 0 To names.Length - 1
            Dim column As New DataGridViewTextBoxColumn With {
                .Name = "colHistoricalCovariance" & (i + 1).ToString(CultureInfo.InvariantCulture),
                .HeaderText = names(i),
                .Width = 90
            }
            dgvHistoricalCovariance.Columns.Add(column)
        Next
        For Each rowName As String In names
            Dim rowIndex As Integer = dgvHistoricalCovariance.Rows.Add()
            dgvHistoricalCovariance.Rows(rowIndex).Cells(0).Value = rowName
            For column As Integer = 0 To names.Length - 1
                Dim key As String = rowName & ChrW(31) & names(column)
                If oldCovariance.ContainsKey(key) Then
                    dgvHistoricalCovariance.Rows(rowIndex).Cells(column + 1).Value = oldCovariance(key)
                End If
            Next
        Next
    End Sub

    Private Sub btImportHistoricalMean_Click(sender As Object, e As System.EventArgs) Handles btImportHistoricalMean.Click
        Try
            EnsureHistoricalGridReady()
            Dim values As Double(,) = ReadSelectedNumericRange()
            Dim p As Integer = lbVariables.Items.Count
            If Not ((values.GetLength(0) = p AndAlso values.GetLength(1) = 1) OrElse
                    (values.GetLength(0) = 1 AndAlso values.GetLength(1) = p)) Then
                Throw New ArgumentException("Select exactly one row or column containing " & p.ToString() & " numeric mean values, without headers.")
            End If
            For i As Integer = 0 To p - 1
                dgvHistoricalMean.Rows(i).Cells(colHistoricalMeanValue.Index).Value =
                    If(values.GetLength(0) = 1, values(0, i), values(i, 0))
            Next
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
    End Sub

    Private Sub btImportHistoricalCovariance_Click(sender As Object, e As System.EventArgs) Handles btImportHistoricalCovariance.Click
        Try
            EnsureHistoricalGridReady()
            Dim values As Double(,) = ReadSelectedNumericRange()
            Dim p As Integer = lbVariables.Items.Count
            If values.GetLength(0) <> p OrElse values.GetLength(1) <> p Then
                Throw New ArgumentException("Select a " & p.ToString() & " by " & p.ToString() & " numeric covariance matrix, without headers.")
            End If
            For row As Integer = 0 To p - 1
                For column As Integer = 0 To p - 1
                    dgvHistoricalCovariance.Rows(row).Cells(column + 1).Value = values(row, column)
                Next
            Next
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
    End Sub

    Private Sub btClearHistoricalModel_Click(sender As Object, e As System.EventArgs) Handles btClearHistoricalModel.Click
        For Each row As DataGridViewRow In dgvHistoricalMean.Rows
            row.Cells(colHistoricalMeanValue.Index).Value = Nothing
        Next
        For Each row As DataGridViewRow In dgvHistoricalCovariance.Rows
            For column As Integer = 1 To dgvHistoricalCovariance.Columns.Count - 1
                row.Cells(column).Value = Nothing
            Next
        Next
    End Sub

    Private Sub EnsureHistoricalGridReady()
        If lbVariables.Items.Count < 2 Then Throw New ArgumentException("Select at least two measurement variables first.")
        If dgvHistoricalMean.Rows.Count <> lbVariables.Items.Count OrElse
           dgvHistoricalCovariance.Rows.Count <> lbVariables.Items.Count Then
            RefreshHistoricalVariables(preserveValues:=True)
        End If
    End Sub

    Private Function ReadSelectedNumericRange() As Double(,)
        If AppGlobals.app Is Nothing Then Throw New InvalidOperationException("Excel is not available.")
        Dim selectedRange As Excel.Range = TryCast(AppGlobals.app.Selection, Excel.Range)
        If selectedRange Is Nothing Then Throw New ArgumentException("Select an Excel range containing numeric values first.")
        If selectedRange.Areas.Count <> 1 Then Throw New ArgumentException("Select one contiguous Excel range.")
        Dim rows As Integer = selectedRange.Rows.Count
        Dim columns As Integer = selectedRange.Columns.Count
        Dim result(rows - 1, columns - 1) As Double
        Dim raw As Object = selectedRange.Value2
        If rows = 1 AndAlso columns = 1 Then
            result(0, 0) = RequiredNumericValue(raw, "Selected cell")
            Return result
        End If
        Dim matrix As Object(,) = TryCast(raw, Object(,))
        If matrix Is Nothing Then Throw New ArgumentException("Excel did not return the selected range as a matrix.")
        For r As Integer = 0 To rows - 1
            For c As Integer = 0 To columns - 1
                result(r, c) = RequiredNumericValue(matrix(r + 1, c + 1),
                    "Selected cell " & (r + 1).ToString() & ", " & (c + 1).ToString())
            Next
        Next
        Return result
    End Function

    Private Shared Function RequiredNumericValue(value As Object, label As String) As Double
        If value Is Nothing OrElse value Is DBNull.Value Then Throw New ArgumentException(label & " is blank.")
        Dim number As Double
        If Not TryParseDouble(Convert.ToString(value, CultureInfo.CurrentCulture), number) Then
            Throw New ArgumentException(label & " is not numeric.")
        End If
        If Double.IsNaN(number) OrElse Double.IsInfinity(number) Then Throw New ArgumentException(label & " must be finite.")
        Return number
    End Function

    Private Sub rbPhase_CheckedChanged(sender As Object, e As System.EventArgs) Handles rbSinglePhaseI.CheckedChanged, rbPhaseIThenPhaseII.CheckedChanged, rbSinglePhaseII.CheckedChanged
        UpdateQuickPhaseControls()
    End Sub

    Private Sub UpdateQuickPhaseControls()
        spinLastPhaseIPoint.Enabled = rbPhaseIThenPhaseII.Checked
        lblLastPhaseIPoint.Enabled = rbPhaseIThenPhaseII.Checked
    End Sub

    Private Sub btApplyQuickPhaseSetup_Click(sender As Object, e As System.EventArgs) Handles btApplyQuickPhaseSetup.Click
        Try
            If Not rbSinglePhaseI.Checked AndAlso Not rbPhaseIThenPhaseII.Checked AndAlso Not rbSinglePhaseII.Checked Then
                Throw New ArgumentException("Select a quick phase setup before applying it.")
            End If
            Dim context As InputRowContext = GetInputRowContext()
            dgvStages.Rows.Clear()
            If rbSinglePhaseI.Checked Then
                AddStageRow("PhaseI", 1, context.PointCount, SpcPhase.PhaseI)
            ElseIf rbSinglePhaseII.Checked Then
                If SelectedComboValue(Of SpcMultivariateModelSource)(cbModelSource) <>
                   SpcMultivariateModelSource.UseHistoricalParameters Then
                    Throw New ArgumentException("All-Phase-II monitoring requires historical parameters.")
                End If
                AddStageRow("PhaseII", 1, context.PointCount, SpcPhase.PhaseII)
            Else
                Dim lastPhaseI As Integer = CInt(spinLastPhaseIPoint.Value)
                If lastPhaseI < 1 OrElse lastPhaseI >= context.PointCount Then
                    Throw New ArgumentException("The last Phase I point must be between 1 and " &
                                                (context.PointCount - 1).ToString(CultureInfo.CurrentCulture) & ".")
                End If
                AddStageRow("PhaseI", 1, lastPhaseI, SpcPhase.PhaseI)
                AddStageRow("PhaseII", lastPhaseI + 1, context.PointCount, SpcPhase.PhaseII)
            End If
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
    End Sub

    Private Sub btAddStage_Click(sender As Object, e As System.EventArgs) Handles btAddStage.Click
        Dim nextNumber As Integer = dgvStages.Rows.Cast(Of DataGridViewRow)().Count(Function(row) Not row.IsNewRow) + 1
        Dim index As Integer = dgvStages.Rows.Add()
        Dim roww As DataGridViewRow = dgvStages.Rows(index)
        roww.Cells(colStageID.Index).Value = "Stage" & nextNumber.ToString(CultureInfo.InvariantCulture)
        roww.Cells(colStagePhase.Index).Value = "Phase I"
    End Sub

    Private Sub btRemoveStage_Click(sender As Object, e As System.EventArgs) Handles btRemoveStage.Click
        RemoveSelectedGridRows(dgvStages)
        ClearQuickPhaseSelection()
    End Sub

    Private Sub btClearStages_Click(sender As Object, e As System.EventArgs) Handles btClearStages.Click
        dgvStages.Rows.Clear()
        ClearQuickPhaseSelection()
    End Sub

    Private Sub dgvStages_DefaultValuesNeeded(sender As Object, e As DataGridViewRowEventArgs) Handles dgvStages.DefaultValuesNeeded
        e.Row.Cells(colStagePhase.Index).Value = "Phase I"
    End Sub

    Private Sub AddStageRow(stageId As String, firstPoint As Integer, lastPoint As Integer, phase As SpcPhase)
        Dim index As Integer = dgvStages.Rows.Add()
        Dim row As DataGridViewRow = dgvStages.Rows(index)
        row.Cells(colStageID.Index).Value = stageId
        row.Cells(colStageFirstPoint.Index).Value = firstPoint
        row.Cells(colStageLastPoint.Index).Value = lastPoint
        row.Cells(colStagePhase.Index).Value = PhaseText(phase)
    End Sub

    Private Sub btImportStages_Click(sender As Object, e As System.EventArgs) Handles btImportStages.Click
        Try
            Dim stageInfo As VarColumnInfo = OptionalComboColumn(cbStageColumn)
            Dim phaseInfo As VarColumnInfo = OptionalComboColumn(cbPhaseColumn)
            If stageInfo Is Nothing AndAlso phaseInfo Is Nothing Then
                Throw New ArgumentException("Select a stage identifier column, a Phase I/Phase II column, or both.")
            End If
            Dim context As InputRowContext = GetInputRowContext()
            Dim rowStages As String() = If(stageInfo Is Nothing, Nothing,
                                           ReadTextColumn(stageInfo, context.FirstRow, context.LastRow))
            Dim rowPhases As String() = If(phaseInfo Is Nothing, Nothing,
                                           ReadTextColumn(phaseInfo, context.FirstRow, context.LastRow))
            Dim pointStages(context.PointCount - 1) As String
            Dim pointPhases(context.PointCount - 1) As SpcPhase
            For point As Integer = 0 To context.PointCount - 1
                pointPhases(point) = If(phaseInfo Is Nothing, SpcPhase.PhaseI,
                                        ParseCollapsedPhase(rowPhases, context.PointIndexByRow, point))
                If stageInfo IsNot Nothing Then
                    pointStages(point) = CollapsedTextValue(rowStages, context.PointIndexByRow, point, "stage identifier")
                    If pointStages(point).Length = 0 Then Throw New ArgumentException("The stage identifier is blank at chart point " & (point + 1).ToString() & ".")
                End If
            Next
            If stageInfo Is Nothing Then GenerateStageIdsFromPhaseRuns(pointStages, pointPhases)

            dgvStages.Rows.Clear()
            Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim runStart As Integer = 0
            For point As Integer = 1 To context.PointCount
                Dim atEnd As Boolean = point = context.PointCount
                Dim changed As Boolean = Not atEnd AndAlso
                    (Not String.Equals(pointStages(point), pointStages(runStart), StringComparison.Ordinal) OrElse
                     pointPhases(point) <> pointPhases(runStart))
                If Not atEnd AndAlso Not changed Then Continue For
                Dim stageId As String = pointStages(runStart).Trim()
                If Not used.Add(stageId) Then Throw New ArgumentException("Stage '" & stageId & "' occurs in more than one non-contiguous block.")
                AddStageRow(stageId, runStart + 1, point, pointPhases(runStart))
                runStart = point
            Next
            ClearQuickPhaseSelection()
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
    End Sub

    Private Shared Sub GenerateStageIdsFromPhaseRuns(stageIds As String(), phases As SpcPhase())
        Dim phaseCounts As New Dictionary(Of SpcPhase, Integer)()
        Dim start As Integer = 0
        While start < phases.Length
            Dim phase As SpcPhase = phases(start)
            Dim finish As Integer = start
            While finish + 1 < phases.Length AndAlso phases(finish + 1) = phase
                finish += 1
            End While
            Dim number As Integer = If(phaseCounts.ContainsKey(phase), phaseCounts(phase) + 1, 1)
            phaseCounts(phase) = number
            Dim baseId As String = If(phase = SpcPhase.PhaseI, "PhaseI", "PhaseII")
            Dim id As String = If(number = 1, baseId, baseId & "-" & number.ToString(CultureInfo.InvariantCulture))
            For i As Integer = start To finish
                stageIds(i) = id
            Next
            start = finish + 1
        End While
    End Sub

    Private Sub ClearQuickPhaseSelection()
        rbSinglePhaseI.Checked = False
        rbPhaseIThenPhaseII.Checked = False
        rbSinglePhaseII.Checked = False
        UpdateQuickPhaseControls()
    End Sub

    Private Sub btAddExclusion_Click(sender As Object, e As System.EventArgs) Handles btAddExclusion.Click
        Dim index As Integer = dgvExclusions.Rows.Add()
        dgvExclusions.Rows(index).Cells(colExclusionScope.Index).Value = "Parameter estimation and signal evaluation"
    End Sub

    Private Sub btRemoveExclusion_Click(sender As Object, e As System.EventArgs) Handles btRemoveExclusion.Click
        RemoveSelectedGridRows(dgvExclusions)
    End Sub

    Private Sub btClearExclusions_Click(sender As Object, e As System.EventArgs) Handles btClearExclusions.Click
        dgvExclusions.Rows.Clear()
    End Sub

    Private Sub btImportExclusions_Click(sender As Object, e As System.EventArgs) Handles btImportExclusions.Click
        Try
            Dim indicatorInfo As VarColumnInfo = OptionalComboColumn(cbExclusionColumn)
            If indicatorInfo Is Nothing Then Throw New ArgumentException("Select an exclusion indicator column.")
            Dim reasonInfo As VarColumnInfo = OptionalComboColumn(cbExclusionReasonColumn)
            Dim context As InputRowContext = GetInputRowContext()
            Dim indicators As Object() = ReadObjectColumn(indicatorInfo, context.FirstRow, context.LastRow)
            Dim reasons As String() = If(reasonInfo Is Nothing, Nothing,
                                         ReadTextColumn(reasonInfo, context.FirstRow, context.LastRow))
            Dim excluded(context.PointCount - 1) As Boolean
            Dim reasonSets(context.PointCount - 1) As HashSet(Of String)
            For i As Integer = 0 To reasonSets.Length - 1
                reasonSets(i) = New HashSet(Of String)(StringComparer.Ordinal)
            Next
            For row As Integer = 0 To indicators.Length - 1
                Dim point As Integer = context.PointIndexByRow(row)
                If IsExclusionIndicator(indicators(row)) Then
                    excluded(point) = True
                    If reasons IsNot Nothing AndAlso reasons(row).Length > 0 Then reasonSets(point).Add(reasons(row))
                End If
            Next
            dgvExclusions.Rows.Clear()
            Dim scopeText As String = ExclusionScopeText(SelectedComboValue(Of SpcExclusionScope)(cbImportedExclusionScope))
            For point As Integer = 0 To excluded.Length - 1
                If Not excluded(point) Then Continue For
                Dim index As Integer = dgvExclusions.Rows.Add()
                dgvExclusions.Rows(index).Cells(colExclusionPoint.Index).Value = point + 1
                dgvExclusions.Rows(index).Cells(colExclusionScope.Index).Value = scopeText
                dgvExclusions.Rows(index).Cells(colExclusionReason.Index).Value = String.Join("; ", reasonSets(point).ToArray())
            Next
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
    End Sub

    Private Shared Sub RemoveSelectedGridRows(grid As DataGridView)
        Dim rows As New List(Of DataGridViewRow)()
        For Each row As DataGridViewRow In grid.SelectedRows
            If Not row.IsNewRow Then rows.Add(row)
        Next
        If rows.Count = 0 AndAlso grid.CurrentRow IsNot Nothing AndAlso Not grid.CurrentRow.IsNewRow Then rows.Add(grid.CurrentRow)
        For Each row As DataGridViewRow In rows
            grid.Rows.Remove(row)
        Next
    End Sub

    Private Sub Grid_DataError(sender As Object, e As DataGridViewDataErrorEventArgs) Handles dgvHistoricalMean.DataError, dgvHistoricalCovariance.DataError, dgvStages.DataError, dgvExclusions.DataError
        e.ThrowException = False
    End Sub

#End Region

#Region "Request construction and worksheet import"

    Private Function BuildRequest() As SpcMultivariateRequest
        ValidateSelections()
        Dim context As InputRowContext = GetInputRowContext()
        Dim variableInfos As List(Of VarColumnInfo) = RoleColumnInfos(lbVariables)
        Dim measurements(context.RowCount - 1, variableInfos.Count - 1) As Double
        Dim variableNames(variableInfos.Count - 1) As String
        For column As Integer = 0 To variableInfos.Count - 1
            Dim values As Double() = ReadNumericColumn(variableInfos(column), context.FirstRow, context.LastRow)
            variableNames(column) = variableInfos(column).HeaderText
            For row As Integer = 0 To values.Length - 1
                measurements(row, column) = values(row)
            Next
        Next

        Dim subgroupIds As String() = Nothing
        If context.IsGrouped Then
            subgroupIds = ReadTextColumn(ColumnInfo(CStr(lbSubgroupID.Items(0))), context.FirstRow, context.LastRow)
        End If
        Dim labels As String() = If(lbLabels.Items.Count = 1,
                                    ReadTextColumn(ColumnInfo(CStr(lbLabels.Items(0))), context.FirstRow, context.LastRow),
                                    Nothing)
        Dim sequences As Double() = If(lbSequence.Items.Count = 1,
                                       ReadNumericColumn(ColumnInfo(CStr(lbSequence.Items(0))), context.FirstRow, context.LastRow),
                                       Nothing)
        If context.IsGrouped Then NormalizeGroupedMetadata(labels, sequences, context)

        Dim pointAssignments As PointAssignments = BuildPointAssignments(context.PointCount)
        Dim pointExclusions As PointExclusions = BuildPointExclusions(context.PointCount)
        Dim phases(context.RowCount - 1) As SpcPhase
        Dim stages(context.RowCount - 1) As String
        Dim scopes(context.RowCount - 1) As SpcExclusionScope
        Dim reasons(context.RowCount - 1) As String
        Dim sourceRows(context.RowCount - 1) As Integer
        For row As Integer = 0 To context.RowCount - 1
            Dim point As Integer = context.PointIndexByRow(row)
            phases(row) = pointAssignments.Phases(point)
            stages(row) = pointAssignments.StageIds(point)
            scopes(row) = pointExclusions.Scopes(point)
            reasons(row) = pointExclusions.Reasons(point)
            sourceRows(row) = context.FirstRow + row
        Next

        Dim modelSource As SpcMultivariateModelSource = SelectedComboValue(Of SpcMultivariateModelSource)(cbModelSource)
        Dim historicalMean As Double() = Nothing
        Dim historicalCovariance As Double(,) = Nothing
        If modelSource = SpcMultivariateModelSource.UseHistoricalParameters Then
            BuildHistoricalModel(historicalMean, historicalCovariance)
        End If

        Dim pcaCount As Nullable(Of Integer) = Nothing
        If rbPcaFixedComponents.Checked Then pcaCount = CInt(spinPcaComponentCount.Value)
        Dim mewmaLimit As Nullable(Of Double) = Nothing
        If chkSpecifyMewmaControlLimit.Checked Then mewmaLimit = CDbl(spinMewmaControlLimit.Value)
        Dim gvMultiplier As Nullable(Of Double) = Nothing
        If chkSpecifyGvSigmaMultiplier.Checked Then gvMultiplier = CDbl(spinGvSigmaMultiplier.Value)

        Return New SpcMultivariateRequest(
            SelectedChartType(),
            measurements,
            variableNames:=variableNames,
            subgroupIds:=subgroupIds,
            labels:=labels,
            phases:=phases,
            stageIds:=stages,
            sequenceValues:=sequences,
            sourceRowIndices:=sourceRows,
            exclusionScopes:=scopes,
            exclusionReasons:=reasons,
            missingValuePolicy:=SelectedComboValue(Of SpcMissingValuePolicy)(cbMissingValuePolicy),
            modelSource:=modelSource,
            historicalMean:=historicalMean,
            historicalCovariance:=historicalCovariance,
            controlLimitAlpha:=CDbl(spinControlLimitAlpha.Value),
            useLowerHotellingLimit:=chkUseLowerHotellingLimit.Checked,
            covarianceRegularization:=CDbl(spinCovarianceRegularization.Value),
            allowPseudoInverse:=chkAllowPseudoInverse.Checked,
            pcaUseCorrelationMatrix:=SelectedComboValue(Of Boolean)(cbPcaMatrix),
            pcaComponentCount:=pcaCount,
            pcaCumulativeVariance:=CDbl(spinPcaCumulativeVariance.Value) / 100.0R,
            mewmaLambda:=CDbl(spinMewmaLambda.Value),
            mewmaControlLimit:=mewmaLimit,
            mcusumReferenceValue:=CDbl(spinMcusumReferenceValue.Value),
            mcusumDecisionInterval:=CDbl(spinMcusumDecisionInterval.Value),
            generalizedVarianceSigmaMultiplier:=gvMultiplier,
            resetAtStageBoundary:=chkResetAtStageBoundary.Checked,
            resetAtPhaseBoundary:=chkResetAtPhaseBoundary.Checked,
            resetAfterSignal:=chkResetAfterSignal.Checked,
            requestLabel:=Me.Text,
            sequenceGapBehavior:=SelectedComboValue(Of SpcSequenceGapBehavior)(cbSequenceGapBehavior))
    End Function

    Private Sub ValidateSelections()
        If pWorksheet Is Nothing Then Throw New ArgumentException("Select a source worksheet.")
        If lbVariables.Items.Count < 2 Then Throw New ArgumentException("Select at least two measurement-variable columns.")
        If IsGroupedStructure() Then RequireExactlyOne(lbSubgroupID, "subgroup ID")
        If lbLabels.Items.Count > 1 Then Throw New ArgumentException("Select at most one point-label column.")
        If lbSequence.Items.Count > 1 Then Throw New ArgumentException("Select at most one sequence/date/time column.")

        Dim used As New Dictionary(Of String, String)(StringComparer.Ordinal)
        For Each role As KeyValuePair(Of String, ListBox) In {
            New KeyValuePair(Of String, ListBox)("measurement variables", lbVariables),
            New KeyValuePair(Of String, ListBox)("subgroup ID", lbSubgroupID),
            New KeyValuePair(Of String, ListBox)("point label", lbLabels),
            New KeyValuePair(Of String, ListBox)("sequence", lbSequence)
        }
            If role.Value Is lbSubgroupID AndAlso Not IsGroupedStructure() Then Continue For
            For Each item As Object In role.Value.Items
                Dim name As String = item.ToString()
                If used.ContainsKey(name) Then Throw New ArgumentException("Column '" & name & "' is assigned to both " & used(name) & " and " & role.Key & ".")
                used(name) = role.Key
            Next
        Next

        If Not chkWriteSummary.Checked AndAlso Not chkWriteModelDetails.Checked AndAlso
           Not chkCreateControlChart.Checked AndAlso Not chkWriteChartData.Checked AndAlso
           Not chkWriteSignals.Checked AndAlso Not chkWriteDiagnostics.Checked AndAlso
           Not chkWriteSettingsAudit.Checked Then
            Throw New ArgumentException("Select at least one output on the Output and Appearance tab.")
        End If
        If SelectedChartType() = SpcChartType.GeneralizedVariance AndAlso Not IsGroupedStructure() Then
            Throw New ArgumentException("A generalized-variance chart requires rational subgroups.")
        End If
    End Sub

    Private Shared Sub RequireExactlyOne(list As ListBox, roleName As String)
        If list.Items.Count <> 1 Then Throw New ArgumentException("Select exactly one " & roleName & " column.")
    End Sub

    Private Function GetInputRowContext() As InputRowContext
        If lbVariables.Items.Count = 0 Then Throw New ArgumentException("Select the measurement variables first.")
        Dim required As List(Of VarColumnInfo) = RoleColumnInfos(lbVariables)
        Dim grouped As Boolean = IsGroupedStructure()
        If grouped Then
            If lbSubgroupID.Items.Count = 0 Then Throw New ArgumentException("Select a subgroup-ID column first.")
            required.Add(ColumnInfo(CStr(lbSubgroupID.Items(0))))
        End If
        Dim firstRow As Integer = If(required.Any(Function(info) info.HasHeader), 2, 1)
        Dim lastRow As Integer = required.Max(Function(info) LastUsedRow(info.ColumnNumber))
        If lastRow < firstRow Then Throw New ArgumentException("The selected columns contain no data rows.")
        Dim rowCount As Integer = lastRow - firstRow + 1
        Dim pointIndices(rowCount - 1) As Integer
        Dim pointCount As Integer
        If grouped Then
            Dim subgroupIds As String() = ReadTextColumn(ColumnInfo(CStr(lbSubgroupID.Items(0))), firstRow, lastRow)
            Dim groups As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For row As Integer = 0 To subgroupIds.Length - 1
                Dim id As String = subgroupIds(row).Trim()
                If id.Length = 0 Then Throw New ArgumentException("A subgroup ID is blank at worksheet row " & (firstRow + row).ToString() & ".")
                Dim point As Integer
                If Not groups.TryGetValue(id, point) Then
                    point = groups.Count
                    groups.Add(id, point)
                End If
                pointIndices(row) = point
            Next
            pointCount = groups.Count
        Else
            For row As Integer = 0 To rowCount - 1
                pointIndices(row) = row
            Next
            pointCount = rowCount
        End If
        spinLastPhaseIPoint.Maximum = Math.Max(1D, CDec(pointCount))
        If spinLastPhaseIPoint.Value > spinLastPhaseIPoint.Maximum Then spinLastPhaseIPoint.Value = spinLastPhaseIPoint.Maximum
        Return New InputRowContext With {
            .FirstRow = firstRow,
            .LastRow = lastRow,
            .PointIndexByRow = pointIndices,
            .PointCount = pointCount,
            .IsGrouped = grouped
        }
    End Function

    Private Shared Sub NormalizeGroupedMetadata(ByRef labels As String(), ByRef sequences As Double(), context As InputRowContext)
        If labels IsNot Nothing Then
            Dim collapsed(context.PointCount - 1) As String
            For point As Integer = 0 To context.PointCount - 1
                collapsed(point) = CollapsedTextValue(labels, context.PointIndexByRow, point, "label")
            Next
            For row As Integer = 0 To labels.Length - 1
                labels(row) = collapsed(context.PointIndexByRow(row))
            Next
        End If
        If sequences IsNot Nothing Then
            Dim collapsed(context.PointCount - 1) As Double
            For point As Integer = 0 To context.PointCount - 1
                Dim found As Nullable(Of Double) = Nothing
                For row As Integer = 0 To sequences.Length - 1
                    If context.PointIndexByRow(row) <> point OrElse Double.IsNaN(sequences(row)) Then Continue For
                    If found.HasValue AndAlso sequences(row) <> found.Value Then
                        Throw New ArgumentException("Rows belonging to subgroup point " & (point + 1).ToString() & " contain inconsistent sequence values.")
                    End If
                    found = sequences(row)
                Next
                collapsed(point) = If(found.HasValue, found.Value, Double.NaN)
            Next
            For row As Integer = 0 To sequences.Length - 1
                sequences(row) = collapsed(context.PointIndexByRow(row))
            Next
        End If
    End Sub

    Private Function BuildPointAssignments(pointCount As Integer) As PointAssignments
        Dim stageIds(pointCount - 1) As String
        Dim phases(pointCount - 1) As SpcPhase
        Dim assigned(pointCount - 1) As Boolean
        Dim stageCount As Integer = 0
        Dim usedIds As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row As DataGridViewRow In dgvStages.Rows
            If row.IsNewRow OrElse GridRowIsBlank(row) Then Continue For
            stageCount += 1
            Dim id As String = CellText(row, colStageID.Index)
            If id.Length = 0 Then Throw New ArgumentException("Every stage requires a nonblank stage ID.")
            If Not usedIds.Add(id) Then Throw New ArgumentException("Stage ID '" & id & "' is duplicated.")
            Dim firstPoint As Integer = RequiredIntegerCell(row, colStageFirstPoint.Index, "First point")
            Dim lastPoint As Integer = RequiredIntegerCell(row, colStageLastPoint.Index, "Last point")
            If firstPoint < 1 OrElse lastPoint < firstPoint OrElse lastPoint > pointCount Then
                Throw New ArgumentException("Stage '" & id & "' must use a valid one-based point range within 1 to " & pointCount.ToString() & ".")
            End If
            Dim phase As SpcPhase = ParsePhaseText(CellText(row, colStagePhase.Index))
            For point As Integer = firstPoint - 1 To lastPoint - 1
                If assigned(point) Then Throw New ArgumentException("Chart point " & (point + 1).ToString() & " belongs to more than one stage.")
                assigned(point) = True
                stageIds(point) = id
                phases(point) = phase
            Next
        Next
        If stageCount = 0 Then
            For point As Integer = 0 To pointCount - 1
                stageIds(point) = "Stage1"
                phases(point) = SpcPhase.PhaseI
            Next
        Else
            For point As Integer = 0 To pointCount - 1
                If Not assigned(point) Then Throw New ArgumentException("Chart point " & (point + 1).ToString() & " is not covered by the stage grid.")
            Next
        End If
        Return New PointAssignments With {.StageIds = stageIds, .Phases = phases}
    End Function

    Private Function BuildPointExclusions(pointCount As Integer) As PointExclusions
        Dim scopes(pointCount - 1) As SpcExclusionScope
        Dim reasonSets(pointCount - 1) As HashSet(Of String)
        For i As Integer = 0 To pointCount - 1
            reasonSets(i) = New HashSet(Of String)(StringComparer.Ordinal)
        Next
        For Each row As DataGridViewRow In dgvExclusions.Rows
            If row.IsNewRow OrElse GridRowIsBlank(row) Then Continue For
            Dim point As Integer = RequiredIntegerCell(row, colExclusionPoint.Index, "Exclusion point")
            If point < 1 OrElse point > pointCount Then Throw New ArgumentException("Exclusion point must be between 1 and " & pointCount.ToString() & ".")
            scopes(point - 1) = scopes(point - 1) Or ParseExclusionScope(CellText(row, colExclusionScope.Index))
            Dim reason As String = CellText(row, colExclusionReason.Index)
            If reason.Length > 0 Then reasonSets(point - 1).Add(reason)
        Next
        Dim reasons(pointCount - 1) As String
        For i As Integer = 0 To reasons.Length - 1
            reasons(i) = String.Join("; ", reasonSets(i).ToArray())
        Next
        Return New PointExclusions With {.Scopes = scopes, .Reasons = reasons}
    End Function

    Private Sub BuildHistoricalModel(ByRef mean As Double(), ByRef covariance As Double(,))
        EnsureHistoricalGridReady()
        Dim p As Integer = lbVariables.Items.Count
        If SelectedChartType() <> SpcChartType.GeneralizedVariance Then
            ReDim mean(p - 1)
            For i As Integer = 0 To p - 1
                mean(i) = RequiredDoubleCell(dgvHistoricalMean.Rows(i), colHistoricalMeanValue.Index,
                                              "Historical mean for " & CStr(lbVariables.Items(i)))
            Next
        End If
        ReDim covariance(p - 1, p - 1)
        For r As Integer = 0 To p - 1
            For c As Integer = 0 To p - 1
                covariance(r, c) = RequiredDoubleCell(dgvHistoricalCovariance.Rows(r), c + 1,
                                                       "Historical covariance value")
            Next
        Next
    End Sub

    Private Function RoleColumnInfos(list As ListBox) As List(Of VarColumnInfo)
        Dim values As New List(Of VarColumnInfo)()
        For Each item As Object In list.Items
            values.Add(ColumnInfo(item.ToString()))
        Next
        Return values
    End Function

    Private Function ColumnInfo(displayText As String) As VarColumnInfo
        Dim info As VarColumnInfo = Nothing
        If Not pColumnInfo.TryGetValue(displayText, info) Then
            Throw New ArgumentException("Worksheet column '" & displayText & "' is no longer available. Reload the worksheet columns.")
        End If
        Return info
    End Function

    Private Function OptionalComboColumn(combo As ComboBox) As VarColumnInfo
        If combo.SelectedItem Is Nothing OrElse combo.SelectedItem.ToString() = NoneColumnText Then Return Nothing
        Return ColumnInfo(combo.SelectedItem.ToString())
    End Function

    Private Function LastUsedRow(columnNumber As Integer) As Integer
        Dim finalCell As Excel.Range = DirectCast(pWorksheet.Cells(pWorksheet.Rows.Count, columnNumber), Excel.Range)
        Return finalCell.End(Excel.XlDirection.xlUp).Row
    End Function

    Private Function ReadObjectColumn(info As VarColumnInfo, firstRow As Integer, lastRow As Integer) As Object()
        Dim count As Integer = lastRow - firstRow + 1
        Dim result(count - 1) As Object
        Dim range As Excel.Range = pWorksheet.Range(pWorksheet.Cells(firstRow, info.ColumnNumber),
                                                     pWorksheet.Cells(lastRow, info.ColumnNumber))
        Dim raw As Object = range.Value2
        If count = 1 Then
            result(0) = NormalizeWorksheetValue(raw)
        Else
            Dim matrix As Object(,) = TryCast(raw, Object(,))
            If matrix Is Nothing Then Throw New InvalidOperationException("Excel did not return a column matrix for " & info.DisplayText & ".")
            For i As Integer = 0 To count - 1
                result(i) = NormalizeWorksheetValue(matrix(i + 1, 1))
            Next
        End If
        Return result
    End Function

    Private Function ReadNumericColumn(info As VarColumnInfo, firstRow As Integer, lastRow As Integer) As Double()
        Dim raw As Object() = ReadObjectColumn(info, firstRow, lastRow)
        Dim values(raw.Length - 1) As Double
        For i As Integer = 0 To raw.Length - 1
            If raw(i) Is Nothing Then
                values(i) = Double.NaN
            ElseIf TypeOf raw(i) Is DateTime Then
                values(i) = DirectCast(raw(i), DateTime).ToOADate()
            Else
                Dim number As Double
                If Not TryParseDouble(Convert.ToString(raw(i), CultureInfo.CurrentCulture), number) Then
                    Throw New ArgumentException("Column '" & info.HeaderText & "' contains a nonnumeric value at worksheet row " &
                                                (firstRow + i).ToString(CultureInfo.CurrentCulture) & ".")
                End If
                values(i) = number
            End If
        Next
        Return values
    End Function

    Private Function ReadTextColumn(info As VarColumnInfo, firstRow As Integer, lastRow As Integer) As String()
        Dim raw As Object() = ReadObjectColumn(info, firstRow, lastRow)
        Dim values(raw.Length - 1) As String
        For i As Integer = 0 To raw.Length - 1
            values(i) = If(raw(i) Is Nothing, String.Empty, Convert.ToString(raw(i), CultureInfo.CurrentCulture).Trim())
        Next
        Return values
    End Function

    Private Shared Function NormalizeWorksheetValue(value As Object) As Object
        If value Is Nothing OrElse value Is DBNull.Value OrElse TypeOf value Is ErrorWrapper Then Return Nothing
        Dim text As String = TryCast(value, String)
        If text IsNot Nothing AndAlso text.Trim().Length = 0 Then Return Nothing
        Return value
    End Function

#End Region

#Region "Calculation and output"

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        If pBusy Then Return
        Try
            If pWorkbook IsNot Nothing Then pWorkbook.Activate()
            Dim request As SpcMultivariateRequest = BuildRequest()
            BeginComputation()
            Dim result As SpcMultivariateFitResult = SpcMultivariate.Fit(request, AddressOf CancellationRequested)
            If pCancelRequested Then Throw New OperationCanceledException("The multivariate SPC calculation was cancelled.")
            WriteResultsToNewWorkbook(result)
            ProgressBar.Style = ProgressBarStyle.Blocks
            ProgressBar.Value = 100
        Catch ex As OperationCanceledException
            MessageBox.Show("The multivariate SPC calculation was interrupted.", AppGlobals.gsAPP_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As ArgumentException
            ShowInputError(ex.Message)
        Catch ex As InvalidOperationException
            ShowInputError(ex.Message)
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True, "Unable to calculate the multivariate control chart")
        Finally
            EndComputation()
        End Try
    End Sub

    Private Sub BeginComputation()
        pBusy = True
        pCancelRequested = False
        btCompute.Enabled = False
        btnInterrupt.Enabled = True
        ProgressBar.Style = ProgressBarStyle.Marquee
        ProgressBar.MarqueeAnimationSpeed = 25
        Cursor = Cursors.WaitCursor
        Application.DoEvents()
    End Sub

    Private Sub EndComputation()
        pBusy = False
        btnInterrupt.Enabled = False
        btCompute.Enabled = True
        ProgressBar.MarqueeAnimationSpeed = 0
        ProgressBar.Style = ProgressBarStyle.Blocks
        If pCancelRequested Then ProgressBar.Value = 0
        Cursor = Cursors.Default
    End Sub

    Private Function CancellationRequested() As Boolean
        Application.DoEvents()
        Return pCancelRequested
    End Function

    Private Sub btnInterrupt_Click(sender As Object, e As System.EventArgs) Handles btnInterrupt.Click
        pCancelRequested = True
        btnInterrupt.Enabled = False
    End Sub

    Private Sub Ui22MultivariateControlCharts_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not pBusy Then Return
        pCancelRequested = True
        e.Cancel = True
    End Sub

    Private Sub Ui22MultivariateControlCharts_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        If pWorksheet IsNot Nothing OrElse AppGlobals.app Is Nothing Then Return
        Dim activeWorksheet As Excel.Worksheet = TryCast(AppGlobals.app.ActiveSheet, Excel.Worksheet)
        If activeWorksheet IsNot Nothing Then Populate(activeWorksheet)
    End Sub

    Private Sub WriteResultsToNewWorkbook(result As SpcMultivariateFitResult)
        Dim workbook As Excel.Workbook = CreateResultWorkbook()
        Dim firstAvailable As Boolean = True
        Dim firstOutput As Excel.Worksheet = Nothing

        If chkWriteSummary.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Multivariate Summary", firstAvailable)
            firstAvailable = False
            firstOutput = sheet
            WriteResultTables(sheet, workbook, BuildSummaryTables(result))
        End If
        If chkWriteModelDetails.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Model Details", firstAvailable)
            firstAvailable = False
            If firstOutput Is Nothing Then firstOutput = sheet
            WriteResultTables(sheet, workbook, BuildModelTables(result))
        End If
        If chkCreateControlChart.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Multivariate Chart", firstAvailable)
            firstAvailable = False
            If firstOutput Is Nothing Then firstOutput = sheet
            Try
                graphics.SpcControlChartExcel.AddCharts(sheet, BuildChartRenderingResult(result), BuildAppearanceOptions())
            Catch ex As Exception
                Dim warning As New ResultTable()
                warning.AddTitle("Multivariate control-chart warning")
                warning.AddFootnote("The statistical results were calculated, but the Excel chart could not be created: " & ex.Message)
                WriteResultTables(sheet, workbook, New List(Of ResultTable) From {warning})
            End Try
        End If
        If chkWriteChartData.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Chart Data", firstAvailable)
            firstAvailable = False
            If firstOutput Is Nothing Then firstOutput = sheet
            WriteResultTables(sheet, workbook, New List(Of ResultTable) From {BuildChartDataTable(result)})
        End If
        If chkWriteSignals.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Signals", firstAvailable)
            firstAvailable = False
            If firstOutput Is Nothing Then firstOutput = sheet
            WriteResultTables(sheet, workbook, New List(Of ResultTable) From {BuildSignalsTable(result)})
        End If
        If chkWriteDiagnostics.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Diagnostics", firstAvailable)
            firstAvailable = False
            If firstOutput Is Nothing Then firstOutput = sheet
            WriteResultTables(sheet, workbook, BuildDiagnosticTables(result))
        End If
        If chkWriteSettingsAudit.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Settings and Audit", firstAvailable)
            firstAvailable = False
            If firstOutput Is Nothing Then firstOutput = sheet
            WriteResultTables(sheet, workbook, BuildAuditTables(result))
        End If
        If firstOutput IsNot Nothing Then firstOutput.Activate()
    End Sub

    Private Function BuildChartRenderingResult(result As SpcMultivariateFitResult) As SpcFitResult
        Dim sourcePanels As SpcPanelResult() = result.Panels
        Dim panels(sourcePanels.Length - 1) As SpcPanelResult
        For i As Integer = 0 To sourcePanels.Length - 1
            Dim source As SpcPanelResult = sourcePanels(i)
            panels(i) = New SpcPanelResult(
                source.PanelType,
                source.DisplayName,
                source.Points,
                valueAxisTitle:=If(String.IsNullOrWhiteSpace(tbValueAxisTitle.Text), source.ValueAxisTitle, tbValueAxisTitle.Text.Trim()),
                parameterEstimates:=source.ParameterEstimates,
                signals:=source.Signals,
                warnings:=source.Warnings)
        Next
        Dim points As SpcPointResult() = panels(0).Points
        Dim values(points.Length - 1) As Double
        Dim labels(points.Length - 1) As String
        Dim sequences(points.Length - 1) As Double
        Dim sourceRows(points.Length - 1) As Integer
        For i As Integer = 0 To points.Length - 1
            values(i) = points(i).Value
            labels(i) = points(i).Label
            sequences(i) = If(points(i).SequenceValue.HasValue, points(i).SequenceValue.Value, Double.NaN)
            Dim rows As Integer() = points(i).SourceRowIndices
            sourceRows(i) = If(rows.Length > 0, rows(0), points(i).PointIndex)
        Next
        Dim input As SpcInputData = SpcInputData.FromIndividualSequence(values, labels, sequences, sourceRows, "Statistic")
        Dim title As String = If(String.IsNullOrWhiteSpace(tbChartTitle.Text), SelectedChartChoice().DisplayText, tbChartTitle.Text.Trim())
        Dim request As New SpcFitRequest(result.Request.ChartType, input,
                                         requestLabel:=result.Request.RequestLabel,
                                         chartTitle:=title,
                                         valueAxisTitle:=tbValueAxisTitle.Text.Trim())
        Return New SpcFitResult(request, panels, result.Warnings, result.ExecutionTimeMilliseconds)
    End Function

    Private Function BuildAppearanceOptions() As graphics.SpcControlChartAppearanceOptions
        Return New graphics.SpcControlChartAppearanceOptions With {
            .ChartWidth = CDbl(spinChartWidth.Value),
            .PanelHeight = CDbl(spinChartHeight.Value),
            .PanelSpacing = 0R,
            .ChartTitle = If(String.IsNullOrWhiteSpace(tbChartTitle.Text), SelectedChartChoice().DisplayText, tbChartTitle.Text.Trim()),
            .HorizontalAxisTitle = tbHorizontalAxisTitle.Text.Trim(),
            .UseSequenceValuesForHorizontalAxis = chkUseSequenceValuesForHorizontalAxis.Checked,
            .ShowHorizontalAxisOnEveryPanel = True,
            .HorizontalTickLabelOrientation = SelectedComboValue(Of Integer)(cbHorizontalTickOrientation),
            .ShowLegend = chkShowLegend.Checked,
            .ShowMajorGridlines = chkShowMajorGridlines.Checked,
            .ShowPointLabels = chkShowPointLabels.Checked,
            .ShowSignalLabels = chkShowSignalLabels.Checked,
            .ShowExclusionLabels = chkShowExclusionLabels.Checked,
            .ShowLimitLabels = chkShowLimitLabels.Checked,
            .ShowExcludedPoints = chkShowExcludedPoints.Checked,
            .ShowStageBoundaries = chkShowStageBoundaries.Checked,
            .ZoneDisplay = graphics.SpcZoneDisplayMode.None,
            .ShowZoneSeriesInLegend = False,
            .ShowSpecificationLimits = False,
            .ShowTargetLine = False,
            .ValueNumberFormat = If(String.IsNullOrWhiteSpace(tbValueNumberFormat.Text), "0.####", tbValueNumberFormat.Text.Trim())
        }
    End Function

    Private Sub btResetAppearance_Click(sender As Object, e As System.EventArgs) Handles btResetAppearance.Click
        Dim defaults As New graphics.SpcControlChartAppearanceOptions()
        spinChartWidth.Value = ClampDecimal(760D, spinChartWidth.Minimum, spinChartWidth.Maximum)
        spinChartHeight.Value = ClampDecimal(360D, spinChartHeight.Minimum, spinChartHeight.Maximum)
        tbChartTitle.Clear()
        tbValueAxisTitle.Clear()
        tbHorizontalAxisTitle.Text = defaults.HorizontalAxisTitle
        tbValueNumberFormat.Text = defaults.ValueNumberFormat
        chkUseSequenceValuesForHorizontalAxis.Checked = False
        SelectComboValue(cbHorizontalTickOrientation, defaults.HorizontalTickLabelOrientation)
        chkShowLegend.Checked = defaults.ShowLegend
        chkShowMajorGridlines.Checked = defaults.ShowMajorGridlines
        chkShowPointLabels.Checked = defaults.ShowPointLabels
        chkShowSignalLabels.Checked = defaults.ShowSignalLabels
        chkShowExclusionLabels.Checked = defaults.ShowExclusionLabels
        chkShowLimitLabels.Checked = defaults.ShowLimitLabels
        chkShowExcludedPoints.Checked = defaults.ShowExcludedPoints
        chkShowStageBoundaries.Checked = defaults.ShowStageBoundaries
    End Sub

    Private Shared Function ClampDecimal(value As Decimal, minimum As Decimal, maximum As Decimal) As Decimal
        Return Math.Min(maximum, Math.Max(minimum, value))
    End Function

    Private Function CreateResultWorkbook() As Excel.Workbook
        Dim workbook As Excel.Workbook = DirectCast(AppGlobals.app.Workbooks.Add(), Excel.Workbook)
        Dim oldAlerts As Boolean = AppGlobals.app.DisplayAlerts
        Try
            AppGlobals.app.DisplayAlerts = False
            For i As Integer = workbook.Worksheets.Count To 2 Step -1
                DirectCast(workbook.Worksheets(i), Excel.Worksheet).Delete()
            Next
        Finally
            AppGlobals.app.DisplayAlerts = oldAlerts
        End Try
        Return workbook
    End Function

    Private Function CreateResultSheet(workbook As Excel.Workbook, baseName As String, reuseFirst As Boolean) As Excel.Worksheet
        Dim sheet As Excel.Worksheet
        If reuseFirst Then
            sheet = DirectCast(workbook.Worksheets(1), Excel.Worksheet)
            sheet.Name = MakeUniqueWorksheetName(workbook, baseName, sheet)
        Else
            sheet = DirectCast(workbook.Worksheets.Add(After:=workbook.Worksheets(workbook.Worksheets.Count)), Excel.Worksheet)
            sheet.Name = MakeUniqueWorksheetName(workbook, baseName, Nothing)
        End If
        Return sheet
    End Function

    Private Shared Sub WriteResultTables(sheet As Excel.Worksheet, workbook As Excel.Workbook, tables As List(Of ResultTable))
        If tables Is Nothing OrElse tables.Count = 0 Then Return
        Dim writer As New ExcelDnaResultWriter With {.wb = workbook, .ws = sheet}
        writer.setRowPointer(1)
        writer.setColumnPointer(1)
        Dim processor As New ProcessListofResultTables(tables)
        processor.writeToSheet(writer, True)
        Try
            sheet.Columns.AutoFit()
            sheet.Rows.AutoFit()
        Catch
        End Try
    End Sub

#End Region

#Region "Multivariate result tables"

    Private Function BuildSummaryTables(result As SpcMultivariateFitResult) As List(Of ResultTable)
        Dim panel As SpcPanelResult = result.Panels(0)
        Dim model As SpcMultivariateModelResult = result.Model
        Dim signalCount As Integer = panel.Signals.Length
        Dim rows As New List(Of Object()) From {
            Row("Analysis", result.Request.RequestLabel),
            Row("Chart type", ChartTypeText(result.Request.ChartType)),
            Row("Observation structure", If(result.Request.HasSubgroups, "Rational subgroups", "Individual observations")),
            Row("Input rows", result.Request.RowCount),
            Row("Variables", result.Request.VariableCount),
            Row("Chart points", panel.PointCount),
            Row("Model source", ModelSourceText(result.Request.ModelSource)),
            Row("Baseline observations", model.BaselineObservationCount),
            Row("Baseline subgroups", model.BaselineSubgroupCount),
            Row("Covariance degrees of freedom", model.CovarianceDegreesOfFreedom),
            Row("Effective dimension", model.EffectiveDimension),
            Row("Retained PCA components", model.RetainedComponentCount),
            Row("Pseudoinverse used", YesNo(model.UsedPseudoInverse)),
            Row("Signal occurrences", signalCount),
            Row("Signalled points", panel.SignalledPointCount),
            Row("Process status", If(signalCount = 0, "No intrinsic-limit signals detected", "Intrinsic-limit signal(s) detected")),
            Row("Warnings", CollectWarnings(result).Count),
            Row("Execution time (ms)", result.ExecutionTimeMilliseconds)
        }
        Dim tables As New List(Of ResultTable) From {
            CreateTable("Multivariate SPC analysis - summary", {"Item", "Value"}, rows,
                        footnote:="Signal status reflects the selected multivariate chart and its intrinsic control or decision limit.")
        }

        Dim parameterRows As New List(Of Object())()
        For Each estimate As SpcParameterEstimate In panel.ParameterEstimates
            parameterRows.Add(Row(estimate.StageId, estimate.DisplayName, estimate.ParameterName,
                                  estimate.Value, NullableValue(estimate.StandardError), estimate.Method,
                                  NullableValue(estimate.SampleCount)))
        Next
        tables.Add(CreateTable("Multivariate SPC analysis - retained chart parameters",
                               {"Stage", "Parameter", "Identifier", "Value", "Standard Error", "Method", "Sample Count"},
                               parameterRows, emptyMessage:="No retained chart parameters were reported."))
        tables.Add(BuildWarningsTable(result))
        Return tables
    End Function

    Private Function BuildModelTables(result As SpcMultivariateFitResult) As List(Of ResultTable)
        Dim model As SpcMultivariateModelResult = result.Model
        Dim names As String() = result.Request.VariableNames
        Dim tables As New List(Of ResultTable) From {
            CreateTable("Multivariate in-control model - properties", {"Item", "Value"},
                New List(Of Object()) From {
                    Row("Source", ModelSourceText(model.Source)),
                    Row("Baseline observations", model.BaselineObservationCount),
                    Row("Baseline subgroups", model.BaselineSubgroupCount),
                    Row("Covariance degrees of freedom", model.CovarianceDegreesOfFreedom),
                    Row("Effective dimension", model.EffectiveDimension),
                    Row("Retained components", model.RetainedComponentCount),
                    Row("Pseudoinverse used", YesNo(model.UsedPseudoInverse)),
                    Row("Diagonal ridge factor", model.Regularization)
                })
        }
        If model.ProcessMean IsNot Nothing Then tables.Add(CreateVectorTable("Process mean vector", names, model.ProcessMean, "Mean"))
        tables.Add(CreateVectorTable("Analysis scale", names, model.AnalysisScale, "Scale"))
        tables.Add(CreateMatrixTable("Process covariance matrix", names, names, model.ProcessCovariance))
        tables.Add(CreateMatrixTable("Analysis covariance matrix", names, names, model.AnalysisCovariance))
        tables.Add(CreateMatrixTable("Analysis covariance inverse", names, names, model.AnalysisCovarianceInverse))
        tables.Add(CreateIndexedVectorTable("Eigenvalues", model.Eigenvalues, "Component", "Eigenvalue"))
        tables.Add(CreateMatrixTable("Eigenvectors", names, ComponentNames(model.Eigenvectors.GetLength(1)), model.Eigenvectors))
        Return tables
    End Function

    Private Function BuildChartDataTable(result As SpcMultivariateFitResult) As ResultTable
        Dim rows As New List(Of Object())()
        Dim panel As SpcPanelResult = result.Panels(0)
        For Each point As SpcPointResult In panel.Points
            rows.Add(Row(point.PointIndex + 1,
                         JoinIntegers(point.SourceRowIndices, addOne:=False),
                         point.Label,
                         point.StageId,
                         PhaseText(point.Phase),
                         point.Value,
                         point.CenterLine,
                         point.LowerControlLimit,
                         point.UpperControlLimit,
                         YesNo(point.IncludedInParameterEstimation),
                         YesNo(point.IncludedInRuleEvaluation),
                         ExclusionScopeText(point.ExclusionScope),
                         point.ExclusionReason,
                         YesNo(point.IsSignalled),
                         JoinIntegers(point.SignalRuleNumbers, addOne:=False)))
        Next
        Return CreateTable("Multivariate SPC chart data",
            {"Point", "Source Rows", "Label", "Stage", "Phase", "Statistic", "Center Line", "LCL", "UCL",
             "Used to Estimate", "Signal Eligible", "Exclusion Scope", "Exclusion Reason", "Signal", "Signal Numbers"},
            rows)
    End Function

    Private Function BuildSignalsTable(result As SpcMultivariateFitResult) As ResultTable
        Dim rows As New List(Of Object())()
        For Each signal As SpcSignalResult In result.Panels(0).Signals
            rows.Add(Row(signal.RuleNumber,
                         signal.RuleCode,
                         signal.Rule.DisplayName,
                         signal.StageId,
                         signal.TerminalPointIndex + 1,
                         signal.WindowStartPointIndex + 1,
                         signal.WindowEndPointIndex + 1,
                         signal.TriggeredSide.ToString(),
                         JoinIntegers(signal.ContributingPointIndices, addOne:=True),
                         JoinIntegers(signal.MarkedPointIndices, addOne:=True),
                         signal.Message))
        Next
        Return CreateTable("Multivariate SPC signals",
            {"Signal Number", "Code", "Signal", "Stage", "Terminal Point", "Window Start", "Window End",
             "Side", "Contributing Points", "Marked Points", "Message"},
            rows, emptyMessage:="No intrinsic-limit signals were detected.")
    End Function

    Private Function BuildDiagnosticTables(result As SpcMultivariateFitResult) As List(Of ResultTable)
        Dim diagnostics As SpcMultivariatePointDiagnostic() = result.Diagnostics
        Dim signalOnly As Boolean = SelectedComboValue(Of DiagnosticsScope)(cbDiagnosticsScope) = DiagnosticsScope.SignalledPointsOnly
        Dim panel As SpcPanelResult = result.Panels(0)
        Dim selected As New List(Of SpcMultivariatePointDiagnostic)()
        For Each diagnostic As SpcMultivariatePointDiagnostic In diagnostics
            Dim point As SpcPointResult = panel.GetPoint(diagnostic.PointIndex)
            If Not signalOnly OrElse (point IsNot Nothing AndAlso point.IsSignalled) Then selected.Add(diagnostic)
        Next

        Dim names As String() = result.Request.VariableNames
        Dim hasState As Boolean = selected.Any(Function(item) item.StateVector IsNot Nothing)
        Dim hasScores As Boolean = selected.Any(Function(item) item.ComponentScores IsNot Nothing)
        Dim hasResiduals As Boolean = selected.Any(Function(item) item.ResidualVector IsNot Nothing)
        Dim hasContributions As Boolean = selected.Any(Function(item) item.Contributions IsNot Nothing)
        Dim scoreCount As Integer = 0
        For Each item As SpcMultivariatePointDiagnostic In selected
            If item.ComponentScores IsNot Nothing Then scoreCount = Math.Max(scoreCount, item.ComponentScores.Length)
        Next

        Dim headers As New List(Of String) From {
            "Point", "Source Rows", "Label", "Stage", "Phase", "Statistic", "Effective Sample Size"
        }
        For Each name As String In names
            headers.Add("Observed: " & name)
        Next
        If hasState Then
            For Each name As String In names
                headers.Add("State: " & name)
            Next
        End If
        If hasScores Then
            For i As Integer = 1 To scoreCount
                headers.Add("PC Score " & i.ToString())
            Next
        End If
        If hasResiduals Then
            For Each name As String In names
                headers.Add("Residual: " & name)
            Next
        End If
        If hasContributions Then
            For Each name As String In names
                headers.Add("Contribution: " & name)
            Next
            headers.Add("Contribution Total")
            headers.Add("Contribution Basis")
        End If

        Dim rows As New List(Of Object())()
        For Each item As SpcMultivariatePointDiagnostic In selected
            Dim values As New List(Of Object) From {
                item.PointIndex + 1,
                JoinIntegers(item.SourceRowIndices, addOne:=False),
                item.Label,
                item.StageId,
                PhaseText(item.Phase),
                item.Statistic,
                item.EffectiveSampleSize
            }
            AddVectorValues(values, item.ObservationVector, names.Length)
            If hasState Then AddVectorValues(values, item.StateVector, names.Length)
            If hasScores Then AddVectorValues(values, item.ComponentScores, scoreCount)
            If hasResiduals Then AddVectorValues(values, item.ResidualVector, names.Length)
            If hasContributions Then
                AddVectorValues(values, item.Contributions, names.Length)
                values.Add(If(Double.IsNaN(item.ContributionTotal), CType(String.Empty, Object), item.ContributionTotal))
                values.Add(item.ContributionBasis)
            End If
            rows.Add(values.ToArray())
        Next

        Dim tables As New List(Of ResultTable) From {
            CreateTable("Multivariate point diagnostics and contributions", headers.ToArray(), rows,
                        emptyMessage:=If(signalOnly, "No signalled points have diagnostics.", "No point diagnostics were returned."))
        }
        For Each item As SpcMultivariatePointDiagnostic In selected
            If item.SubgroupCovariance Is Nothing Then Continue For
            tables.Add(CreateMatrixTable("Subgroup covariance - point " & (item.PointIndex + 1).ToString(),
                                          names, names, item.SubgroupCovariance))
        Next
        Return tables
    End Function

    Private Function BuildAuditTables(result As SpcMultivariateFitResult) As List(Of ResultTable)
        Dim request As SpcMultivariateRequest = result.Request
        Dim settings As New List(Of Object()) From {
            Row("Request label", request.RequestLabel),
            Row("Chart type", ChartTypeText(request.ChartType)),
            Row("Observation structure", If(request.HasSubgroups, "Rational subgroups", "Individual observations")),
            Row("Missing-value policy", MissingPolicyText(request.MissingValuePolicy)),
            Row("Model source", ModelSourceText(request.ModelSource)),
            Row("Control-limit alpha", request.ControlLimitAlpha),
            Row("Use lower Hotelling limit", YesNo(request.UseLowerHotellingLimit)),
            Row("Diagonal ridge factor", request.CovarianceRegularization),
            Row("Allow pseudoinverse", YesNo(request.AllowPseudoInverse)),
            Row("PCA matrix", If(request.PcaUseCorrelationMatrix, "Correlation", "Covariance")),
            Row("PCA component count", NullableValue(request.PcaComponentCount)),
            Row("PCA cumulative variance", request.PcaCumulativeVariance),
            Row("MEWMA lambda", request.MewmaLambda),
            Row("MEWMA supplied UCL", NullableValue(request.MewmaControlLimit)),
            Row("MCUSUM k", request.McusumReferenceValue),
            Row("MCUSUM h", request.McusumDecisionInterval),
            Row("Generalized-variance sigma multiplier", NullableValue(request.GeneralizedVarianceSigmaMultiplier)),
            Row("Reset at stage boundary", YesNo(request.ResetAtStageBoundary)),
            Row("Reset at phase boundary", YesNo(request.ResetAtPhaseBoundary)),
            Row("Reset after signal", YesNo(request.ResetAfterSignal)),
            Row("Sequence-gap behaviour", SequenceGapBehaviorText(request.SequenceGapBehavior)),
            Row("Chart title", tbChartTitle.Text.Trim()),
            Row("Value-axis title", tbValueAxisTitle.Text.Trim()),
            Row("Horizontal-axis title", tbHorizontalAxisTitle.Text.Trim()),
            Row("Use sequence on horizontal axis", YesNo(chkUseSequenceValuesForHorizontalAxis.Checked)),
            Row("Chart width", spinChartWidth.Value),
            Row("Chart height", spinChartHeight.Value),
            Row("Execution time (ms)", result.ExecutionTimeMilliseconds)
        }

        Dim stageRows As New List(Of Object())()
        Dim panelPoints As SpcPointResult() = result.Panels(0).Points
        If panelPoints.Length > 0 Then
            Dim start As Integer = 0
            For i As Integer = 1 To panelPoints.Length
                Dim atEnd As Boolean = i = panelPoints.Length
                Dim changed As Boolean = Not atEnd AndAlso
                    (Not String.Equals(panelPoints(i).StageId, panelPoints(start).StageId, StringComparison.Ordinal) OrElse
                     panelPoints(i).Phase <> panelPoints(start).Phase)
                If Not atEnd AndAlso Not changed Then Continue For
                stageRows.Add(Row(panelPoints(start).StageId,
                                  panelPoints(start).PointIndex + 1,
                                  panelPoints(i - 1).PointIndex + 1,
                                  PhaseText(panelPoints(start).Phase)))
                start = i
            Next
        End If

        Dim exclusionRows As New List(Of Object())()
        Dim scopes As SpcExclusionScope() = request.ExclusionScopes
        Dim reasons As String() = request.ExclusionReasons
        Dim sourceRows As Integer() = request.SourceRowIndices
        For i As Integer = 0 To scopes.Length - 1
            If scopes(i) = SpcExclusionScope.None Then Continue For
            exclusionRows.Add(Row(sourceRows(i), ExclusionScopeText(scopes(i)), reasons(i)))
        Next

        Return New List(Of ResultTable) From {
            CreateTable("Multivariate SPC settings and audit", {"Setting", "Value"}, settings),
            CreateTable("Effective stages", {"Stage", "First Point", "Last Point", "Phase"}, stageRows,
                        emptyMessage:="No chart points were returned."),
            CreateTable("Source-row exclusions", {"Source Row", "Scope", "Reason"}, exclusionRows,
                        emptyMessage:="No explicit exclusions were supplied."),
            BuildWarningsTable(result)
        }
    End Function

    Private Function BuildWarningsTable(result As SpcMultivariateFitResult) As ResultTable
        Dim rows As New List(Of Object())()
        Dim warnings As List(Of String) = CollectWarnings(result)
        For i As Integer = 0 To warnings.Count - 1
            rows.Add(Row(i + 1, warnings(i)))
        Next
        Return CreateTable("Multivariate SPC warnings", {"Number", "Warning"}, rows,
                           emptyMessage:="No calculation warnings were reported.")
    End Function

    Private Shared Function CollectWarnings(result As SpcMultivariateFitResult) As List(Of String)
        Dim values As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        For Each warning As String In result.Warnings
            If warning.Length > 0 AndAlso seen.Add(warning) Then values.Add(warning)
        Next
        For Each panel As SpcPanelResult In result.Panels
            For Each warning As String In panel.Warnings
                If warning.Length > 0 AndAlso seen.Add(warning) Then values.Add(warning)
            Next
        Next
        Return values
    End Function

    Private Shared Function CreateTable(title As String, headers As String(), rows As IList(Of Object()),
                                        Optional emptyMessage As String = Nothing,
                                        Optional footnote As String = Nothing) As ResultTable
        Dim table As New ResultTable()
        If title.Length > 0 Then table.AddTitle(title)
        table.AddHeaderTopRow(CType(headers.Clone(), String()))
        If rows.Count > 0 Then
            Dim body(rows.Count - 1, headers.Length - 1) As Object
            For r As Integer = 0 To rows.Count - 1
                If rows(r).Length <> headers.Length Then Throw New InvalidOperationException("A result row does not match its table header.")
                For c As Integer = 0 To headers.Length - 1
                    body(r, c) = rows(r)(c)
                Next
            Next
            table.SetBody(body)
        ElseIf Not String.IsNullOrWhiteSpace(emptyMessage) Then
            table.AddFootnote(emptyMessage)
        End If
        If Not String.IsNullOrWhiteSpace(footnote) Then table.AddFootnote(footnote)
        Return table
    End Function

    Private Shared Function CreateVectorTable(title As String, names As String(), values As Double(), valueHeader As String) As ResultTable
        Dim rows As New List(Of Object())()
        If values IsNot Nothing Then
            For i As Integer = 0 To values.Length - 1
                rows.Add(Row(names(i), values(i)))
            Next
        End If
        Return CreateTable(title, {"Variable", valueHeader}, rows, emptyMessage:="Not applicable to this chart.")
    End Function

    Private Shared Function CreateIndexedVectorTable(title As String, values As Double(), indexHeader As String, valueHeader As String) As ResultTable
        Dim rows As New List(Of Object())()
        For i As Integer = 0 To values.Length - 1
            rows.Add(Row(i + 1, values(i)))
        Next
        Return CreateTable(title, {indexHeader, valueHeader}, rows)
    End Function

    Private Shared Function CreateMatrixTable(title As String, rowNames As String(), columnNames As String(), values As Double(,)) As ResultTable
        Dim headers As New List(Of String) From {"Variable"}
        headers.AddRange(columnNames)
        Dim rows As New List(Of Object())()
        For r As Integer = 0 To values.GetLength(0) - 1
            Dim row(values.GetLength(1)) As Object
            row(0) = rowNames(r)
            For c As Integer = 0 To values.GetLength(1) - 1
                row(c + 1) = values(r, c)
            Next
            rows.Add(row)
        Next
        Return CreateTable(title, headers.ToArray(), rows)
    End Function

    Private Shared Function ComponentNames(count As Integer) As String()
        Dim names(count - 1) As String
        For i As Integer = 0 To count - 1
            names(i) = "PC" & (i + 1).ToString(CultureInfo.InvariantCulture)
        Next
        Return names
    End Function

    Private Shared Sub AddVectorValues(target As List(Of Object), values As Double(), count As Integer)
        For i As Integer = 0 To count - 1
            If values IsNot Nothing AndAlso i < values.Length Then
                target.Add(values(i))
            Else
                target.Add(String.Empty)
            End If
        Next
    End Sub

    Private Shared Function Row(ParamArray values As Object()) As Object()
        Return values
    End Function

#End Region

#Region "Parsing and display helpers"

    Private Shared Function GridRowIsBlank(row As DataGridViewRow) As Boolean
        For Each cell As DataGridViewCell In row.Cells
            If cell.Value IsNot Nothing AndAlso Convert.ToString(cell.Value, CultureInfo.CurrentCulture).Trim().Length > 0 Then Return False
        Next
        Return True
    End Function

    Private Shared Function CellText(row As DataGridViewRow, columnIndex As Integer) As String
        Dim value As Object = row.Cells(columnIndex).Value
        Return If(value Is Nothing, String.Empty, Convert.ToString(value, CultureInfo.CurrentCulture).Trim())
    End Function

    Private Shared Function RequiredIntegerCell(row As DataGridViewRow, columnIndex As Integer, label As String) As Integer
        Dim value As Integer
        If Not Integer.TryParse(CellText(row, columnIndex), NumberStyles.Integer, CultureInfo.CurrentCulture, value) Then
            Throw New ArgumentException(label & " must be an integer.")
        End If
        Return value
    End Function

    Private Shared Function RequiredDoubleCell(row As DataGridViewRow, columnIndex As Integer, label As String) As Double
        Dim value As Double
        If Not TryParseDouble(CellText(row, columnIndex), value) Then Throw New ArgumentException(label & " must be numeric.")
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Throw New ArgumentException(label & " must be finite.")
        Return value
    End Function

    Private Shared Function TryParseDouble(text As String, ByRef value As Double) As Boolean
        If Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, value) Then Return True
        Return Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, value)
    End Function

    Private Shared Function ParsePhaseText(text As String) As SpcPhase
        Dim normalized As String = text.Trim().Replace(" ", String.Empty).ToUpperInvariant()
        If normalized = "PHASEI" OrElse normalized = "I" OrElse normalized = "1" Then Return SpcPhase.PhaseI
        If normalized = "PHASEII" OrElse normalized = "II" OrElse normalized = "2" Then Return SpcPhase.PhaseII
        Throw New ArgumentException("Phase must be 'Phase I' or 'Phase II'.")
    End Function

    Private Shared Function ParseCollapsedPhase(values As String(), pointIndices As Integer(), point As Integer) As SpcPhase
        Dim found As Nullable(Of SpcPhase) = Nothing
        For i As Integer = 0 To values.Length - 1
            If pointIndices(i) <> point Then Continue For
            Dim current As SpcPhase = ParsePhaseText(values(i))
            If found.HasValue AndAlso found.Value <> current Then
                Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent phase values.")
            End If
            found = current
        Next
        If Not found.HasValue Then Throw New ArgumentException("No phase value is available for chart point " & (point + 1).ToString() & ".")
        Return found.Value
    End Function

    Private Shared Function CollapsedTextValue(values As String(), pointIndices As Integer(), point As Integer, label As String) As String
        Dim found As String = Nothing
        For i As Integer = 0 To values.Length - 1
            If pointIndices(i) <> point Then Continue For
            Dim current As String = values(i).Trim()
            If found Is Nothing Then
                found = current
            ElseIf Not String.Equals(found, current, StringComparison.Ordinal) Then
                Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent " & label & " values.")
            End If
        Next
        Return If(found, String.Empty)
    End Function

    Private Shared Function ParseExclusionScope(text As String) As SpcExclusionScope
        Select Case text.Trim().ToLowerInvariant()
            Case "parameter estimation" : Return SpcExclusionScope.ParameterEstimation
            Case "signal evaluation", "rule evaluation" : Return SpcExclusionScope.RuleEvaluation
            Case "parameter estimation and signal evaluation", "parameter estimation and rules" : Return SpcExclusionScope.EstimationAndRules
            Case Else : Throw New ArgumentException("Select a valid exclusion scope.")
        End Select
    End Function

    Private Shared Function ExclusionScopeText(scope As SpcExclusionScope) As String
        Select Case scope
            Case SpcExclusionScope.None : Return "None"
            Case SpcExclusionScope.ParameterEstimation : Return "Parameter estimation"
            Case SpcExclusionScope.RuleEvaluation : Return "Signal evaluation"
            Case SpcExclusionScope.EstimationAndRules : Return "Parameter estimation and signal evaluation"
            Case Else : Return scope.ToString()
        End Select
    End Function

    Private Shared Function IsExclusionIndicator(value As Object) As Boolean
        If value Is Nothing Then Return False
        If TypeOf value Is Boolean Then Return DirectCast(value, Boolean)
        Dim numeric As Double
        If TryParseDouble(Convert.ToString(value, CultureInfo.CurrentCulture), numeric) Then Return numeric <> 0R
        Select Case Convert.ToString(value, CultureInfo.CurrentCulture).Trim().ToLowerInvariant()
            Case "true", "yes", "y", "x", "exclude", "excluded" : Return True
            Case "false", "no", "n", "include", "included", "" : Return False
            Case Else : Throw New ArgumentException("Exclusion indicators must be blank/0/No or nonzero/Yes/True/X.")
        End Select
    End Function

    Private Shared Function PhaseText(value As SpcPhase) As String
        Return If(value = SpcPhase.PhaseI, "Phase I", "Phase II")
    End Function

    Private Shared Function MissingPolicyText(value As SpcMissingValuePolicy) As String
        Return If(value = SpcMissingValuePolicy.Reject, "Reject", "Omit incomplete observation")
    End Function

    Private Shared Function ModelSourceText(value As SpcMultivariateModelSource) As String
        Return If(value = SpcMultivariateModelSource.EstimateFromPhaseI, "Estimate from Phase I", "Use historical parameters")
    End Function

    Private Shared Function SequenceGapBehaviorText(value As SpcSequenceGapBehavior) As String
        Select Case value
            Case SpcSequenceGapBehavior.BreakSequence : Return "Break sequence"
            Case SpcSequenceGapBehavior.SkipPointAndContinue : Return "Skip point and continue"
            Case Else : Return value.ToString()
        End Select
    End Function

    Private Shared Function ChartTypeText(value As SpcChartType) As String
        Select Case value
            Case SpcChartType.HotellingT2 : Return "Hotelling T-squared"
            Case SpcChartType.GeneralizedVariance : Return "Generalized variance"
            Case SpcChartType.PcaT2 : Return "PCA T-squared"
            Case SpcChartType.PcaQ : Return "PCA Q"
            Case SpcChartType.Mewma : Return "MEWMA"
            Case SpcChartType.Mcusum : Return "MCUSUM (Crosier)"
            Case Else : Return value.ToString()
        End Select
    End Function

    Private Shared Function YesNo(value As Boolean) As String
        Return If(value, "Yes", "No")
    End Function

    Private Shared Function NullableValue(value As Nullable(Of Double)) As Object
        Return If(value.HasValue, CType(value.Value, Object), String.Empty)
    End Function

    Private Shared Function NullableValue(value As Nullable(Of Integer)) As Object
        Return If(value.HasValue, CType(value.Value, Object), String.Empty)
    End Function

    Private Shared Function JoinIntegers(values As Integer(), addOne As Boolean) As String
        If values Is Nothing OrElse values.Length = 0 Then Return String.Empty
        Dim text(values.Length - 1) As String
        For i As Integer = 0 To values.Length - 1
            Dim value As Integer = values(i)
            If addOne Then value += 1
            text(i) = value.ToString(CultureInfo.InvariantCulture)
        Next
        Return String.Join(", ", text)
    End Function

    Private Shared Function MakeUniqueWorksheetName(workbook As Excel.Workbook,
                                                    baseName As String,
                                                    reusableSheet As Excel.Worksheet) As String
        Dim cleaned As String = CleanWorksheetName(baseName)
        Dim candidate As String = cleaned
        Dim suffix As Integer = 1
        While WorksheetNameExists(workbook, candidate, reusableSheet)
            suffix += 1
            Dim suffixText As String = " (" & suffix.ToString(CultureInfo.InvariantCulture) & ")"
            candidate = cleaned.Substring(0, Math.Min(cleaned.Length, 31 - suffixText.Length)) & suffixText
        End While
        Return candidate
    End Function

    Private Shared Function CleanWorksheetName(value As String) As String
        Dim cleaned As String = If(value, String.Empty).Trim()
        For Each invalid As Char In New Char() {":"c, "\"c, "/"c, "?"c, "*"c, "["c, "]"c}
            cleaned = cleaned.Replace(invalid, " "c)
        Next
        cleaned = cleaned.Trim("'"c, " "c)
        If cleaned.Length = 0 Then cleaned = "Multivariate SPC"
        If cleaned.Length > 31 Then cleaned = cleaned.Substring(0, 31)
        Return cleaned
    End Function

    Private Shared Function WorksheetNameExists(workbook As Excel.Workbook,
                                                name As String,
                                                reusableSheet As Excel.Worksheet) As Boolean
        For Each sheetObject As Object In workbook.Worksheets
            Dim sheet As Excel.Worksheet = TryCast(sheetObject, Excel.Worksheet)
            If sheet Is Nothing OrElse sheet Is reusableSheet Then Continue For
            If String.Equals(sheet.Name, name, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    Private Shared Sub ShowInputError(message As String)
        MessageBox.Show(message, AppGlobals.gsAPP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
    End Sub

#End Region

End Class
