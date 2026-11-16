Option Strict On
Option Explicit On
Imports System
Imports System.Drawing
Imports System.Globalization
Imports BESHStatNG.ExcelCommon
Imports ComboBox = BESHStatNG.Test.Harness46.FakeControl46
Namespace Harness46
Friend Class UibyIDProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend Const BoxPlotLegacyPaletteIndex As Integer = 4
 Friend WithEvents RefEdit1 As New FakeRefEdit46()
 Friend WithEvents RefEdit2 As New FakeRefEdit46()
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents TabControl1 As New FakeControl46()
 Friend WithEvents cbBoxHorizontalGridlines As New FakeControl46()
 Friend WithEvents cbBoxOutline As New FakeControl46()
 Friend WithEvents ckBartlett As New FakeControl46()
 Friend WithEvents ckBoxConnectMeans As New FakeControl46()
 Friend WithEvents ckCDFExtendEmpirical As New FakeControl46()
 Friend WithEvents ckCDFPercentileLabels As New FakeControl46()
 Friend WithEvents ckCDFPercentileLines As New FakeControl46()
 Friend WithEvents ckCDFShowLegend As New FakeControl46()
 Friend WithEvents ckFlignerKilleen As New FakeControl46()
 Friend WithEvents ckLevene As New FakeControl46()
 Friend WithEvents ckSquaredRanks As New FakeControl46()
 Friend WithEvents cmbBoxPalette As New FakeControl46()
 Friend WithEvents cmbCDFDistribution As New FakeControl46()
 Friend WithEvents cmbCDFEmpiricalMethod As New FakeControl46()
 Friend WithEvents cmbCDFLegendMode As New FakeControl46()
 Friend WithEvents cmbCDFPlotType As New FakeControl46()
 Friend WithEvents cmbViolinBandwidth As New FakeControl46()
 Friend WithEvents cmdBoxIndividualObs As New FakeControl46()
 Friend WithEvents cmdBoxMean As New FakeControl46()
 Friend WithEvents lblAlpha_UTT As New FakeControl46()
 Friend WithEvents lblBoxChartHeight As New FakeControl46()
 Friend WithEvents lblBoxChartWidth As New FakeControl46()
 Friend WithEvents lblBoxFillTransparency As New FakeControl46()
 Friend WithEvents lblCDFDistribution As New FakeControl46()
 Friend WithEvents lblCDFEmpiricalMethod As New FakeControl46()
 Friend WithEvents lblCDFLegendMode As New FakeControl46()
 Friend WithEvents lblCDFPercentiles As New FakeControl46()
 Friend WithEvents lblCatHistSeriesOverlap As New FakeControl46()
 Friend WithEvents lblMarginHint_UTT As New FakeControl46()
 Friend WithEvents lblMargin_UTT As New FakeControl46()
 Friend WithEvents lblRefedit1 As New FakeControl46()
 Friend WithEvents lblRefedit2 As New FakeControl46()
 Friend WithEvents lblViolinBandwidthAdjustment As New FakeControl46()
 Friend WithEvents nudBoxChartHeight As New FakeControl46()
 Friend WithEvents nudBoxChartWidth As New FakeControl46()
 Friend WithEvents nudBoxFillTransparency As New FakeControl46()
 Friend WithEvents nudCatHistSeriesOverlap As New FakeControl46()
 Friend WithEvents nudViolinBandwidthAdjustment As New FakeControl46()
 Friend WithEvents optByColumn As New FakeControl46()
 Friend WithEvents optByID As New FakeControl46()
 Friend WithEvents optCatHistBarsWithLegend As New FakeControl46()
 Friend WithEvents optCatHistComparativeHorizontal As New FakeControl46()
 Friend WithEvents optCatHistComparativeVertical As New FakeControl46()
 Friend WithEvents optCatHistDifferentSampleSizes As New FakeControl46()
 Friend WithEvents optCatHistStackedBar As New FakeControl46()
 Friend WithEvents optHypothesisEquivalence_UTT As New FakeControl46()
 Friend WithEvents optHypothesisNonInferiority_UTT As New FakeControl46()
 Friend WithEvents optHypothesisSuperiority_UTT As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents tbMargin_UTT As New FakeControl46()
 Friend WithEvents txtCDFPercentiles As New FakeControl46()
    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        Dim roles = UnivariateSelectionPolicy.InputRoles(
            Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve,
            Me.optByID.Checked)
        If roles.FirstInputRequired Then
            If CheckRefEdit(Me.RefEdit1.Address, roles.InputsMustBeSingleColumn) Then
                Me.TabControl1.SelectedIndex = 0
                RefEditReset(Me.RefEdit1)
                bOut = True
            End If

            If CheckRefEdit(Me.RefEdit2.Address, roles.InputsMustBeSingleColumn) Then
                Me.TabControl1.SelectedIndex = 0
                RefEditReset(Me.RefEdit2)
                bOut = True
            End If
        Else
            If CheckRefEdit(Me.RefEdit2.Address) Then
                Me.TabControl1.SelectedIndex = 0
                RefEditReset(Me.RefEdit2)
                bOut = True
            End If
        End If

        If Me.optOutputRange.Checked Then
            If CheckRefEdit(Me.RefEditOutput.Address) Then
                Me.TabControl1.SelectedIndex = 0
                RefEditReset(Me.RefEditOutput)
                bOut = True
            End If
        End If
        Return bOut
    End Function
    Private Function GetHomogeneityMethods() As HomogeneityMethods
        Dim selected As HomogeneityMethods = HomogeneityMethods.None
        If Me.ckFlignerKilleen.Checked Then selected = selected Or HomogeneityMethods.FlignerKilleen
        If Me.ckLevene.Checked Then selected = selected Or HomogeneityMethods.LeveneMedian
        If Me.ckSquaredRanks.Checked Then selected = selected Or HomogeneityMethods.SquaredRanks
        If Me.ckBartlett.Checked Then selected = selected Or HomogeneityMethods.Bartlett
        Return selected
    End Function
    Private Sub optOutputRange_Click(sender As Object, e As System.EventArgs) Handles optOutputRange.Click
        Me.RefEditOutput.Enabled = True
        Me.RefEditOutput.txtAddress.Select()
    End Sub
    Private Sub optWorksheet_Click(sender As Object, e As System.EventArgs) Handles optWorksheet.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optWorkbook_Click(sender As Object, e As System.EventArgs) Handles optWorkbook.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optByColumn_Click(sender As Object, e As System.EventArgs) Handles optByColumn.Click
        If Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve Then
            If Me.Tag = HelpTopic.ROCCurve Then
                Me.lblRefedit1.Text = "Group with characteristic present (patients)"
                Me.lblRefedit2.Text = "Group with characteristic absent (controls)"
            ElseIf Me.Tag = HelpTopic.UnpairedTTest Then
                Me.ApplyUnpairedTtestInputLabels()
            Else
                Me.lblRefedit1.Text = "Data: Group 1"
                Me.lblRefedit2.Text = "Data: Group 2"
            End If

            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.txtAddress.Select()
        Else
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.Enabled = False
            Me.lblRefedit1.Enabled = False
            If Me.Tag = HelpTopic.ViolinPlot OrElse Me.Tag = HelpTopic.CategoricalHistogram OrElse Me.Tag = HelpTopic.CDFplot Then
                Me.lblRefedit2.Text = "Data columns:"
            End If
            Me.RefEdit2.txtAddress.Select()
        End If
    End Sub
    Private Sub optByID_Click(sender As Object, e As System.EventArgs) Handles optByID.Click
        If Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve Then
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.txtAddress.Select()
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"
        Else
            Me.RefEdit1.Enabled = True
            Me.lblRefedit1.Enabled = True
            If Me.Tag = HelpTopic.ViolinPlot OrElse Me.Tag = HelpTopic.CategoricalHistogram OrElse Me.Tag = HelpTopic.CDFplot Then
                Me.lblRefedit1.Text = "Group ID:"
                Me.lblRefedit2.Text = "Data:"
            End If
        End If
        Me.RefEdit1.txtAddress.Select()
    End Sub
    Private Sub cmbCDFPlotType_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbCDFPlotType.SelectedIndexChanged
        If Me.Tag = HelpTopic.CDFplot Then Me.UpdateCDFOptionState()
    End Sub
    Private Sub ckCDFPercentileLines_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckCDFPercentileLines.CheckedChanged
        If Me.Tag = HelpTopic.CDFplot Then Me.UpdateCDFOptionState()
    End Sub
    Private Sub ckCDFShowLegend_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckCDFShowLegend.CheckedChanged
        If Me.Tag = HelpTopic.CDFplot Then Me.UpdateCDFOptionState()
    End Sub
    Private Sub UpdateCDFOptionState()
        Dim empiricalOnly As Boolean = (Me.cmbCDFPlotType.SelectedIndex = 0)
        Dim fittedOnly As Boolean = (Me.cmbCDFPlotType.SelectedIndex = 2)

        Me.cmbCDFDistribution.Enabled = Not empiricalOnly
        Me.lblCDFDistribution.Enabled = Not empiricalOnly

        Me.cmbCDFEmpiricalMethod.Enabled = Not fittedOnly
        Me.lblCDFEmpiricalMethod.Enabled = Not fittedOnly
        Me.ckCDFExtendEmpirical.Enabled = Not fittedOnly

        Dim percentileOptionsEnabled As Boolean = Me.ckCDFPercentileLines.Checked
        Me.lblCDFPercentiles.Enabled = percentileOptionsEnabled
        Me.txtCDFPercentiles.Enabled = percentileOptionsEnabled
        Me.ckCDFPercentileLabels.Enabled = percentileOptionsEnabled

        Me.cmbCDFLegendMode.Enabled = Me.ckCDFShowLegend.Checked
        Me.lblCDFLegendMode.Enabled = Me.ckCDFShowLegend.Checked
    End Sub
    Private Sub cmdBoxMean_CheckedChanged(sender As Object, e As System.EventArgs) Handles cmdBoxMean.CheckedChanged
        If Me.Tag = HelpTopic.BoxAndWhiskers Then Me.UpdateBoxPlotOptionState()
    End Sub
    Private Sub cmbBoxPalette_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbBoxPalette.SelectedIndexChanged
        If Me.Tag = HelpTopic.BoxAndWhiskers Then Me.UpdateBoxPlotOptionState()
    End Sub
    Private Sub UpdateBoxPlotOptionState()
        Dim useLegacyAppearance As Boolean = (Me.cmbBoxPalette.SelectedIndex = BoxPlotLegacyPaletteIndex)
        Dim customAppearanceEnabled As Boolean = Not useLegacyAppearance

        'The legacy palette is a preset for the complete historical renderer, not
        'only a grey colour palette. Disable controls that are intentionally ignored
        'while preserving their values so they are restored when another palette is
        'selected.
        Me.cmdBoxMean.Enabled = customAppearanceEnabled
        Me.cmdBoxIndividualObs.Enabled = customAppearanceEnabled
        Me.ckBoxConnectMeans.Enabled = customAppearanceEnabled AndAlso Me.cmdBoxMean.Checked

        Me.nudBoxFillTransparency.Enabled = customAppearanceEnabled
        Me.lblBoxFillTransparency.Enabled = customAppearanceEnabled
        Me.cbBoxHorizontalGridlines.Enabled = customAppearanceEnabled
        Me.cbBoxOutline.Enabled = customAppearanceEnabled
        Me.nudBoxChartWidth.Enabled = customAppearanceEnabled
        Me.lblBoxChartWidth.Enabled = customAppearanceEnabled
        Me.nudBoxChartHeight.Enabled = customAppearanceEnabled
        Me.lblBoxChartHeight.Enabled = customAppearanceEnabled
    End Sub
    Private Sub ViolinBandwidth_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbViolinBandwidth.SelectedIndexChanged
        If Me.Tag = HelpTopic.ViolinPlot Then Me.UpdateViolinBandwidthOptionState()
    End Sub
    Private Sub UpdateViolinBandwidthOptionState()
        Dim manualBandwidth As Boolean = (Me.cmbViolinBandwidth.SelectedIndex = 2)

        If manualBandwidth Then
            Me.lblViolinBandwidthAdjustment.Text = "Manual bandwidth:"
            Me.nudViolinBandwidthAdjustment.DecimalPlaces = 6
            Me.nudViolinBandwidthAdjustment.Increment = 0.01D
            Me.nudViolinBandwidthAdjustment.Minimum = 0.000001D
            Me.nudViolinBandwidthAdjustment.Maximum = 1000000000D
        Else
            Me.lblViolinBandwidthAdjustment.Text = "Bandwidth Adjustment:"
            Me.nudViolinBandwidthAdjustment.DecimalPlaces = 2
            Me.nudViolinBandwidthAdjustment.Increment = 0.1D
            Me.nudViolinBandwidthAdjustment.Minimum = 0.01D
            Me.nudViolinBandwidthAdjustment.Maximum = 100D
        End If
    End Sub
    Private Sub CategoricalHistogramPlotType_CheckedChanged(sender As Object, e As System.EventArgs) Handles optCatHistBarsWithLegend.CheckedChanged,
                                                                                                             optCatHistStackedBar.CheckedChanged,
                                                                                                             optCatHistDifferentSampleSizes.CheckedChanged,
                                                                                                             optCatHistComparativeHorizontal.CheckedChanged,
                                                                                                             optCatHistComparativeVertical.CheckedChanged
        If Me.Tag = HelpTopic.CategoricalHistogram Then Me.UpdateCategoricalHistogramOptionState()
    End Sub
    Private Sub UpdateCategoricalHistogramOptionState()
        Dim isComparative As Boolean = Me.optCatHistComparativeHorizontal.Checked OrElse Me.optCatHistComparativeVertical.Checked
        Dim overlapEnabled As Boolean = Not Me.optCatHistStackedBar.Checked AndAlso Not isComparative
        Me.lblCatHistSeriesOverlap.Enabled = overlapEnabled
        Me.nudCatHistSeriesOverlap.Enabled = overlapEnabled
    End Sub
    Private Sub UnpairedTtestHypothesis_CheckedChanged(sender As Object, e As System.EventArgs) Handles optHypothesisSuperiority_UTT.CheckedChanged,
                                                                                                        optHypothesisNonInferiority_UTT.CheckedChanged,
                                                                                                        optHypothesisEquivalence_UTT.CheckedChanged
        If Me.Tag = HelpTopic.UnpairedTTest Then
            Me.UpdateUnpairedTtestOptionVisibility()
            Me.ApplyUnpairedTtestInputLabels()
        End If
    End Sub
    Private Sub ApplyUnpairedTtestInputLabels()
        If Me.optByColumn.Checked Then
            Me.lblRefedit1.Text = "Control / Reference group:"
            Me.lblRefedit2.Text = "Experimental / Test group:"
        Else
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"
        End If
    End Sub
    Private Sub UpdateUnpairedTtestOptionVisibility()
        If Me.optHypothesisSuperiority_UTT.Checked Then
            Me.lblAlpha_UTT.Text = "Two-sided alpha:"
            Me.lblMargin_UTT.Visible = False
            Me.tbMargin_UTT.Visible = False
            Me.lblMarginHint_UTT.Visible = False
        ElseIf Me.optHypothesisNonInferiority_UTT.Checked Then
            Me.lblAlpha_UTT.Text = "One-sided alpha:"
            Me.lblMargin_UTT.Text = "NI margin:"
            Me.lblMargin_UTT.Visible = True
            Me.tbMargin_UTT.Visible = True
            Me.lblMarginHint_UTT.Visible = True
        ElseIf Me.optHypothesisEquivalence_UTT.Checked Then
            Me.lblAlpha_UTT.Text = "One-sided alpha:"
            Me.lblMargin_UTT.Text = "Equivalence margin (±):"
            Me.lblMargin_UTT.Visible = True
            Me.tbMargin_UTT.Visible = True
            Me.lblMarginHint_UTT.Visible = True
        End If
    End Sub
    Private Function ValidateUnpairedTtestOptionInputs() As Boolean
        If Me.Tag <> HelpTopic.UnpairedTTest Then Return False

        If (Me.optHypothesisNonInferiority_UTT.Checked OrElse Me.optHypothesisEquivalence_UTT.Checked) AndAlso Me.optByID.Checked Then
            FakeMessages46.Record("For noninferiority or equivalence, use 'By Column' input so the direction is explicit: control/reference in Input 1 and experimental/test in Input 2.", vbExclamation)
            Me.TabControl1.SelectedIndex = 0
            Return True
        End If

        If Me.optHypothesisNonInferiority_UTT.Checked OrElse Me.optHypothesisEquivalence_UTT.Checked Then
            Dim marginValue As Double
            If Not Double.TryParse(Me.tbMargin_UTT.Text, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, marginValue) Then
                FakeMessages46.Record("Enter a numeric positive margin using '.' as the decimal separator.", vbExclamation)
                Return True
            End If
            If marginValue <= 0 Then
                FakeMessages46.Record("Margin must be greater than zero.", vbExclamation)
                Return True
            End If
        End If

        Return False
    End Function
