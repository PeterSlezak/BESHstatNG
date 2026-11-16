Option Strict On
Option Explicit On
Imports System
Imports System.Drawing
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.regression
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Test.Harness46
Namespace Harness47
Friend Partial Class UiGLMProjection47
 Friend Tag As HelpTopic

 Friend WithEvents cbFamily As New FakeControl47()
 Friend WithEvents cbLink As New FakeControl47()
 Friend WithEvents cbOutputCalibrationTable As New FakeControl47()
 Friend WithEvents cbPerformClasification As New FakeControl47()
 Friend WithEvents grpClassification As New FakeControl47()
 Friend WithEvents lblCallibrationBinsN As New FakeControl47()
 Friend WithEvents lblPower As New FakeControl47()
 Friend WithEvents spinBtnCallibrationBinsN As New FakeControl47()
 Friend WithEvents tbClassificationTreshold As New FakeControl47()
 Friend WithEvents tbPower As New FakeControl47()
    Private Shared Function GetFamilyCodeFromDisplayName(familyDisplayName As String) As String
        Select Case familyDisplayName
            Case "Binomial"
                Return "Binomial"
            Case "Poisson"
                Return "Poisson"
            Case "Negative Binomial"
                Return "NegativeBinomial"
            Case "Gaussian"
                Return "Gaussian"
            Case "Gamma"
                Return "Gamma"
            Case Else
                Return String.Empty
        End Select
    End Function
    Private Sub RefreshLinkOptionsForSelectedFamily(Optional preferredLink As String = Nothing)
        Dim selectedFamilyName As String = String.Empty
        If Me.cbFamily.SelectedItem IsNot Nothing Then
            selectedFamilyName = Me.cbFamily.SelectedItem.ToString()
        End If

        Dim linkToSelect As String = preferredLink
        If String.IsNullOrWhiteSpace(linkToSelect) AndAlso Me.cbLink.SelectedItem IsNot Nothing Then
            linkToSelect = Me.cbLink.SelectedItem.ToString()
        End If

        Dim familyCode As String = GetFamilyCodeFromDisplayName(selectedFamilyName)

        Me.cbLink.BeginUpdate()
        Try
            Me.cbLink.Items.Clear()

            If String.IsNullOrWhiteSpace(familyCode) Then
                Me.cbLink.SelectedIndex = -1
                UpdatePowerLinkState()
                UpdateClassificationOptionsState()
                Return
            End If

            Dim fam As regression.Family = regression.createFamily(familyCode)

            For Each sLink As String In regression.Link.LinkList.Values
                If fam.testLink(sLink) Then
                    Me.cbLink.Items.Add(sLink)
                End If
            Next

            If Not String.IsNullOrWhiteSpace(linkToSelect) Then
                Dim existingIndex As Integer = Me.cbLink.FindStringExact(linkToSelect)
                If existingIndex >= 0 Then
                    Me.cbLink.SelectedIndex = existingIndex
                End If
            End If

            If Me.cbLink.SelectedIndex = -1 AndAlso Me.cbLink.Items.Count > 0 Then
                Me.cbLink.SelectedIndex = 0
            End If

            UpdatePowerLinkState()
            UpdateClassificationOptionsState()
        Finally
            Me.cbLink.EndUpdate()
        End Try
    End Sub
    Private Function IsCurrentBinomialGlmFamily() As Boolean
        If Me.Tag <> HelpTopic.GeneralizedLinearModelsGLM Then Return False
        If Me.cbFamily.SelectedItem Is Nothing Then Return False
        Return String.Equals(GetFamilyCodeFromDisplayName(Me.cbFamily.SelectedItem.ToString()), "Binomial", StringComparison.OrdinalIgnoreCase)
    End Function
    Private Function GetClassificationThresholdUiValue() As Double
        Dim txt As String = Me.tbClassificationTreshold.Text.Trim()
        If txt = String.Empty Then
            Me.tbClassificationTreshold.Text = SourceNumbers46.FormatUiDouble(0.5R)
            Return 0.5R
        End If
        Dim threshold As Double = SourceNumbers46.ParseUiDouble(txt, "classification threshold")
        If threshold < 0.0R OrElse threshold > 1.0R Then Throw New FormatException("Classification threshold must be between 0 and 1.")
        Return threshold
    End Function
    Private Sub UpdateClassificationOptionsState(Optional bAlreadyInit As Boolean = True)
        Dim enabledForFamily As Boolean = IsCurrentBinomialGlmFamily()

        Me.grpClassification.Enabled = enabledForFamily
        Me.cbPerformClasification.Enabled = enabledForFamily
        Me.lblCallibrationBinsN.Enabled = enabledForFamily AndAlso Me.cbOutputCalibrationTable.Checked
        Me.spinBtnCallibrationBinsN.Enabled = enabledForFamily AndAlso Me.cbOutputCalibrationTable.Checked

        If Not enabledForFamily Then
            If bAlreadyInit Then setTextBoxProperties(Me.tbClassificationTreshold, Color.White, String.Empty)
        ElseIf String.IsNullOrWhiteSpace(Me.tbClassificationTreshold.Text) Then
            Me.tbClassificationTreshold.Text = SourceNumbers46.FormatUiDouble(0.5R)
        End If

        If Me.cbPerformClasification.Checked And enabledForFamily Then
            Me.grpClassification.Enabled = True
        ElseIf Not Me.cbPerformClasification.Checked And enabledForFamily Then
            Me.grpClassification.Enabled = False
        End If
    End Sub
    Private Sub UpdatePowerLinkState()
        Dim usePowerLink As Boolean =
            Me.cbLink.SelectedItem IsNot Nothing AndAlso
            Me.cbLink.SelectedItem.ToString() = "Power"

        Me.lblPower.Enabled = usePowerLink
        Me.tbPower.Enabled = usePowerLink
    End Sub
    Private Sub cbFamily_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbFamily.SelectedIndexChanged
        If Me.cbFamily.SelectedItem Is Nothing Then Return
        RefreshLinkOptionsForSelectedFamily(GetCanonicalLinkFromDisplayName(Me.cbFamily.SelectedItem.ToString()))
    End Sub
    Private Sub cbLink_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbLink.SelectedIndexChanged
        UpdatePowerLinkState()
    End Sub
    Private Sub cbOutputCalibrationTable_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbOutputCalibrationTable.CheckedChanged
        UpdateClassificationOptionsState()
    End Sub
    Private Sub cbPerformClasification_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbPerformClasification.CheckedChanged
        If Me.cbPerformClasification.Checked And IsCurrentBinomialGlmFamily() Then
            Me.grpClassification.Enabled = True
        ElseIf Not Me.cbPerformClasification.Checked And IsCurrentBinomialGlmFamily() Then
            Me.grpClassification.Enabled = False
        End If
    End Sub
