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

Public Class Ui21ControlCharts

    Private Const MinimumFormWidth As Integer = 900
    Private Const MinimumFormHeight As Integer = 740
    Private Const NoneColumnText As String = "(none)"

    Private pWorksheet As Excel.Worksheet
    Private pWorkbook As Excel.Workbook
    Private pColumnInfo As New Dictionary(Of String, VarColumnInfo)(StringComparer.Ordinal)
    Private pCancelRequested As Boolean
    Private pBusy As Boolean
    Private pSuppressUiEvents As Boolean
    Private pLastShewhartRulePreset As SpcRulePreset = SpcRulePreset.RuleOneOnly
    Private pSavedCustomRules As SpcRuleDefinition() = Array.Empty(Of SpcRuleDefinition)()

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
                       family As SpcChartFamily,
                       description As String,
                       requirements As String)
            Me.DisplayText = displayText
            Me.ChartType = chartType
            Me.Family = family
            Me.Description = description
            Me.Requirements = requirements
        End Sub

        Public ReadOnly Property DisplayText As String
        Public ReadOnly Property ChartType As SpcChartType
        Public ReadOnly Property Family As SpcChartFamily
        Public ReadOnly Property Description As String
        Public ReadOnly Property Requirements As String

        Public Overrides Function ToString() As String
            Return DisplayText
        End Function
    End Class

    Private NotInheritable Class InputRowContext
        Public Property FirstRow As Integer
        Public Property LastRow As Integer
        Public Property Layout As SpcDataLayout
        Public Property PointIndexByRow As Integer()
        Public Property PointCount As Integer

        Public ReadOnly Property RowCount As Integer
            Get
                Return LastRow - FirstRow + 1
            End Get
        End Property
    End Class

    Private Shared ReadOnly ChartChoices As ChartChoice() = {
        New ChartChoice("Individuals", SpcChartType.Individuals, SpcChartFamily.ShewhartVariables,
                        "Plots one ordered measurement per sample. Use it when rational subgroups are not available.",
                        "Select exactly one measurements/values column. Sample label and sequence/date/time are optional."),
        New ChartChoice("Moving Range", SpcChartType.MovingRange, SpcChartFamily.ShewhartVariables,
                        "Plots moving ranges between ordered individual observations to monitor short-term variation.",
                        "Select exactly one ordered measurements/values column."),
        New ChartChoice("Individuals–Moving Range", SpcChartType.IndividualsMovingRange, SpcChartFamily.ShewhartVariables,
                        "Creates aligned Individuals and Moving Range panels from one ordered measurement series.",
                        "Select exactly one ordered measurements/values column."),
        New ChartChoice("X-bar", SpcChartType.XBar, SpcChartFamily.ShewhartVariables,
                        "Plots subgroup means to monitor changes in process location.",
                        "For subgroups across rows, select at least two measurement columns. For stacked observations, select one values column and one subgroup-ID column."),
        New ChartChoice("Range", SpcChartType.SubgroupRange, SpcChartFamily.ShewhartVariables,
                        "Plots subgroup ranges to monitor within-subgroup dispersion.",
                        "For subgroups across rows, select at least two measurement columns. For stacked observations, select one values column and one subgroup-ID column."),
        New ChartChoice("Standard Deviation", SpcChartType.SubgroupStandardDeviation, SpcChartFamily.ShewhartVariables,
                        "Plots subgroup standard deviations to monitor within-subgroup dispersion.",
                        "For subgroups across rows, select at least two measurement columns. For stacked observations, select one values column and one subgroup-ID column."),
        New ChartChoice("X-bar–R", SpcChartType.XBarR, SpcChartFamily.ShewhartVariables,
                        "Creates aligned subgroup-mean and subgroup-range panels.",
                        "For subgroups across rows, select at least two measurement columns. For stacked observations, select one values column and one subgroup-ID column."),
        New ChartChoice("X-bar–S", SpcChartType.XBarS, SpcChartFamily.ShewhartVariables,
                        "Creates aligned subgroup-mean and subgroup-standard-deviation panels.",
                        "For subgroups across rows, select at least two measurement columns. For stacked observations, select one values column and one subgroup-ID column."),
        New ChartChoice("p", SpcChartType.PChart, SpcChartFamily.ShewhartAttributes,
                        "Plots the nonconforming proportion; sample size may vary between samples.",
                        "Select one nonconforming-count column and one sample-size column."),
        New ChartChoice("np", SpcChartType.NpChart, SpcChartFamily.ShewhartAttributes,
                        "Plots the number of nonconforming items. Sample size must be constant.",
                        "Select one nonconforming-count column and one sample-size column."),
        New ChartChoice("c", SpcChartType.CChart, SpcChartFamily.ShewhartAttributes,
                        "Plots defect counts when the opportunity or inspection area is constant.",
                        "Select exactly one defect-count column."),
        New ChartChoice("u", SpcChartType.UChart, SpcChartFamily.ShewhartAttributes,
                        "Plots defects per unit when exposure or opportunity varies.",
                        "Select one defect-count column and one exposure/opportunities column."),
        New ChartChoice("CUSUM", SpcChartType.Cusum, SpcChartFamily.TimeWeighted,
                        "Uses standardized cumulative sums to detect sustained small shifts in process location.",
                        "Select exactly one ordered measurements/values column."),
        New ChartChoice("EWMA", SpcChartType.Ewma, SpcChartFamily.TimeWeighted,
                        "Uses an exponentially weighted moving average to emphasize recent observations.",
                        "Select exactly one ordered measurements/values column."),
        New ChartChoice("Moving Average", SpcChartType.MovingAverage, SpcChartFamily.TimeWeighted,
                        "Plots a rolling average of consecutive individual observations.",
                        "Select exactly one ordered measurements/values column.")
    }

    Public Sub New(analysis As String, tagn As Integer)
        InitializeComponent()
        Me.Text = analysis
        Me.Tag = tagn

        ConfigureResponsiveLayout()
        ConfigureGridColumns()
        InitializeOptionControls()
        WireRoleButtons()
        Me.WireHelp(Me.btnHelp)
    End Sub