End Class
Friend Class Ui0OneRefeditMulticolProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Private Function CheckKiteInputRange() As Boolean
  Throw New InvalidOperationException("HOST_BOUNDARY_KITE_RANGE")
 End Function
 Friend WithEvents RefEdit1 As New FakeRefEdit46()
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents TabMultipage As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data1------------------------------
        If CheckRefEdit(Me.RefEdit1.Address) Then
            Me.TabMultipage.SelectedIndex = 0
            RefEditReset(Me.RefEdit1)
            bOut = True
        End If

        If Not bOut AndAlso Me.Tag = HelpTopic.KiteChart Then
            If Me.CheckKiteInputRange() Then bOut = True
        End If

        If Me.optOutputRange.Checked Then
            If CheckRefEdit(Me.RefEditOutput.Address) Then
                Me.TabMultipage.SelectedIndex = 0
                RefEditReset(Me.RefEditOutput)
                bOut = True
            End If
        End If

        Return bOut
    End Function
    Private Sub optOutputRange_Click(sender As Object, e As System.EventArgs) Handles optOutputRange.Click
        Me.RefEditOutput.Enabled = True
        Me.RefEditOutput.txtAddress.Select()
    End Sub
    Private Sub optWorksheet_Click(sender As Object, e As System.EventArgs) Handles optWorksheet.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optWorkbook_Click(sender As Object, e As System.EventArgs) Handles optWorkbook.Click
        Me.RefEditOutput.Enabled = False
    End Sub