End Class
Friend Partial Class Ui13GEEProjection47
 Friend Tag As HelpTopic

 Friend WithEvents cbFamily As New FakeControl47()
 Friend WithEvents cbLink As New FakeControl47()
 Friend WithEvents cbOutputCalibrationTable As New FakeControl47()
 Friend WithEvents cbPerformClasification As New FakeControl47()
 Friend WithEvents grpClassification As New FakeControl47()
 Friend WithEvents lblCallibrationBinsN As New FakeControl47()
 Friend WithEvents lblPower As New FakeControl47()
 Friend WithEvents spinBtnCallibrationBinsN As New FakeControl47()
 Friend WithEvents tbClassificationTreshold As New FakeControl47()
 Friend WithEvents tbPower As New FakeControl47()
    Private Shared Function GetFamilyCodeFromDisplayName(familyDisplayName As String) As String
        Return GeeWorkflowPolicy.FamilyCode(familyDisplayName)
    End Function
    Private Sub RefreshLinkOptionsForSelectedFamily(Optional preferredLink As String = Nothing)
        Dim selectedFamilyName As String = String.Empty
        If Me.cbFamily.SelectedItem IsNot Nothing Then
            selectedFamilyName = Me.cbFamily.SelectedItem.ToString()
        End If

        Dim linkToSelect As String = preferredLink
        If String.IsNullOrWhiteSpace(linkToSelect) AndAlso Me.cbLink.SelectedItem IsNot Nothing Then
            linkToSelect = Me.cbLink.SelectedItem.ToString()
        End If

        Dim familyCode As String = GetFamilyCodeFromDisplayName(selectedFamilyName)

        Me.cbLink.BeginUpdate()
        Try
            Me.cbLink.Items.Clear()

            If String.IsNullOrWhiteSpace(familyCode) Then
                Me.cbLink.SelectedIndex = -1
                UpdatePowerLinkState()
                UpdateClassificationOptionsState()
                Return
            End If

            For Each sLink As String In GeeWorkflowPolicy.SupportedLinks(familyCode)
                Me.cbLink.Items.Add(sLink)
            Next

            If Not String.IsNullOrWhiteSpace(linkToSelect) Then
                Dim existingIndex As Integer = Me.cbLink.FindStringExact(linkToSelect)
                If existingIndex >= 0 Then
                    Me.cbLink.SelectedIndex = existingIndex
                End If
            End If

            If Me.cbLink.SelectedIndex = -1 AndAlso Me.cbLink.Items.Count > 0 Then
                Me.cbLink.SelectedIndex = 0
            End If

            UpdatePowerLinkState()
            UpdateClassificationOptionsState()

        Finally
            Me.cbLink.EndUpdate()
        End Try
    End Sub
    Private Function IsCurrentBinomialGeeFamily() As Boolean
        If Me.Tag <> HelpTopic.GeneralizedEstimatingEquationsGEE Then Return False
        If Me.cbFamily.SelectedItem Is Nothing Then Return False
        Return String.Equals(GetFamilyCodeFromDisplayName(Me.cbFamily.SelectedItem.ToString()), "Binomial", StringComparison.OrdinalIgnoreCase)
    End Function
    Private Function GetClassificationThresholdUiValue() As Double
        Dim txt As String = Me.tbClassificationTreshold.Text.Trim()
        If txt = String.Empty Then
            Me.tbClassificationTreshold.Text = SourceNumbers46.FormatUiDouble(0.5R)
            Return 0.5R
        End If

        Dim threshold As Double = SourceNumbers46.ParseUiDouble(txt, "classification threshold")
        If threshold < 0.0R OrElse threshold > 1.0R Then
            Throw New FormatException("Classification threshold must be between 0 and 1.")
        End If
        Return threshold
    End Function
    Private Sub UpdateClassificationOptionsState(Optional bAlreadyInit As Boolean = True)
        Dim enabledForFamily As Boolean = IsCurrentBinomialGeeFamily()

        Me.grpClassification.Enabled = enabledForFamily
        Me.cbPerformClasification.Enabled = enabledForFamily
        Me.lblCallibrationBinsN.Enabled = enabledForFamily AndAlso Me.cbOutputCalibrationTable.Checked
        Me.spinBtnCallibrationBinsN.Enabled = enabledForFamily AndAlso Me.cbOutputCalibrationTable.Checked

        If Not enabledForFamily Then
            If bAlreadyInit Then setTextBoxProperties(Me.tbClassificationTreshold, Color.White, String.Empty)
        ElseIf String.IsNullOrWhiteSpace(Me.tbClassificationTreshold.Text) Then
            Me.tbClassificationTreshold.Text = SourceNumbers46.FormatUiDouble(0.5R)
        End If

        If Me.cbPerformClasification.Checked And enabledForFamily Then
            Me.grpClassification.Enabled = True
        ElseIf Not Me.cbPerformClasification.Checked And enabledForFamily Then
            Me.grpClassification.Enabled = False
        End If
    End Sub
    Private Sub UpdatePowerLinkState()
        Dim usePowerLink As Boolean =
            Me.cbLink.SelectedItem IsNot Nothing AndAlso
            Me.cbLink.SelectedItem.ToString() = "Power"

        Me.lblPower.Enabled = usePowerLink
        Me.tbPower.Enabled = usePowerLink
    End Sub
    Private Sub cbFamily_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbFamily.SelectedIndexChanged
        If Me.cbFamily.SelectedItem Is Nothing Then Return
        RefreshLinkOptionsForSelectedFamily(regression.GetCanonicalLinkFromDisplayName(Me.cbFamily.SelectedItem.ToString()))
    End Sub
    Private Sub cbLink_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbLink.SelectedIndexChanged
        UpdatePowerLinkState()
    End Sub
    Private Sub cbOutputCalibrationTable_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbOutputCalibrationTable.CheckedChanged
        UpdateClassificationOptionsState()
    End Sub
    Private Sub cbPerformClasification_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbPerformClasification.CheckedChanged
        If Me.cbPerformClasification.Checked And IsCurrentBinomialGeeFamily() Then
            Me.grpClassification.Enabled = True
        ElseIf Not Me.cbPerformClasification.Checked And IsCurrentBinomialGeeFamily() Then
            Me.grpClassification.Enabled = False
        End If
    End Sub