#Region "Initialization and responsive layout"

    Private Sub InitializeOptionControls()
        pSuppressUiEvents = True
        Try
            FillCombo(cbChartFamily,
                      New ComboItem(Of SpcChartFamily)("Shewhart — Variables", SpcChartFamily.ShewhartVariables),
                      New ComboItem(Of SpcChartFamily)("Shewhart — Attributes", SpcChartFamily.ShewhartAttributes),
                      New ComboItem(Of SpcChartFamily)("Time-weighted", SpcChartFamily.TimeWeighted))

            FillCombo(cbMissingValuePolicy,
                      New ComboItem(Of SpcMissingValuePolicy)("Reject", SpcMissingValuePolicy.Reject),
                      New ComboItem(Of SpcMissingValuePolicy)("Omit complete point/subgroup", SpcMissingValuePolicy.OmitPoint),
                      New ComboItem(Of SpcMissingValuePolicy)("Use available measurements", SpcMissingValuePolicy.UseAvailableMeasurements))

            FillCombo(cbParameterSource,
                      New ComboItem(Of SpcParameterSource)("Estimate from Phase I", SpcParameterSource.EstimateFromPhaseI),
                      New ComboItem(Of SpcParameterSource)("Use historical parameters", SpcParameterSource.UseHistoricalParameters),
                      New ComboItem(Of SpcParameterSource)("Defined by stage", SpcParameterSource.DefinedByStage))

            FillCombo(cbControlLimitMethod,
                      New ComboItem(Of SpcControlLimitMethod)("Traditional sigma limits", SpcControlLimitMethod.ShewhartSigma),
                      New ComboItem(Of SpcControlLimitMethod)("Exact probability limits", SpcControlLimitMethod.ExactProbability))

            FillCombo(cbWithinSigmaEstimator,
                      New ComboItem(Of SpcWithinSigmaEstimator)("Automatic", SpcWithinSigmaEstimator.Automatic),
                      New ComboItem(Of SpcWithinSigmaEstimator)("Average range", SpcWithinSigmaEstimator.AverageRange),
                      New ComboItem(Of SpcWithinSigmaEstimator)("Average standard deviation", SpcWithinSigmaEstimator.AverageStandardDeviation),
                      New ComboItem(Of SpcWithinSigmaEstimator)("Pooled standard deviation", SpcWithinSigmaEstimator.PooledStandardDeviation),
                      New ComboItem(Of SpcWithinSigmaEstimator)("Moving range", SpcWithinSigmaEstimator.MovingRange),
                      New ComboItem(Of SpcWithinSigmaEstimator)("Median moving range", SpcWithinSigmaEstimator.MedianMovingRange),
                      New ComboItem(Of SpcWithinSigmaEstimator)("Sample standard deviation", SpcWithinSigmaEstimator.SampleStandardDeviation),
                      New ComboItem(Of SpcWithinSigmaEstimator)("Median absolute deviation", SpcWithinSigmaEstimator.MedianAbsoluteDeviation))

            FillCombo(cbNaturalLimitPolicy,
                      New ComboItem(Of SpcNaturalLimitPolicy)("Clip to feasible range", SpcNaturalLimitPolicy.ClipToFeasibleRange),
                      New ComboItem(Of SpcNaturalLimitPolicy)("Retain calculated limits", SpcNaturalLimitPolicy.RetainCalculatedLimits))

            FillCombo(cbRulePreset,
                      New ComboItem(Of SpcRulePreset)("None", SpcRulePreset.None),
                      New ComboItem(Of SpcRulePreset)("Rule 1 only", SpcRulePreset.RuleOneOnly),
                      New ComboItem(Of SpcRulePreset)("Western Electric rules 1–4", SpcRulePreset.WesternElectric),
                      New ComboItem(Of SpcRulePreset)("Nelson rules 1–8", SpcRulePreset.Nelson),
                      New ComboItem(Of SpcRulePreset)("Paper/Montgomery eight rules", SpcRulePreset.PaperMontgomeryEightRules),
                      New ComboItem(Of SpcRulePreset)("Custom", SpcRulePreset.Custom))

            FillCombo(cbRulePhaseScope,
                      New ComboItem(Of SpcRulePhaseScope)("All", SpcRulePhaseScope.All),
                      New ComboItem(Of SpcRulePhaseScope)("Phase I only", SpcRulePhaseScope.PhaseI),
                      New ComboItem(Of SpcRulePhaseScope)("Phase II only", SpcRulePhaseScope.PhaseII),
                      New ComboItem(Of SpcRulePhaseScope)("None", SpcRulePhaseScope.None))

            FillCombo(cbSequenceGapBehavior,
                      New ComboItem(Of SpcSequenceGapBehavior)("Break sequence", SpcSequenceGapBehavior.BreakSequence),
                      New ComboItem(Of SpcSequenceGapBehavior)("Skip point and continue", SpcSequenceGapBehavior.SkipPointAndContinue))

            FillCombo(cbSignalMarkingMode,
                      New ComboItem(Of SpcSignalMarkingMode)("Terminal point only", SpcSignalMarkingMode.TerminalPointOnly),
                      New ComboItem(Of SpcSignalMarkingMode)("Entire pattern", SpcSignalMarkingMode.EntirePattern))

            FillCombo(cbImportedExclusionScope,
                      New ComboItem(Of SpcExclusionScope)("Parameter estimation and rules", SpcExclusionScope.EstimationAndRules),
                      New ComboItem(Of SpcExclusionScope)("Parameter estimation", SpcExclusionScope.ParameterEstimation),
                      New ComboItem(Of SpcExclusionScope)("Rule evaluation", SpcExclusionScope.RuleEvaluation))

            FillCombo(cbHorizontalTickOrientation,
                      New ComboItem(Of Integer)("0°", 0),
                      New ComboItem(Of Integer)("45°", 45),
                      New ComboItem(Of Integer)("90°", 90))

            FillCombo(cbZoneDisplay,
                      New ComboItem(Of graphics.SpcZoneDisplayMode)("None", graphics.SpcZoneDisplayMode.None),
                      New ComboItem(Of graphics.SpcZoneDisplayMode)("Lines", graphics.SpcZoneDisplayMode.Lines),
                      New ComboItem(Of graphics.SpcZoneDisplayMode)("Shaded bands", graphics.SpcZoneDisplayMode.ShadedBands))
            SelectComboValue(cbZoneDisplay, graphics.SpcZoneDisplayMode.Lines)

            spinSigmaMultiplier.DecimalPlaces = 2
            spinSigmaMultiplier.Increment = 0.1D
            spinSigmaMultiplier.Minimum = 0.1D
            spinSigmaMultiplier.Maximum = 10D
            spinSigmaMultiplier.Value = 3D

            spinMovingRangeLength.Minimum = 2D
            spinMovingRangeLength.Maximum = 25D
            spinMovingRangeLength.Value = 2D

            spinEwmaLambda.DecimalPlaces = 2
            spinEwmaLambda.Increment = 0.05D
            spinEwmaLambda.Minimum = 0.01D
            spinEwmaLambda.Maximum = 1D
            spinEwmaLambda.Value = 0.2D

            spinCusumReferenceValue.DecimalPlaces = 2
            spinCusumReferenceValue.Increment = 0.1D
            spinCusumReferenceValue.Minimum = 0D
            spinCusumReferenceValue.Maximum = 25D
            spinCusumReferenceValue.Value = 0.5D

            spinCusumDecisionInterval.DecimalPlaces = 2
            spinCusumDecisionInterval.Increment = 0.1D
            spinCusumDecisionInterval.Minimum = 0.1D
            spinCusumDecisionInterval.Maximum = 100D
            spinCusumDecisionInterval.Value = 5D

            spinHeadStart.DecimalPlaces = 2
            spinHeadStart.Increment = 0.1D
            spinHeadStart.Minimum = 0D
            spinHeadStart.Maximum = 100D
            spinHeadStart.Value = 0D

            spinMovingAverageSpan.Minimum = 2D
            spinMovingAverageSpan.Maximum = 1000D
            spinMovingAverageSpan.Value = 3D
            spinLastPhaseIPoint.Minimum = 1D
            spinLastPhaseIPoint.Maximum = 1000000D

            ComboBox1.Visible = False
            Label1.Visible = False
            grpGeneralOptions.Visible = True
            tbLowerSpecificationLimit.Clear()
            tbTarget.Clear()
            tbUpperSpecificationLimit.Clear()
            lblLowerSpecificationLimit.Text = "Lower specification limit (LSL)"
            lblTarget.Text = "Target"
            lblUpperSpecificationLimit.Text = "Upper specification limit (USL)"
            chkShowSpecificationLimits.Checked = False
            chkShowTargetLine.Checked = False
            chkUseSequenceValuesForHorizontalAxis.Checked = False
            chkShowHorizontalAxisOnEveryPanel.Checked = False
            tbHorizontalAxisTitle.Text = "Sample"
            tbValueNumberFormat.Text = "0.####"
            rbSinglePhaseI.Checked = True
            btnInterrupt.Enabled = False
            ProgressBar.Minimum = 0
            ProgressBar.Maximum = 100
            ProgressBar.Value = 0

            SelectComboValue(cbChartFamily, SpcChartFamily.ShewhartVariables)
            PopulateChartTypes(SpcChartFamily.ShewhartVariables, SpcChartType.Individuals)
            SelectComboValue(cbRulePreset, SpcRulePreset.RuleOneOnly)
        Finally
            pSuppressUiEvents = False
        End Try

        UpdateChartDependentControls()
        LoadSelectedRulePreset()
        UpdateQuickPhaseControls()
        UpdateSpecificationControls()
    End Sub

    Private Sub ConfigureGridColumns()
        dgvHistoricalParameters.AllowUserToAddRows = False
        dgvHistoricalParameters.AllowUserToDeleteRows = True
        dgvHistoricalParameters.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        colHistoryStageID.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

        dgvStages.AllowUserToAddRows = True
        dgvStages.AllowUserToDeleteRows = True
        dgvStages.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        colStageDisplayName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colStageReferenceID.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colStagePhase.Items.Clear()
        colStagePhase.Items.AddRange("Phase I", "Phase II")
        colStageLimitMode.Items.Clear()
        colStageLimitMode.Items.AddRange("Estimate from stage data", "Use reference stage", "Use historical parameters")
        colStageReferenceID.Items.Clear()
        colStageReferenceID.Items.Add(String.Empty)

        dgvExclusions.AllowUserToAddRows = False
        dgvExclusions.AllowUserToDeleteRows = True
        colExclusionReason.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colExclusionScope.Items.Clear()
        colExclusionScope.Items.AddRange("Parameter estimation", "Rule evaluation", "Parameter estimation and rules")

        dgvRules.AllowUserToAddRows = False
        dgvRules.AllowUserToDeleteRows = False
        colRuleName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colRuleKind.Items.Clear()
        colRuleKind.Items.AddRange("Beyond sigma", "K of M beyond sigma", "Run on one side", "Monotonic trend",
                                   "Alternating", "All within sigma", "All beyond sigma on both sides")
        colRuleSide.Items.Clear()
        colRuleSide.Items.AddRange("Either side", "Upper side only", "Lower side only")
        colRuleScope.Items.Clear()
        colRuleScope.Items.AddRange("Location panels", "Dispersion panels", "Attribute panels", "Time-weighted panels",
                                    "Location and attribute panels", "All Shewhart panels", "All panels")
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
        ApplyResponsiveLayout()
    End Sub

    Private Sub ResponsiveLayoutChanged(sender As Object, e As System.EventArgs)
        ApplyResponsiveLayout()
    End Sub

    Private Sub ApplyResponsiveLayout()
        If TabControl1 Is Nothing Then Return

        LayoutChartDataTab()
        LayoutParametersTab()
        LayoutPhasesTab()
        LayoutRulesTab()
        LayoutOutputTab()
    End Sub

    Private Shared Function InnerWidth(page As TabPage) As Integer
        Return Math.Max(763, page.ClientSize.Width - 14)
    End Function

    Private Sub LayoutChartDataTab()
        Dim width As Integer = InnerWidth(TabPage1_ChartData)
        grpChartSelection.Width = width
        grpWorksheet.Width = width

        Dim descriptionX As Integer = Math.Max(395, CInt(Math.Floor(width * 0.51R)))
        cbChartFamily.Width = Math.Max(239, descriptionX - cbChartFamily.Left - 3)
        cbChartType.Width = cbChartFamily.Width
        cbDataLayout.Width = cbChartFamily.Width
        lblChartDescription.Left = descriptionX
        lblChartDescription.Width = Math.Max(250, width - descriptionX - 6)

        Dim rightX As Integer = Math.Max(495, CInt(Math.Floor(width * 0.62R)))
        Dim addX As Integer = rightX - 90
        Dim removeX As Integer = rightX - 45
        Dim leftWidth As Integer = Math.Max(180, addX - lbAllColumns.Left - 12)
        Dim rightWidth As Integer = Math.Max(220, width - rightX - 5)

        lbAllColumns.Width = leftWidth
        lblDataRequirements.Left = lbAllColumns.Left
        lblDataRequirements.Width = Math.Max(300, width - lblDataRequirements.Left - 5)

        btReload.Left = lbAllColumns.Left + lbAllColumns.Width + 5
        btClearDataRoles.Left = width - btClearDataRoles.Width - 6

        Dim addButtons As Button() = {btAddValues, btAddSubgroupID, btAddCounts, btAddSampleSize, btAddExposure, btAddLabels, btAddSequence}
        Dim removeButtons As Button() = {btRemoveValues, btRemoveSubgroupID, btRemoveCounts, btRemoveSampleSize, btRemoveExposure, btRemoveLabels, btRemoveSequence}
        For Each control As Button In addButtons
            control.Left = addX
        Next
        For Each control As Button In removeButtons
            control.Left = removeX
        Next

        Dim roleLists As ListBox() = {lbValues, lbSubgroupID, lbCounts, lbSampleSize, lbExposure, lbLabels, lbSequence}
        Dim roleLabels As Label() = {lblValues, lblSubgroupID, lblCount, lblSampleSize, lblExposure, lblLabels, lblSequence}
        For Each control As ListBox In roleLists
            control.Left = rightX
            control.Width = rightWidth
        Next
        For Each control As Label In roleLabels
            control.Left = rightX
        Next

        'Use the controls' actual scaled heights rather than fixed Y coordinates.
        'The latter caused labels to overlap the ListBox borders at 125%/150% DPI.
        Dim labelControlGap As Integer = Math.Max(4, CInt(Math.Ceiling(lblValues.Height * 0.25R)))
        Dim rowGap As Integer = Math.Max(6, CInt(Math.Ceiling(lblValues.Height * 0.35R)))
        Dim requirementGap As Integer = Math.Max(8, CInt(Math.Ceiling(lblValues.Height * 0.5R)))
        Dim bottomMargin As Integer = requirementGap

        lbValues.Top = lblValues.Bottom + labelControlGap
        AlignRoleButtons(lbValues, btAddValues, btRemoveValues)

        Dim lowerLabels As Label() = {lblSubgroupID, lblCount, lblSampleSize, lblExposure, lblLabels, lblSequence}
        Dim lowerLists As ListBox() = {lbSubgroupID, lbCounts, lbSampleSize, lbExposure, lbLabels, lbSequence}
        Dim lowerAddButtons As Button() = {btAddSubgroupID, btAddCounts, btAddSampleSize, btAddExposure, btAddLabels, btAddSequence}
        Dim lowerRemoveButtons As Button() = {btRemoveSubgroupID, btRemoveCounts, btRemoveSampleSize, btRemoveExposure, btRemoveLabels, btRemoveSequence}

        Dim minimumValuesHeight As Integer = lbValues.ItemHeight * 4 + 4
        Dim tailHeight As Integer = rowGap + requirementGap + lblDataRequirements.Height + bottomMargin
        For i As Integer = 0 To lowerLabels.Length - 1
            tailHeight += lowerLabels(i).Height + labelControlGap + lowerLists(i).Height
            If i < lowerLabels.Length - 1 Then tailHeight += rowGap
        Next

        Dim nonClientHeight As Integer = grpWorksheet.Height - grpWorksheet.ClientSize.Height
        Dim requiredGroupHeight As Integer = lbValues.Top + minimumValuesHeight + tailHeight + nonClientHeight
        Dim availableGroupHeight As Integer = TabPage1_ChartData.ClientSize.Height - grpWorksheet.Top - 8
        grpWorksheet.Height = Math.Max(requiredGroupHeight, availableGroupHeight)

        lbValues.Height = Math.Max(minimumValuesHeight,
                                   grpWorksheet.ClientSize.Height - lbValues.Top - tailHeight)
        AlignRoleButtons(lbValues, btAddValues, btRemoveValues)

        Dim nextTop As Integer = lbValues.Bottom + rowGap
        For i As Integer = 0 To lowerLabels.Length - 1
            lowerLabels(i).Top = nextTop
            lowerLists(i).Top = lowerLabels(i).Bottom + labelControlGap
            AlignRoleButtons(lowerLists(i), lowerAddButtons(i), lowerRemoveButtons(i))
            nextTop = lowerLists(i).Bottom + rowGap
        Next

        lblDataRequirements.Top = nextTop - rowGap + requirementGap
        lbAllColumns.Height = Math.Max(100, lblDataRequirements.Top - lbAllColumns.Top - requirementGap)

        TabPage1_ChartData.AutoScrollMinSize = New Size(790, grpWorksheet.Bottom + 8)
    End Sub

    Private Shared Sub AlignRoleButtons(list As ListBox,
                                       addButton As Button,
                                       removeButton As Button)
        Dim buttonTop As Integer = list.Top + CInt(Math.Floor((list.Height - addButton.Height) / 2.0R))
        addButton.Top = buttonTop
        removeButton.Top = buttonTop
    End Sub

    Private Sub LayoutParametersTab()
        Dim width As Integer = InnerWidth(TabPage2_ParametersLimits)
        grpGeneralOptions.Width = width
        grpHistoricalParameters.Width = width
        grpTimeWeightedParameters.Width = width

        Dim combos As ComboBox() = {cbMissingValuePolicy, cbParameterSource, cbControlLimitMethod,
                                    cbWithinSigmaEstimator, cbNaturalLimitPolicy}
        For Each control As ComboBox In combos
            control.Width = Math.Max(239, width - control.Left - 15)
        Next

        Dim nextY As Integer = grpGeneralOptions.Bottom + 6
        Dim timeHeight As Integer = If(grpTimeWeightedParameters.Visible, 165, 0)
        If grpHistoricalParameters.Visible Then
            Dim available As Integer = TabPage2_ParametersLimits.ClientSize.Height - nextY - 8
            If timeHeight > 0 Then available -= timeHeight + 6
            grpHistoricalParameters.Top = nextY
            grpHistoricalParameters.Height = Math.Max(180, available)
            dgvHistoricalParameters.Width = width - 15
            dgvHistoricalParameters.Height = Math.Max(120, grpHistoricalParameters.Height - 56)
            nextY = grpHistoricalParameters.Bottom + 6
        End If

        If grpTimeWeightedParameters.Visible Then
            grpTimeWeightedParameters.Top = nextY
            grpTimeWeightedParameters.Height = timeHeight
            lblTimeWeightedNote.Width = Math.Max(280, width - lblTimeWeightedNote.Left - 10)
            nextY = grpTimeWeightedParameters.Bottom + 6
        End If

        TabPage2_ParametersLimits.AutoScrollMinSize = New Size(790, Math.Max(grpGeneralOptions.Bottom + 8, nextY))
    End Sub

    Private Sub LayoutPhasesTab()
        Dim width As Integer = InnerWidth(TabPage3_PhasesExclusions)
        grpPhaseColumns.Width = width
        grpQuickPhaseSetup.Width = width
        dgvStages.Width = width
        grpExclusions.Width = width

        Dim sourceCombos As ComboBox() = {cbStageColumn, cbPhaseColumn, cbExclusionColumn,
                                         cbExclusionReasonColumn, cbImportedExclusionScope}
        For Each control As ComboBox In sourceCombos
            control.Width = Math.Max(239, width - control.Left - 317)
        Next
        btImportStages.Left = width - btImportStages.Width - 79
        btImportExclusions.Left = width - btImportExclusions.Width - 79

        Dim remaining As Integer = Math.Max(360, TabPage3_PhasesExclusions.ClientSize.Height - dgvStages.Top - 12)
        Dim stageHeight As Integer = Math.Max(150, CInt(Math.Floor(remaining * 0.45R)))
        dgvStages.Height = stageHeight
        grpExclusions.Top = dgvStages.Bottom + 6
        grpExclusions.Height = Math.Max(180, TabPage3_PhasesExclusions.ClientSize.Height - grpExclusions.Top - 8)
        dgvExclusions.Width = width - 15
        dgvExclusions.Height = Math.Max(120, grpExclusions.Height - 60)

        TabPage3_PhasesExclusions.AutoScrollMinSize = New Size(790, grpExclusions.Bottom + 8)
    End Sub

    Private Sub LayoutRulesTab()
        Dim width As Integer = InnerWidth(TabPage4_SignalRules)
        grpRulePreset.Width = width
        grpSequenceOptions.Width = width

        btCopyPresetToCustom.Left = width - btCopyPresetToCustom.Width - 49
        btLoadRulePreset.Left = btCopyPresetToCustom.Left - btLoadRulePreset.Width - 16
        cbRulePreset.Width = Math.Max(220, btLoadRulePreset.Left - cbRulePreset.Left - 16)
        lblRulePresetDescription.Width = width - 16

        tbRuleDescription.Width = Math.Max(300, width - tbRuleDescription.Left)
        Dim sequenceTop As Integer = Math.Max(509, TabPage4_SignalRules.ClientSize.Height - grpSequenceOptions.Height - 8)
        dgvRules.Width = width
        dgvRules.Height = Math.Max(240, sequenceTop - dgvRules.Top - 6)
        grpSequenceOptions.Top = dgvRules.Bottom + 6
        lblRuleApplicability.Left = Math.Max(490, width - lblRuleApplicability.Width - 6)
        lblRuleApplicability.Width = Math.Max(220, width - lblRuleApplicability.Left - 6)
        cbRulePhaseScope.Width = Math.Max(220, lblRuleApplicability.Left - cbRulePhaseScope.Left - 4)
        cbSequenceGapBehavior.Width = cbRulePhaseScope.Width
        cbSignalMarkingMode.Width = cbRulePhaseScope.Width

        TabPage4_SignalRules.AutoScrollMinSize = New Size(790, grpSequenceOptions.Bottom + 8)
    End Sub

    Private Sub LayoutOutputTab()
        Dim width As Integer = InnerWidth(TabPage5_OutputAppearance)
        Dim groups As GroupBox() = {grpOutput, grpTitleAxes, grpChartDisplay, grpSpecifications, grpChartDimensions}
        For Each group As GroupBox In groups
            group.Width = width
        Next

        tbChartTitle.Width = Math.Max(240, CInt(Math.Floor(width * 0.42R)) - tbChartTitle.Left)
        Dim rightX As Integer = Math.Max(494, CInt(Math.Floor(width * 0.62R)))
        tbValueAxisTitle.Left = rightX
        tbHorizontalAxisTitle.Left = rightX
        tbValueNumberFormat.Left = rightX
        lblValueAxisTitle.Left = rightX - 124
        lblHorizontalAxisTitle.Left = rightX - 124
        lblValueNumberFormat.Left = rightX - 124
        tbValueAxisTitle.Width = Math.Max(200, width - rightX - 29)
        tbHorizontalAxisTitle.Width = tbValueAxisTitle.Width
        tbValueNumberFormat.Width = tbValueAxisTitle.Width

        tbLowerSpecificationLimit.Width = Math.Max(240, width - tbLowerSpecificationLimit.Left - 124)
        tbTarget.Width = tbLowerSpecificationLimit.Width
        tbUpperSpecificationLimit.Width = tbLowerSpecificationLimit.Width
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