End Class
Friend Class UiTwoInputRefeditsProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend WithEvents RefEdit1 As New FakeRefEdit46()
 Friend WithEvents RefEdit2 As New FakeRefEdit46()
 Friend WithEvents RefEditDemingSDx As New FakeRefEdit46()
 Friend WithEvents RefEditDemingSDy As New FakeRefEdit46()
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents TabMultipage As New FakeControl46()
 Friend WithEvents ckDemingFitIntercept As New FakeControl46()
 Friend WithEvents cmbDemingVarianceModel As New FakeControl46()
 Friend WithEvents lblBootstrapReplicatesDeming As New FakeControl46()
 Friend WithEvents lblBootstrapReplicatesKappa As New FakeControl46()
 Friend WithEvents lblBootstrapReplicatesLinCCC As New FakeControl46()
 Friend WithEvents lblDemingCVx As New FakeControl46()
 Friend WithEvents lblDemingCVy As New FakeControl46()
 Friend WithEvents lblDemingSDx As New FakeControl46()
 Friend WithEvents lblDemingSDy As New FakeControl46()
 Friend WithEvents lblErrorRatio As New FakeControl46()
 Friend WithEvents optAnalyticalClosedForm As New FakeControl46()
 Friend WithEvents optAnalyticalLinnet As New FakeControl46()
 Friend WithEvents optDemingBootstrapBCa As New FakeControl46()
 Friend WithEvents optDemingBootstrapPercentile As New FakeControl46()
 Friend WithEvents optJackknife As New FakeControl46()
 Friend WithEvents optKappaAnalytical As New FakeControl46()
 Friend WithEvents optKappaBootstrapBCa As New FakeControl46()
 Friend WithEvents optKappaBootstrapPercentile As New FakeControl46()
 Friend WithEvents optKappaJackknife As New FakeControl46()
 Friend WithEvents optLinCCCAnalytical As New FakeControl46()
 Friend WithEvents optLinCCCBootstrapBCa As New FakeControl46()
 Friend WithEvents optLinCCCBootstrapPercentile As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents spinBtnBootstrapReplicatesDeming As New FakeControl46()
 Friend WithEvents spinBtnBootstrapReplicatesKappa As New FakeControl46()
 Friend WithEvents spinBtnBootstrapReplicatesLinCCC As New FakeControl46()
 Friend WithEvents spinBtnDemingCVx As New FakeControl46()
 Friend WithEvents spinBtnDemingCVy As New FakeControl46()
 Friend WithEvents spinBtnErrorRatio As New FakeControl46()
    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If CheckRefEdit(Me.RefEdit1.Address, If(Me.Tag = HelpTopic.HotellingSTSquaredTest, False, True)) Then
            Me.TabMultipage.SelectedIndex = 0
            RefEditReset(Me.RefEdit1)
            bOut = True
        End If
        If CheckRefEdit(Me.RefEdit2.Address, If(Me.Tag = HelpTopic.HotellingSTSquaredTest, False, True)) Then
            Me.TabMultipage.SelectedIndex = 0
            RefEditReset(Me.RefEdit2)
            bOut = True
        End If
        If Me.optOutputRange.Checked Then
            If CheckRefEdit(Me.RefEditOutput.Address) Then
                Me.TabMultipage.SelectedIndex = 0
                RefEditReset(Me.RefEditOutput)
                bOut = True
            End If
        End If
        Return bOut
    End Function
    Private Sub ApplyDemingControlState()
        If Me.Tag <> HelpTopic.DemingRegression Then Exit Sub

        Dim modelIndex As Integer = Me.cmbDemingVarianceModel.SelectedIndex
        If modelIndex < 0 Then modelIndex = 0

        Dim useBootstrap As Boolean = Me.optDemingBootstrapPercentile.Checked OrElse Me.optDemingBootstrapBCa.Checked
        Me.lblBootstrapReplicatesDeming.Enabled = useBootstrap
        Me.spinBtnBootstrapReplicatesDeming.Enabled = useBootstrap

        Dim allowAnalytical As Boolean = (modelIndex = 0 AndAlso Me.ckDemingFitIntercept.Checked)
        Me.optAnalyticalClosedForm.Enabled = allowAnalytical
        Me.optAnalyticalLinnet.Enabled = allowAnalytical
        If Not allowAnalytical AndAlso (Me.optAnalyticalClosedForm.Checked OrElse Me.optAnalyticalLinnet.Checked) Then
            Me.optJackknife.Checked = True
        End If

        Me.lblErrorRatio.Enabled = (modelIndex = 0)
        Me.spinBtnErrorRatio.Enabled = (modelIndex = 0)

        Dim showCV As Boolean = (modelIndex = 1)
        Me.lblDemingCVx.Enabled = showCV
        Me.spinBtnDemingCVx.Enabled = showCV
        Me.lblDemingCVy.Enabled = showCV
        Me.spinBtnDemingCVy.Enabled = showCV

        Dim showPointwise As Boolean = (modelIndex = 2)
        Me.lblDemingSDx.Enabled = showPointwise
        Me.RefEditDemingSDx.Enabled = showPointwise
        Me.lblDemingSDy.Enabled = showPointwise
        Me.RefEditDemingSDy.Enabled = showPointwise
    End Sub
    Private Sub DemingControlStateChanged(sender As Object, e As System.EventArgs) _
    Handles cmbDemingVarianceModel.SelectedIndexChanged,
            ckDemingFitIntercept.CheckedChanged,
            optJackknife.CheckedChanged,
            optAnalyticalLinnet.CheckedChanged,
            optAnalyticalClosedForm.CheckedChanged,
            optDemingBootstrapPercentile.CheckedChanged,
            optDemingBootstrapBCa.CheckedChanged
        Me.ApplyDemingControlState()
    End Sub
    Private Sub ApplyLinCCCControlState()
        Dim useBootstrap As Boolean = Me.optLinCCCBootstrapPercentile.Checked OrElse Me.optLinCCCBootstrapBCa.Checked
        Me.lblBootstrapReplicatesLinCCC.Enabled = useBootstrap
        Me.spinBtnBootstrapReplicatesLinCCC.Enabled = useBootstrap
    End Sub
    Private Sub optLinCCCAnalytical_CheckedChanged(sender As Object, e As System.EventArgs) _
    Handles optLinCCCAnalytical.CheckedChanged, optLinCCCBootstrapPercentile.CheckedChanged, optLinCCCBootstrapBCa.CheckedChanged

        Me.ApplyLinCCCControlState()
    End Sub
    Private Sub ApplyKappaControlState()
        Dim useBootstrap As Boolean = Me.optKappaBootstrapPercentile.Checked OrElse Me.optKappaBootstrapBCa.Checked
        Me.lblBootstrapReplicatesKappa.Enabled = useBootstrap
        Me.spinBtnBootstrapReplicatesKappa.Enabled = useBootstrap
    End Sub
    Private Sub optKappaAnalytical_CheckedChanged(sender As Object, e As System.EventArgs) _
        Handles optKappaAnalytical.CheckedChanged, optKappaJackknife.CheckedChanged, optKappaBootstrapPercentile.CheckedChanged, optKappaBootstrapBCa.CheckedChanged

        Me.ApplyKappaControlState()
    End Sub
    Private Sub optOutputRange_Click(sender As Object, e As System.EventArgs) Handles optOutputRange.Click
        Me.RefEditOutput.Enabled = True
        Me.RefEditOutput.txtAddress.Select()
    End Sub
    Private Sub optWorksheet_Click(sender As Object, e As System.EventArgs) Handles optWorksheet.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optWorkbook_Click(sender As Object, e As System.EventArgs) Handles optWorkbook.Click
        Me.RefEditOutput.Enabled = False
    End Sub