End Class
Friend Partial Class Ui18MMRMProjection47
 Friend WithEvents cbMMRMCovOptimizerMode As New FakeControl47()
 Friend WithEvents cbMMRMCovGradientMode As New FakeControl47()
 Friend Tag As HelpTopic
    Private Const MMRM_PROGRESS_REFRESH_INTERVAL_MS As Double = 100.0
    Private Const MMRM_GROUP_AUTO As String = "(Auto)"
    Private Const MMRM_GROUP_NONE As String = "(None)"
    Private Const MMRM_BASELINE_SMALLEST As String = "(Smallest)"
    Private Const MMRM_CONTROL_FIRST As String = "(First)"
    Private Const MMRM_MODE_NONE As String = "None"
    Private Const MMRM_MODE_PAIRWISE As String = "Pairwise among group levels"
    Private Const MMRM_MODE_CONTROL As String = "Each group vs control"
    Private Const MMRM_MODE_SELECTED As String = "Selected comparison only"
    Private Const MMRM_DIR_HIGHER_MINUS_LOWER As String = "Higher level - lower level"
    Private Const MMRM_DIR_TREATMENT_MINUS_CONTROL As String = "Treatment - control"
    Private Const MMRM_DIR_CONTROL_MINUS_TREATMENT As String = "Control - treatment"
    Private Const MMRM_LSMEANS_OBSERVED_GRID As String = "Observed design grid"
    Private Const MMRM_LSMEANS_REFERENCE_GRID As String = "Reference grid"
    Private Const MMRM_RG_WEIGHT_EQUAL As String = "Equal class-cell weights"
    Private Const MMRM_RG_WEIGHT_OBSERVED As String = "Observed class-cell weights"
    Private Const MMRM_RG_COVARIATE_MEANS As String = "Continuous covariates at observed means"
    Private Const MMRM_RG_COVARIATE_ZERO As String = "Continuous covariates at 0"
    Private Const MMRM_MULT_NONE As String = "None"
    Private Const MMRM_MULT_BONFERRONI As String = "Bonferroni"
    Private Const MMRM_MULT_HOLM As String = "Holm"
    Private Const MMRM_MULT_SIDAK As String = "Sidak"
    Private Const MMRM_OPT_AI As String = "AI/Fisher scoring (default)"
    Private Const MMRM_OPT_BFGS_AUTO As String = "Projected BFGS (auto gradient)"
    Private Const MMRM_OPT_BFGS_ANALYTIC As String = "Projected BFGS (analytic gradient)"
    Private Const MMRM_OPT_BFGS_NUMERICAL As String = "Projected BFGS (finite-difference gradient)"
    Private Const MMRM_GRAD_AUTO As String = "Auto (analytic where available)"
    Private Const MMRM_GRAD_ANALYTIC As String = "Analytic score"
    Private Const MMRM_GRAD_VALIDATE As String = "Analytic score + finite-difference validation"
    Private Const MMRM_GRAD_NUMERICAL As String = "Numerical finite difference"
 Friend WithEvents cbMMRMBaselineVisit As New FakeControl47()
 Friend WithEvents cbMMRMComparisonLevel As New FakeControl47()
 Friend WithEvents cbMMRMContrastDirection As New FakeControl47()
 Friend WithEvents cbMMRMContrastMode As New FakeControl47()
 Friend WithEvents cbMMRMControlLevel As New FakeControl47()
 Friend WithEvents cbMMRMGroupingFactor As New FakeControl47()
 Friend WithEvents cbMMRMLSMeansMode As New FakeControl47()
 Friend WithEvents cbMMRMMultiplicity As New FakeControl47()
 Friend WithEvents cbMMRMRefGridCovariates As New FakeControl47()
 Friend WithEvents cbMMRMRefGridWeighting As New FakeControl47()
 Friend WithEvents ckMMRMChangeFromBaseline As New FakeControl47()
 Friend WithEvents ckMMRMClassInfo As New FakeControl47()
 Friend WithEvents ckMMRMDiffInChange As New FakeControl47()
 Friend WithEvents ckMMRMEstimatedMeans As New FakeControl47()
 Friend WithEvents lblMMRMComparisonLevel As New FakeControl47()
 Friend WithEvents lblMMRMLSMeansMode As New FakeControl47()
 Friend WithEvents lblMMRMMultiplicity As New FakeControl47()
 Friend WithEvents lblMMRMRefGridCovariates As New FakeControl47()
 Friend WithEvents lblMMRMRefGridWeighting As New FakeControl47()
    Private Sub UpdateMMRMContrastControlEnabledState()
        Dim meansOn As Boolean = Me.ckMMRMEstimatedMeans.Checked
        Dim classOn As Boolean = Me.ckMMRMClassInfo.Checked

        Dim groupSelection As String = If(Me.cbMMRMGroupingFactor.SelectedItem Is Nothing,
                                      MMRM_GROUP_AUTO,
                                      CStr(Me.cbMMRMGroupingFactor.SelectedItem))

        Dim groupPossible As Boolean =
        Not String.Equals(groupSelection, MMRM_GROUP_NONE, StringComparison.OrdinalIgnoreCase)

        Dim contrastMode As String = If(Me.cbMMRMContrastMode.SelectedItem Is Nothing,
                                    MMRM_MODE_NONE,
                                    CStr(Me.cbMMRMContrastMode.SelectedItem))

        Dim isNoneMode As Boolean =
        String.Equals(contrastMode, MMRM_MODE_NONE, StringComparison.OrdinalIgnoreCase)

        Dim isControlMode As Boolean =
        String.Equals(contrastMode, MMRM_MODE_CONTROL, StringComparison.OrdinalIgnoreCase)

        Dim isSelectedMode As Boolean =
        String.Equals(contrastMode, MMRM_MODE_SELECTED, StringComparison.OrdinalIgnoreCase)

        Dim needsControlLevel As Boolean = isControlMode OrElse isSelectedMode
        Dim needsComparisonLevel As Boolean = isSelectedMode
        Dim needsDirection As Boolean = (isControlMode OrElse isSelectedMode)

        Me.cbMMRMGroupingFactor.Enabled = classOn OrElse meansOn
        Me.cbMMRMBaselineVisit.Enabled = meansOn AndAlso Me.ckMMRMChangeFromBaseline.Checked
        Me.cbMMRMContrastMode.Enabled = meansOn AndAlso groupPossible
        Me.cbMMRMControlLevel.Enabled = meansOn AndAlso groupPossible AndAlso needsControlLevel
        Me.cbMMRMComparisonLevel.Enabled = meansOn AndAlso groupPossible AndAlso needsComparisonLevel
        Me.lblMMRMComparisonLevel.Enabled = Me.cbMMRMComparisonLevel.Enabled
        Me.cbMMRMContrastDirection.Enabled = meansOn AndAlso groupPossible AndAlso needsDirection

        Me.ckMMRMChangeFromBaseline.Enabled = meansOn
        Me.ckMMRMDiffInChange.Enabled = meansOn AndAlso Me.ckMMRMChangeFromBaseline.Checked AndAlso groupPossible AndAlso Not isNoneMode

        Dim referenceGridControlsOn As Boolean = meansOn AndAlso IsMMRMReferenceGridModeSelected()
        Me.cbMMRMLSMeansMode.Enabled = meansOn
        Me.lblMMRMLSMeansMode.Enabled = meansOn
        Me.cbMMRMRefGridWeighting.Enabled = referenceGridControlsOn
        Me.lblMMRMRefGridWeighting.Enabled = referenceGridControlsOn
        Me.cbMMRMRefGridCovariates.Enabled = referenceGridControlsOn
        Me.lblMMRMRefGridCovariates.Enabled = referenceGridControlsOn
        Me.cbMMRMMultiplicity.Enabled = meansOn AndAlso groupPossible AndAlso Not isNoneMode
        Me.lblMMRMMultiplicity.Enabled = meansOn AndAlso groupPossible AndAlso Not isNoneMode

    End Sub
    Private Function IsMMRMReferenceGridModeSelected() As Boolean
        Return Me.cbMMRMLSMeansMode IsNot Nothing AndAlso MixedModelReportPolicy.MeansMode(SelectedComboText(Me.cbMMRMLSMeansMode, MMRM_LSMEANS_OBSERVED_GRID)) = MixedModelMeansMode.ReferenceGrid
    End Function
    Private Function SelectedComboText(cb As FakeControl47, fallback As String) As String
        If cb Is Nothing OrElse cb.SelectedItem Is Nothing Then Return fallback
        Dim s As String = CStr(cb.SelectedItem)
        If String.IsNullOrWhiteSpace(s) Then Return fallback
        Return s
    End Function
    Private Sub cbMMRMControlLevel_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbMMRMControlLevel.SelectedIndexChanged
        Try
            UpdateMMRMContrastControlEnabledState()
        Catch ex As Exception
            FakeLogger47.Warn("cbMMRMControlLevel_SelectedIndexChanged failed: " & ex.ToString())
        End Try
    End Sub
    Private Sub cbMMRMContrastMode_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbMMRMContrastMode.SelectedIndexChanged
        Try
            UpdateMMRMContrastControlEnabledState()
        Catch ex As Exception
            FakeLogger47.Warn("cbMMRMContrastMode_SelectedIndexChanged failed: " & ex.ToString())
        End Try
    End Sub
    Private Sub MMRMReferenceGridControlChanged(sender As Object, e As System.EventArgs) Handles cbMMRMLSMeansMode.SelectedIndexChanged,
        cbMMRMRefGridWeighting.SelectedIndexChanged,
        cbMMRMRefGridCovariates.SelectedIndexChanged

        Try
            UpdateMMRMContrastControlEnabledState()
        Catch ex As Exception
            FakeLogger47.Warn("MMRMReferenceGridControlChanged failed: " & ex.Message)
        End Try
    End Sub
    Private Sub MMRMContrastControl_CheckedOrSelectedChanged(sender As Object, e As System.EventArgs) Handles ckMMRMClassInfo.CheckedChanged,
        ckMMRMEstimatedMeans.CheckedChanged,
        ckMMRMDiffInChange.CheckedChanged,
        ckMMRMChangeFromBaseline.CheckedChanged,
        cbMMRMGroupingFactor.SelectedIndexChanged,
        cbMMRMContrastDirection.SelectedIndexChanged,
        cbMMRMComparisonLevel.SelectedIndexChanged,
        cbMMRMBaselineVisit.SelectedIndexChanged,
        cbMMRMContrastMode.SelectedIndexChanged,
        cbMMRMControlLevel.SelectedIndexChanged

        Try
            UpdateMMRMContrastControlEnabledState()

            ' When the grouping factor changes, observed group levels must be refreshed.
            ' For other controls this is harmless and keeps the UI current.
            If sender Is Me.cbMMRMGroupingFactor OrElse sender Is Me.cbMMRMContrastMode Then
                FakeAcquisition47.RefreshMMRM()
            End If

        Catch ex As Exception
            FakeLogger47.Warn("MMRMContrastControl_CheckedOrSelectedChanged failed: " & ex.Message)
        End Try
    End Sub
    Private Function ParseMMRMCovarianceOptimizerMode(selection As String) As regression.MixedModelCovarianceOptimizerMode
        If String.IsNullOrWhiteSpace(selection) Then Return regression.MixedModelCovarianceOptimizerMode.AverageInformationReml
        Return regression.MixedModelFrontEndHelpers.ParseCovarianceOptimizerMode(selection, regression.MixedModelCovarianceOptimizerMode.ProjectedBfgs)
    End Function
    Private Function ParseMMRMCovarianceGradientMode(selection As String) As regression.MixedModelCovarianceGradientMode
        Return regression.MixedModelFrontEndHelpers.ParseCovarianceGradientMode(selection, regression.MixedModelCovarianceGradientMode.Auto)
    End Function
    Private Sub ApplyMMRMCovarianceOptimizerSelections(ByRef ctl As regression.MixedModelControl)
        MixedModelWorkflowPolicy.ApplyOptimizer(MixedModelWorkflowKind.MMRM, ctl,
            SelectedComboText(Me.cbMMRMCovOptimizerMode, MMRM_OPT_AI),
            SelectedComboText(Me.cbMMRMCovGradientMode, MMRM_GRAD_AUTO))
    End Sub
