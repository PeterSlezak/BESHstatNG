Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.CausalInference
Imports Microsoft.Office.Interop
Imports System.Threading.Tasks

Public Class Ui20PropensityScoreMatching
    Private pWorksheet As Excel.Worksheet
    Private pWorkbook As Excel.Workbook
    Private pColumnInfo As New Dictionary(Of String, VarColumnInfo)(StringComparer.Ordinal) 'information of variable/column names inported into the input listbox
    Private TermSpecs As Dictionary(Of String, TermSpec)
    Private EffectsController As RegressionEffectsController

    Private Class PsmGuiData
        Public RawModelData As psmData
        Public Input As PsmInputData
        Public SourceRowIds As Integer()
        Public ExpandedData As Double(,)
        Public ExpandedVarNames As String()
        Public RawCovariateKeys As List(Of String)
        Public ImportedReference As String
        Public DroppedRowsDuringAlignment As Integer
    End Class

    Sub New(analysis As String)
        InitializeComponent()
        Me.Text = analysis
        Me.ProgressBar1.Minimum = 0
        Me.ProgressBar1.Maximum = 100
        Me.ProgressBar1.Value = 0
        Me.lblProgress.Text = "Elapsed Time: "

        Me.TermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
        Me.EffectsController = New RegressionEffectsController(Me.lbSelectedVariables,
                                                               Me.lbSelectedEffectsList,
                                                               Me.TermSpecs)

        AddHandler btAddTreatment.Click, Sub() MoveSelectedToSingle(lbTreatment)
        AddHandler btAddOutcome.Click, Sub() MoveSelectedToSingle(lbOutcome)
        AddHandler btAddCovariates.Click, Sub()
                                              MoveSelectedToMany(lbCovariates)
                                              RefreshSelectedVariablesFromSourceList()
                                          End Sub
        AddHandler btAddScore.Click, Sub() MoveSelectedToSingle(lbScore)
        AddHandler btAddExact.Click, Sub() MoveSelectedToMany(lbExact)
        AddHandler btAddID.Click, Sub() MoveSelectedToSingle(lbId)
        AddHandler cbScoreMethod.SelectedIndexChanged, AddressOf OptionComboChanged
        AddHandler cbRunMethod.SelectedIndexChanged, AddressOf OptionComboChanged
        AddHandler cbEstimand.SelectedIndexChanged, AddressOf OptionComboChanged
        AddHandler cbDistanceMetric.SelectedIndexChanged, AddressOf OptionComboChanged
        AddHandler cbCaliperScale.SelectedIndexChanged, AddressOf OptionComboChanged

        InitializeDefaultUiValues()
        SetOptionControlsForCurrentSelections()

        ' Add any initialization after the InitializeComponent() call.
        Me.TabControl1.Anchor = Windows.Forms.AnchorStyles.Left Or
                                Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right Or
                                Windows.Forms.AnchorStyles.Top
        Me.btCalculate.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right
        Me.btnHelp.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right
        Me.ProgressBar1.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right Or
                                Windows.Forms.AnchorStyles.Left
        Me.lblProgress.Anchor = Windows.Forms.AnchorStyles.Left Or
                                Windows.Forms.AnchorStyles.Bottom

        Me.lbAllColumns.Anchor = Windows.Forms.AnchorStyles.Left Or
                                 Windows.Forms.AnchorStyles.Bottom Or
                                 Windows.Forms.AnchorStyles.Top
        Me.lbTreatment.Anchor = Windows.Forms.AnchorStyles.Left Or
                        Windows.Forms.AnchorStyles.Right Or
                        Windows.Forms.AnchorStyles.Top
        Me.lbOutcome.Anchor = Windows.Forms.AnchorStyles.Left Or
                             Windows.Forms.AnchorStyles.Right Or
                             Windows.Forms.AnchorStyles.Top
        Me.lbId.Anchor = Windows.Forms.AnchorStyles.Left Or
                              Windows.Forms.AnchorStyles.Right Or
                              Windows.Forms.AnchorStyles.Top
        Me.lbScore.Anchor = Windows.Forms.AnchorStyles.Left Or
                              Windows.Forms.AnchorStyles.Right Or
                              Windows.Forms.AnchorStyles.Top
        Me.lbExact.Anchor = Windows.Forms.AnchorStyles.Left Or
                              Windows.Forms.AnchorStyles.Right Or
                              Windows.Forms.AnchorStyles.Top
        Me.lbCovariates.Anchor = Windows.Forms.AnchorStyles.Left Or
                         Windows.Forms.AnchorStyles.Right Or
                         Windows.Forms.AnchorStyles.Top Or
                         Windows.Forms.AnchorStyles.Bottom

        Me.lblNote.Anchor = Windows.Forms.AnchorStyles.Right Or
                            Windows.Forms.AnchorStyles.Bottom
        Me.cbSheetsList.Anchor = Windows.Forms.AnchorStyles.Top Or
                                 Windows.Forms.AnchorStyles.Right
        Me.btReload.Anchor = Windows.Forms.AnchorStyles.Top Or
                             Windows.Forms.AnchorStyles.Right

        Me.lbSelectedVariables.Anchor = Windows.Forms.AnchorStyles.Left Or
                                        Windows.Forms.AnchorStyles.Bottom Or
                                        Windows.Forms.AnchorStyles.Top
        Me.lbSelectedEffectsList.Anchor = Windows.Forms.AnchorStyles.Left Or
                                          Windows.Forms.AnchorStyles.Right Or
                                          Windows.Forms.AnchorStyles.Top Or
                                          Windows.Forms.AnchorStyles.Bottom
        Me.tbRemoveSelectedEffects.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                            Windows.Forms.AnchorStyles.Right
        Me.btClearAllSelectedEffects.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                            Windows.Forms.AnchorStyles.Right

        Me.WireHelp(Me.btnHelp)
    End Sub

    Sub Populate(ws As Object)
        Dim VarRng As Object, ws_temp As Object
        pWorksheet = ws
        pWorkbook = ws.parent
        Dim FinalCol = LastColumnInSheet(ws)
        Dim MaxRows = MaxRowsInSheet(ws)
        VarRng = ws.Range(ws.Cells(1, 1), ws.Cells(1, FinalCol)) 'Create range object to contain variable names
        Me.pColumnInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns, bNumeric_only:=False) 'Cycle through the range and add all non-empty variable names to the listbox

        'We may call this method multiple times so populate sheet combo box only once
        Me.cbSheetsList.Items.Clear()
        For Each ws_temp In pWorkbook.Worksheets
            Me.cbSheetsList.Items.Add(ws_temp.name)
        Next
        Me.cbSheetsList.SelectedIndex = Me.cbSheetsList.FindStringExact(Me.pWorkbook.ActiveSheet.name)
    End Sub

    Private Sub ReloadColumnLists()
        If pWorksheet Is Nothing Then Return
        lbAllColumns.Items.Clear()
        lbTreatment.Items.Clear()
        lbOutcome.Items.Clear()
        lbCovariates.Items.Clear()
        lbScore.Items.Clear()
        lbExact.Items.Clear()
        lbId.Items.Clear()
        pColumnInfo.Clear()

        Dim finalCol As Integer = LastColumnInSheet(pWorksheet)
        Dim maxRows As Integer = MaxRowsInSheet(pWorksheet)
        Dim varRng As Excel.Range = pWorksheet.Range(pWorksheet.Cells(1, 1), pWorksheet.Cells(1, finalCol))
        pColumnInfo = VarNamesToLBox(varRng, maxRows, lbAllColumns, False)

        cbSheetsList.Items.Clear()
        For Each ws As Excel.Worksheet In pWorkbook.Worksheets
            cbSheetsList.Items.Add(ws.Name)
        Next
        Dim idx As Integer = cbSheetsList.FindStringExact(pWorksheet.Name)
        If idx >= 0 Then cbSheetsList.SelectedIndex = idx
    End Sub

    Private Sub MoveSelectedToSingle(target As ListBox)
        If lbAllColumns.SelectedItem Is Nothing Then Return
        target.Items.Clear()
        target.Items.Add(lbAllColumns.SelectedItem.ToString())
    End Sub

    Private Sub MoveSelectedToMany(target As ListBox)
        For Each item As Object In lbAllColumns.SelectedItems
            Dim text As String = item.ToString()
            If Not target.Items.Contains(text) Then target.Items.Add(text)
        Next
    End Sub

    Private Sub ValidateInputs(ByRef bWait As Boolean, ByRef strWarning As String)
        bWait = False
        strWarning = String.Empty

        If Me.lbTreatment.Items.Count <> 1 Then
            strWarning = "Please select exactly one treatment indicator variable coded 0/1."
            bWait = True
            Exit Sub
        End If

        If Me.lbOutcome.Items.Count <> 1 Then
            strWarning = "Please select exactly one outcome variable."
            bWait = True
            Exit Sub
        End If

        If Me.lbCovariates.Items.Count = 0 Then
            strWarning = "Please select at least one covariate."
            bWait = True
            Exit Sub
        End If

        Dim scoreMethod As PsmScoreMethod = ParseEnum(Of PsmScoreMethod)(Me.cbScoreMethod.Text)

        If scoreMethod <> PsmScoreMethod.Supplied Then
            If Me.lbSelectedEffectsList.Items.Count = 0 AndAlso Not Me.ckIntercept.Checked Then
                strWarning = "No intercept and no propensity-model effects were specified."
                bWait = True
                Exit Sub
            End If

            If Me.lbSelectedEffectsList.Items.Count = 0 Then
                strWarning = "Please add at least one propensity-model effect on the Propensity model tab."
                bWait = True
                Exit Sub
            End If
        End If

        If scoreMethod = PsmScoreMethod.Supplied AndAlso Me.lbScore.Items.Count <> 1 Then
            strWarning = "Supplied score method requires exactly one supplied propensity-score variable."
            bWait = True
            Exit Sub
        End If

        If Not ValidateNoDuplicateRoleAssignments(strWarning) Then
            bWait = True
            Exit Sub
        End If

        Try
            Dim fitOptions As PsmComprehensiveFitOptions = BuildFitOptions()
            fitOptions.StandardOptions.Validate()
            ValidateMethodSpecificOptions(fitOptions)
        Catch ex As Exception
            strWarning = ex.Message
            bWait = True
            Exit Sub
        End Try
    End Sub

    Private Function ValidateNoDuplicateRoleAssignments(ByRef warning As String) As Boolean
        Dim roles As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Dim rolePairs As New List(Of KeyValuePair(Of String, String))()

        AddRole(rolePairs, "Treatment", Me.lbTreatment)
        AddRole(rolePairs, "Outcome", Me.lbOutcome)
        AddRole(rolePairs, "Supplied score", Me.lbScore)
        AddRole(rolePairs, "ID", Me.lbId)
        AddRole(rolePairs, "Exact group", Me.lbExact)
        AddRole(rolePairs, "Covariate", Me.lbCovariates)

        For Each pair As KeyValuePair(Of String, String) In rolePairs
            If pair.Key = String.Empty Then Continue For
            If roles.ContainsKey(pair.Key) Then
                warning = "Variable '" & pair.Key & "' is selected for both " & roles(pair.Key) & " and " & pair.Value & ". Please assign each variable to only one role."
                Return False
            End If
            roles(pair.Key) = pair.Value
        Next

        Return True
    End Function

    Private Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        Try
            Dim bWait As Boolean, strWarning As String
            Me.pWorkbook.Activate()

            strWarning = String.Empty
            ValidateInputs(bWait, strWarning)
            If bWait Then
                If strWarning <> String.Empty Then MsgBox(strWarning)
                Exit Sub
            End If

            BeginPsmComputation()
            Dim myData As PsmGuiData = Me.GetData()
            If myData Is Nothing OrElse myData.RawModelData Is Nothing OrElse myData.RawModelData.bZeroValid Then
                MsgBox("No valid observations")
                Exit Sub
            End If

            Me.RunPSM(myData)
            FinishPsmComputation("Calculation completed.")

        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True)
        Finally
            EndPsmComputation()
        End Try
    End Sub

    Private Sub RunPSM(myData As PsmGuiData)
        Me.lblProgress.Text = "Fitting propensity score analysis..."
        Windows.Forms.Application.DoEvents()

        Dim fitOptions As PsmComprehensiveFitOptions = BuildFitOptions()
        Dim fitResult As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(myData.Input, fitOptions)

        Me.lblProgress.Text = "Writing propensity score matching output..."
        Windows.Forms.Application.DoEvents()

        WritePsmResults(myData, fitResult)
    End Sub

    Private Function GetData() As PsmGuiData
        Dim out As New PsmGuiData()
        Dim scoreMethod As PsmScoreMethod = ParseEnum(Of PsmScoreMethod)(Me.cbScoreMethod.Text)
        Dim importer As New psmData()
        Dim spec As New PsmDataImportSpec With {
            .Worksheet = pWorksheet,
            .VariableColumnsInfo = Me.pColumnInfo,
            .TreatmentKey = CStr(Me.lbTreatment.Items(0)),
            .OutcomeKey = CStr(Me.lbOutcome.Items(0)),
            .SelectedCovariateKeys = GetListBoxItems(Me.lbCovariates),
            .EffectItems = Me.lbSelectedEffectsList.Items,
            .TermSpecs = Me.TermSpecs,
            .OmitCategoricalReference = Me.ckIntercept.Checked,
            .ScoreMethod = scoreMethod,
            .SuppliedScoreKey = If(Me.lbScore.Items.Count = 1, CStr(Me.lbScore.Items(0)), String.Empty),
            .IdKey = If(Me.lbId.Items.Count = 1, CStr(Me.lbId.Items(0)), String.Empty),
            .ExactGroupKeys = GetListBoxItems(Me.lbExact)
        }

        importer.DataImportFromWorksheet(spec)

        out.RawModelData = importer
        out.Input = importer.Input
        out.SourceRowIds = importer.RowIds
        out.ExpandedData = importer.ExpandedData
        out.ExpandedVarNames = importer.ExpandedVarNames
        out.RawCovariateKeys = importer.RawCovariateKeys
        out.ImportedReference = importer.ImportedReferenceSummary
        out.DroppedRowsDuringAlignment = importer.DroppedRowsDuringAlignment
        Return out
    End Function

    Private Sub btReload_Click(sender As Object, e As System.EventArgs) Handles btReload.Click
        Dim newSheet As Object
        Me.lbAllColumns.Items.Clear()

        If Me.cbSheetsList.SelectedIndex <> -1 Then
            If pWorksheet.Name <> Me.cbSheetsList.SelectedItem.ToString() Then
                Me.lbTreatment.Items.Clear()
                Me.lbOutcome.Items.Clear()
                Me.lbId.Items.Clear()
                Me.lbScore.Items.Clear()
                Me.lbExact.Items.Clear()
                Me.lbCovariates.Items.Clear()
                Me.lbAllColumns.Items.Clear()
                Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
            End If

            newSheet = pWorkbook.Worksheets(Me.cbSheetsList.SelectedItem.ToString())
            Me.Populate(newSheet)
        Else
            Me.Populate(pWorksheet)
        End If
    End Sub

    Private Sub btRemoveTreatment_Click(sender As Object, e As System.EventArgs) Handles btRemoveTreatment.Click
        Remove_Item(Me.lbTreatment)
    End Sub

    Private Sub btRemoveOutcome_Click(sender As Object, e As System.EventArgs) Handles btRemoveOutcome.Click
        Remove_Item(Me.lbOutcome)
    End Sub

    Private Sub btRemoveID_Click(sender As Object, e As System.EventArgs) Handles btRemoveID.Click
        Remove_Item(Me.lbId)
    End Sub

    Private Sub btRemoveScore_Click(sender As Object, e As System.EventArgs) Handles btRemoveScore.Click
        Remove_Item(Me.lbScore)
    End Sub

    Private Sub btRemoveExact_Click(sender As Object, e As System.EventArgs) Handles btRemoveExact.Click
        Remove_Item(Me.lbExact)
    End Sub

    Private Sub btRemoveCovariates_Click(sender As Object, e As System.EventArgs) Handles btRemoveCovariates.Click
        Remove_Item(Me.lbCovariates, "selected")
        RefreshSelectedVariablesFromSourceList()
    End Sub

    Private Sub RefreshSelectedVariablesFromSourceList()
        If Me.lbSelectedVariables Is Nothing OrElse Me.lbCovariates Is Nothing Then Exit Sub

        Dim changed As Boolean = Not IsEqualListBox(Me.lbCovariates, Me.lbSelectedVariables)
        If Not changed AndAlso Me.lbSelectedVariables.Items.Count > 0 Then Exit Sub

        If changed Then
            Remove_Item(Me.lbSelectedVariables)
            For i As Integer = 0 To Me.lbCovariates.Items.Count - 1
                Me.lbSelectedVariables.Items.Add(Me.lbCovariates.Items(i))
            Next

            If Not TermSpecsUseOnlySelectedVariables(Me.lbSelectedEffectsList, Me.TermSpecs) Then
                If MsgBox("There is a variable in selected fixed effects that was removed from the model source variable(s) list." & vbNewLine & vbNewLine &
                      "Clear selected fixed-effects list?", vbYesNo + vbExclamation, "Clear selected fixed effects?") = vbYes Then
                    Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
                End If
            End If
        Else
            For i As Integer = 0 To Me.lbCovariates.Items.Count - 1
                Me.lbSelectedVariables.Items.Add(Me.lbCovariates.Items(i))
            Next
        End If
    End Sub

    Private Function TermSpecsUseOnlySelectedVariables(effectList As Windows.Forms.ListBox, termSpecs As Dictionary(Of String, TermSpec)) As Boolean
        Dim required As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(effectList.Items, termSpecs)
        For Each key As String In required
            If Not Me.lbSelectedVariables.Items.Contains(key) Then Return False
        Next
        Return True
    End Function

    Private Sub tbRemoveSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "selected", Me.TermSpecs)
    End Sub

    Private Sub btAddEffect_Click(sender As Object, e As System.EventArgs) Handles btAddEffect.Click
        Me.EffectsController.AddMainEffectsFromSelectedVars()
    End Sub

    Private Sub btAddEffectCategoricalFactor_Click(sender As Object, e As System.EventArgs) Handles btAddEffectCategoricalFactor.Click
        Me.EffectsController.AddCategoricalEffectsFromSelectedVars()
    End Sub

    Private Sub btnPoly_Click(sender As Object, e As System.EventArgs) Handles btnPoly.Click
        Me.EffectsController.AddPolynomialEffectsFromSelectedVars(CInt(Me.spinBtnPoly.Value))
    End Sub

    Private Sub btn2Interactions_Click(sender As Object, e As System.EventArgs) Handles btn2Interactions.Click
        Me.EffectsController.AddTwoWayInteractionsFromSelectedVars()
    End Sub

    Private Sub btnCustomInteraction_Click(sender As Object, e As System.EventArgs) Handles btnCustomInteraction.Click
        Me.EffectsController.AddCustomInteractionFromSelectedVars()
    End Sub

    Private Sub btClearAllSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
    End Sub

    Private Sub OptionComboChanged(sender As Object, e As System.EventArgs)
        SetOptionControlsForCurrentSelections()
    End Sub

    '--------------------------------------------------------------------------
    ' Helpers
    '--------------------------------------------------------------------------

    Private Function BuildFitOptions() As PsmComprehensiveFitOptions
        Dim standard As New PsmOptions With {
            .ScoreMethod = ParseEnum(Of PsmScoreMethod)(cbScoreMethod.Text),
            .Estimand = ParseEnum(Of PsmEstimand)(cbEstimand.Text),
            .DistanceMetric = ParseEnum(Of PsmDistanceMetric)(cbDistanceMetric.Text),
            .MatchingRatio = Math.Max(1, ParseUiInteger(tbMatchingRatio.Text, "matching ratio")),
            .WithReplacement = chkWithReplacement.Checked,
            .CaliperScale = ParseEnum(Of PsmCaliperScale)(cbCaliperScale.Text),
            .Caliper = ParseOptionalDouble(tbCaliper.Text, Double.NaN),
            .MatchingOrder = ParseEnum(Of PsmMatchingOrder)(cbMatchingOrder.Text),
            .CommonSupport = ParseEnum(Of PsmCommonSupportMode)(cbCommonSupport.Text),
            .IncludeIntercept = ckIntercept.Checked,
            .StandardizeCovariates = chkStandardizeCovariates.Checked,
            .LogisticMaxIterations = Math.Max(1, ParseUiInteger(tbMaxIterations.Text, "maximum iterations")),
            .LogisticTolerance = ParseUiDouble(tbTolerance.Text, "tolerance"),
            .LogisticRidgePenalty = Math.Max(0.0, ParseUiDouble(tbRidgePenalty.Text, "ridge penalty")),
            .BalanceSmdThreshold = ParseUiDouble(tbLoveThreshold.Text, "Love-plot/SMD threshold"),
            .SubclassificationStrata = Math.Max(2, ParseUiInteger(tbStrata.Text, "subclassification strata")),
            .TrimPropensityLower = ParseUiDouble(tbTrimLower.Text, "lower propensity trim"),
            .TrimPropensityUpper = ParseUiDouble(tbTrimUpper.Text, "upper propensity trim")
        }

        Dim fitOptions As New PsmComprehensiveFitOptions With {
            .StandardOptions = standard,
            .RunMethod = ParseEnum(Of PsmBackendRunMethod)(cbRunMethod.Text),
            .IncludeDoublyRobustEstimate = chkDoublyRobust.Checked,
            .IncludeOverlapDiagnostics = chkOverlapDiagnostics.Checked,
            .IncludeWeightDiagnostics = chkWeightDiagnostics.Checked,
            .IncludeLovePlotRows = chkLovePlot.Checked,
            .OverlapBinCount = Math.Max(2, ParseUiInteger(tbOverlapBins.Text, "overlap bins")),
            .LovePlotThreshold = ParseUiDouble(tbLoveThreshold.Text, "Love-plot threshold"),
            .ExtremeWeightCutoff = ParseUiDouble(tbExtremeWeight.Text, "extreme weight cutoff")
        }

        If fitOptions.RunMethod = PsmBackendRunMethod.CoarsenedExactMatching Then
            Dim bins As Integer = Math.Max(2, ParseUiInteger(tbCemBins.Text, "CEM quantile bins"))
            fitOptions.CoarseningSpec = New PsmCoarseningSpec With {
                .DefaultCovariateBins = bins,
                .PropensityScoreBins = bins,
                .Estimand = standard.Estimand,
                .NormalizeWeightsToSampleSize = standard.NormalizeWeightsToSampleSize
            }
        End If

        NormalizeFitOptionsForRunMethod(fitOptions)
        Return fitOptions
    End Function

    Private Sub InitializeDefaultUiValues()
        Me.tbTolerance.Text = FormatUiDouble(0.0000001)
        Me.tbRidgePenalty.Text = FormatUiDouble(0.0000001)
        Me.tbTrimLower.Text = FormatUiDouble(0.0)
        Me.tbTrimUpper.Text = FormatUiDouble(1.0)
        Me.tbLoveThreshold.Text = FormatUiDouble(0.1)
        Me.tbMaxIterations.Text = "100"
        Me.tbMatchingRatio.Text = "1"
        Me.tbStrata.Text = "5"
        Me.tbCemBins.Text = "5"
        Me.tbOverlapBins.Text = "20"
        Me.tbExtremeWeight.Text = FormatUiDouble(10.0)
        Me.tbCaliper.Text = String.Empty

        PopulateEnumCombo(Me.cbScoreMethod, GetType(PsmScoreMethod), PsmScoreMethod.LogisticRegression.ToString())
        PopulateEnumCombo(Me.cbRunMethod, GetType(PsmBackendRunMethod), PsmBackendRunMethod.StandardNearestNeighbor.ToString())
        PopulateEnumCombo(Me.cbEstimand, GetType(PsmEstimand), PsmEstimand.ATT.ToString())
        PopulateEnumCombo(Me.cbDistanceMetric, GetType(PsmDistanceMetric), PsmDistanceMetric.PropensityScore.ToString())
        PopulateEnumCombo(Me.cbCaliperScale, GetType(PsmCaliperScale), PsmCaliperScale.None.ToString())
        PopulateEnumCombo(Me.cbMatchingOrder, GetType(PsmMatchingOrder), PsmMatchingOrder.PropensityDescending.ToString())
        PopulateEnumCombo(Me.cbCommonSupport, GetType(PsmCommonSupportMode), PsmCommonSupportMode.None.ToString())

        Me.chkWithReplacement.Checked = False
        Me.chkStandardizeCovariates.Checked = True
        Me.chkDoublyRobust.Checked = True
        Me.chkOverlapDiagnostics.Checked = True
        Me.chkWeightDiagnostics.Checked = True
        Me.chkLovePlot.Checked = True
        Me.chkWriteDiagnostics.Checked = True
        Me.chkWriteMatches.Checked = True
    End Sub

    Private Shared Function ParseEnum(Of TEnum As Structure)(text As String) As TEnum
        Return CType([Enum].Parse(GetType(TEnum), text), TEnum)
    End Function

    Private Shared Function ParseOptionalDouble(text As String, defaultValue As Double) As Double
        If String.IsNullOrWhiteSpace(text) Then Return defaultValue
        Return ParseUiDouble(text, "optional numeric value")
    End Function

    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs)
        MessageBox.Show("PSM currently runs synchronously. Interrupt support will be added with the long-running GUI worker batch.", "Propensity Score Matching", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub PopulateEnumCombo(combo As ComboBox, enumType As Type, defaultName As String)
        combo.DropDownStyle = ComboBoxStyle.DropDownList
        combo.Items.Clear()
        For Each name As String In [Enum].GetNames(enumType)
            combo.Items.Add(name)
        Next
        Dim idx As Integer = combo.FindStringExact(defaultName)
        If idx >= 0 Then
            combo.SelectedIndex = idx
        ElseIf combo.Items.Count > 0 Then
            combo.SelectedIndex = 0
        End If
    End Sub

    Private Sub AddRole(target As List(Of KeyValuePair(Of String, String)), roleName As String, list As ListBox)
        For Each item As Object In list.Items
            target.Add(New KeyValuePair(Of String, String)(CStr(item), roleName))
        Next
    End Sub

    Private Sub ValidateMethodSpecificOptions(fitOptions As PsmComprehensiveFitOptions)
        PsmMethodCapabilities.ValidateFitOptions(fitOptions)
    End Sub

    Private Sub BeginPsmComputation()
        Me.btCalculate.Enabled = False
        Me.ProgressBar1.Style = ProgressBarStyle.Marquee
        Me.lblProgress.Text = "Preparing propensity score matching..."
        Windows.Forms.Application.DoEvents()
    End Sub

    Private Sub FinishPsmComputation(message As String)
        Me.ProgressBar1.Style = ProgressBarStyle.Continuous
        Me.ProgressBar1.Value = 0
        Me.lblProgress.Text = message
        Windows.Forms.Application.DoEvents()
    End Sub

    Private Sub EndPsmComputation()
        Try
            Me.ProgressBar1.Style = ProgressBarStyle.Continuous
            Me.btCalculate.Enabled = True
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

    Private Sub SetOptionControlsForCurrentSelections()
        Dim scoreMethod As PsmScoreMethod = PsmScoreMethod.LogisticRegression
        Dim runMethod As PsmBackendRunMethod = PsmBackendRunMethod.StandardNearestNeighbor
        Dim estimand As PsmEstimand = PsmEstimand.ATT
        Dim caliperScale As PsmCaliperScale = PsmCaliperScale.None
        Dim distanceMetric As PsmDistanceMetric = PsmDistanceMetric.PropensityScore

        Try
            If Me.cbScoreMethod.SelectedItem IsNot Nothing Then scoreMethod = ParseEnum(Of PsmScoreMethod)(Me.cbScoreMethod.Text)
            If Me.cbRunMethod.SelectedItem IsNot Nothing Then runMethod = ParseEnum(Of PsmBackendRunMethod)(Me.cbRunMethod.Text)
            If Me.cbEstimand.SelectedItem IsNot Nothing Then estimand = ParseEnum(Of PsmEstimand)(Me.cbEstimand.Text)
            If Me.cbCaliperScale.SelectedItem IsNot Nothing Then caliperScale = ParseEnum(Of PsmCaliperScale)(Me.cbCaliperScale.Text)
            If Me.cbDistanceMetric.SelectedItem IsNot Nothing Then distanceMetric = ParseEnum(Of PsmDistanceMetric)(Me.cbDistanceMetric.Text)
        Catch
        End Try

        RefreshEstimandChoicesForRunMethod(runMethod)
        Try
            estimand = ParseEnum(Of PsmEstimand)(Me.cbEstimand.Text)
        Catch
            estimand = PsmEstimand.ATT
        End Try

        Dim supplied As Boolean = (scoreMethod = PsmScoreMethod.Supplied)
        Me.lbScore.Enabled = supplied
        Me.btAddScore.Enabled = supplied
        Me.btRemoveScore.Enabled = supplied
        Me.lblScore.Enabled = supplied
        'Keep the score-method, trimming, and common-support controls enabled even when
        'the user supplies propensity scores. Only logistic-regression iteration controls
        'are disabled, otherwise the user cannot switch back from Supplied mode.
        Me.grpIterOptions.Enabled = True
        Me.tbMaxIterations.Enabled = Not supplied
        Me.lblMaxIterations.Enabled = Not supplied
        Me.tbTolerance.Enabled = Not supplied
        Me.lblTolerance.Enabled = Not supplied
        Me.tbRidgePenalty.Enabled = Not supplied
        Me.lblRidgePenalty.Enabled = Not supplied

        Dim matching As Boolean = PsmMethodCapabilities.UsesMatchingControls(runMethod)
        Dim nearestNeighbor As Boolean = PsmMethodCapabilities.UsesNearestNeighborOptions(runMethod)
        Dim optimalPair As Boolean = PsmMethodCapabilities.UsesOptimalPairOptions(runMethod)

        If optimalPair Then
            Me.tbMatchingRatio.Text = "1"
            Me.chkWithReplacement.Checked = False
        End If

        If distanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper AndAlso caliperScale = PsmCaliperScale.None Then
            Dim idx As Integer = Me.cbCaliperScale.FindStringExact(PsmCaliperScale.StandardizedLogitPropensityScore.ToString())
            If idx >= 0 Then
                Me.cbCaliperScale.SelectedIndex = idx
                caliperScale = PsmCaliperScale.StandardizedLogitPropensityScore
            End If
            If String.IsNullOrWhiteSpace(Me.tbCaliper.Text) Then Me.tbCaliper.Text = FormatUiDouble(0.2)
        End If

        Me.tbMatchingRatio.Enabled = nearestNeighbor
        Me.lblMatchingRatio.Enabled = Me.tbMatchingRatio.Enabled
        Me.chkWithReplacement.Enabled = nearestNeighbor
        Me.cbDistanceMetric.Enabled = matching
        Me.lblDistanceMetric.Enabled = matching
        Me.cbMatchingOrder.Enabled = nearestNeighbor
        Me.lblMatchingOrder.Enabled = Me.cbMatchingOrder.Enabled
        Me.cbCaliperScale.Enabled = matching
        Me.lblCaliperScale.Enabled = matching
        Me.tbCaliper.Enabled = matching AndAlso caliperScale <> PsmCaliperScale.None
        Me.lblCaliper.Enabled = Me.tbCaliper.Enabled
        Me.tbStrata.Enabled = PsmMethodCapabilities.UsesSubclassificationControls(runMethod)
        Me.lblStrata.Enabled = Me.tbStrata.Enabled
        Me.tbCemBins.Enabled = PsmMethodCapabilities.UsesCemControls(runMethod)
        Me.lblCemBins.Enabled = Me.tbCemBins.Enabled
        Me.chkDoublyRobust.Enabled = PsmMethodCapabilities.SupportsDoublyRobust(runMethod, estimand)
        If Not Me.chkDoublyRobust.Enabled Then Me.chkDoublyRobust.Checked = False
    End Sub

    Private Sub RefreshEstimandChoicesForRunMethod(runMethod As PsmBackendRunMethod)
        Dim current As String = If(Me.cbEstimand.SelectedItem Is Nothing, PsmEstimand.ATT.ToString(), Me.cbEstimand.Text)
        Dim allowed As PsmEstimand() = PsmMethodCapabilities.SupportedEstimands(runMethod)
        Dim allowedNames As String() = allowed.Select(Function(e) e.ToString()).ToArray()
        If Me.cbEstimand.Items.Count = allowedNames.Length Then
            Dim same As Boolean = True
            For i As Integer = 0 To allowedNames.Length - 1
                If Not String.Equals(CStr(Me.cbEstimand.Items(i)), allowedNames(i), StringComparison.Ordinal) Then
                    same = False
                    Exit For
                End If
            Next
            If same Then Return
        End If

        Me.cbEstimand.Items.Clear()
        For Each name As String In allowedNames
            Me.cbEstimand.Items.Add(name)
        Next

        Dim idx As Integer = Me.cbEstimand.FindStringExact(current)
        If idx < 0 Then idx = Me.cbEstimand.FindStringExact(PsmEstimand.ATT.ToString())
        If idx < 0 AndAlso Me.cbEstimand.Items.Count > 0 Then idx = 0
        If idx >= 0 Then Me.cbEstimand.SelectedIndex = idx
    End Sub

    Private Sub NormalizeFitOptionsForRunMethod(fitOptions As PsmComprehensiveFitOptions)
        Dim runMethod As PsmBackendRunMethod = fitOptions.RunMethod
        If fitOptions.StandardOptions Is Nothing Then Return

        If runMethod = PsmBackendRunMethod.OptimalPairMatching Then
            fitOptions.StandardOptions.MatchingRatio = 1
            fitOptions.StandardOptions.WithReplacement = False
            fitOptions.StandardOptions.MatchingOrder = PsmMatchingOrder.AsInput
        End If

        If Not PsmMethodCapabilities.UsesMatchingControls(runMethod) Then
            fitOptions.StandardOptions.DistanceMetric = PsmDistanceMetric.PropensityScore
            fitOptions.StandardOptions.CaliperScale = PsmCaliperScale.None
            fitOptions.StandardOptions.Caliper = Double.NaN
            fitOptions.StandardOptions.MatchingRatio = 1
            fitOptions.StandardOptions.WithReplacement = False
            fitOptions.StandardOptions.MatchingOrder = PsmMatchingOrder.AsInput
        End If
    End Sub

    Private Sub WritePsmResults(myData As PsmGuiData, fitResult As PsmComprehensiveResult)
        Dim fitOptions As PsmComprehensiveFitOptions = BuildFitOptions()
        Dim wb As Excel.Workbook = CreatePsmResultWorkbook()

        '1) The analyzed input data is always the first sheet so users can audit
        'exactly which source rows reached the backend after DataObj alignment.
        Dim inputWs As Excel.Worksheet = CreateOrAddResultSheet(wb, "Input Data", reuseFirstSheet:=True)
        WriteResultTablesToWorksheet(wb, inputWs,
                                     New List(Of ResultTable) From {PsmFormattedResultTables.AnalyzedInputDataTable(myData.Input, myData.SourceRowIds)})

        '2) Compact/general results go to a single formatted sheet.
        Dim resultWs As Excel.Worksheet = CreateOrAddResultSheet(wb, "Results", reuseFirstSheet:=False)
        WriteResultTablesToWorksheet(wb, resultWs,
                                     PsmFormattedResultTables.GeneralResultTables(fitResult, fitOptions, DataImportSummaryTable(myData)))

        '3) Diagnostics and audit outputs are separated because they can be wider
        'or more numerous than the main report.
        If Me.chkWriteDiagnostics.Checked Then
            WriteResultTablesToNewSheet(wb, "Diagnostics",
                                        PsmFormattedResultTables.DiagnosticsTables(myData.Input, fitResult, includeDefaultSensitivity:=True))

            WriteResultTablesToNewSheet(wb, "Row Audit",
                                        PsmFormattedResultTables.RowAuditTables(myData.Input, fitResult, myData.SourceRowIds))
        End If

        'The Love plot gets its own worksheet with a formatted source table and
        'an embedded Excel chart.  The source table is still written through
        'ResultTable / ProcessListofResultTables, matching the other result sheets.
        If Me.chkLovePlot.Checked Then WriteLovePlotSheetIfAvailable(wb, fitResult)

        '4) Large matched outputs receive dedicated sheets.
        If Me.chkWriteMatches.Checked Then
            WriteResultTablesToNewSheet(wb, "Matched Pairs", PsmFormattedResultTables.MatchedPairsTables(myData.Input, fitResult))

            WriteResultTablesToNewSheet(wb, "Matched Data",
                                        PsmFormattedResultTables.MatchedDatasetTables(myData.Input, fitResult, myData.SourceRowIds))
        End If

        '5) CEM-specific outputs are written only when available.
        If fitResult IsNot Nothing AndAlso fitResult.CoarsenedExactResult IsNot Nothing Then
            WriteResultTablesToNewSheet(wb, "CEM", PsmFormattedResultTables.CoarsenedExactTables(myData.Input, fitResult))
        End If

        Try
            resultWs.Activate()
        Catch
        End Try
    End Sub

    Private Function CreatePsmResultWorkbook() As Excel.Workbook
        Dim wb As Excel.Workbook = CType(AppGlobals.app.Workbooks.Add(), Excel.Workbook)

        'Excel may be configured to create multiple default sheets.  Remove the
        'extra sheets so the output workbook contains only the intentional PSM
        'sheets created below.
        Dim oldAlerts As Boolean = AppGlobals.app.DisplayAlerts
        Try
            AppGlobals.app.DisplayAlerts = False
            For i As Integer = wb.Worksheets.Count To 2 Step -1
                CType(wb.Worksheets(i), Excel.Worksheet).Delete()
            Next
        Finally
            AppGlobals.app.DisplayAlerts = oldAlerts
        End Try
        Return wb
    End Function

    Private Function CreateOrAddResultSheet(wb As Excel.Workbook, baseName As String,
                                            Optional reuseFirstSheet As Boolean = False) As Excel.Worksheet
        Dim ws As Excel.Worksheet
        If reuseFirstSheet Then
            ws = CType(wb.Worksheets(1), Excel.Worksheet)
            ws.Name = CleanWorksheetName(baseName)
        Else
            ws = CType(wb.Worksheets.Add(After:=wb.Worksheets(wb.Worksheets.Count)), Excel.Worksheet)
            ws.Name = MakeUniqueWorksheetName(wb, baseName)
        End If
        Return ws
    End Function

    Private Sub WriteResultTablesToNewSheet(wb As Excel.Workbook, sheetName As String, tables As List(Of ResultTable))
        If tables Is Nothing OrElse tables.Count = 0 Then Return
        Dim ws As Excel.Worksheet = CreateOrAddResultSheet(wb, sheetName, reuseFirstSheet:=False)
        WriteResultTablesToWorksheet(wb, ws, tables)
    End Sub

    Private Sub WriteLovePlotSheetIfAvailable(wb As Excel.Workbook, fitResult As PsmComprehensiveResult)
        If fitResult Is Nothing OrElse fitResult.LovePlotRows Is Nothing OrElse fitResult.LovePlotRows.Count = 0 Then Return

        Dim ws As Excel.Worksheet = CreateOrAddResultSheet(wb, "Love Plot", reuseFirstSheet:=False)
        Dim sourceTable As ResultTable = PsmFormattedResultTables.MatrixToResultTable(
            "Love plot source data",
            graphics.PsmLovePlotExcel.BuildPlotDataTable(fitResult.LovePlotRows),
            "The chart uses absolute standardized mean differences. Points to the right of the vertical threshold line may indicate residual imbalance.")

        WriteResultTablesToWorksheet(wb, ws, New List(Of ResultTable) From {sourceTable})

        Try
            graphics.PsmLovePlotExcel.AddChart(ws, fitResult.LovePlotRows)
            ws.Columns.AutoFit()
        Catch ex As Exception
            Dim warningTable As ResultTable = PsmFormattedResultTables.MatrixToResultTable(
                "Love plot chart warning",
                PsmResult.EmptyTable("The Love plot data were written, but the Excel chart could not be created: " & ex.Message))
            WriteResultTablesToWorksheet(wb, ws, New List(Of ResultTable) From {warningTable})
        End Try
    End Sub

    Private Sub WriteResultTablesToWorksheet(wb As Excel.Workbook, ws As Excel.Worksheet, tables As List(Of ResultTable))
        If tables Is Nothing OrElse tables.Count = 0 Then Return

        Dim writeRes As New ExcelDnaResultWriter()
        writeRes.wb = wb
        writeRes.ws = ws
        writeRes.setRowPointer(1)
        writeRes.setColumnPointer(1)

        Dim rr As New ProcessListofResultTables(tables)
        rr.writeToSheet(writeRes, True)

        Try
            ws.Columns.AutoFit()
            ws.Rows.AutoFit()
        Catch
        End Try
    End Sub

    Private Function DataImportSummaryTable(myData As PsmGuiData) As Object(,)
        Dim rows As New List(Of Object())()
        rows.Add(New Object() {"Source worksheet", If(pWorksheet Is Nothing, "", pWorksheet.Name)})
        rows.Add(New Object() {"Imported reference", myData.ImportedReference})
        rows.Add(New Object() {"Rows used", If(myData.Input Is Nothing, 0, myData.Input.RowCount)})
        rows.Add(New Object() {"Rows dropped while aligning outcome/score/exact/ID", myData.DroppedRowsDuringAlignment})
        rows.Add(New Object() {"Treatment", CStr(Me.lbTreatment.Items(0))})
        rows.Add(New Object() {"Outcome", CStr(Me.lbOutcome.Items(0))})
        rows.Add(New Object() {"Supplied score", If(Me.lbScore.Items.Count = 1, CStr(Me.lbScore.Items(0)), "")})
        rows.Add(New Object() {"ID", If(Me.lbId.Items.Count = 1, CStr(Me.lbId.Items(0)), "Excel row number")})
        rows.Add(New Object() {"Exact group(s)", String.Join(", ", GetListBoxItems(Me.lbExact).ToArray())})
        rows.Add(New Object() {"Raw covariates", String.Join(", ", GetListBoxItems(Me.lbCovariates).ToArray())})
        rows.Add(New Object() {"Expanded covariates", If(myData.Input Is Nothing OrElse myData.Input.CovariateNames Is Nothing, "", String.Join(", ", myData.Input.CovariateNames))})

        Dim table(rows.Count, 1) As Object
        table(0, 0) = "Item" : table(0, 1) = "Value"
        For i As Integer = 0 To rows.Count - 1
            table(i + 1, 0) = rows(i)(0)
            table(i + 1, 1) = rows(i)(1)
        Next
        Return table
    End Function

    Private Function GetListBoxItems(list As ListBox) As List(Of String)
        Dim out As New List(Of String)()
        For Each item As Object In list.Items
            out.Add(CStr(item))
        Next
        Return out
    End Function


    Private Function MakeUniqueWorksheetName(wb As Excel.Workbook, baseName As String) As String
        Dim cleaned As String = CleanWorksheetName(baseName)
        If cleaned.Length = 0 Then cleaned = "PSM"
        Dim candidate As String = cleaned
        Dim suffix As Integer = 1
        Do While WorksheetNameExists(wb, candidate)
            suffix += 1
            Dim tail As String = "_" & suffix.ToString()
            Dim head As String = cleaned
            If head.Length + tail.Length > 31 Then head = head.Substring(0, 31 - tail.Length)
            candidate = head & tail
        Loop
        Return candidate
    End Function

    Private Shared Function CleanWorksheetName(name As String) As String
        If String.IsNullOrWhiteSpace(name) Then Return "PSM"
        Dim invalid As Char() = New Char() {":"c, "\"c, "/"c, "?"c, "*"c, "["c, "]"c}
        Dim cleaned As String = name.Trim()
        For Each ch As Char In invalid
            cleaned = cleaned.Replace(ch, "_"c)
        Next
        If cleaned.Length > 31 Then cleaned = cleaned.Substring(0, 31)
        Return cleaned
    End Function

    Private Shared Function WorksheetNameExists(wb As Excel.Workbook, sheetName As String) As Boolean
        Try
            For Each ws As Excel.Worksheet In wb.Worksheets
                If String.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
        Catch
        End Try
        Return False
    End Function
End Class