End Class
Partial Friend Class Ui82x2Projection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend WithEvents RefEdit1_WorksheetData As New FakeRefEdit46()
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents lblC1 As New FakeControl46()
 Friend WithEvents lblC2 As New FakeControl46()
 Friend WithEvents lblR1 As New FakeControl46()
 Friend WithEvents lblR2 As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optScreenData As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents optWorksheetData As New FakeControl46()
 Friend WithEvents spinBtnA As New FakeControl46()
 Friend WithEvents spinBtnB As New FakeControl46()
 Friend WithEvents spinBtnC As New FakeControl46()
 Friend WithEvents spinBtnD As New FakeControl46()
    Private Sub optWorksheetData_Click(sender As Object, e As System.EventArgs) Handles optWorksheetData.Click
        Me.RefEdit1_WorksheetData.Enabled = True
        Me.lblC1.Enabled = False
        Me.lblC2.Enabled = False
        Me.lblR1.Enabled = False
        Me.lblR2.Enabled = False
        Me.spinBtnA.Enabled = False
        Me.spinBtnB.Enabled = False
        Me.spinBtnC.Enabled = False
        Me.spinBtnD.Enabled = False
        If Me.optWorksheetData.Checked Then Me.RefEdit1_WorksheetData.txtAddress.Select()
    End Sub
    Private Sub optScreenData_Click(sender As Object, e As System.EventArgs) Handles optScreenData.Click
        Me.RefEdit1_WorksheetData.Enabled = False
        Me.lblC1.Enabled = True
        Me.lblC2.Enabled = True
        Me.lblR1.Enabled = True
        Me.lblR2.Enabled = True
        Me.spinBtnA.Enabled = True
        Me.spinBtnB.Enabled = True
        Me.spinBtnC.Enabled = True
        Me.spinBtnD.Enabled = True
    End Sub
    Private Sub optOutputRange_Click(sender As Object, e As System.EventArgs) Handles optOutputRange.Click
        Me.RefEditOutput.Enabled = True
        Me.RefEditOutput.txtAddress.Select()
    End Sub
    Private Sub optWorksheet_Click(sender As Object, e As System.EventArgs) Handles optWorksheet.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optWorkbook_Click(sender As Object, e As System.EventArgs) Handles optWorkbook.Click
        Me.RefEditOutput.Enabled = False
    End Sub
End Class
Partial Friend Class Ui8ProportionsProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents cbHypothesisType As New FakeStringControl46()
 Friend WithEvents lbl1 As New FakeControl46()
 Friend WithEvents lbl2 As New FakeControl46()
 Friend WithEvents lbl3 As New FakeControl46()
 Friend WithEvents lbl4 As New FakeControl46()
 Friend WithEvents lblAlpha As New FakeControl46()
 Friend WithEvents lblHypothesisType As New FakeControl46()
 Friend WithEvents lblMargin As New FakeControl46()
 Friend WithEvents lblMarginHint_IndProp As New FakeControl46()
 Friend WithEvents optIndependent As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optPaired As New FakeControl46()
 Friend WithEvents optSingle As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents spinBtnA As New FakeControl46()
 Friend WithEvents spinBtnC As New FakeControl46()
 Friend WithEvents spinBtnD As New FakeControl46()
 Friend WithEvents spinBtnMargin_IndProp As New FakeControl46()
 Friend Sub New()
  AddHandler Me.cbHypothesisType.SelectedIndexChanged, AddressOf Me.HypothesisTypeChanged
 End Sub
    Private Sub optSingle_CheckedChanged(sender As Object, e As System.EventArgs) Handles optSingle.CheckedChanged
        Me.lbl1.Text = "Total number of observations in the study"
        Me.lbl2.Text = "Number of responders"
        Me.lbl3.Visible = False
        Me.lbl4.Visible = False
        Me.spinBtnC.Visible = False
        Me.spinBtnD.Visible = False
        Me.lblMargin.Visible = False
        Me.spinBtnMargin_IndProp.Visible = False
        Me.lblMarginHint_IndProp.Visible = False
        Me.cbHypothesisType.Visible = False
        Me.lblHypothesisType.Visible = False
        Me.lblAlpha.Text = "alpha"
        Me.spinBtnA.Select()
    End Sub
    Private Sub optIndependent_CheckedChanged(sender As Object, e As System.EventArgs) Handles optIndependent.CheckedChanged
        Me.lbl1.Text = "Total number of observations in Sampe 1"
        Me.lbl2.Text = "Number of responders in Sample 1"
        Me.lbl3.Text = "Total number of observations in Sampe 2"
        Me.lbl4.Text = "Number of responders in Sample 2"
        Me.lbl3.Visible = True
        Me.lbl4.Visible = True
        Me.spinBtnC.Visible = True
        Me.spinBtnD.Visible = True
        Me.lblMargin.Visible = True
        Me.spinBtnMargin_IndProp.Visible = True
        Me.lblMarginHint_IndProp.Visible = True
        Me.cbHypothesisType.Visible = True
        Me.lblHypothesisType.Visible = True
        UpdateIndependentHypothesisUi()
        Me.spinBtnA.Select()
    End Sub
    Private Sub optPaired_CheckedChanged(sender As Object, e As System.EventArgs) Handles optPaired.CheckedChanged
        Me.lbl1.Text = "Total number of observations in the study"
        Me.lbl2.Text = "Number of responders in 1st category only"
        Me.lbl3.Text = "Namber of responders in 2nd category only"
        Me.lbl4.Text = "Number of responders in both categories"
        Me.lbl3.Visible = True
        Me.lbl4.Visible = True
        Me.spinBtnC.Visible = True
        Me.spinBtnD.Visible = True
        Me.lblMargin.Visible = False
        Me.spinBtnMargin_IndProp.Visible = False
        Me.lblMarginHint_IndProp.Visible = False
        Me.cbHypothesisType.Visible = False
        Me.lblHypothesisType.Visible = False
        Me.spinBtnA.Select()
    End Sub
    Private Sub HypothesisTypeChanged(sender As Object, e As System.EventArgs)
        Me.UpdateIndependentHypothesisUi()
    End Sub
    Private Function ValidateIndependentMargin() As Boolean
        If Me.spinBtnMargin_IndProp Is Nothing Then Return True
        Dim mode As String = Me.cbHypothesisType.SelectedItem
        If mode <> "Noninferiority" AndAlso mode <> "Equivalence" Then Return True

        Dim hypothesis = If(mode = "Noninferiority", ExcelCommon.IndependentProportionsHypothesis.Noninferiority, ExcelCommon.IndependentProportionsHypothesis.Equivalence)
        Dim report = ExcelCommon.ProportionsInputPolicy.ValidateMargin(hypothesis, CDbl(Me.spinBtnMargin_IndProp.Value))
        If report.HasErrors Then
            FakeMessages46.Record(report.Issues(0).Message, vbOKOnly, "Data input")
            Me.spinBtnMargin_IndProp.Select()
            Return False
        End If

        Return True
    End Function
    Private Sub UpdateIndependentHypothesisUi()
        Dim isIndependent As Boolean = Me.optIndependent.Checked
        Me.lblMargin.Visible = isIndependent
        Me.spinBtnMargin_IndProp.Visible = isIndependent
        If Not isIndependent Then Exit Sub

        Select Case Me.cbHypothesisType.SelectedItem
            Case "Noninferiority"
                Me.lblAlpha.Text = "One-sided alpha"
                Me.lblMargin.Text = "Noninferiority margin"
                Me.lblMargin.Visible = True
                Me.spinBtnMargin_IndProp.Visible = True

                Me.lblMarginHint_IndProp.Text = "Enter a positive margin. The null limit is -margin on the (Experimental - Control) scale."
                Me.lblMarginHint_IndProp.Visible = True

            Case "Equivalence"
                Me.lblAlpha.Text = "One-sided alpha"
                Me.lblMargin.Text = "Equivalence margin"
                Me.lblMargin.Visible = True
                Me.spinBtnMargin_IndProp.Visible = True
                Me.lblMarginHint_IndProp.Text = "Enter a positive symmetric margin. The equivalence region is [-margin, +margin] on the (Experimental - Control) scale."
                Me.lblMarginHint_IndProp.Visible = True

            Case Else
                Me.lblAlpha.Text = "alpha"
                Me.lblMargin.Visible = False
                Me.spinBtnMargin_IndProp.Visible = False
                Me.lblMarginHint_IndProp.Visible = False
        End Select
    End Sub
    Private Sub optOutputRange_Click(sender As Object, e As System.EventArgs) Handles optOutputRange.Click
        Me.RefEditOutput.Enabled = True
        Me.RefEditOutput.txtAddress.Select()
    End Sub
    Private Sub optWorksheet_Click(sender As Object, e As System.EventArgs) Handles optWorksheet.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optWorkbook_Click(sender As Object, e As System.EventArgs) Handles optWorkbook.Click
        Me.RefEditOutput.Enabled = False
    End Sub