End Class
Friend Partial Class Ui19LMMProjection47
 Friend WithEvents cbLMMCovOptimizerMode As New FakeControl47()
 Friend WithEvents cbLMMCovGradientMode As New FakeControl47()
 Friend Tag As HelpTopic
    Private Const LMM_PROGRESS_REFRESH_INTERVAL_MS As Double = 100.0
    Private Const LMM_OPT_AI As String = "AI/Fisher scoring (default)"
    Private Const LMM_OPT_BFGS_AUTO As String = "Projected BFGS (auto gradient)"
    Private Const LMM_OPT_BFGS_ANALYTIC As String = "Projected BFGS (analytic gradient)"
    Private Const LMM_OPT_BFGS_NUMERICAL As String = "Projected BFGS (finite-difference gradient)"
    Private Const LMM_GRAD_AUTO As String = "Auto (analytic where available)"
    Private Const LMM_GRAD_ANALYTIC As String = "Analytic score"
    Private Const LMM_GRAD_VALIDATE As String = "Analytic score + finite-difference validation"
    Private Const LMM_GRAD_NUMERICAL As String = "Numerical finite difference"
 Friend WithEvents btAddRandomEffectCategoricalFactor As New FakeControl47()
 Friend WithEvents btnRandom2Interactions As New FakeControl47()
 Friend WithEvents btnRandomCustomInteraction As New FakeControl47()
 Friend WithEvents btnRandomPoly As New FakeControl47()
 Friend WithEvents cbCovarStruct As New FakeControl47()
 Friend WithEvents cbRandomCovarStruct As New FakeControl47()
 Friend WithEvents spinBtnRandomPoly As New FakeControl47()
    Private Sub UpdateLMMCovarianceDependentControls()
        Try
            Dim needsVisit As Boolean = Me.cbCovarStruct.SelectedItem IsNot Nothing AndAlso regression.MixedModelFrontEndHelpers.ResidualStructureRequiresVisit(CStr(Me.cbCovarStruct.SelectedItem))

            'Keep the general random-effect authoring controls enabled. Buttons that can expand
            'to multiple random-effect columns promote the G-side covariance to VC/Diag before adding the term.
            Me.btnRandom2Interactions.Enabled = True
            Me.btnRandomCustomInteraction.Enabled = True
            Me.btAddRandomEffectCategoricalFactor.Enabled = True
            Me.btnRandomPoly.Enabled = True
            Me.spinBtnRandomPoly.Enabled = True
        Catch
        End Try
    End Sub
    Private Sub cbCovarStruct_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbCovarStruct.SelectedIndexChanged
        UpdateLMMCovarianceDependentControls()
    End Sub
    Private Sub cbRandomCovarStruct_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbRandomCovarStruct.SelectedIndexChanged
        UpdateLMMCovarianceDependentControls()
    End Sub
    Private Function SelectedComboText(cb As FakeControl47, fallback As String) As String
        If cb Is Nothing OrElse cb.SelectedItem Is Nothing Then Return fallback
        Dim s As String = CStr(cb.SelectedItem)
        If String.IsNullOrWhiteSpace(s) Then Return fallback
        Return s
    End Function
    Private Sub ApplyLMMCovarianceOptimizerSelections(ByRef ctl As regression.MixedModelControl)
        MixedModelWorkflowPolicy.ApplyOptimizer(MixedModelWorkflowKind.LMM, ctl,
            SelectedComboText(Me.cbLMMCovOptimizerMode, LMM_OPT_AI),
            SelectedComboText(Me.cbLMMCovGradientMode, LMM_GRAD_AUTO))
    End Sub