#Region "Worksheet and data-role selection"

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
        Dim combos As ComboBox() = {cbStageColumn, cbPhaseColumn, cbExclusionColumn, cbExclusionReasonColumn}
        For Each combo As ComboBox In combos
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
            Finally
                combo.EndUpdate()
            End Try
        Next
    End Sub

    Private Sub WireRoleButtons()
        AddHandler btAddValues.Click, Sub() MoveSelectedColumns(lbValues, allowMany:=True)
        AddHandler btAddSubgroupID.Click, Sub() MoveSelectedColumns(lbSubgroupID, allowMany:=False)
        AddHandler btAddCounts.Click, Sub() MoveSelectedColumns(lbCounts, allowMany:=False)
        AddHandler btAddSampleSize.Click, Sub() MoveSelectedColumns(lbSampleSize, allowMany:=False)
        AddHandler btAddExposure.Click, Sub() MoveSelectedColumns(lbExposure, allowMany:=False)
        AddHandler btAddLabels.Click, Sub() MoveSelectedColumns(lbLabels, allowMany:=False)
        AddHandler btAddSequence.Click, Sub() MoveSelectedColumns(lbSequence, allowMany:=False)

        AddHandler btRemoveValues.Click, Sub() RemoveSelectedItems(lbValues)
        AddHandler btRemoveSubgroupID.Click, Sub() RemoveSelectedItems(lbSubgroupID)
        AddHandler btRemoveCounts.Click, Sub() RemoveSelectedItems(lbCounts)
        AddHandler btRemoveSampleSize.Click, Sub() RemoveSelectedItems(lbSampleSize)
        AddHandler btRemoveExposure.Click, Sub() RemoveSelectedItems(lbExposure)
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
            MessageBox.Show("Select exactly one available column for this role.", AppGlobals.gsAPP_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        For Each columnName As String In selected
            Dim assignedRole As String = FindAssignedRole(columnName, target)
            If assignedRole.Length > 0 Then
                MessageBox.Show("Column '" & columnName & "' is already assigned to " & assignedRole & ".",
                                AppGlobals.gsAPP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Continue For
            End If
            If Not allowMany Then target.Items.Clear()
            If Not target.Items.Contains(columnName) Then target.Items.Add(columnName)
        Next
    End Sub

    Private Function FindAssignedRole(columnName As String, target As ListBox) As String
        Dim roles As New List(Of KeyValuePair(Of String, ListBox)) From {
            New KeyValuePair(Of String, ListBox)("measurements/values", lbValues),
            New KeyValuePair(Of String, ListBox)("subgroup ID", lbSubgroupID),
            New KeyValuePair(Of String, ListBox)("count", lbCounts),
            New KeyValuePair(Of String, ListBox)("sample size", lbSampleSize),
            New KeyValuePair(Of String, ListBox)("exposure", lbExposure),
            New KeyValuePair(Of String, ListBox)("sample label", lbLabels),
            New KeyValuePair(Of String, ListBox)("sequence/date/time", lbSequence)
        }
        For Each role As KeyValuePair(Of String, ListBox) In roles
            If role.Value Is target Then Continue For
            If role.Value.Items.Contains(columnName) Then Return role.Key
        Next
        Return String.Empty
    End Function

    Private Shared Sub RemoveSelectedItems(list As ListBox)
        Dim selected As New List(Of Object)()
        For Each item As Object In list.SelectedItems
            selected.Add(item)
        Next
        For Each item As Object In selected
            list.Items.Remove(item)
        Next
    End Sub

    Private Sub ClearDataRoles()
        For Each list As ListBox In AllRoleLists()
            list.Items.Clear()
        Next
    End Sub

    Private Function AllRoleLists() As ListBox()
        Return {lbValues, lbSubgroupID, lbCounts, lbSampleSize, lbExposure, lbLabels, lbSequence}
    End Function

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

    Private Sub cbChartFamily_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbChartFamily.SelectedIndexChanged
        If pSuppressUiEvents OrElse cbChartFamily.SelectedItem Is Nothing Then Return
        PopulateChartTypes(SelectedComboValue(Of SpcChartFamily)(cbChartFamily), Nothing)
        UpdateChartDependentControls()
    End Sub

    Private Sub PopulateChartTypes(family As SpcChartFamily, preferred As Nullable(Of SpcChartType))
        pSuppressUiEvents = True
        Try
            cbChartType.BeginUpdate()
            cbChartType.Items.Clear()
            For Each choice As ChartChoice In ChartChoices
                If choice.Family = family Then cbChartType.Items.Add(choice)
            Next
            If preferred.HasValue Then
                For i As Integer = 0 To cbChartType.Items.Count - 1
                    Dim choice As ChartChoice = DirectCast(cbChartType.Items(i), ChartChoice)
                    If choice.ChartType = preferred.Value Then
                        cbChartType.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If
            If cbChartType.SelectedIndex < 0 AndAlso cbChartType.Items.Count > 0 Then cbChartType.SelectedIndex = 0
        Finally
            cbChartType.EndUpdate()
            pSuppressUiEvents = False
        End Try
    End Sub

    Private Sub cbChartType_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbChartType.SelectedIndexChanged
        If pSuppressUiEvents Then Return
        UpdateChartDependentControls()
    End Sub

    Private Function SelectedChartChoice() As ChartChoice
        Dim choice As ChartChoice = TryCast(cbChartType.SelectedItem, ChartChoice)
        If choice Is Nothing Then Throw New InvalidOperationException("Select a control-chart type.")
        Return choice
    End Function

    Private Function SelectedChartType() As SpcChartType
        Return SelectedChartChoice().ChartType
    End Function

    Private Sub UpdateChartDependentControls()
        If cbChartType.SelectedItem Is Nothing Then Return
        Dim choice As ChartChoice = SelectedChartChoice()
        lblChartDescription.Text = choice.Description
        lblDataRequirements.Text = choice.Requirements
        PopulateDataLayouts(choice.ChartType)
        UpdateRoleAvailability(choice.ChartType)
        UpdateLimitOptions(choice.ChartType)
        UpdateHistoricalColumns(choice.ChartType)
        UpdateTimeWeightedControls(choice.ChartType)
        UpdateRuleAvailability(choice.ChartType)
        UpdateSpecificationControls()
        UpdateParameterSourceVisibility()
        ApplyResponsiveLayout()
    End Sub

    Private Sub PopulateDataLayouts(chartType As SpcChartType)
        Dim previous As Nullable(Of SpcDataLayout) = Nothing
        Dim selected As ComboItem(Of SpcDataLayout) = TryCast(cbDataLayout.SelectedItem, ComboItem(Of SpcDataLayout))
        If selected IsNot Nothing Then previous = selected.Value

        pSuppressUiEvents = True
        Try
            cbDataLayout.Items.Clear()
            If IsSubgroupChart(chartType) Then
                cbDataLayout.Items.Add(New ComboItem(Of SpcDataLayout)("Subgroups across rows", SpcDataLayout.WideSubgroups))
                cbDataLayout.Items.Add(New ComboItem(Of SpcDataLayout)("Stacked observations", SpcDataLayout.StackedObservations))
            ElseIf IsAttributeChart(chartType) Then
                cbDataLayout.Items.Add(New ComboItem(Of SpcDataLayout)("Aggregated counts", SpcDataLayout.AggregatedCounts))
            Else
                cbDataLayout.Items.Add(New ComboItem(Of SpcDataLayout)("Individual sequence", SpcDataLayout.IndividualSequence))
            End If

            If previous.HasValue Then SelectComboValue(cbDataLayout, previous.Value)
            If cbDataLayout.SelectedIndex < 0 Then cbDataLayout.SelectedIndex = 0
        Finally
            pSuppressUiEvents = False
        End Try
    End Sub

    Private Sub cbDataLayout_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbDataLayout.SelectedIndexChanged
        If pSuppressUiEvents OrElse cbChartType.SelectedItem Is Nothing Then Return
        UpdateRoleAvailability(SelectedChartType())
    End Sub

    Private Sub UpdateRoleAvailability(chartType As SpcChartType)
        Dim layout As SpcDataLayout = SelectedComboValue(Of SpcDataLayout)(cbDataLayout)
        Dim valuesEnabled As Boolean = Not IsAttributeChart(chartType)
        Dim subgroupEnabled As Boolean = IsSubgroupChart(chartType) AndAlso layout = SpcDataLayout.StackedObservations
        Dim countEnabled As Boolean = IsAttributeChart(chartType)
        Dim sampleSizeEnabled As Boolean = chartType = SpcChartType.PChart OrElse chartType = SpcChartType.NpChart
        Dim exposureEnabled As Boolean = chartType = SpcChartType.UChart

        SetRoleEnabled(lbValues, lblValues, btAddValues, btRemoveValues, valuesEnabled)
        SetRoleEnabled(lbSubgroupID, lblSubgroupID, btAddSubgroupID, btRemoveSubgroupID, subgroupEnabled)
        SetRoleEnabled(lbCounts, lblCount, btAddCounts, btRemoveCounts, countEnabled)
        SetRoleEnabled(lbSampleSize, lblSampleSize, btAddSampleSize, btRemoveSampleSize, sampleSizeEnabled)
        SetRoleEnabled(lbExposure, lblExposure, btAddExposure, btRemoveExposure, exposureEnabled)
        SetRoleEnabled(lbLabels, lblLabels, btAddLabels, btRemoveLabels, True)
        SetRoleEnabled(lbSequence, lblSequence, btAddSequence, btRemoveSequence, True)

        lbValues.SelectionMode = If(layout = SpcDataLayout.WideSubgroups,
                                    SelectionMode.MultiExtended,
                                    SelectionMode.One)
    End Sub

    Private Shared Sub SetRoleEnabled(list As ListBox,
                                      label As Label,
                                      addButton As Button,
                                      removeButton As Button,
                                      enabled As Boolean)
        If Not enabled Then
            ' Disabled assignments are ignored by request construction. Remove
            ' them immediately so the form cannot display stale roles from the
            ' previously selected chart or data layout.
            list.Items.Clear()
        End If

        list.Enabled = enabled
        label.Enabled = enabled
        addButton.Enabled = enabled
        removeButton.Enabled = enabled
    End Sub

    Private Sub UpdateLimitOptions(chartType As SpcChartType)
        Dim attribute As Boolean = IsAttributeChart(chartType)
        cbControlLimitMethod.Enabled = attribute
        lblControlLimitMethod.Enabled = attribute
        If Not attribute Then SelectComboValue(cbControlLimitMethod, SpcControlLimitMethod.ShewhartSigma)

        Dim variableOrTime As Boolean = Not attribute
        cbWithinSigmaEstimator.Enabled = variableOrTime
        lblWithinSigmaEstimator.Enabled = variableOrTime
        spinMovingRangeLength.Enabled = variableOrTime
        lblMovingRangeLength.Enabled = variableOrTime
        chkUseBiasCorrection.Enabled = variableOrTime
    End Sub

    Private Sub UpdateHistoricalColumns(chartType As SpcChartType)
        Dim variables As Boolean = Not IsAttributeChart(chartType)
        colHistoryMean.Visible = variables
        colHistorySigma.Visible = variables
        colHistoryProportion.Visible = chartType = SpcChartType.PChart OrElse chartType = SpcChartType.NpChart
        colHistoryMeanCount.Visible = chartType = SpcChartType.CChart
        colHistoryMeanRate.Visible = chartType = SpcChartType.UChart
    End Sub

    Private Sub UpdateTimeWeightedControls(chartType As SpcChartType)
        Dim timeWeighted As Boolean = IsTimeWeightedChart(chartType)
        grpTimeWeightedParameters.Visible = timeWeighted
        spinEwmaLambda.Enabled = chartType = SpcChartType.Ewma
        lblEwmaLambda.Enabled = spinEwmaLambda.Enabled
        spinCusumReferenceValue.Enabled = chartType = SpcChartType.Cusum
        lblCusumReferenceValue.Enabled = spinCusumReferenceValue.Enabled
        spinCusumDecisionInterval.Enabled = chartType = SpcChartType.Cusum
        lblCusumDecisionInterval.Enabled = spinCusumDecisionInterval.Enabled
        spinHeadStart.Enabled = chartType = SpcChartType.Cusum
        lblHeadStart.Enabled = spinHeadStart.Enabled
        spinMovingAverageSpan.Enabled = chartType = SpcChartType.MovingAverage
        lblMovingAverageSpan.Enabled = spinMovingAverageSpan.Enabled
        chkUseSteadyStateLimits.Enabled = chartType = SpcChartType.Ewma
    End Sub

    Private Sub UpdateRuleAvailability(chartType As SpcChartType)
        Dim timeWeighted As Boolean = IsTimeWeightedChart(chartType)
        If timeWeighted Then
            If cbRulePreset.Enabled AndAlso cbRulePreset.SelectedItem IsNot Nothing Then
                Dim current As SpcRulePreset = SelectedComboValue(Of SpcRulePreset)(cbRulePreset)
                If current <> SpcRulePreset.None Then pLastShewhartRulePreset = current
                If current = SpcRulePreset.Custom Then
                    Try
                        pSavedCustomRules = BuildCustomRules(allowEmpty:=True)
                    Catch
                        'Keep the last valid custom snapshot while the time-weighted
                        'chart temporarily disables rule editing.
                    End Try
                End If
            End If
            pSuppressUiEvents = True
            SelectComboValue(cbRulePreset, SpcRulePreset.None)
            pSuppressUiEvents = False
            LoadSelectedRulePreset()
        ElseIf Not cbRulePreset.Enabled Then
            cbRulePreset.Enabled = True
            btLoadRulePreset.Enabled = True
            btCopyPresetToCustom.Enabled = True
            cbRulePhaseScope.Enabled = True
            cbSequenceGapBehavior.Enabled = True
            cbSignalMarkingMode.Enabled = True
            pSuppressUiEvents = True
            SelectComboValue(cbRulePreset, pLastShewhartRulePreset)
            pSuppressUiEvents = False
            If pLastShewhartRulePreset = SpcRulePreset.Custom Then
                RestoreCustomRules()
            Else
                LoadSelectedRulePreset()
            End If
        End If

        cbRulePreset.Enabled = Not timeWeighted
        btLoadRulePreset.Enabled = Not timeWeighted
        btCopyPresetToCustom.Enabled = Not timeWeighted
        cbRulePhaseScope.Enabled = Not timeWeighted
        ' The gap option also controls how rule-excluded observations affect the
        ' CUSUM/EWMA recursion and moving-average window.
        cbSequenceGapBehavior.Enabled = True
        lblSequenceGapBehavior.Enabled = True
        cbSignalMarkingMode.Enabled = Not timeWeighted
        lblRuleApplicability.Text = If(timeWeighted,
            "CUSUM, EWMA and moving-average charts report their intrinsic decision-limit signals. Rule presets are disabled; the gap behavior controls excluded observations.",
            "Rules are applied only to compatible panels. Location/attribute presets do not automatically apply to dispersion panels.")
    End Sub

    Private Sub RestoreCustomRules()
        dgvRules.Rows.Clear()
        For Each rule As SpcRuleDefinition In pSavedCustomRules
            AddRuleRow(rule, enabled:=True)
        Next
        dgvRules.ReadOnly = False
        btAddRule.Enabled = True
        btRemoveRule.Enabled = True
        btResetCustomRules.Enabled = True
        lblRulePresetDescription.Text = SpcRuleCatalog.GetPresetDescription(SpcRulePreset.Custom)
        tbRuleDescription.Clear()
    End Sub

    Private Sub cbParameterSource_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbParameterSource.SelectedIndexChanged
        If pSuppressUiEvents Then Return
        UpdateParameterSourceVisibility()
    End Sub

    Private Sub UpdateParameterSourceVisibility()
        If cbParameterSource.SelectedItem Is Nothing Then Return
        Dim source As SpcParameterSource = SelectedComboValue(Of SpcParameterSource)(cbParameterSource)
        grpHistoricalParameters.Visible = source <> SpcParameterSource.EstimateFromPhaseI
        ApplyResponsiveLayout()
    End Sub

    Private Sub UpdateSpecificationControls()
        If cbChartType.SelectedItem Is Nothing Then Return
        Dim supported As Boolean = ChartCanUseSpecificationLines(SelectedChartType())
        grpSpecifications.Enabled = supported
        If Not supported Then
            chkShowSpecificationLimits.Checked = False
            chkShowTargetLine.Checked = False
        End If
    End Sub

    Private Shared Function IsAttributeChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.PChart OrElse
               chartType = SpcChartType.NpChart OrElse
               chartType = SpcChartType.CChart OrElse
               chartType = SpcChartType.UChart
    End Function

    Private Shared Function IsSubgroupChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.XBar OrElse
               chartType = SpcChartType.SubgroupRange OrElse
               chartType = SpcChartType.SubgroupStandardDeviation OrElse
               chartType = SpcChartType.XBarR OrElse
               chartType = SpcChartType.XBarS
    End Function

    Private Shared Function IsTimeWeightedChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.Cusum OrElse
               chartType = SpcChartType.Ewma OrElse
               chartType = SpcChartType.MovingAverage
    End Function

    Private Shared Function ChartCanUseSpecificationLines(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.Individuals OrElse
               chartType = SpcChartType.IndividualsMovingRange OrElse
               chartType = SpcChartType.XBar OrElse
               chartType = SpcChartType.XBarR OrElse
               chartType = SpcChartType.XBarS OrElse
               chartType = SpcChartType.Ewma OrElse
               chartType = SpcChartType.MovingAverage
    End Function

#End Region

#Region "Stages, exclusions, historical parameters, and rules"

    Private Sub btAddHistoricalParameter_Click(sender As Object, e As System.EventArgs) Handles btAddHistoricalParameter.Click
        dgvHistoricalParameters.Rows.Add()
    End Sub

    Private Sub btRemoveHistoricalParameter_Click(sender As Object, e As System.EventArgs) Handles btRemoveHistoricalParameter.Click
        RemoveSelectedGridRows(dgvHistoricalParameters)
    End Sub

    Private Sub btClearHistoricalParameters_Click(sender As Object, e As System.EventArgs) Handles btClearHistoricalParameters.Click
        dgvHistoricalParameters.Rows.Clear()
    End Sub

    Private Sub rbPhase_CheckedChanged(sender As Object, e As System.EventArgs) Handles rbSinglePhaseI.CheckedChanged, rbPhaseIThenPhaseII.CheckedChanged
        UpdateQuickPhaseControls()
    End Sub

    Private Sub UpdateQuickPhaseControls()
        spinLastPhaseIPoint.Enabled = rbPhaseIThenPhaseII.Checked
        lblLastPhaseIPoint.Enabled = rbPhaseIThenPhaseII.Checked
    End Sub

    Private Sub btApplyQuickPhaseSetup_Click(sender As Object, e As System.EventArgs) Handles btApplyQuickPhaseSetup.Click
        Try
            If Not rbSinglePhaseI.Checked AndAlso Not rbPhaseIThenPhaseII.Checked Then
                Throw New ArgumentException("Select a quick phase setup before applying it.")
            End If
            Dim context As InputRowContext = GetInputRowContext()
            Dim parameterSource As SpcParameterSource = SelectedComboValue(Of SpcParameterSource)(cbParameterSource)
            Dim phaseILimitMode As SpcStageLimitMode = If(parameterSource = SpcParameterSource.UseHistoricalParameters,
                                                          SpcStageLimitMode.UseHistoricalParameters,
                                                          SpcStageLimitMode.EstimateFromStageData)
            dgvStages.Rows.Clear()
            If rbSinglePhaseI.Checked Then
                AddStageRow("PhaseI", "Phase I", 1, context.PointCount, SpcPhase.PhaseI,
                            phaseILimitMode, String.Empty)
            Else
                Dim lastPhaseI As Integer = CInt(spinLastPhaseIPoint.Value)
                If lastPhaseI < 1 OrElse lastPhaseI >= context.PointCount Then
                    Throw New ArgumentException("The last Phase I point must be between 1 and " &
                                                (context.PointCount - 1).ToString(CultureInfo.CurrentCulture) & ".")
                End If
                AddStageRow("PhaseI", "Phase I", 1, lastPhaseI, SpcPhase.PhaseI,
                            phaseILimitMode, String.Empty)
                If parameterSource = SpcParameterSource.UseHistoricalParameters Then
                    AddStageRow("PhaseII", "Phase II", lastPhaseI + 1, context.PointCount, SpcPhase.PhaseII,
                                SpcStageLimitMode.UseHistoricalParameters, String.Empty)
                Else
                    AddStageRow("PhaseII", "Phase II", lastPhaseI + 1, context.PointCount, SpcPhase.PhaseII,
                                SpcStageLimitMode.UseReferenceStage, "PhaseI")
                End If
            End If
            RefreshStageReferenceItems()
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
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
                If stageInfo IsNot Nothing Then
                    pointStages(point) = CollapsedTextValue(rowStages, context.PointIndexByRow, point, "stage identifier")
                End If
                If stageInfo IsNot Nothing AndAlso pointStages(point).Trim().Length = 0 Then
                    Throw New ArgumentException("The stage identifier is blank at chart point " & (point + 1).ToString() & ".")
                End If
                pointPhases(point) = If(phaseInfo Is Nothing, SpcPhase.PhaseI,
                                        ParseCollapsedPhase(rowPhases, context.PointIndexByRow, point))
            Next

            If stageInfo Is Nothing Then
                Dim phaseRunCounts As New Dictionary(Of SpcPhase, Integer)()
                Dim runStartPoint As Integer = 0
                While runStartPoint < context.PointCount
                    Dim phase As SpcPhase = pointPhases(runStartPoint)
                    Dim runEndPoint As Integer = runStartPoint
                    While runEndPoint + 1 < context.PointCount AndAlso pointPhases(runEndPoint + 1) = phase
                        runEndPoint += 1
                    End While
                    Dim runNumber As Integer = 1
                    If phaseRunCounts.ContainsKey(phase) Then runNumber = phaseRunCounts(phase) + 1
                    phaseRunCounts(phase) = runNumber
                    Dim baseId As String = If(phase = SpcPhase.PhaseI, "PhaseI", "PhaseII")
                    Dim generatedId As String = If(runNumber = 1, baseId, baseId & "-" & runNumber.ToString(CultureInfo.InvariantCulture))
                    For point As Integer = runStartPoint To runEndPoint
                        pointStages(point) = generatedId
                    Next
                    runStartPoint = runEndPoint + 1
                End While
            End If

            dgvStages.Rows.Clear()
            Dim usedIds As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim runStart As Integer = 0
            Dim lastPhaseIId As String = String.Empty
            For point As Integer = 1 To context.PointCount
                Dim atEnd As Boolean = point = context.PointCount
                Dim changed As Boolean = Not atEnd AndAlso
                    (Not String.Equals(pointStages(point), pointStages(runStart), StringComparison.Ordinal) OrElse
                     pointPhases(point) <> pointPhases(runStart))
                If Not atEnd AndAlso Not changed Then Continue For

                Dim stageId As String = pointStages(runStart).Trim()
                If Not usedIds.Add(stageId) Then
                    Throw New ArgumentException("Stage '" & stageId & "' occurs in more than one non-contiguous block.")
                End If

                Dim phase As SpcPhase = pointPhases(runStart)
                Dim limitMode As SpcStageLimitMode
                Dim referenceId As String = String.Empty
                If SelectedComboValue(Of SpcParameterSource)(cbParameterSource) = SpcParameterSource.UseHistoricalParameters Then
                    limitMode = SpcStageLimitMode.UseHistoricalParameters
                    If phase = SpcPhase.PhaseI Then lastPhaseIId = stageId
                ElseIf phase = SpcPhase.PhaseI Then
                    limitMode = SpcStageLimitMode.EstimateFromStageData
                    lastPhaseIId = stageId
                Else
                    If lastPhaseIId.Length = 0 Then
                        Throw New ArgumentException("A Phase II stage requires an earlier Phase I reference stage or historical parameters.")
                    End If
                    limitMode = SpcStageLimitMode.UseReferenceStage
                    referenceId = lastPhaseIId
                End If

                AddStageRow(stageId, stageId, runStart + 1, point, phase, limitMode, referenceId)
                runStart = point
            Next
            RefreshStageReferenceItems()
            ClearQuickPhaseSelection()
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
    End Sub

    Private Sub ClearQuickPhaseSelection()
        ' Imported stage definitions take precedence over the quick setup. Clear
        ' both radio buttons so the visible state reflects the active stage grid.
        rbSinglePhaseI.Checked = False
        rbPhaseIThenPhaseII.Checked = False
        UpdateQuickPhaseControls()
    End Sub

    Private Sub AddStageRow(stageId As String,
                            displayName As String,
                            firstPoint As Integer,
                            lastPoint As Integer,
                            phase As SpcPhase,
                            limitMode As SpcStageLimitMode,
                            referenceStageId As String)
        Dim rowIndex As Integer = dgvStages.Rows.Add()
        Dim row As DataGridViewRow = dgvStages.Rows(rowIndex)
        row.Cells(colStageID.Index).Value = stageId
        row.Cells(colStageDisplayName.Index).Value = displayName
        row.Cells(colStageFirstPoint.Index).Value = firstPoint
        row.Cells(colStageLastPoint.Index).Value = lastPoint
        row.Cells(colStagePhase.Index).Value = If(phase = SpcPhase.PhaseI, "Phase I", "Phase II")
        row.Cells(colStageLimitMode.Index).Value = StageLimitModeText(limitMode)
        row.Cells(colStageReferenceID.Index).Value = referenceStageId
    End Sub

    Private Sub dgvStages_DefaultValuesNeeded(sender As Object, e As DataGridViewRowEventArgs) Handles dgvStages.DefaultValuesNeeded
        e.Row.Cells(colStagePhase.Index).Value = "Phase I"
        e.Row.Cells(colStageLimitMode.Index).Value = "Estimate from stage data"
    End Sub

    Private Sub dgvStages_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvStages.CellValueChanged
        If e.ColumnIndex = colStageID.Index Then RefreshStageReferenceItems()
    End Sub

    Private Sub RefreshStageReferenceItems()
        If colStageReferenceID Is Nothing Then Return
        Dim existingValues As New List(Of String)()
        For Each row As DataGridViewRow In dgvStages.Rows
            If row.IsNewRow Then Continue For
            Dim value As String = CellText(row, colStageReferenceID.Index)
            If value.Length > 0 Then existingValues.Add(value)
        Next

        colStageReferenceID.Items.Clear()
        colStageReferenceID.Items.Add(String.Empty)
        For Each row As DataGridViewRow In dgvStages.Rows
            If row.IsNewRow Then Continue For
            Dim stageId As String = CellText(row, colStageID.Index)
            If stageId.Length > 0 AndAlso Not colStageReferenceID.Items.Contains(stageId) Then
                colStageReferenceID.Items.Add(stageId)
            End If
        Next
        For Each value As String In existingValues
            If Not colStageReferenceID.Items.Contains(value) Then colStageReferenceID.Items.Add(value)
        Next
    End Sub

    Private Sub btAddExclusion_Click(sender As Object, e As System.EventArgs) Handles btAddExclusion.Click
        Dim index As Integer = dgvExclusions.Rows.Add()
        dgvExclusions.Rows(index).Cells(colExclusionScope.Index).Value = "Parameter estimation and rules"
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
                If IsExclusionIndicator(indicators(row)) Then excluded(point) = True
                If reasons IsNot Nothing AndAlso reasons(row).Trim().Length > 0 Then reasonSets(point).Add(reasons(row).Trim())
            Next

            dgvExclusions.Rows.Clear()
            Dim scopeText As String = ExclusionScopeText(SelectedComboValue(Of SpcExclusionScope)(cbImportedExclusionScope))
            For point As Integer = 0 To excluded.Length - 1
                If Not excluded(point) Then Continue For
                Dim rowIndex As Integer = dgvExclusions.Rows.Add()
                Dim row As DataGridViewRow = dgvExclusions.Rows(rowIndex)
                row.Cells(colExclusionPoint.Index).Value = point + 1
                row.Cells(colExclusionScope.Index).Value = scopeText
                row.Cells(colExclusionReason.Index).Value = String.Join("; ", reasonSets(point).ToArray())
            Next
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
    End Sub

    Private Sub cbRulePreset_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbRulePreset.SelectedIndexChanged
        If pSuppressUiEvents OrElse cbRulePreset.SelectedItem Is Nothing Then Return
        Dim preset As SpcRulePreset = SelectedComboValue(Of SpcRulePreset)(cbRulePreset)
        If preset <> SpcRulePreset.None AndAlso preset <> SpcRulePreset.Custom Then pLastShewhartRulePreset = preset
        LoadSelectedRulePreset()
    End Sub

    Private Sub btLoadRulePreset_Click(sender As Object, e As System.EventArgs) Handles btLoadRulePreset.Click
        LoadSelectedRulePreset()
    End Sub

    Private Sub LoadSelectedRulePreset()
        If cbRulePreset.SelectedItem Is Nothing Then Return
        Dim preset As SpcRulePreset = SelectedComboValue(Of SpcRulePreset)(cbRulePreset)
        lblRulePresetDescription.Text = SpcRuleCatalog.GetPresetDescription(preset)
        dgvRules.Rows.Clear()

        If preset <> SpcRulePreset.Custom Then
            Dim rules As SpcRuleDefinition() = SpcRuleCatalog.GetRules(preset)
            For Each rule As SpcRuleDefinition In rules
                AddRuleRow(rule, enabled:=True)
            Next
        End If

        Dim editable As Boolean = preset = SpcRulePreset.Custom AndAlso cbRulePreset.Enabled
        dgvRules.ReadOnly = Not editable
        btAddRule.Enabled = editable
        btRemoveRule.Enabled = editable
        btResetCustomRules.Enabled = editable
        tbRuleDescription.Clear()
    End Sub

    Private Sub btCopyPresetToCustom_Click(sender As Object, e As System.EventArgs) Handles btCopyPresetToCustom.Click
        Try
            If cbRulePreset.SelectedItem Is Nothing Then Return
            Dim preset As SpcRulePreset = SelectedComboValue(Of SpcRulePreset)(cbRulePreset)
            Dim rules As SpcRuleDefinition() = If(preset = SpcRulePreset.Custom,
                                                  BuildCustomRules(allowEmpty:=True),
                                                  SpcRuleCatalog.GetRules(preset))
            pSuppressUiEvents = True
            SelectComboValue(cbRulePreset, SpcRulePreset.Custom)
            pSuppressUiEvents = False
            dgvRules.Rows.Clear()
            For Each rule As SpcRuleDefinition In rules
                AddRuleRow(rule, enabled:=True)
            Next
            dgvRules.ReadOnly = False
            btAddRule.Enabled = True
            btRemoveRule.Enabled = True
            btResetCustomRules.Enabled = True
            lblRulePresetDescription.Text = SpcRuleCatalog.GetPresetDescription(SpcRulePreset.Custom)
        Catch ex As Exception
            ShowInputError(ex.Message)
        End Try
    End Sub

    Private Sub btAddRule_Click(sender As Object, e As System.EventArgs) Handles btAddRule.Click
        Dim nextNumber As Integer = 1
        Dim usedCodes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row As DataGridViewRow In dgvRules.Rows
            If row.IsNewRow Then Continue For
            Dim numberValue As Integer
            If Integer.TryParse(CellText(row, colRuleNumber.Index), numberValue) Then nextNumber = Math.Max(nextNumber, numberValue + 1)
            usedCodes.Add(CellText(row, colRuleCode.Index))
        Next
        Dim code As String = "C" & nextNumber.ToString(CultureInfo.InvariantCulture)
        While usedCodes.Contains(code)
            nextNumber += 1
            code = "C" & nextNumber.ToString(CultureInfo.InvariantCulture)
        End While
        AddRuleRow(New SpcRuleDefinition(code, nextNumber, SpcRuleKind.BeyondSigma, 1, 1, 3.0R,
                                         displayName:="Custom rule " & nextNumber.ToString(),
                                         description:="One point beyond three sigma."), enabled:=True)
    End Sub

    Private Sub btRemoveRule_Click(sender As Object, e As System.EventArgs) Handles btRemoveRule.Click
        RemoveSelectedGridRows(dgvRules)
    End Sub

    Private Sub btResetCustomRules_Click(sender As Object, e As System.EventArgs) Handles btResetCustomRules.Click
        dgvRules.Rows.Clear()
        tbRuleDescription.Clear()
    End Sub

    Private Sub dgvRules_SelectionChanged(sender As Object, e As System.EventArgs) Handles dgvRules.SelectionChanged
        If dgvRules.CurrentRow Is Nothing OrElse dgvRules.CurrentRow.IsNewRow Then
            tbRuleDescription.Clear()
            Return
        End If
        tbRuleDescription.Text = If(TryCast(dgvRules.CurrentRow.Tag, String), String.Empty)
    End Sub

    Private Sub AddRuleRow(rule As SpcRuleDefinition, enabled As Boolean)
        Dim index As Integer = dgvRules.Rows.Add()
        Dim row As DataGridViewRow = dgvRules.Rows(index)
        row.Cells(colRuleEnabled.Index).Value = enabled
        row.Cells(colRuleNumber.Index).Value = rule.RuleNumber
        row.Cells(colRuleCode.Index).Value = rule.RuleCode
        row.Cells(colRuleName.Index).Value = rule.DisplayName
        row.Cells(colRuleKind.Index).Value = RuleKindText(rule.Kind)
        row.Cells(colRuleWindow.Index).Value = rule.WindowSize
        row.Cells(colRuleMinimumPoints.Index).Value = rule.MinimumPoints
        row.Cells(colRuleSigma.Index).Value = rule.SigmaThreshold
        row.Cells(colRuleSide.Index).Value = RuleSideText(rule.Side)
        row.Cells(colRuleScope.Index).Value = RuleScopeText(rule.Scope)
        row.Tag = rule.Description
    End Sub

    Private Shared Sub RemoveSelectedGridRows(grid As DataGridView)
        Dim rows As New List(Of DataGridViewRow)()
        For Each row As DataGridViewRow In grid.SelectedRows
            If Not row.IsNewRow Then rows.Add(row)
        Next
        If rows.Count = 0 AndAlso grid.CurrentRow IsNot Nothing AndAlso Not grid.CurrentRow.IsNewRow Then
            rows.Add(grid.CurrentRow)
        End If
        For Each row As DataGridViewRow In rows
            grid.Rows.Remove(row)
        Next
    End Sub

    Private Sub Grid_DataError(sender As Object, e As DataGridViewDataErrorEventArgs) Handles dgvHistoricalParameters.DataError, dgvStages.DataError, dgvExclusions.DataError, dgvRules.DataError
        e.ThrowException = False
    End Sub

#End Region

#Region "Request construction and worksheet import"

    Private Function BuildRequest() As SpcFitRequest
        ValidateRoleSelections()
        Dim chartType As SpcChartType = SelectedChartType()
        Dim input As SpcInputData = BuildInputData()

        Dim limitOptions As New SpcControlLimitOptions With {
            .ParameterSource = SelectedComboValue(Of SpcParameterSource)(cbParameterSource),
            .Method = SelectedComboValue(Of SpcControlLimitMethod)(cbControlLimitMethod),
            .SigmaMultiplier = CDbl(spinSigmaMultiplier.Value),
            .WithinSigmaEstimator = SelectedComboValue(Of SpcWithinSigmaEstimator)(cbWithinSigmaEstimator),
            .NaturalLimitPolicy = SelectedComboValue(Of SpcNaturalLimitPolicy)(cbNaturalLimitPolicy),
            .MovingRangeLength = CInt(spinMovingRangeLength.Value),
            .UseBiasCorrection = chkUseBiasCorrection.Checked
        }

        Dim ruleOptions As New SpcRuleOptions With {
            .Preset = If(IsTimeWeightedChart(chartType), SpcRulePreset.None,
                         SelectedComboValue(Of SpcRulePreset)(cbRulePreset)),
            .PhaseScope = SelectedComboValue(Of SpcRulePhaseScope)(cbRulePhaseScope),
            .GapBehavior = SelectedComboValue(Of SpcSequenceGapBehavior)(cbSequenceGapBehavior),
            .MarkingMode = SelectedComboValue(Of SpcSignalMarkingMode)(cbSignalMarkingMode)
        }
        If ruleOptions.Preset = SpcRulePreset.Custom Then ruleOptions.CustomRules = BuildCustomRules(allowEmpty:=False)

        Dim analysisOptions As New SpcAnalysisOptions With {
            .MissingValuePolicy = SelectedComboValue(Of SpcMissingValuePolicy)(cbMissingValuePolicy),
            .ControlLimits = limitOptions,
            .Rules = ruleOptions,
            .Stages = BuildStagesFromGrid(input),
            .Exclusions = BuildExclusionsFromGrid(input)
        }

        Dim request As New SpcFitRequest(
            chartType,
            input,
            analysisOptions:=analysisOptions,
            historicalParameters:=BuildHistoricalParameters(),
            specificationLimits:=BuildSpecificationLimits(chartType),
            chartParameters:=BuildChartParameters(chartType),
            requestLabel:=Me.Text,
            chartTitle:=tbChartTitle.Text.Trim(),
            valueAxisTitle:=tbValueAxisTitle.Text.Trim())

        SpcEngine.Validate(request)
        Return request
    End Function

    Private Sub ValidateRoleSelections()
        If pWorksheet Is Nothing Then Throw New ArgumentException("Select a source worksheet.")
        Dim chartType As SpcChartType = SelectedChartType()
        Dim layout As SpcDataLayout = SelectedComboValue(Of SpcDataLayout)(cbDataLayout)

        If IsAttributeChart(chartType) Then
            RequireExactlyOne(lbCounts, "count")
            If chartType = SpcChartType.PChart OrElse chartType = SpcChartType.NpChart Then
                RequireExactlyOne(lbSampleSize, "sample size")
            ElseIf chartType = SpcChartType.UChart Then
                RequireExactlyOne(lbExposure, "exposure/opportunities")
            End If
        ElseIf IsSubgroupChart(chartType) AndAlso layout = SpcDataLayout.WideSubgroups Then
            If lbValues.Items.Count < 2 Then Throw New ArgumentException("Select at least two measurement columns for subgroups across rows.")
        Else
            RequireExactlyOne(lbValues, "measurements/values")
            If layout = SpcDataLayout.StackedObservations Then RequireExactlyOne(lbSubgroupID, "subgroup ID")
        End If

        If lbLabels.Items.Count > 1 Then Throw New ArgumentException("Select at most one sample-label column.")
        If lbSequence.Items.Count > 1 Then Throw New ArgumentException("Select at most one sequence/date/time column.")
        ValidateActiveRoleDuplicates(chartType, layout)

        If Not chkWriteSummary.Checked AndAlso
           Not chkCreateControlCharts.Checked AndAlso
           Not chkWriteChartData.Checked AndAlso
           Not chkWriteSignals.Checked AndAlso
           Not chkWriteSettingsAudit.Checked Then
            Throw New ArgumentException("Select at least one output on the Output and Appearance tab.")
        End If
    End Sub

    Private Shared Sub RequireExactlyOne(list As ListBox, roleName As String)
        If list.Items.Count <> 1 Then Throw New ArgumentException("Select exactly one " & roleName & " column.")
    End Sub

    Private Sub ValidateActiveRoleDuplicates(chartType As SpcChartType, layout As SpcDataLayout)
        Dim active As New List(Of KeyValuePair(Of String, ListBox))()
        If IsAttributeChart(chartType) Then
            active.Add(New KeyValuePair(Of String, ListBox)("count", lbCounts))
            If chartType = SpcChartType.PChart OrElse chartType = SpcChartType.NpChart Then
                active.Add(New KeyValuePair(Of String, ListBox)("sample size", lbSampleSize))
            End If
            If chartType = SpcChartType.UChart Then active.Add(New KeyValuePair(Of String, ListBox)("exposure", lbExposure))
        Else
            active.Add(New KeyValuePair(Of String, ListBox)("measurements/values", lbValues))
            If layout = SpcDataLayout.StackedObservations Then active.Add(New KeyValuePair(Of String, ListBox)("subgroup ID", lbSubgroupID))
        End If
        If lbLabels.Items.Count > 0 Then active.Add(New KeyValuePair(Of String, ListBox)("sample label", lbLabels))
        If lbSequence.Items.Count > 0 Then active.Add(New KeyValuePair(Of String, ListBox)("sequence", lbSequence))

        Dim used As New Dictionary(Of String, String)(StringComparer.Ordinal)
        For Each role As KeyValuePair(Of String, ListBox) In active
            For Each item As Object In role.Value.Items
                Dim name As String = item.ToString()
                If used.ContainsKey(name) Then
                    Throw New ArgumentException("Column '" & name & "' is assigned to both " & used(name) & " and " & role.Key & ".")
                End If
                used(name) = role.Key
            Next
        Next
    End Sub

    Private Function BuildInputData() As SpcInputData
        Dim context As InputRowContext = GetInputRowContext()
        Dim labels As String() = If(lbLabels.Items.Count = 1,
                                    ReadTextColumn(ColumnInfo(CStr(lbLabels.Items(0))), context.FirstRow, context.LastRow),
                                    Nothing)
        Dim sequenceValues As Double() = If(lbSequence.Items.Count = 1,
                                            ReadNumericColumn(ColumnInfo(CStr(lbSequence.Items(0))), context.FirstRow, context.LastRow),
                                            Nothing)
        Dim sourceRows(context.RowCount - 1) As Integer
        For i As Integer = 0 To sourceRows.Length - 1
            sourceRows(i) = context.FirstRow + i
        Next

        Select Case context.Layout
            Case SpcDataLayout.IndividualSequence
                Dim valueInfo As VarColumnInfo = ColumnInfo(CStr(lbValues.Items(0)))
                Return SpcInputData.FromIndividualSequence(
                    ReadNumericColumn(valueInfo, context.FirstRow, context.LastRow),
                    labels:=labels,
                    sequenceValues:=sequenceValues,
                    sourceRowIndices:=sourceRows,
                    valueName:=valueInfo.HeaderText)

            Case SpcDataLayout.WideSubgroups
                Dim infos As List(Of VarColumnInfo) = RoleColumnInfos(lbValues)
                Dim matrix(context.RowCount - 1, infos.Count - 1) As Double
                Dim names(infos.Count - 1) As String
                For column As Integer = 0 To infos.Count - 1
                    Dim values As Double() = ReadNumericColumn(infos(column), context.FirstRow, context.LastRow)
                    names(column) = infos(column).HeaderText
                    For row As Integer = 0 To values.Length - 1
                        matrix(row, column) = values(row)
                    Next
                Next
                Return SpcInputData.FromWideSubgroups(matrix, labels, sequenceValues, sourceRows, names)

            Case SpcDataLayout.StackedObservations
                Dim valueInfo As VarColumnInfo = ColumnInfo(CStr(lbValues.Items(0)))
                Dim subgroupInfo As VarColumnInfo = ColumnInfo(CStr(lbSubgroupID.Items(0)))
                Return SpcInputData.FromStackedObservations(
                    ReadNumericColumn(valueInfo, context.FirstRow, context.LastRow),
                    ReadTextColumn(subgroupInfo, context.FirstRow, context.LastRow),
                    labels:=labels,
                    sequenceValues:=sequenceValues,
                    sourceRowIndices:=sourceRows,
                    valueName:=valueInfo.HeaderText)

            Case SpcDataLayout.AggregatedCounts
                Dim counts As Double() = ReadNumericColumn(ColumnInfo(CStr(lbCounts.Items(0))), context.FirstRow, context.LastRow)
                Dim sampleSizes As Double() = Nothing
                Dim exposures As Double() = Nothing
                If lbSampleSize.Items.Count = 1 Then
                    sampleSizes = ReadNumericColumn(ColumnInfo(CStr(lbSampleSize.Items(0))), context.FirstRow, context.LastRow)
                End If
                If lbExposure.Items.Count = 1 Then
                    exposures = ReadNumericColumn(ColumnInfo(CStr(lbExposure.Items(0))), context.FirstRow, context.LastRow)
                End If
                Return SpcInputData.FromAggregatedCounts(counts, sampleSizes, exposures, labels, sequenceValues, sourceRows)

            Case Else
                Throw New ArgumentOutOfRangeException("DataLayout")
        End Select
    End Function

    Private Function GetInputRowContext() As InputRowContext
        Dim layout As SpcDataLayout = SelectedComboValue(Of SpcDataLayout)(cbDataLayout)
        Dim required As New List(Of VarColumnInfo)()
        If layout = SpcDataLayout.AggregatedCounts Then
            If lbCounts.Items.Count = 0 Then Throw New ArgumentException("Select the required count column first.")
            required.Add(ColumnInfo(CStr(lbCounts.Items(0))))
            If (SelectedChartType() = SpcChartType.PChart OrElse SelectedChartType() = SpcChartType.NpChart) AndAlso lbSampleSize.Items.Count > 0 Then
                required.Add(ColumnInfo(CStr(lbSampleSize.Items(0))))
            End If
            If SelectedChartType() = SpcChartType.UChart AndAlso lbExposure.Items.Count > 0 Then
                required.Add(ColumnInfo(CStr(lbExposure.Items(0))))
            End If
        Else
            If lbValues.Items.Count = 0 Then Throw New ArgumentException("Select the required measurements/values column(s) first.")
            required.AddRange(RoleColumnInfos(lbValues))
            If layout = SpcDataLayout.StackedObservations Then
                If lbSubgroupID.Items.Count = 0 Then Throw New ArgumentException("Select a subgroup-ID column first.")
                required.Add(ColumnInfo(CStr(lbSubgroupID.Items(0))))
            End If
        End If

        Dim firstRow As Integer = If(required.Any(Function(info) info.HasHeader), 2, 1)
        Dim lastRow As Integer = 0
        For Each info As VarColumnInfo In required
            lastRow = Math.Max(lastRow, LastUsedRow(info.ColumnNumber))
        Next
        If lastRow < firstRow Then Throw New ArgumentException("The selected columns contain no data rows.")

        Dim rowCount As Integer = lastRow - firstRow + 1
        Dim pointIndexByRow(rowCount - 1) As Integer
        Dim pointCount As Integer
        If layout = SpcDataLayout.StackedObservations Then
            Dim subgroupInfo As VarColumnInfo = ColumnInfo(CStr(lbSubgroupID.Items(0)))
            Dim subgroupIds As String() = ReadTextColumn(subgroupInfo, firstRow, lastRow)
            Dim groups As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For row As Integer = 0 To subgroupIds.Length - 1
                Dim id As String = subgroupIds(row).Trim()
                If id.Length = 0 Then Throw New ArgumentException("A subgroup ID is blank at worksheet row " & (firstRow + row).ToString() & ".")
                Dim point As Integer
                If Not groups.TryGetValue(id, point) Then
                    point = groups.Count
                    groups.Add(id, point)
                End If
                pointIndexByRow(row) = point
            Next
            pointCount = groups.Count
        Else
            For row As Integer = 0 To pointIndexByRow.Length - 1
                pointIndexByRow(row) = row
            Next
            pointCount = rowCount
        End If

        spinLastPhaseIPoint.Maximum = Math.Max(1D, CDec(pointCount))
        If spinLastPhaseIPoint.Value > spinLastPhaseIPoint.Maximum Then spinLastPhaseIPoint.Value = spinLastPhaseIPoint.Maximum
        Return New InputRowContext With {
            .FirstRow = firstRow,
            .LastRow = lastRow,
            .Layout = layout,
            .PointIndexByRow = pointIndexByRow,
            .PointCount = pointCount
        }
    End Function

    Private Function BuildStagesFromGrid(input As SpcInputData) As SpcStageDefinition()
        Dim stages As New List(Of SpcStageDefinition)()
        Dim logicalPointCount As Integer = GetLogicalPointCount(input)
        For Each row As DataGridViewRow In dgvStages.Rows
            If row.IsNewRow OrElse GridRowIsBlank(row) Then Continue For
            Dim stageId As String = CellText(row, colStageID.Index)
            Dim displayName As String = CellText(row, colStageDisplayName.Index)
            Dim firstPoint As Integer = RequiredIntegerCell(row, colStageFirstPoint.Index, "First point")
            Dim lastPoint As Integer = RequiredIntegerCell(row, colStageLastPoint.Index, "Last point")
            Dim phase As SpcPhase = ParsePhaseText(CellText(row, colStagePhase.Index))
            Dim limitMode As SpcStageLimitMode = ParseStageLimitMode(CellText(row, colStageLimitMode.Index))
            Dim referenceId As String = CellText(row, colStageReferenceID.Index)
            If lastPoint > logicalPointCount Then
                Throw New ArgumentException("Stage '" & stageId & "' ends after the last chart point (" &
                                            logicalPointCount.ToString(CultureInfo.CurrentCulture) & ").")
            End If
            stages.Add(New SpcStageDefinition(stageId, firstPoint - 1, lastPoint - 1, phase, limitMode,
                                              If(referenceId.Length = 0, Nothing, referenceId), displayName))
        Next

        If stages.Count = 0 AndAlso SelectedComboValue(Of SpcParameterSource)(cbParameterSource) = SpcParameterSource.DefinedByStage Then
            Throw New ArgumentException("Define at least one stage when 'Defined by stage' is selected.")
        End If
        Return stages.ToArray()
    End Function

    Private Function BuildExclusionsFromGrid(input As SpcInputData) As SpcExclusionDefinition()
        Dim exclusions As New List(Of SpcExclusionDefinition)()
        Dim logicalPointCount As Integer = GetLogicalPointCount(input)
        For Each row As DataGridViewRow In dgvExclusions.Rows
            If row.IsNewRow OrElse GridRowIsBlank(row) Then Continue For
            Dim point As Integer = RequiredIntegerCell(row, colExclusionPoint.Index, "Exclusion point")
            If point < 1 Then Throw New ArgumentException("Exclusion point numbers are one-based and must be positive.")
            If point > logicalPointCount Then
                Throw New ArgumentException("Exclusion point " & point.ToString(CultureInfo.CurrentCulture) &
                                            " is beyond the last chart point (" & logicalPointCount.ToString(CultureInfo.CurrentCulture) & ").")
            End If
            Dim scope As SpcExclusionScope = ParseExclusionScope(CellText(row, colExclusionScope.Index))
            exclusions.Add(New SpcExclusionDefinition(point - 1, scope, CellText(row, colExclusionReason.Index)))
        Next
        Return exclusions.ToArray()
    End Function

    Private Function BuildHistoricalParameters() As SpcHistoricalParameters()
        If SelectedComboValue(Of SpcParameterSource)(cbParameterSource) = SpcParameterSource.EstimateFromPhaseI Then
            Return Array.Empty(Of SpcHistoricalParameters)()
        End If

        Dim values As New List(Of SpcHistoricalParameters)()
        For Each row As DataGridViewRow In dgvHistoricalParameters.Rows
            If row.IsNewRow OrElse GridRowIsBlank(row) Then Continue For
            Dim stageId As String = CellText(row, colHistoryStageID.Index)
            Dim processMean As Nullable(Of Double) = OptionalDoubleCell(row, colHistoryMean.Index, "Process mean")
            Dim processSigma As Nullable(Of Double) = OptionalDoubleCell(row, colHistorySigma.Index, "Process SD")
            Dim proportion As Nullable(Of Double) = OptionalDoubleCell(row, colHistoryProportion.Index, "Proportion")
            Dim meanCount As Nullable(Of Double) = OptionalDoubleCell(row, colHistoryMeanCount.Index, "Mean count")
            Dim meanRate As Nullable(Of Double) = OptionalDoubleCell(row, colHistoryMeanRate.Index, "Mean rate")

            If Not processMean.HasValue AndAlso Not processSigma.HasValue AndAlso
               Not proportion.HasValue AndAlso Not meanCount.HasValue AndAlso Not meanRate.HasValue Then
                If stageId.Length = 0 Then Continue For
                Throw New ArgumentException("Historical-parameter row for stage '" & stageId &
                                            "' contains no parameter relevant to the selected chart.")
            End If

            values.Add(New SpcHistoricalParameters(
                stageId:=NullIfEmpty(stageId),
                processMean:=processMean,
                processSigma:=processSigma,
                nonconformingProportion:=proportion,
                meanDefectCount:=meanCount,
                meanDefectRate:=meanRate))
        Next
        Return values.ToArray()
    End Function

    Private Shared Function GetLogicalPointCount(input As SpcInputData) As Integer
        If input.Layout <> SpcDataLayout.StackedObservations Then Return input.RowCount
        Dim ids As String() = input.SubgroupIds
        Dim distinct As New HashSet(Of String)(StringComparer.Ordinal)
        For Each id As String In ids
            distinct.Add(If(id, String.Empty).Trim())
        Next
        Return distinct.Count
    End Function

    Private Function BuildSpecificationLimits(chartType As SpcChartType) As SpcSpecificationLimits
        If Not ChartCanUseSpecificationLines(chartType) Then Return New SpcSpecificationLimits()
        Return New SpcSpecificationLimits(
            OptionalDoubleText(tbLowerSpecificationLimit.Text, "Lower specification limit"),
            OptionalDoubleText(tbTarget.Text, "Target"),
            OptionalDoubleText(tbUpperSpecificationLimit.Text, "Upper specification limit"))
    End Function

    Private Function BuildChartParameters(chartType As SpcChartType) As SpcChartParameters
        Select Case chartType
            Case SpcChartType.Ewma
                Return New SpcChartParameters(ewmaLambda:=CDbl(spinEwmaLambda.Value),
                                              useSteadyStateLimits:=chkUseSteadyStateLimits.Checked)
            Case SpcChartType.Cusum
                Return New SpcChartParameters(cusumReferenceValue:=CDbl(spinCusumReferenceValue.Value),
                                              cusumDecisionInterval:=CDbl(spinCusumDecisionInterval.Value),
                                              headStart:=CDbl(spinHeadStart.Value))
            Case SpcChartType.MovingAverage
                Return New SpcChartParameters(movingAverageSpan:=CInt(spinMovingAverageSpan.Value))
            Case Else
                Return New SpcChartParameters()
        End Select
    End Function

    Private Function BuildCustomRules(allowEmpty As Boolean) As SpcRuleDefinition()
        Dim rules As New List(Of SpcRuleDefinition)()
        For Each row As DataGridViewRow In dgvRules.Rows
            If row.IsNewRow OrElse GridRowIsBlank(row) Then Continue For
            Dim enabled As Boolean = CellBoolean(row, colRuleEnabled.Index, True)
            If Not enabled Then Continue For
            rules.Add(New SpcRuleDefinition(
                CellText(row, colRuleCode.Index),
                RequiredIntegerCell(row, colRuleNumber.Index, "Rule number"),
                ParseRuleKind(CellText(row, colRuleKind.Index)),
                RequiredIntegerCell(row, colRuleWindow.Index, "Rule window"),
                RequiredIntegerCell(row, colRuleMinimumPoints.Index, "Required points"),
                RequiredDoubleCell(row, colRuleSigma.Index, "Sigma threshold"),
                ParseRuleSide(CellText(row, colRuleSide.Index)),
                ParseRuleScope(CellText(row, colRuleScope.Index)),
                CellText(row, colRuleName.Index),
                If(TryCast(row.Tag, String), String.Empty)))
        Next
        If rules.Count = 0 AndAlso Not allowEmpty Then Throw New ArgumentException("Enable at least one custom signal rule.")
        Return rules.ToArray()
    End Function

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

#Region "Calculation and new-workbook output"

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        If pBusy Then Return
        Try
            If pWorkbook IsNot Nothing Then pWorkbook.Activate()
            Dim request As SpcFitRequest = BuildRequest()
            BeginComputation()
            Dim result As SpcFitResult = SpcEngine.Fit(request, AddressOf CancellationRequested)
            If pCancelRequested Then Throw New OperationCanceledException("The SPC calculation was cancelled.")
            WriteResultsToNewWorkbook(result)
            ProgressBar.Style = ProgressBarStyle.Blocks
            ProgressBar.Value = 100
        Catch ex As OperationCanceledException
            MessageBox.Show("The SPC calculation was interrupted.", AppGlobals.gsAPP_TITLE,
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As ArgumentException
            ShowInputError(ex.Message)
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True, "Unable to calculate the control chart")
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

    Private Sub Ui21ControlCharts_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not pBusy Then Return
        pCancelRequested = True
        e.Cancel = True
    End Sub

    Private Sub Ui21ControlCharts_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        If pWorksheet IsNot Nothing OrElse AppGlobals.app Is Nothing Then Return
        Dim activeWorksheet As Excel.Worksheet = TryCast(AppGlobals.app.ActiveSheet, Excel.Worksheet)
        If activeWorksheet IsNot Nothing Then Populate(activeWorksheet)
    End Sub

    Private Sub WriteResultsToNewWorkbook(result As SpcFitResult)
        Dim workbook As Excel.Workbook = CreateResultWorkbook()
        Dim firstSheetAvailable As Boolean = True
        Dim firstOutputSheet As Excel.Worksheet = Nothing

        If chkWriteSummary.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "SPC Summary", firstSheetAvailable)
            firstSheetAvailable = False
            firstOutputSheet = sheet
            WriteResultTables(sheet, workbook, SpcResultTables.BuildSummaryTables(result))
        End If

        If chkCreateControlCharts.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Control Charts", firstSheetAvailable)
            firstSheetAvailable = False
            If firstOutputSheet Is Nothing Then firstOutputSheet = sheet
            Try
                graphics.SpcControlChartExcel.AddCharts(sheet, result, BuildAppearanceOptions())
            Catch ex As Exception
                Dim warning As New ResultTable()
                warning.AddTitle("SPC control-chart warning")
                warning.AddFootnote("The statistical results were calculated, but the Excel chart could not be created: " & ex.Message)
                WriteResultTables(sheet, workbook, New List(Of ResultTable) From {warning})
            End Try
        End If

        If chkWriteChartData.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Chart Data", firstSheetAvailable)
            firstSheetAvailable = False
            If firstOutputSheet Is Nothing Then firstOutputSheet = sheet
            WriteResultTables(sheet, workbook, SpcResultTables.BuildChartDataTables(result, separatePanels:=False))
        End If

        If chkWriteSignals.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Signals", firstSheetAvailable)
            firstSheetAvailable = False
            If firstOutputSheet Is Nothing Then firstOutputSheet = sheet
            WriteResultTables(sheet, workbook, SpcResultTables.BuildSignalTables(result))
        End If

        If chkWriteSettingsAudit.Checked Then
            Dim sheet As Excel.Worksheet = CreateResultSheet(workbook, "Settings and Audit", firstSheetAvailable)
            firstSheetAvailable = False
            If firstOutputSheet Is Nothing Then firstOutputSheet = sheet
            WriteResultTables(sheet, workbook, SpcResultTables.BuildAuditTables(result))
        End If

        If firstOutputSheet IsNot Nothing Then firstOutputSheet.Activate()
    End Sub

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

    Private Function CreateResultSheet(workbook As Excel.Workbook,
                                       baseName As String,
                                       reuseFirstSheet As Boolean) As Excel.Worksheet
        Dim sheet As Excel.Worksheet
        If reuseFirstSheet Then
            sheet = DirectCast(workbook.Worksheets(1), Excel.Worksheet)
            sheet.Name = MakeUniqueWorksheetName(workbook, baseName, sheet)
        Else
            sheet = DirectCast(workbook.Worksheets.Add(After:=workbook.Worksheets(workbook.Worksheets.Count)), Excel.Worksheet)
            sheet.Name = MakeUniqueWorksheetName(workbook, baseName, Nothing)
        End If
        Return sheet
    End Function

    Private Shared Sub WriteResultTables(sheet As Excel.Worksheet,
                                         workbook As Excel.Workbook,
                                         tables As List(Of ResultTable))
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

    Private Function BuildAppearanceOptions() As graphics.SpcControlChartAppearanceOptions
        Return New graphics.SpcControlChartAppearanceOptions With {
            .ChartWidth = CDbl(spinChartWidth.Value),
            .PanelHeight = CDbl(spinPanelHeight.Value),
            .PanelSpacing = CDbl(spinPanelSpacing.Value),
            .ChartTitle = tbChartTitle.Text.Trim(),
            .HorizontalAxisTitle = tbHorizontalAxisTitle.Text.Trim(),
            .UseSequenceValuesForHorizontalAxis = chkUseSequenceValuesForHorizontalAxis.Checked,
            .ShowHorizontalAxisOnEveryPanel = chkShowHorizontalAxisOnEveryPanel.Checked,
            .HorizontalTickLabelOrientation = SelectedComboValue(Of Integer)(cbHorizontalTickOrientation),
            .ShowLegend = chkShowLegend.Checked,
            .ShowMajorGridlines = chkShowMajorGridlines.Checked,
            .ShowPointLabels = chkShowPointLabels.Checked,
            .ShowSignalLabels = chkShowSignalLabels.Checked,
            .ShowExclusionLabels = chkShowExclusionLabels.Checked,
            .ShowLimitLabels = chkShowLimitLabels.Checked,
            .ShowExcludedPoints = chkShowExcludedPoints.Checked,
            .ShowStageBoundaries = chkShowStageBoundaries.Checked,
            .ZoneDisplay = SelectedComboValue(Of graphics.SpcZoneDisplayMode)(cbZoneDisplay),
            .ShowZoneSeriesInLegend = chkShowZoneSeriesInLegend.Checked,
            .ShowSpecificationLimits = chkShowSpecificationLimits.Checked,
            .ShowTargetLine = chkShowTargetLine.Checked,
            .ValueNumberFormat = If(String.IsNullOrWhiteSpace(tbValueNumberFormat.Text), "0.####", tbValueNumberFormat.Text.Trim())
        }
    End Function

    Private Sub btResetAppearance_Click(sender As Object, e As System.EventArgs) Handles btResetAppearance.Click
        Dim defaults As New graphics.SpcControlChartAppearanceOptions()
        spinChartWidth.Value = ClampDecimal(CDec(defaults.ChartWidth), spinChartWidth.Minimum, spinChartWidth.Maximum)
        spinPanelHeight.Value = ClampDecimal(CDec(defaults.PanelHeight), spinPanelHeight.Minimum, spinPanelHeight.Maximum)
        spinPanelSpacing.Value = ClampDecimal(CDec(defaults.PanelSpacing), spinPanelSpacing.Minimum, spinPanelSpacing.Maximum)
        tbChartTitle.Clear()
        tbValueAxisTitle.Clear()
        tbHorizontalAxisTitle.Text = defaults.HorizontalAxisTitle
        tbValueNumberFormat.Text = defaults.ValueNumberFormat
        chkUseSequenceValuesForHorizontalAxis.Checked = defaults.UseSequenceValuesForHorizontalAxis
        chkShowHorizontalAxisOnEveryPanel.Checked = defaults.ShowHorizontalAxisOnEveryPanel
        SelectComboValue(cbHorizontalTickOrientation, defaults.HorizontalTickLabelOrientation)
        chkShowLegend.Checked = defaults.ShowLegend
        chkShowMajorGridlines.Checked = defaults.ShowMajorGridlines
        chkShowPointLabels.Checked = defaults.ShowPointLabels
        chkShowSignalLabels.Checked = defaults.ShowSignalLabels
        chkShowExclusionLabels.Checked = defaults.ShowExclusionLabels
        chkShowLimitLabels.Checked = defaults.ShowLimitLabels
        chkShowExcludedPoints.Checked = defaults.ShowExcludedPoints
        chkShowStageBoundaries.Checked = defaults.ShowStageBoundaries
        SelectComboValue(cbZoneDisplay, defaults.ZoneDisplay)
        chkShowZoneSeriesInLegend.Checked = defaults.ShowZoneSeriesInLegend
        chkShowSpecificationLimits.Checked = False
        chkShowTargetLine.Checked = False
    End Sub

    Private Shared Function ClampDecimal(value As Decimal, minimum As Decimal, maximum As Decimal) As Decimal
        Return Math.Min(maximum, Math.Max(minimum, value))
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

    Private Shared Function CellBoolean(row As DataGridViewRow, columnIndex As Integer, defaultValue As Boolean) As Boolean
        Dim value As Object = row.Cells(columnIndex).Value
        If value Is Nothing Then Return defaultValue
        Return Convert.ToBoolean(value, CultureInfo.CurrentCulture)
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
        Return value
    End Function

    Private Shared Function OptionalDoubleCell(row As DataGridViewRow,
                                               columnIndex As Integer,
                                               label As String) As Nullable(Of Double)
        If Not row.Cells(columnIndex).Visible Then Return Nothing
        Return OptionalDoubleText(CellText(row, columnIndex), label)
    End Function

    Private Shared Function OptionalDoubleText(text As String, label As String) As Nullable(Of Double)
        If String.IsNullOrWhiteSpace(text) Then Return Nothing
        Dim value As Double
        If Not TryParseDouble(text.Trim(), value) Then Throw New ArgumentException(label & " must be numeric.")
        Return value
    End Function

    Private Shared Function TryParseDouble(text As String, ByRef value As Double) As Boolean
        If Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, value) Then Return True
        Return Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, value)
    End Function

    Private Shared Function NullIfEmpty(text As String) As String
        Return If(String.IsNullOrWhiteSpace(text), Nothing, text.Trim())
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

    Private Shared Function CollapsedTextValue(values As String(),
                                               pointIndices As Integer(),
                                               point As Integer,
                                               label As String) As String
        Dim found As String = Nothing
        For i As Integer = 0 To values.Length - 1
            If pointIndices(i) <> point Then Continue For
            Dim current As String = values(i).Trim()
            If found Is Nothing Then
                found = current
            ElseIf Not String.Equals(found, current, StringComparison.Ordinal) Then
                Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() &
                                            " contain inconsistent " & label & " values.")
            End If
        Next
        Return If(found, String.Empty)
    End Function

    Private Shared Function ParseStageLimitMode(text As String) As SpcStageLimitMode
        Select Case text.Trim().ToLowerInvariant()
            Case "estimate from stage data" : Return SpcStageLimitMode.EstimateFromStageData
            Case "use reference stage" : Return SpcStageLimitMode.UseReferenceStage
            Case "use historical parameters" : Return SpcStageLimitMode.UseHistoricalParameters
            Case Else : Throw New ArgumentException("Select a valid stage limit mode.")
        End Select
    End Function

    Private Shared Function StageLimitModeText(mode As SpcStageLimitMode) As String
        Select Case mode
            Case SpcStageLimitMode.EstimateFromStageData : Return "Estimate from stage data"
            Case SpcStageLimitMode.UseReferenceStage : Return "Use reference stage"
            Case SpcStageLimitMode.UseHistoricalParameters : Return "Use historical parameters"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(mode))
        End Select
    End Function

    Private Shared Function ParseExclusionScope(text As String) As SpcExclusionScope
        Select Case text.Trim().ToLowerInvariant()
            Case "parameter estimation" : Return SpcExclusionScope.ParameterEstimation
            Case "rule evaluation" : Return SpcExclusionScope.RuleEvaluation
            Case "parameter estimation and rules" : Return SpcExclusionScope.EstimationAndRules
            Case Else : Throw New ArgumentException("Select a valid exclusion scope.")
        End Select
    End Function

    Private Shared Function ExclusionScopeText(scope As SpcExclusionScope) As String
        Select Case scope
            Case SpcExclusionScope.ParameterEstimation : Return "Parameter estimation"
            Case SpcExclusionScope.RuleEvaluation : Return "Rule evaluation"
            Case SpcExclusionScope.EstimationAndRules : Return "Parameter estimation and rules"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(scope))
        End Select
    End Function

    Private Shared Function IsExclusionIndicator(value As Object) As Boolean
        If value Is Nothing Then Return False
        If TypeOf value Is Boolean Then Return DirectCast(value, Boolean)
        Dim numeric As Double
        If TryParseDouble(Convert.ToString(value, CultureInfo.CurrentCulture), numeric) Then Return numeric <> 0.0R
        Select Case Convert.ToString(value, CultureInfo.CurrentCulture).Trim().ToLowerInvariant()
            Case "true", "yes", "y", "x", "exclude", "excluded" : Return True
            Case "false", "no", "n", "include", "included", "" : Return False
            Case Else : Throw New ArgumentException("Exclusion indicators must be blank/0/No or nonzero/Yes/True/X.")
        End Select
    End Function

    Private Shared Function ParseRuleKind(text As String) As SpcRuleKind
        Select Case text.Trim().ToLowerInvariant()
            Case "beyond sigma" : Return SpcRuleKind.BeyondSigma
            Case "k of m beyond sigma" : Return SpcRuleKind.KOfMConsecutiveBeyondSigma
            Case "run on one side" : Return SpcRuleKind.RunOnOneSide
            Case "monotonic trend" : Return SpcRuleKind.MonotonicTrend
            Case "alternating" : Return SpcRuleKind.Alternating
            Case "all within sigma" : Return SpcRuleKind.AllWithinSigma
            Case "all beyond sigma on both sides" : Return SpcRuleKind.AllBeyondSigmaOnBothSides
            Case Else : Throw New ArgumentException("Select a valid rule pattern.")
        End Select
    End Function

    Private Shared Function RuleKindText(kind As SpcRuleKind) As String
        Select Case kind
            Case SpcRuleKind.BeyondSigma : Return "Beyond sigma"
            Case SpcRuleKind.KOfMConsecutiveBeyondSigma : Return "K of M beyond sigma"
            Case SpcRuleKind.RunOnOneSide : Return "Run on one side"
            Case SpcRuleKind.MonotonicTrend : Return "Monotonic trend"
            Case SpcRuleKind.Alternating : Return "Alternating"
            Case SpcRuleKind.AllWithinSigma : Return "All within sigma"
            Case SpcRuleKind.AllBeyondSigmaOnBothSides : Return "All beyond sigma on both sides"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(kind))
        End Select
    End Function

    Private Shared Function ParseRuleSide(text As String) As SpcRuleSide
        Select Case text.Trim().ToLowerInvariant()
            Case "either side" : Return SpcRuleSide.EitherSide
            Case "upper side only" : Return SpcRuleSide.UpperSideOnly
            Case "lower side only" : Return SpcRuleSide.LowerSideOnly
            Case Else : Throw New ArgumentException("Select a valid rule side.")
        End Select
    End Function

    Private Shared Function RuleSideText(side As SpcRuleSide) As String
        Select Case side
            Case SpcRuleSide.EitherSide : Return "Either side"
            Case SpcRuleSide.UpperSideOnly : Return "Upper side only"
            Case SpcRuleSide.LowerSideOnly : Return "Lower side only"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(side))
        End Select
    End Function

    Private Shared Function ParseRuleScope(text As String) As SpcRuleScope
        Select Case text.Trim().ToLowerInvariant()
            Case "location panels" : Return SpcRuleScope.LocationPanels
            Case "dispersion panels" : Return SpcRuleScope.DispersionPanels
            Case "attribute panels" : Return SpcRuleScope.AttributePanels
            Case "time-weighted panels" : Return SpcRuleScope.TimeWeightedPanels
            Case "location and attribute panels" : Return SpcRuleScope.LocationAndAttributePanels
            Case "all shewhart panels" : Return SpcRuleScope.AllShewhartPanels
            Case "all panels" : Return SpcRuleScope.All
            Case Else : Throw New ArgumentException("Select a valid rule scope.")
        End Select
    End Function

    Private Shared Function RuleScopeText(scope As SpcRuleScope) As String
        Select Case scope
            Case SpcRuleScope.LocationPanels : Return "Location panels"
            Case SpcRuleScope.DispersionPanels : Return "Dispersion panels"
            Case SpcRuleScope.AttributePanels : Return "Attribute panels"
            Case SpcRuleScope.TimeWeightedPanels : Return "Time-weighted panels"
            Case SpcRuleScope.LocationAndAttributePanels : Return "Location and attribute panels"
            Case SpcRuleScope.AllShewhartPanels : Return "All Shewhart panels"
            Case SpcRuleScope.All : Return "All panels"
            Case Else : Return "All panels"
        End Select
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
        If cleaned.Length = 0 Then cleaned = "SPC"
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