End Class
Friend Class Ui9ANOVA2nestedProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend WithEvents RefEdit1_Group As New FakeRefEdit46()
 Friend WithEvents RefEdit2_nested As New FakeRefEdit46()
 Friend WithEvents RefEdit3_Data As New FakeRefEdit46()
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents ckBlandAllowFallback As New FakeControl46()
 Friend WithEvents ckBlandDecisionLimitsEnable As New FakeControl46()
 Friend WithEvents ckBlandExcludeSingletonSubjects As New FakeControl46()
 Friend WithEvents ckBlandUseTDistribution As New FakeControl46()
 Friend WithEvents cmbBlandMode As New FakeControl46()
 Friend WithEvents cmbBlandPlotMode As New FakeControl46()
 Friend WithEvents cmbBlandScale As New FakeControl46()
 Friend WithEvents lblBlandBootstrapReps As New FakeControl46()
 Friend WithEvents lblBlandDecisionLimitsHelp As New FakeControl46()
 Friend WithEvents lblBlandLowerAllowable As New FakeControl46()
 Friend WithEvents lblBlandMinPairs As New FakeControl46()
 Friend WithEvents lblBlandMinSubjects As New FakeControl46()
 Friend WithEvents lblBlandUpperAllowable As New FakeControl46()
 Friend WithEvents optBlandAnalytical As New FakeControl46()
 Friend WithEvents optBlandBootstrap As New FakeControl46()
 Friend WithEvents optBlandBootstrapBCa As New FakeControl46()
 Friend WithEvents optBlandJackknife As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents spinBtnBlandMinPairs As New FakeControl46()
 Friend WithEvents spinBtnBlandMinSubjects As New FakeControl46()
 Friend WithEvents tbBlandBootstrapReps As New FakeControl46()
 Friend WithEvents tbBlandLowerAllowable As New FakeControl46()
 Friend WithEvents tbBlandUpperAllowable As New FakeControl46()
    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If Me.Tag = HelpTopic.TwoWayNestedANOVA Then
            If CheckRefEdit(Me.RefEdit1_Group.Address, True) Then
                RefEditReset(Me.RefEdit1_Group)
                bOut = True
            End If
        ElseIf (Me.Tag = HelpTopic.PassingBablokRegression OrElse Me.Tag = HelpTopic.BlandAltman) And Me.RefEdit1_Group.Address <> String.Empty Then
            If CheckRefEdit(Me.RefEdit1_Group.Address, True) Then
                RefEditReset(Me.RefEdit1_Group)
                bOut = True
            End If
        End If

        If CheckRefEdit(Me.RefEdit2_nested.Address, True) Then
            RefEditReset(Me.RefEdit2_nested)
            bOut = True
        End If

        Dim bOneColumn As Boolean = True
        If Me.Tag = HelpTopic.TwoWayNestedANOVA Then
            bOneColumn = False
        ElseIf Me.Tag = HelpTopic.PassingBablokRegression OrElse Me.Tag = HelpTopic.BlandAltman Then
            bOneColumn = True
        End If

        If CheckRefEdit(Me.RefEdit3_Data.Address, bOneColumn) Then
            RefEditReset(Me.RefEdit3_Data)
            bOut = True
        End If

        If Me.optOutputRange.Checked Then
            If CheckRefEdit(Me.RefEditOutput.Address) Then
                RefEditReset(Me.RefEditOutput)
                bOut = True
            End If
        End If
        Return bOut
    End Function
    Private Sub UpdateBlandAltmanOptionState() Handles cmbBlandMode.SelectedIndexChanged,
                                                   cmbBlandScale.SelectedIndexChanged,
                                                   optBlandAnalytical.CheckedChanged,
                                                   optBlandJackknife.CheckedChanged,
                                                   optBlandBootstrap.CheckedChanged,
                                                   optBlandBootstrapBCa.CheckedChanged,
                                                   ckBlandDecisionLimitsEnable.CheckedChanged
        If Me.cmbBlandMode Is Nothing Then Exit Sub

        Dim repeatedRequested As Boolean = (Me.cmbBlandMode.SelectedIndex <> 1)
        Me.cmbBlandPlotMode.Enabled = repeatedRequested
        Me.ckBlandExcludeSingletonSubjects.Enabled = repeatedRequested
        Me.ckBlandAllowFallback.Enabled = repeatedRequested
        Me.spinBtnBlandMinSubjects.Enabled = repeatedRequested
        Me.spinBtnBlandMinPairs.Enabled = repeatedRequested
        Me.lblBlandMinSubjects.Enabled = repeatedRequested
        Me.lblBlandMinPairs.Enabled = repeatedRequested

        Dim useBootstrap As Boolean = Me.optBlandBootstrap.Checked OrElse Me.optBlandBootstrapBCa.Checked
        Me.tbBlandBootstrapReps.Enabled = useBootstrap
        Me.lblBlandBootstrapReps.Enabled = useBootstrap
        Me.ckBlandUseTDistribution.Enabled = Not useBootstrap

        Me.ApplyBlandDecisionLimitState()
    End Sub
    Private Sub ApplyBlandDecisionLimitState()
        Dim enabled As Boolean = Me.ckBlandDecisionLimitsEnable.Checked

        Me.lblBlandLowerAllowable.Enabled = enabled
        Me.tbBlandLowerAllowable.Enabled = enabled
        Me.lblBlandUpperAllowable.Enabled = enabled
        Me.tbBlandUpperAllowable.Enabled = enabled
        Me.lblBlandDecisionLimitsHelp.Enabled = enabled

        If Not enabled Then
            Me.lblBlandDecisionLimitsHelp.Text = "Enable this to compare the fitted bias and limits of agreement with pre-specified lower and upper acceptable limits."
            Exit Sub
        End If

        Dim scaleText As String = If(Me.cmbBlandScale Is Nothing OrElse Me.cmbBlandScale.SelectedIndex < 0, "the active analysis scale", Me.cmbBlandScale.Text)
        Dim repeatedRequested As Boolean = False
        If Me.cmbBlandMode IsNot Nothing Then repeatedRequested = (Me.cmbBlandMode.SelectedIndex = 2 OrElse Me.cmbBlandMode.SelectedIndex = 0)

        Dim scaleHint As String
        Select Case Me.cmbBlandScale.SelectedIndex
            Case 1, 2, 3
                scaleHint = "Enter the allowable lower and upper limits on the selected percent scale."
            Case 4
                scaleHint = "Enter the allowable lower and upper limits on the selected log-ratio scale."
            Case Else
                scaleHint = "Enter the allowable lower and upper limits on the raw-difference scale."
        End Select

        Dim repeatedHint As String = If(repeatedRequested,
                                    " If the repeated-measures model is used, the assessment is based on the repeated-measures Bland–Altman bias and limits of agreement.",
                                    String.Empty)

        Me.lblBlandDecisionLimitsHelp.Text =
            $"Decision-limit / allowable-bias reporting now follows the current Bland–Altman analysis settings. " &
            $"Limits are interpreted on the current scale ({scaleText})." & Environment.NewLine &
            scaleHint & repeatedHint
    End Sub
    Private Sub optOutputRange_Click(sender As Object, e As System.EventArgs) Handles optOutputRange.Click
        Me.RefEditOutput.Enabled = True
        Me.RefEditOutput.txtAddress.Select()
    End Sub
    Private Sub optWorksheet_Click(sender As Object, e As System.EventArgs) Handles optWorksheet.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optWorkbook_Click(sender As Object, e As System.EventArgs) Handles optWorkbook.Click
        Me.RefEditOutput.Enabled = False
    End Sub