End Class
Friend Partial Class Ui11PCAProjection47
 Friend Tag As HelpTopic

 Friend WithEvents cbDAPriors As New FakeControl47()
 Friend WithEvents cbDAValidation As New FakeControl47()
 Friend WithEvents cbFACommunalityInit As New FakeControl47()
 Friend WithEvents cbFAExtraction As New FakeControl47()
 Friend WithEvents cbFARotation As New FakeControl47()
 Friend WithEvents cbHierarchicalDistance As New FakeControl47()
 Friend WithEvents ckDAStratified As New FakeControl47()
 Friend WithEvents ckFAKaiserNormalization As New FakeControl47()
 Friend WithEvents lblDAFolds As New FakeControl47()
 Friend WithEvents lblDAHoldoutFraction As New FakeControl47()
 Friend WithEvents lblDASeed As New FakeControl47()
 Friend WithEvents lblDAUserPriors As New FakeControl47()
 Friend WithEvents lblFAPromaxPower As New FakeControl47()
 Friend WithEvents lblHierarchicalMinkowskiPower As New FakeControl47()
 Friend WithEvents nudDAFolds As New FakeControl47()
 Friend WithEvents nudFAEigen As New FakeControl47()
 Friend WithEvents nudFAFactors As New FakeControl47()
 Friend WithEvents nudFAPromaxPower As New FakeControl47()
 Friend WithEvents nudFAVariance As New FakeControl47()
 Friend WithEvents nudHierarchicalClusters As New FakeControl47()
 Friend WithEvents optFAExtractEigen As New FakeControl47()
 Friend WithEvents optFAExtractFixed As New FakeControl47()
 Friend WithEvents optFAExtractVariance As New FakeControl47()
 Friend WithEvents optHierarchicalCutByClusters As New FakeControl47()
 Friend WithEvents optHierarchicalCutByHeight As New FakeControl47()
 Friend WithEvents tbDAHoldoutFraction As New FakeControl47()
 Friend WithEvents tbDARegularization As New FakeControl47()
 Friend WithEvents tbDASeed As New FakeControl47()
 Friend WithEvents tbDAUserPriors As New FakeControl47()
 Friend WithEvents tbHierarchicalCutHeight As New FakeControl47()
 Friend WithEvents tbHierarchicalMinkowskiPower As New FakeControl47()
 Friend Sub New()