End Class
Partial Friend Class Ui12SampleSizeTtestSinglePropProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
End Class
Friend Class Ui3XYZplotProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend pFigure As Object
 Friend pbPreventEvents As Boolean
 Friend Recalculations As Integer
 Private Sub Recalculate()
  Recalculations += 1
 End Sub
 Friend WithEvents btResetView As New FakeControl46()
 Friend WithEvents cbPointLabelPosition As New FakeControl46()
 Friend WithEvents ckDataPointLabels As New FakeControl46()
 Friend WithEvents ckGridlines As New FakeControl46()
 Friend WithEvents ckXYplanePoints As New FakeControl46()
 Friend WithEvents ckXZplanePoints As New FakeControl46()
 Friend WithEvents ckYZplanePoints As New FakeControl46()
 Friend WithEvents ckZdropLines As New FakeControl46()
 Friend WithEvents spinBtnLabelFontSize As New FakeControl46()
 Friend WithEvents spinBtnMarkerSize As New FakeControl46()
 Friend WithEvents spinBtnRotationX As New FakeControl46()
 Friend WithEvents spinBtnRotationZ As New FakeControl46()
 Friend WithEvents spinBtnShiftX As New FakeControl46()
 Friend WithEvents spinBtnShiftY As New FakeControl46()
 Friend WithEvents spinBtnXYPlanePointSize As New FakeControl46()
 Friend WithEvents spinBtnXZPlanePointSize As New FakeControl46()
 Friend WithEvents spinBtnYZPlanePointSize As New FakeControl46()
 Friend WithEvents spinBtnZoom As New FakeControl46()
    Private Sub spinBtnRotationX_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnRotationX.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub
    Private Sub spinBtnRotationZ_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnRotationZ.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub
    Private Sub spinBtnZoom_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnZoom.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub
    Private Sub spinBtnShiftX_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnShiftX.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub
    Private Sub spinBtnShiftY_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnShiftY.ValueChanged
        If pFigure IsNot Nothing AndAlso Not Me.pbPreventEvents Then Call Recalculate()
    End Sub
    Private Sub btResetView_Click(sender As Object, e As System.EventArgs) Handles btResetView.Click
        Me.pbPreventEvents = True
        Me.spinBtnRotationX.Value = 120
        Me.spinBtnRotationZ.Value = 60
        Me.spinBtnZoom.Value = 0
        Me.spinBtnShiftY.Value = 50
        Me.spinBtnShiftX.Value = 50
        Me.pbPreventEvents = False
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub spinBtnXYPlanePointSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnXYPlanePointSize.ValueChanged
        If pFigure IsNot Nothing And Me.ckXYplanePoints.Checked Then Call Recalculate()
    End Sub
    Private Sub spinBtnYZPlanePointSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnYZPlanePointSize.ValueChanged
        If pFigure IsNot Nothing And Me.ckYZplanePoints.Checked Then Call Recalculate()
    End Sub
    Private Sub spinBtnXZPlanePointSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnXZPlanePointSize.ValueChanged
        If pFigure IsNot Nothing And Me.ckXZplanePoints.Checked Then Call Recalculate()
    End Sub
    Private Sub spinBtnLabelFontSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnLabelFontSize.ValueChanged
        If pFigure IsNot Nothing And Me.ckDataPointLabels.Checked Then Call Recalculate()
    End Sub
    Private Sub ckGridlines_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckGridlines.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub spinBtnMarkerSize_ValueChanged(sender As Object, e As System.EventArgs) Handles spinBtnMarkerSize.ValueChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub cbPointLabelPosition_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbPointLabelPosition.SelectedIndexChanged
        If pFigure IsNot Nothing And Me.ckDataPointLabels.Checked Then Call Recalculate()
    End Sub
    Private Sub ckDataPointLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckDataPointLabels.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub ckXYplanePoints_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckXYplanePoints.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub ckYZplanePoints_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckYZplanePoints.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub ckXZplanePoints_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckXZplanePoints.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
    Private Sub ckZdropLines_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckZdropLines.CheckedChanged
        If pFigure IsNot Nothing Then Call Recalculate()
    End Sub
End Class
Friend Class Ui01BulletChartProjection46
 Friend WithEvents cmbDefaultDirection As New FakeControl46()
 Friend Tag As HelpTopic
 Friend Text As String = ""
    Private NotInheritable Class ComboItem(Of T)
        Public Sub New(displayText As String, value As T)
            Me.DisplayText = displayText
            Me.Value = value
        End Sub

        Public ReadOnly Property DisplayText As String
        Public ReadOnly Property Value As T

        Public Overrides Function ToString() As String
            Return Me.DisplayText
        End Function
    End Class
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents ckOmitIncompleteRows As New FakeControl46()
 Friend WithEvents ckShowActualValueLabels As New FakeControl46()
 Friend WithEvents ckShowScaleLabels As New FakeControl46()
 Friend WithEvents ckShowTargetValueLabels As New FakeControl46()
 Friend WithEvents lblScaleNumberFormat As New FakeControl46()
 Friend WithEvents lblValueNumberFormat As New FakeControl46()
 Friend WithEvents nudTargetMajorTickCount As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents txtScaleNumberFormat As New FakeControl46()
 Friend WithEvents txtValueNumberFormat As New FakeControl46()
    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub
    Private Sub ckShowScaleLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowScaleLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub ckShowActualValueLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowActualValueLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub ckShowTargetValueLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowTargetValueLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub UpdateOptionControlStates()
        Me.txtScaleNumberFormat.Enabled = Me.ckShowScaleLabels.Checked
        Me.lblScaleNumberFormat.Enabled = Me.txtScaleNumberFormat.Enabled

        Dim showValueLabels As Boolean = Me.ckShowActualValueLabels.Checked OrElse
                                         Me.ckShowTargetValueLabels.Checked
        Me.txtValueNumberFormat.Enabled = showValueLabels
        Me.lblValueNumberFormat.Enabled = showValueLabels
    End Sub
    Private Function GetPlotOptions() As BulletChartOptions
        Return New BulletChartOptions With {
            .OmitIncompleteRows = Me.ckOmitIncompleteRows.Checked,
            .DefaultDirection = GetComboValue(Of BulletChartDirection)(Me.cmbDefaultDirection, "a default direction"),
            .TargetMajorTickCount = CInt(Me.nudTargetMajorTickCount.Value)
        }
    End Function
    Private Shared Function GetComboValue(Of T)(comboBox As ComboBox,
                                                 controlDescription As String) As T
        Dim item As ComboItem(Of T) = TryCast(comboBox.SelectedItem, ComboItem(Of T))
        If item Is Nothing Then
            Throw New InvalidOperationException("Select " & controlDescription & ".")
        End If
        Return item.Value
    End Function
    Private Shared Sub ValidateOptionalNumericFormat(formatText As String, displayName As String)
        If String.IsNullOrWhiteSpace(formatText) Then Return

        Try
            Dim ignored As String = 12.5R.ToString(formatText.Trim(), CultureInfo.CurrentCulture)
        Catch ex As FormatException
            Throw New ArgumentException(displayName & " is not a valid .NET numeric format string.")
        End Try
    End Sub
End Class
Friend Class Ui01ConvexHullPlotProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents ckIncludeCollinearBoundaryPoints As New FakeControl46()
 Friend WithEvents nudPaddingPercentX As New FakeControl46()
 Friend WithEvents nudPaddingPercentY As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents tbCollinearityTolerance As New FakeControl46()
    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub
    Private Function GetPlotOptions() As ConvexHullPlotOptions
        Return New ConvexHullPlotOptions With {
            .IncludeCollinearBoundaryPoints = Me.ckIncludeCollinearBoundaryPoints.Checked,
            .PaddingPercentX = CDbl(Me.nudPaddingPercentX.Value),
            .PaddingPercentY = CDbl(Me.nudPaddingPercentY.Value),
            .CollinearityTolerance = ParseUiDouble(Me.tbCollinearityTolerance.Text,
                                                   "collinearity tolerance")
        }
    End Function
End Class
Friend Class Ui01DumbbellPlotProjection46
 Friend WithEvents cmbSortMode As New FakeControl46()
 Friend Tag As HelpTopic
 Friend Text As String = ""
    Private NotInheritable Class ComboItem(Of T)
        Public Sub New(displayText As String, value As T)
            Me.DisplayText = displayText
            Me.Value = value
        End Sub

        Public ReadOnly Property DisplayText As String
        Public ReadOnly Property Value As T

        Public Overrides Function ToString() As String
            Return Me.DisplayText
        End Function
    End Class
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents ckOmitIncompleteRows As New FakeControl46()
 Friend WithEvents ckReverseCategoryOrder As New FakeControl46()
 Friend WithEvents nudAddObsJitter As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub
    Private Function GetPlotOptions() As DumbbellPlotOptions
        Return New DumbbellPlotOptions With {
            .SortMode = GetComboValue(Of DumbbellPlotSortMode)(Me.cmbSortMode, "how categories should be sorted"),
            .ReverseCategoryOrder = Me.ckReverseCategoryOrder.Checked,
            .OmitIncompleteRows = Me.ckOmitIncompleteRows.Checked,
            .AuxiliaryJitterFraction = CDbl(Me.nudAddObsJitter.Value)
        }
    End Function
    Private Shared Function GetComboValue(Of T)(comboBox As ComboBox,
                                                 controlDescription As String) As T
        Dim item As ComboItem(Of T) = TryCast(comboBox.SelectedItem, ComboItem(Of T))
        If item Is Nothing Then
            Throw New InvalidOperationException("Select " & controlDescription & ".")
        End If
        Return item.Value
    End Function
End Class
Friend Class Ui01LadderPlotProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
    Private NotInheritable Class ComboItem(Of T)
        Public Sub New(displayText As String, value As T)
            Me.DisplayText = displayText
            Me.Value = value
        End Sub

        Public ReadOnly Property DisplayText As String
        Public ReadOnly Property Value As T

        Public Overrides Function ToString() As String
            Return Me.DisplayText
        End Function
    End Class
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents btnConnectorColor As New FakeControl46()
 Friend WithEvents btnSingleColor As New FakeControl46()
 Friend WithEvents ckAvoidEndpointLabelOverlap As New FakeControl46()
 Friend WithEvents ckColorEndpointLabelsBySeries As New FakeControl46()
 Friend WithEvents ckHorizontalJitter As New FakeControl46()
 Friend WithEvents ckOmitIncompleteRows As New FakeControl46()
 Friend WithEvents ckShowFirstEndpointLabels As New FakeControl46()
 Friend WithEvents ckShowLines As New FakeControl46()
 Friend WithEvents ckShowMarkers As New FakeControl46()
 Friend WithEvents ckShowSecondEndpointLabels As New FakeControl46()
 Friend WithEvents cmbColorMode As New FakeControl46()
 Friend WithEvents cmbConnectorLineStyle As New FakeControl46()
 Friend WithEvents cmbEndpointLabelContent As New FakeControl46()
 Friend WithEvents cmdMarkerStyle As New FakeControl46()
 Friend WithEvents lblConnectorColor As New FakeControl46()
 Friend WithEvents lblEndpointLabelContent As New FakeControl46()
 Friend WithEvents lblEndpointValueFormat As New FakeControl46()
 Friend WithEvents lblLineStyle As New FakeControl46()
 Friend WithEvents lblLineWidth As New FakeControl46()
 Friend WithEvents lblMarkerSize As New FakeControl46()
 Friend WithEvents lblMarkerStyle As New FakeControl46()
 Friend WithEvents lblSingleColor As New FakeControl46()
 Friend WithEvents lblTransparency As New FakeControl46()
 Friend WithEvents nudConnectorTransparency As New FakeControl46()
 Friend WithEvents nudConnectorWidth As New FakeControl46()
 Friend WithEvents nudHorizontalJitter As New FakeControl46()
 Friend WithEvents nudMarkerSize As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents pnlConnectorColor As New FakeControl46()
 Friend WithEvents pnlSingleColor As New FakeControl46()
 Friend WithEvents txtEndpointValueFormat As New FakeControl46()
    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub
    Private Sub ckHorizontalJitter_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckHorizontalJitter.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub ckShowLines_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowLines.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub ckShowMarkers_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowMarkers.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub cmbColorMode_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbColorMode.SelectedIndexChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub ckShowFirstEndpointLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowFirstEndpointLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub ckShowSecondEndpointLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowSecondEndpointLabels.CheckedChanged
        Me.UpdateOptionControlStates()
    End Sub
    Private Sub UpdateOptionControlStates()
        Me.nudHorizontalJitter.Enabled = Me.ckHorizontalJitter.Checked

        Dim showMarkers As Boolean = Me.ckShowMarkers.Checked
        Me.cmdMarkerStyle.Enabled = showMarkers
        Me.nudMarkerSize.Enabled = showMarkers
        Me.lblMarkerStyle.Enabled = showMarkers
        Me.lblMarkerSize.Enabled = showMarkers

        Dim showLines As Boolean = Me.ckShowLines.Checked
        Me.cmbConnectorLineStyle.Enabled = showLines
        Me.nudConnectorWidth.Enabled = showLines
        Me.nudConnectorTransparency.Enabled = showLines
        Me.lblLineStyle.Enabled = showLines
        Me.lblLineWidth.Enabled = showLines
        Me.lblTransparency.Enabled = showLines

        Dim singleColorMode As Boolean = False
        Dim selectedColorMode As ComboItem(Of LadderColorMode) = TryCast(Me.cmbColorMode.SelectedItem,
                                                                         ComboItem(Of LadderColorMode))
        If selectedColorMode IsNot Nothing Then
            singleColorMode = selectedColorMode.Value = LadderColorMode.SingleColor
        End If

        Me.btnSingleColor.Enabled = singleColorMode
        Me.pnlSingleColor.Enabled = singleColorMode
        Me.lblSingleColor.Enabled = singleColorMode

        'The current renderer colours lines and markers together for By observation/By group.
        'The separate connector colour is therefore meaningful only in Single color mode.
        Me.btnConnectorColor.Enabled = showLines AndAlso singleColorMode
        Me.pnlConnectorColor.Enabled = showLines AndAlso singleColorMode
        Me.lblConnectorColor.Enabled = showLines AndAlso singleColorMode

        Dim showEndpointLabels As Boolean = Me.ckShowFirstEndpointLabels.Checked OrElse
                                            Me.ckShowSecondEndpointLabels.Checked
        Me.cmbEndpointLabelContent.Enabled = showEndpointLabels
        Me.txtEndpointValueFormat.Enabled = showEndpointLabels
        Me.lblEndpointLabelContent.Enabled = showEndpointLabels
        Me.lblEndpointValueFormat.Enabled = showEndpointLabels
        Me.ckAvoidEndpointLabelOverlap.Enabled = showEndpointLabels
        Me.ckColorEndpointLabelsBySeries.Enabled = showEndpointLabels
    End Sub
    Private Function GetPlotOptions() As LadderPlotOptions
        Return New LadderPlotOptions With {
            .OmitIncompleteRows = Me.ckOmitIncompleteRows.Checked,
            .HorizontalJitterFraction = If(Me.ckHorizontalJitter.Checked,
                                           CDbl(Me.nudHorizontalJitter.Value),
                                           0.0R)
        }
    End Function