AddHandler Me.cbHierarchicalDistance.SelectedIndexChanged, AddressOf Me.HierarchicalDistanceChanged
AddHandler Me.optHierarchicalCutByClusters.CheckedChanged, AddressOf Me.HierarchicalMembershipModeChanged
AddHandler Me.optHierarchicalCutByHeight.CheckedChanged, AddressOf Me.HierarchicalMembershipModeChanged
AddHandler Me.cbFAExtraction.SelectedIndexChanged, AddressOf Me.FactorAnalysisExtractionChanged
AddHandler Me.cbFARotation.SelectedIndexChanged, AddressOf Me.FactorAnalysisRotationChanged
AddHandler Me.optFAExtractFixed.CheckedChanged, AddressOf Me.FactorAnalysisRetentionChanged
AddHandler Me.optFAExtractEigen.CheckedChanged, AddressOf Me.FactorAnalysisRetentionChanged
AddHandler Me.optFAExtractVariance.CheckedChanged, AddressOf Me.FactorAnalysisRetentionChanged
AddHandler Me.cbDAPriors.SelectedIndexChanged, AddressOf Me.DiscriminantPriorModeChanged
AddHandler Me.cbDAValidation.SelectedIndexChanged, AddressOf Me.DiscriminantValidationChanged
 End Sub
    Private Sub HierarchicalDistanceChanged(sender As Object, e As System.EventArgs)
        Dim useMinkowski As Boolean = (Me.GetSelectedHierarchicalDistanceMetric() = Multivariate.HierarchicalDistanceMetric.Minkowski)
        If Me.lblHierarchicalMinkowskiPower IsNot Nothing Then Me.lblHierarchicalMinkowskiPower.Enabled = useMinkowski
        If Me.tbHierarchicalMinkowskiPower IsNot Nothing Then Me.tbHierarchicalMinkowskiPower.Enabled = useMinkowski
    End Sub
    Private Sub HierarchicalMembershipModeChanged(sender As Object, e As System.EventArgs)
        Dim byClusters As Boolean = (Me.optHierarchicalCutByClusters IsNot Nothing AndAlso Me.optHierarchicalCutByClusters.Checked)
        If Me.nudHierarchicalClusters IsNot Nothing Then Me.nudHierarchicalClusters.Enabled = byClusters
        If Me.tbHierarchicalCutHeight IsNot Nothing Then Me.tbHierarchicalCutHeight.Enabled = Not byClusters
    End Sub
    Private Function GetSelectedHierarchicalDistanceMetric() As Multivariate.HierarchicalDistanceMetric
        Return MultivariateLegacyChoices.GetSelectedHierarchicalDistanceMetric(Me.cbHierarchicalDistance.SelectedIndex)
    End Function
    Private Sub FactorAnalysisExtractionChanged(sender As Object, e As System.EventArgs)
        Dim method = Me.GetSelectedFAExtractionMethod()
        Dim needsCommunalityStart As Boolean = (method <> Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents)

        If Me.cbFACommunalityInit IsNot Nothing Then Me.cbFACommunalityInit.Enabled = needsCommunalityStart
    End Sub
    Private Sub FactorAnalysisRotationChanged(sender As Object, e As System.EventArgs)
        Dim rotation = Me.GetSelectedFARotationMethod()
        Dim hasRotation As Boolean = (rotation <> Multivariate.FactorAnalysisRotationMethod.None)
        Dim isPromax As Boolean = (rotation = Multivariate.FactorAnalysisRotationMethod.Promax)

        If Me.ckFAKaiserNormalization IsNot Nothing Then Me.ckFAKaiserNormalization.Enabled = hasRotation
        If Me.lblFAPromaxPower IsNot Nothing Then Me.lblFAPromaxPower.Enabled = isPromax
        If Me.nudFAPromaxPower IsNot Nothing Then Me.nudFAPromaxPower.Enabled = isPromax
    End Sub
    Private Sub FactorAnalysisRetentionChanged(sender As Object, e As System.EventArgs)
        If Me.nudFAFactors IsNot Nothing Then Me.nudFAFactors.Enabled = Me.optFAExtractFixed.Checked
        If Me.nudFAEigen IsNot Nothing Then Me.nudFAEigen.Enabled = Me.optFAExtractEigen.Checked
        If Me.nudFAVariance IsNot Nothing Then Me.nudFAVariance.Enabled = Me.optFAExtractVariance.Checked
    End Sub
    Private Function GetSelectedFAExtractionMethod() As Multivariate.FactorAnalysisExtractionMethod
        Return MultivariateLegacyChoices.GetSelectedFAExtractionMethod(Me.cbFAExtraction.SelectedIndex)
    End Function
    Private Function GetSelectedFARotationMethod() As Multivariate.FactorAnalysisRotationMethod
        Return MultivariateLegacyChoices.GetSelectedFARotationMethod(Me.cbFARotation.SelectedIndex)
    End Function
    Private Function GetSelectedFARetentionMethod() As Multivariate.FactorAnalysisRetentionMethod
        Return MultivariateLegacyChoices.GetSelectedFARetentionMethod(Me.optFAExtractEigen.Checked, Me.optFAExtractVariance.Checked)
    End Function
    Private Function GetSelectedFARetentionValue() As Double
        Return MultivariateWorkflowPolicy.FactorRetentionValue(Me.optFAExtractEigen.Checked, Me.optFAExtractVariance.Checked,
            CDbl(Me.nudFAEigen.Value), CDbl(Me.nudFAVariance.Value), CDbl(Me.nudFAFactors.Value))
    End Function
    Private Sub DiscriminantPriorModeChanged(sender As Object, e As System.EventArgs)
        Me.UpdateDiscriminantOptionStates()
    End Sub
    Private Sub DiscriminantValidationChanged(sender As Object, e As System.EventArgs)
        Me.UpdateDiscriminantOptionStates()
    End Sub
    Private Function GetSelectedDiscriminantPriorMode() As Multivariate.DiscriminantPriorMode
        Return MultivariateLegacyChoices.GetSelectedDiscriminantPriorMode(If(Me.cbDAPriors Is Nothing OrElse Me.cbDAPriors.SelectedItem Is Nothing, String.Empty, CStr(Me.cbDAPriors.SelectedItem)))
    End Function
    Private Function GetSelectedDiscriminantValidationMode() As Multivariate.DiscriminantValidationMode
        Return MultivariateLegacyChoices.GetSelectedDiscriminantValidationMode(If(Me.cbDAValidation Is Nothing OrElse Me.cbDAValidation.SelectedItem Is Nothing, String.Empty, CStr(Me.cbDAValidation.SelectedItem)))
    End Function
    Private Function GetSelectedDiscriminantRandomSeed() As Integer
        Dim txt As String = Me.tbDASeed.Text
        If txt.Trim() = String.Empty Then Return Integer.MinValue
        Return FakeText47.ParseUiInteger(txt, "Random seed")
    End Function
    Private Function GetSelectedDiscriminantRegularization() As Double
        Dim txt As String = Me.tbDARegularization.Text
        If txt.Trim() = String.Empty Then Return 0.00000001
        Return SourceNumbers46.ParseUiDouble(txt, "Covariance regularization")
    End Function
    Private Sub UpdateDiscriminantOptionStates()
        If Me.Tag <> HelpTopic.DiscriminantAnalysis Then Exit Sub

        Dim priorMode As Multivariate.DiscriminantPriorMode = Me.GetSelectedDiscriminantPriorMode()
        Dim validationMode As Multivariate.DiscriminantValidationMode = Me.GetSelectedDiscriminantValidationMode()

        Dim enableUserPriors As Boolean = (priorMode = Multivariate.DiscriminantPriorMode.UserSpecified)
        Me.tbDAUserPriors.Enabled = enableUserPriors
        Me.lblDAUserPriors.Enabled = enableUserPriors

        Dim enableFolds As Boolean = False
        Dim enableHoldout As Boolean = False
        Dim enableResamplingOptions As Boolean = False

        Select Case validationMode
            Case Multivariate.DiscriminantValidationMode.KFold
                enableFolds = True
                enableResamplingOptions = True

            Case Multivariate.DiscriminantValidationMode.Holdout
                enableHoldout = True
                enableResamplingOptions = True

            Case Else
                ' None / Leave-one-out: keep all subordinate validation controls disabled.
        End Select

        Me.nudDAFolds.Enabled = enableFolds
        Me.lblDAFolds.Enabled = enableFolds

        Me.tbDAHoldoutFraction.Enabled = enableHoldout
        Me.lblDAHoldoutFraction.Enabled = enableHoldout

        Me.ckDAStratified.Enabled = enableResamplingOptions
        Me.tbDASeed.Enabled = enableResamplingOptions
        Me.lblDASeed.Enabled = enableResamplingOptions
    End Sub
End Class
End Namespace