End Class
Friend Class Ui01PolarPlotProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents ckConnectPoints As New FakeControl46()
 Friend WithEvents optClockwise As New FakeControl46()
 Friend WithEvents optCounterClockwise As New FakeControl46()
 Friend WithEvents optDegrees As New FakeControl46()
 Friend WithEvents optEast As New FakeControl46()
 Friend WithEvents optNorth As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optPercentage As New FakeControl46()
 Friend WithEvents optRadians As New FakeControl46()
 Friend WithEvents optSouth As New FakeControl46()
 Friend WithEvents optWest As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
 Friend WithEvents tbAngularTickInterval As New FakeControl46()
 Friend WithEvents tbRadialTickInterval As New FakeControl46()
    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub
    Private Function GetPlotOptions() As PolarPlotOptions
        Dim options As New PolarPlotOptions()

        If Me.optRadians.Checked Then
            options.AngleUnit = PolarAngleUnit.Radians
        ElseIf Me.optPercentage.Checked Then
            options.AngleUnit = PolarAngleUnit.Percentage
        ElseIf Me.optDegrees.Checked Then
            options.AngleUnit = PolarAngleUnit.Degrees
        Else
            Throw New InvalidOperationException("Select an angle unit for the polar plot.")
        End If

        If Me.optClockwise.Checked Then
            options.Rotation = PolarRotation.Clockwise
        ElseIf Me.optCounterClockwise.Checked Then
            options.Rotation = PolarRotation.Counterclockwise
        Else
            Throw New InvalidOperationException("Select a rotation direction for the polar plot.")
        End If

        If Me.optNorth.Checked Then
            options.ZeroAngle = PolarZeroAngle.North
        ElseIf Me.optSouth.Checked Then
            options.ZeroAngle = PolarZeroAngle.South
        ElseIf Me.optWest.Checked Then
            options.ZeroAngle = PolarZeroAngle.West
        ElseIf Me.optEast.Checked Then
            options.ZeroAngle = PolarZeroAngle.East
        Else
            Throw New InvalidOperationException("Select a zero-angle position for the polar plot.")
        End If

        options.ConnectPoints = Me.ckConnectPoints.Checked

        If Me.tbAngularTickInterval.Text <> String.Empty And CheckNumeric(Me.tbAngularTickInterval) Then options.AngularTickInterval = ParseUiDouble(Me.tbAngularTickInterval.Text, "Angular Tick Interval")
        If Me.tbRadialTickInterval.Text <> String.Empty And CheckNumeric(Me.tbRadialTickInterval) Then options.RadialTickInterval = ParseUiDouble(Me.tbRadialTickInterval.Text, "Radial Tick Interval")

        Return options
    End Function
End Class
Friend Class Ui01SankeyPlotProjection46
 Friend Tag As HelpTopic
 Friend Text As String = ""
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents ckConnectAcrossMissingStages As New FakeControl46()
 Friend WithEvents ckShowNodeLabels As New FakeControl46()
 Friend WithEvents cmbNodeLabelMode As New FakeControl46()
 Friend WithEvents cmbNodeOrder As New FakeControl46()
 Friend WithEvents cmbPercentageBasis As New FakeControl46()
 Friend WithEvents cmbRibbonColorMode As New FakeControl46()
 Friend WithEvents cmbVerticalAlignment As New FakeControl46()
 Friend WithEvents lblNodeLabelMode As New FakeControl46()
 Friend WithEvents lblPercentageBasis As New FakeControl46()
 Friend WithEvents nudNodeGap As New FakeControl46()
 Friend WithEvents optOutputRange As New FakeControl46()
 Friend WithEvents optWorkbook As New FakeControl46()
 Friend WithEvents optWorksheet As New FakeControl46()
    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub
    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub
    Private Sub ckShowNodeLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowNodeLabels.CheckedChanged
        Me.UpdateLabelControlState()
    End Sub
    Private Sub cmbNodeLabelMode_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbNodeLabelMode.SelectedIndexChanged
        Me.UpdateLabelControlState()
    End Sub
    Private Sub UpdateLabelControlState()
        Dim labelsEnabled As Boolean = Me.ckShowNodeLabels.Checked
        Me.lblNodeLabelMode.Enabled = labelsEnabled
        Me.cmbNodeLabelMode.Enabled = labelsEnabled

        Dim usesPercentage As Boolean = labelsEnabled AndAlso
                                        (Me.cmbNodeLabelMode.SelectedIndex = 2 OrElse
                                         Me.cmbNodeLabelMode.SelectedIndex = 3)
        Me.lblPercentageBasis.Enabled = usesPercentage
        Me.cmbPercentageBasis.Enabled = usesPercentage
    End Sub
    Private Function GetPlotOptions() As SankeyPlotOptions
        Dim options As New SankeyPlotOptions With {
            .NodeGapFraction = CDbl(Me.nudNodeGap.Value) / 100.0R,
            .NodeOrder = Me.GetSelectedNodeOrder(),
            .VerticalAlignment = Me.GetSelectedVerticalAlignment(),
            .ConnectAcrossMissingStages = Me.ckConnectAcrossMissingStages.Checked
        }
        Return options
    End Function
    Private Function GetSelectedNodeOrder() As SankeyNodeOrderMode
        Select Case Me.cmbNodeOrder.SelectedIndex
            Case 1
                Return SankeyNodeOrderMode.FirstAppearance
            Case 2
                Return SankeyNodeOrderMode.Alphabetical
            Case 3
                Return SankeyNodeOrderMode.WeightDescending
            Case Else
                Return SankeyNodeOrderMode.Automatic
        End Select
    End Function
    Private Function GetSelectedVerticalAlignment() As SankeyVerticalAlignment
        Select Case Me.cmbVerticalAlignment.SelectedIndex
            Case 1
                Return SankeyVerticalAlignment.Top
            Case 2
                Return SankeyVerticalAlignment.Bottom
            Case Else
                Return SankeyVerticalAlignment.Center
        End Select
    End Function
    Private Function GetSelectedRibbonColorMode() As SankeyRibbonColorMode
        Select Case Me.cmbRibbonColorMode.SelectedIndex
            Case 1
                Return SankeyRibbonColorMode.TargetNode
            Case 2
                Return SankeyRibbonColorMode.Neutral
            Case Else
                Return SankeyRibbonColorMode.SourceNode
        End Select
    End Function
    Private Function GetSelectedNodeLabelMode() As SankeyNodeLabelMode
        Select Case Me.cmbNodeLabelMode.SelectedIndex
            Case 1
                Return SankeyNodeLabelMode.NameAndValue
            Case 2
                Return SankeyNodeLabelMode.NameAndPercentage
            Case 3
                Return SankeyNodeLabelMode.NamePercentageAndValue
            Case Else
                Return SankeyNodeLabelMode.NameOnly
        End Select
    End Function
    Private Function GetSelectedPercentageBasis() As SankeyPercentageBasis
        If Me.cmbPercentageBasis.SelectedIndex = 1 Then
            Return SankeyPercentageBasis.StageTotal
        End If
        Return SankeyPercentageBasis.ReferenceTotal
    End Function
End Class
End Namespace
