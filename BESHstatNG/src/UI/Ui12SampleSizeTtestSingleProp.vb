Imports System.IO
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.SampleSizeCalc
Imports Microsoft.Office.Interop.Excel

Public Class Ui12SampleSizeTtestSingleProp

    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()

        Me.Text = analysis
        Me.lblSettings.Text = "Use " & Chr(34) & AppGlobals.app.DecimalSeparator & Chr(34) & " as a decimal separator and " &
                                      Chr(34) & AppGlobals.app.ThousandsSeparator & Chr(34) & " as a thousands separator."
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)
        Me.lblCustom4.Visible = False
        Me.tbCustom4.Visible = False
        Me.cbHypothesisType.Visible = False
        Me.lblHypothesisType.Visible = False

        ' Add any initialization after the InitializeComponent() call.
        If Me.Text = "Sample Size - Paired T-test" Then
            Me.lblMeanDiff.Text = "Mean Difference"
            Me.lblSD.Text = "Standard Deviation"
            Me.tbMeanDiff.Text = "5"
            Me.tbSD.Text = "10"
            Me.lblKappa.Visible = False
            Me.tbKappa.Visible = False
            Me.lblAlpha.Text = "Alpha"

        ElseIf Me.Text = "Sample Size - Unpaired T-test" Then
            ConfigureChoiceCombo("Hypothesis Type", "Superiority", "Noninferiority", "Equivalence")
            ApplyUnpairedTLayout()

        ElseIf Me.Text = "Sample Size - Single Proportion" Then
            Me.lblKappa.Visible = False
            Me.tbKappa.Visible = False
            Me.lblMeanDiff.Text = "Proportion"
            Me.lblSD.Text = "Null Hypothesis Proportion"
            Me.tbMeanDiff.Text = FormatUiDouble(0.5)
            Me.tbSD.Text = FormatUiDouble(0.4)
            Me.lblAlpha.Text = "Alpha"

        ElseIf Me.Text = "Sample Size - Independent Proportions" Then
            ConfigureChoiceCombo("Hypothesis Type", "Superiority", "Noninferiority", "Equivalence")
            ApplyIndependentProportionsLayout()
            Me.tbMeanDiff.Text = FormatUiDouble(0.5)
            Me.tbSD.Text = FormatUiDouble(0.4)

        ElseIf Me.Text = "Sample Size - Log-rank Test" Then
            Me.lblMeanDiff.Text = "Hazard Ratio"
            Me.lblSD.Text = "Control Event Proportion"
            Me.lblKappa.Text = "Experimental Event Proportion"
            Me.lblCustom4.Text = "Ratio of control to experimental subjects"
            Me.lblCustom4.Visible = True
            Me.tbCustom4.Visible = True

            Me.tbMeanDiff.Text = FormatUiDouble(0.75)
            Me.tbSD.Text = FormatUiDouble(0.4)
            Me.tbKappa.Text = FormatUiDouble(0.3)
            Me.tbCustom4.Text = "1"
            Me.lblAlpha.Text = "Alpha"

        ElseIf Me.Text = "Sample Size - Cox Regression" Then
            ConfigureChoiceCombo("Covariate Type", "Binary Covariate", "Continuous Covariate")
            ApplyCoxLayout()

        ElseIf Me.Text = "Sample Size - Intraclass Correlation (ICC)" Then
            Me.lblMeanDiff.Text = "Null ICC"
            Me.lblSD.Text = "Alternative ICC"
            Me.lblKappa.Text = "Observations per Subject"
            Me.tbMeanDiff.Text = FormatUiDouble(0.6)
            Me.tbSD.Text = FormatUiDouble(0.8)
            Me.tbKappa.Text = "2"
            Me.lblAlpha.Text = "One-sided Alpha"

        ElseIf Me.Text = "Sample Size - Agreement (Bland-Altman)" Then
            Me.lblMeanDiff.Text = "SD of Differences"
            Me.lblSD.Text = "Desired LoA CI Half-Width"
            Me.lblKappa.Text = "LoA Multiplier"
            Me.tbMeanDiff.Text = "10"
            Me.tbSD.Text = "5"
            Me.tbKappa.Text = FormatUiDouble(1.96)
            Me.lblAlpha.Text = "Alpha"
            Me.lblBeta.Visible = False
            Me.spinBtnBeta.Visible = False

        End If

        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim strErr As String = String.Empty, wait As Boolean = False

            Me.CheckData(strErr, wait)
            If strErr <> String.Empty Then
                MsgBox(strErr, vbExclamation, AppGlobals.gsAPP_TITLE)
                Exit Sub
            End If

            If Me.Text = "Sample Size - Paired T-test" Then
                Me.RunPTtest()
            ElseIf Me.Text = "Sample Size - Unpaired T-test" Then
                Me.RunUPTtest()
            ElseIf Me.Text = "Sample Size - Single Proportion" Then
                Me.RunSingleProp()
            ElseIf Me.Text = "Sample Size - Independent Proportions" Then
                Me.RunIndependentProp()
            ElseIf Me.Text = "Sample Size - Log-rank Test" Then
                Me.RunLogRank()
            ElseIf Me.Text = "Sample Size - Cox Regression" Then
                Me.RunCox()
            ElseIf Me.Text = "Sample Size - Intraclass Correlation (ICC)" Then
                Me.RunICC()
            ElseIf Me.Text = "Sample Size - Agreement (Bland-Altman)" Then
                Me.RunBlandAltman()
            End If
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunLogRank()
        Dim hazardRatio As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Hazard ratio")
        Dim controlEventProportion As Double = ParseUiDouble(Me.tbSD.Text, "Control event proportion")
        Dim experimentalEventProportion As Double = ParseUiDouble(Me.tbKappa.Text, "Experimental event proportion")
        Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        Dim result = SampleSizeCalculator.CalculateLogRankSampleSize(hazardRatio,
                                                        controlEventProportion, experimentalEventProportion,
                                                        kappa, Alpha, Beta, True)

        Dim out As String =
            $"Inputs: Hazard ratio={hazardRatio}; Control event proportion={controlEventProportion}; Experimental event proportion={experimentalEventProportion}; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
            $"alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Required events:{result.RequiredEvents}{vbNewLine}" &
            $"Average event proportion={result.AverageEventProportion}{vbNewLine}" &
            $"Estimated Number of Controls:{result.NumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{vbNewLine}" &
            $"Estimated Total Number of Subjects:{result.TotalNumberOfSubjects}{vbNewLine}{vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunCox()
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim rSquaredWithOtherCovariates As Double = ParseOptionalUiDouble(Me.tbCustom4.Text, 0.0)
        Dim overallEventProportion As Double = ParseOptionalUiDouble(Me.tbKappa.Text, Double.NaN)

        If SelectedComboValue() = "Continuous Covariate" Then
            Dim hazardRatioPerUnit As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Hazard ratio per unit")
            Dim covariateSd As Double = ParseUiDouble(Me.tbSD.Text, "Covariate standard deviation")

            Dim result = SampleSizeCalculator.CalculateCoxEventCountContinuousCovariate(hazardRatioPerUnit, covariateSd, Alpha, Beta,
                                                                           rSquaredWithOtherCovariates, overallEventProportion, True)

            Dim subjectsText As String = If(result.EstimatedNumberOfSubjects > 0,
                                         result.EstimatedNumberOfSubjects.ToString(), "not estimated (overall event proportion not provided)")

            Dim overallEventText As String = If(Double.IsNaN(result.OverallEventProportion),
                                               "not provided", result.OverallEventProportion.ToString())

            Dim out As String =
                $"Inputs: Hazard ratio per unit={hazardRatioPerUnit}; Covariate SD={covariateSd}; R-squared with other covariates={rSquaredWithOtherCovariates}; Overall event proportion={overallEventText}.{vbNewLine}" &
                $"alpha={Alpha}; beta={Beta}.{vbNewLine}" &
                $"Required events:{result.RequiredEvents}{vbNewLine}" &
                $"Estimated Number of Subjects:{subjectsText}{vbNewLine}" &
                $"log(HR)={result.LogHazardRatio}; Effective variance={result.EffectiveVariance}{vbNewLine}{vbNewLine}"

            Me.tbOutput.AppendText(out)
            Exit Sub
        End If

        Dim hazardRatio As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Hazard ratio")
        Dim kappa As Double = ParseUiDouble(Me.tbSD.Text, "Ratio of control to experimental subjects")

        Dim binaryResult = SampleSizeCalculator.CalculateCoxEventCountBinaryCovariate(hazardRatio, kappa, Alpha, Beta,
                                                                       rSquaredWithOtherCovariates,
                                                                       overallEventProportion, True)

        Dim binarySubjectsText As String = If(binaryResult.EstimatedNumberOfSubjects > 0,
                                               binaryResult.EstimatedNumberOfSubjects.ToString(), "not estimated (overall event proportion not provided)")

        Dim binaryOverallEventText As String = If(Double.IsNaN(binaryResult.OverallEventProportion),
                                                   "not provided", binaryResult.OverallEventProportion.ToString())

        Dim binaryOut As String =
            $"Inputs: Hazard ratio={hazardRatio}; Ratio of control to experimental subjects={kappa}; R-squared with other covariates={rSquaredWithOtherCovariates}; Overall event proportion={binaryOverallEventText}.{vbNewLine}" &
            $"alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Required events:{binaryResult.RequiredEvents}{vbNewLine}" &
            $"Estimated Number of Subjects:{binarySubjectsText}{vbNewLine}" &
            $"log(HR)={binaryResult.LogHazardRatio}; Effective variance={binaryResult.EffectiveVariance}{vbNewLine}{vbNewLine}"

        Me.tbOutput.AppendText(binaryOut)
    End Sub

    Private Sub RunICC()
        Dim nullIcc As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Null ICC")
        Dim alternativeIcc As Double = ParseUiDouble(Me.tbSD.Text, "Alternative ICC")
        Dim observationsPerSubjectValue As Double = ParseUiDouble(Me.tbKappa.Text, "Observations per subject")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        If observationsPerSubjectValue < 2 OrElse observationsPerSubjectValue <> Math.Truncate(observationsPerSubjectValue) Then
            MsgBox("Observations per subject must be an integer greater than or equal to 2.", vbExclamation, AppGlobals.gsAPP_TITLE)
            Exit Sub
        End If

        Dim result = SampleSizeCalculator.CalculateIccHypothesisTestSampleSize(nullIcc, alternativeIcc,
                                                                     CInt(observationsPerSubjectValue), Alpha, Beta)

        Dim out As String =
            $"Inputs: Null ICC={nullIcc}; Alternative ICC={alternativeIcc}; Observations per subject={CInt(observationsPerSubjectValue)}.{vbNewLine}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Estimated Number of Subjects:{result.NumberOfSubjects}{vbNewLine}" &
            $"Achieved power={result.AchievedPower}{vbNewLine}{vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunBlandAltman()
        Dim sdDifference As Double = ParseUiDouble(Me.tbMeanDiff.Text, "SD of differences")
        Dim desiredHalfWidth As Double = ParseUiDouble(Me.tbSD.Text, "Desired LoA CI half-width")
        Dim loaMultiplier As Double = ParseUiDouble(Me.tbKappa.Text, "LoA multiplier")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)

        Dim result = SampleSizeCalculator.CalculateBlandAltmanLoASampleSize(sdDifference, desiredHalfWidth, Alpha, loaMultiplier)

        Dim out As String =
            $"Inputs: SD of differences={sdDifference}; Desired LoA CI half-width={desiredHalfWidth}; LoA multiplier={loaMultiplier}; alpha={Alpha}.{vbNewLine}" &
            $"Estimated Number of Pairs:{result.NumberOfPairs}{vbNewLine}" &
            $"Achieved LoA CI half-width={result.AchievedHalfWidth}{vbNewLine}{vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub


    Private Sub RunIndependentProp()
        Dim bFocusSet As Boolean
        Dim sAllErrors As String = String.Empty

        Dim controlProp As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Control Group Proportion")
        If controlProp <= 0 OrElse controlProp >= 1 Then
            If Not bFocusSet Then
                Me.tbMeanDiff.Select()
                bFocusSet = True
            End If
            sAllErrors += "Control Group Proportion should satisfy 0 < p < 1." & vbLf
        End If

        Dim experimentalProp As Double = ParseUiDouble(Me.tbSD.Text, "Experimental Group Proportion")
        If experimentalProp <= 0 OrElse experimentalProp >= 1 Then
            If Not bFocusSet Then
                Me.tbSD.Select()
                bFocusSet = True
            End If
            sAllErrors += "Experimental Group Proportion should satisfy 0 < p < 1." & vbLf
        End If

        If Len(sAllErrors) > 0 Then
            MsgBox(sAllErrors, vbExclamation, AppGlobals.gsAPP_TITLE)
            Exit Sub
        End If

        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        If SelectedComboValue() = "Noninferiority" Then
            Dim marginAbsolute As Double = ParseUiDouble(Me.tbKappa.Text, "Noninferiority margin")
            Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")

            Dim result = SampleSizeCalculator.CalculateNonInferiorityIndependentProportions(controlProp, experimentalProp,
                                                                              -Math.Abs(marginAbsolute),
                                                                              kappa, Alpha, Beta)

            Dim out As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Noninferiority margin=-{Math.Abs(marginAbsolute)} on (Experimental - Control) scale; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"For uncorrected chi-square test:{vbNewLine}" &
            $"Estimated Number of Controls:{result.UncorrectedNumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{result.UncorrectedNumberOfExperimental} {vbNewLine}" &
            $"For corrected chi-square or Fisher's exact test:{vbNewLine}" &
            $"Estimated Number of Controls:{result.CorrectedNumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{result.CorrectedNumberOfExperimental}{vbNewLine}{vbNewLine}"

            Me.tbOutput.AppendText(out)
            Exit Sub
        End If

        If SelectedComboValue() = "Equivalence" Then
            Dim marginAbsolute As Double = ParseUiDouble(Me.tbKappa.Text, "Equivalence margin")
            Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")

            Dim result = SampleSizeCalculator.CalculateEquivalenceIndependentProportions(controlProp, experimentalProp,
                                                                           -Math.Abs(marginAbsolute),
                                                                           Math.Abs(marginAbsolute),
                                                                           kappa, Alpha, Beta)

            Dim out As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Equivalence bounds=[-{Math.Abs(marginAbsolute)}, +{Math.Abs(marginAbsolute)}]; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Lower-bound requirement (uncorrected): Controls={result.LowerBoundUncorrectedNumberOfControls}; Experimental={result.LowerBoundUncorrectedNumberOfExperimental}{vbNewLine}" &
            $"Upper-bound requirement (uncorrected): Controls={result.UpperBoundUncorrectedNumberOfControls}; Experimental={result.UpperBoundUncorrectedNumberOfExperimental}{vbNewLine}" &
            $"Lower-bound requirement (corrected/Fisher): Controls={result.LowerBoundCorrectedNumberOfControls}; Experimental={result.LowerBoundCorrectedNumberOfExperimental}{vbNewLine}" &
            $"Upper-bound requirement (corrected/Fisher): Controls={result.UpperBoundCorrectedNumberOfControls}; Experimental={result.UpperBoundCorrectedNumberOfExperimental}{vbNewLine}" &
            $"Driving bound: {result.DrivingBound}{vbNewLine}" &
            $"Final uncorrected requirement: Controls={result.UncorrectedNumberOfControls}; Experimental={result.UncorrectedNumberOfExperimental}{vbNewLine}" &
            $"Final corrected/Fisher requirement: Controls={result.CorrectedNumberOfControls}; Experimental={result.CorrectedNumberOfExperimental}{vbNewLine}{vbNewLine}"

            Me.tbOutput.AppendText(out)
            Exit Sub
        End If

        If controlProp = experimentalProp Then
            MsgBox("Control and experimental proportions should not be equal for superiority planning.", vbExclamation, AppGlobals.gsAPP_TITLE)
            Exit Sub
        End If

        Dim kappaSuperiority As Double = ParseUiDouble(Me.tbKappa.Text, "Ratio of control to experimental subjects")
        Dim resultSuperiority = SampleSizeCalculator.CalculateIndependentProportions(controlProp, experimentalProp, kappaSuperiority, Alpha, Beta)

        Dim outSuperiority As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Ratio of control to experimental subjects={kappaSuperiority}.{vbNewLine}" &
            $"alpha={Alpha}; beta={Beta}.{vbNewLine}For uncorrected chi-square test:{vbNewLine}" &
            $"Estimated Number of Controls:{resultSuperiority.UncorrectedNumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.UncorrectedNumberOfExperimental}{vbNewLine}For corrected chi-square or Fisher's exact test:{vbNewLine}" &
            $"Estimated Number of Controls:{resultSuperiority.CorrectedNumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.CorrectedNumberOfExperimental}{vbNewLine}{vbNewLine}"

        Me.tbOutput.AppendText(outSuperiority)
    End Sub

    Private Sub RunSingleProp()
        Dim bFocusSet As Boolean, sAllErrors As String = String.Empty

        'Check input values
        Dim prop As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Proportion")
        If prop < 0 Or prop > 1 Then
            If Not bFocusSet Then
                Me.tbMeanDiff.Select()
                bFocusSet = True
            End If
            sAllErrors += "Proportion: Proportion should be 0 < Proportion < 1. " + vbLf
        End If

        Dim H0Prop As Double = ParseUiDouble(Me.tbSD.Text, "Null Hypothesis Proportion")
        If H0Prop < 0 Or H0Prop > 1 Then
            If Not bFocusSet Then
                Me.tbSD.Select()
                bFocusSet = True
            End If
            sAllErrors += "Null Hypothesis Proportion should be 0 < Proportion < 1. " + vbLf
        End If

        If prop = H0Prop Then
            If Not bFocusSet Then
                Me.tbMeanDiff.Select()
                bFocusSet = True
            End If
            sAllErrors += "Proportion: Proportion should be not equal to H0 Proportion. " + vbLf
        End If

        If Len(sAllErrors) > 0 Then
            MsgBox(sAllErrors, vbExclamation, AppGlobals.gsAPP_TITLE)
            Exit Sub 'Display any error we got in the main call
        End If

        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim result = SampleSizeCalculator.CalculateSingleProportion(prop, H0Prop, Alpha, Beta)

        Dim out As String = $"Inputs: Proportion={prop}; Null Hypothesis Proportion={H0Prop}; alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                        $"Est. Number of Subjects:{result.NumberOfSubjects} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunPTtest()
        Dim diff As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Mean difference")
        Dim sd As Double = ParseUiDouble(Me.tbSD.Text, "Standard deviation")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim result = SampleSizeCalculator.CalculatePairedTTest(diff, sd, Alpha, Beta)

        Dim out As String = $"Inputs: Mean difference={diff}; SD={sd}; alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                        $"Est. Number of Pairs:{result.NumberOfPairs} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunUPTtest()
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        If SelectedComboValue() = "Noninferiority" Then
            Dim expectedDifference As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Expected mean difference")
            Dim marginAbsolute As Double = ParseUiDouble(Me.tbSD.Text, "Noninferiority margin")
            Dim sd As Double = ParseUiDouble(Me.tbKappa.Text, "Standard deviation")
            Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")

            Dim result = SampleSizeCalculator.CalculateNonInferiorityUnpairedTTest(expectedDifference,
                                                                     -Math.Abs(marginAbsolute), sd, kappa, Alpha, Beta)

            Dim out As String =
                $"Inputs: Expected mean difference={expectedDifference}; Noninferiority margin=-{Math.Abs(marginAbsolute)} on (Experimental - Control) scale; SD={sd}; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
                $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
                $"Estimated Number of Controls:{result.NumberOfControls}{vbNewLine}" &
                $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{vbNewLine}{vbNewLine}"

            Me.tbOutput.AppendText(out)
            Exit Sub
        End If

        If SelectedComboValue() = "Equivalence" Then
            Dim expectedDifference As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Expected mean difference")
            Dim marginAbsolute As Double = ParseUiDouble(Me.tbSD.Text, "Equivalence margin")
            Dim sd As Double = ParseUiDouble(Me.tbKappa.Text, "Standard deviation")
            Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")

            Dim result = SampleSizeCalculator.CalculateEquivalenceUnpairedTTest(expectedDifference, -Math.Abs(marginAbsolute),
                                                                  Math.Abs(marginAbsolute), sd, kappa, Alpha, Beta)

            Dim out As String =
                $"Inputs: Expected mean difference={expectedDifference}; Equivalence bounds=[-{Math.Abs(marginAbsolute)}, +{Math.Abs(marginAbsolute)}]; SD={sd}; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
                $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
                $"Lower-bound requirement: Controls={result.LowerBoundNumberOfControls}; Experimental={result.LowerBoundNumberOfExperimental}{vbNewLine}" &
                $"Upper-bound requirement: Controls={result.UpperBoundNumberOfControls}; Experimental={result.UpperBoundNumberOfExperimental}{vbNewLine}" &
                $"Driving bound: {result.DrivingBound}{vbNewLine}" &
                $"Estimated Number of Controls:{result.NumberOfControls}{vbNewLine}" &
                $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{vbNewLine}{vbNewLine}"

            Me.tbOutput.AppendText(out)
            Exit Sub
        End If

        Dim diff As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Mean difference")
        Dim sdSuperiority As Double = ParseUiDouble(Me.tbSD.Text, "Standard deviation")
        Dim kappaSuperiority As Double = ParseUiDouble(Me.tbKappa.Text, "Ratio of control to experimental subjects")
        Dim resultSuperiority = SampleSizeCalculator.CalculateUnpairedTTest(diff, sdSuperiority, kappaSuperiority, Alpha, Beta)

        Dim outSuperiority As String =
            $"Inputs: Mean difference={diff}; SD={sdSuperiority}; Ratio of control to experimental subjects={kappaSuperiority}.{vbNewLine}" &
            $"alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Estimated Number of Controls:{resultSuperiority.NumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.NumberOfExperimental}{vbNewLine}{vbNewLine}"

        Me.tbOutput.AppendText(outSuperiority)
    End Sub

    Private Sub CheckData(ByRef sAllErrors As String, ByRef bwait As Boolean)
        Dim sError As String = String.Empty
        Dim bFocusSet As Boolean = False
        Dim dummy As Double

        sAllErrors = String.Empty
        bwait = False

        ' ----------------------------------------------------------------------
        ' Numeric validation for required visible fields
        ' ----------------------------------------------------------------------
        If Not CheckNumeric(tbMeanDiff, dummy, sError) Then
            AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, sError)
            bwait = True
        End If

        If Not CheckNumeric(tbSD, dummy, sError) Then
            AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, sError)
            bwait = True
        End If

        If RequiresKappaValue() Then
            If Not CheckNumeric(tbKappa, dummy, sError) Then
                AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, sError)
                bwait = True
            End If
        Else
            TryValidateOptionalNumeric(Me.tbKappa, Me.lblKappa.Text, sAllErrors, bFocusSet, bwait)
        End If

        If RequiresCustom4Value() Then
            If Not CheckNumeric(tbCustom4, dummy, sError) Then
                AddUiError(sAllErrors, bFocusSet, Me.tbCustom4, Me.lblCustom4.Text, sError)
                bwait = True
            End If
        Else
            TryValidateOptionalNumeric(Me.tbCustom4, Me.lblCustom4.Text, sAllErrors, bFocusSet, bwait)
        End If

        If bwait Then Exit Sub

        ' ----------------------------------------------------------------------
        ' Domain / range validation after numeric parsing succeeds
        ' ----------------------------------------------------------------------
        Select Case Me.Text

            Case "Sample Size - Paired T-test"
                Dim diff As Double = ParseUiDouble(Me.tbMeanDiff.Text, Me.lblMeanDiff.Text)
                Dim sd As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)

                If diff = 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should not be 0.")
                If sd <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should be > 0.")

            Case "Sample Size - Unpaired T-test"
                Dim v1 As Double = ParseUiDouble(Me.tbMeanDiff.Text, Me.lblMeanDiff.Text)
                Dim v2 As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)
                Dim v3 As Double = ParseUiDouble(Me.tbKappa.Text, Me.lblKappa.Text)

                If SelectedComboValue() = "Superiority" Then
                    If v1 = 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should not be 0 for superiority planning.")
                    If v2 <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should be > 0.")
                    If v3 <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should be > 0.")

                ElseIf SelectedComboValue() = "Noninferiority" Then
                    Dim ratio As Double = ParseUiDouble(Me.tbCustom4.Text, Me.lblCustom4.Text)
                    If v2 <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should be > 0.")
                    If v3 <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should be > 0.")
                    If ratio <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbCustom4, Me.lblCustom4.Text, "should be > 0.")

                ElseIf SelectedComboValue() = "Equivalence" Then
                    Dim ratio As Double = ParseUiDouble(Me.tbCustom4.Text, Me.lblCustom4.Text)
                    If v2 <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should be > 0.")
                    If v3 <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should be > 0.")
                    If ratio <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbCustom4, Me.lblCustom4.Text, "should be > 0.")
                End If

            Case "Sample Size - Single Proportion"
                Dim p As Double = ParseUiDouble(Me.tbMeanDiff.Text, Me.lblMeanDiff.Text)
                Dim p0 As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)

                If p <= 0 OrElse p >= 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should satisfy 0 < p < 1.")
                If p0 <= 0 OrElse p0 >= 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should satisfy 0 < p < 1.")
                If p = p0 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should not equal the null-hypothesis proportion.")

            Case "Sample Size - Independent Proportions"
                Dim pControl As Double = ParseUiDouble(Me.tbMeanDiff.Text, Me.lblMeanDiff.Text)
                Dim pExperimental As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)

                If pControl <= 0 OrElse pControl >= 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should satisfy 0 < p < 1.")
                If pExperimental <= 0 OrElse pExperimental >= 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should satisfy 0 < p < 1.")

                If SelectedComboValue() = "Superiority" Then
                    Dim ratio As Double = ParseUiDouble(Me.tbKappa.Text, Me.lblKappa.Text)
                    If ratio <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should be > 0.")
                    If pControl = pExperimental Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should not equal the experimental proportion for superiority planning.")

                ElseIf SelectedComboValue() = "Noninferiority" OrElse SelectedComboValue() = "Equivalence" Then
                    Dim margin As Double = ParseUiDouble(Me.tbKappa.Text, Me.lblKappa.Text)
                    Dim ratio As Double = ParseUiDouble(Me.tbCustom4.Text, Me.lblCustom4.Text)
                    If margin <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should be > 0.")
                    If ratio <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbCustom4, Me.lblCustom4.Text, "should be > 0.")
                End If

            Case "Sample Size - Log-rank Test"
                Dim hr As Double = ParseUiDouble(Me.tbMeanDiff.Text, Me.lblMeanDiff.Text)
                Dim pControl As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)
                Dim pExperimental As Double = ParseUiDouble(Me.tbKappa.Text, Me.lblKappa.Text)
                Dim ratio As Double = ParseUiDouble(Me.tbCustom4.Text, Me.lblCustom4.Text)

                If hr <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should be > 0.")
                If hr = 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should not be 1.")
                If pControl <= 0 OrElse pControl >= 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should satisfy 0 < p < 1.")
                If pExperimental <= 0 OrElse pExperimental >= 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should satisfy 0 < p < 1.")
                If ratio <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbCustom4, Me.lblCustom4.Text, "should be > 0.")

            Case "Sample Size - Cox Regression"
                Dim hr As Double = ParseUiDouble(Me.tbMeanDiff.Text, Me.lblMeanDiff.Text)
                If hr <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should be > 0.")
                If hr = 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should not be 1.")

                If SelectedComboValue() = "Continuous Covariate" Then
                    Dim covariateSd As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)
                    If covariateSd <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should be > 0.")
                Else
                    Dim ratio As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)
                    If ratio <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should be > 0.")
                End If

                If Me.tbKappa.Text.Trim() <> String.Empty Then
                    Dim overallEventProportion As Double = ParseUiDouble(Me.tbKappa.Text, Me.lblKappa.Text)
                    If overallEventProportion <= 0 OrElse overallEventProportion >= 1 Then
                        AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should satisfy 0 < p < 1 when provided.")
                    End If
                End If

                If Me.tbCustom4.Text.Trim() <> String.Empty Then
                    Dim rSquared As Double = ParseUiDouble(Me.tbCustom4.Text, Me.lblCustom4.Text)
                    If rSquared < 0 OrElse rSquared >= 1 Then
                        AddUiError(sAllErrors, bFocusSet, Me.tbCustom4, Me.lblCustom4.Text, "should satisfy 0 <= R-squared < 1 when provided.")
                    End If
                End If

            Case "Sample Size - Intraclass Correlation (ICC)"
                Dim nullIcc As Double = ParseUiDouble(Me.tbMeanDiff.Text, Me.lblMeanDiff.Text)
                Dim alternativeIcc As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)
                Dim observationsPerSubject As Double = ParseUiDouble(Me.tbKappa.Text, Me.lblKappa.Text)

                If nullIcc < 0 OrElse nullIcc >= 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should satisfy 0 <= ICC < 1.")
                If alternativeIcc <= 0 OrElse alternativeIcc >= 1 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should satisfy 0 < ICC < 1.")
                If alternativeIcc <= nullIcc Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should be greater than the null ICC.")
                If observationsPerSubject < 2 OrElse observationsPerSubject <> Math.Truncate(observationsPerSubject) Then
                    AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should be an integer greater than or equal to 2.")
                End If

            Case "Sample Size - Agreement (Bland-Altman)"
                Dim sdDifference As Double = ParseUiDouble(Me.tbMeanDiff.Text, Me.lblMeanDiff.Text)
                Dim desiredHalfWidth As Double = ParseUiDouble(Me.tbSD.Text, Me.lblSD.Text)
                Dim loaMultiplier As Double = ParseUiDouble(Me.tbKappa.Text, Me.lblKappa.Text)

                If sdDifference <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbMeanDiff, Me.lblMeanDiff.Text, "should be > 0.")
                If desiredHalfWidth <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbSD, Me.lblSD.Text, "should be > 0.")
                If loaMultiplier <= 0 Then AddUiError(sAllErrors, bFocusSet, Me.tbKappa, Me.lblKappa.Text, "should be > 0.")

        End Select

        bwait = (sAllErrors <> String.Empty)
    End Sub

    Private Sub btnSaveToSheet_Click(sender As Object, e As System.EventArgs) Handles btnSaveToSheet.Click
        If Me.tbOutput.Text <> String.Empty Then
            AppGlobals.app.ActiveWorkbook.Worksheets.Add()
            Dim sh As Worksheet = AppGlobals.app.ActiveWorkbook.ActiveSheet
            sh.Cells(1, 1) = Me.Text
            Dim i As Integer = 2
            For Each s As String In Me.tbOutput.Lines
                Dim line As String = s
                sh.Cells(i, 1) = line
                i += 1
            Next
        End If
    End Sub

    Private Sub cbHypothesisType_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbHypothesisType.SelectedIndexChanged
        Select Case Me.Text
            Case "Sample Size - Unpaired T-test"
                ApplyUnpairedTLayout()
            Case "Sample Size - Independent Proportions"
                ApplyIndependentProportionsLayout()
            Case "Sample Size - Cox Regression"
                ApplyCoxLayout()
        End Select
    End Sub

    ' -------------------------------------------------------------------------
    ' Helpers
    ' -------------------------------------------------------------------------
    Private Sub ConfigureChoiceCombo(labelText As String, ParamArray items() As String)
        Me.lblHypothesisType.Text = labelText
        Me.cbHypothesisType.Visible = True
        Me.lblHypothesisType.Visible = True
        Me.cbHypothesisType.Items.Clear()
        Me.cbHypothesisType.Items.AddRange(items)
        If Me.cbHypothesisType.Items.Count > 0 Then Me.cbHypothesisType.SelectedIndex = 0
    End Sub

    Private Function SelectedComboValue() As String
        If Me.cbHypothesisType.SelectedItem Is Nothing Then Return String.Empty
        Return Me.cbHypothesisType.SelectedItem.ToString()
    End Function

    Private Function RequiresKappaValue() As Boolean
        Select Case Me.Text
            Case "Sample Size - Paired T-test", "Sample Size - Single Proportion"
                Return False
            Case "Sample Size - Cox Regression"
                Return False   ' overall event proportion is optional
            Case Else
                Return Me.tbKappa.Visible
        End Select
    End Function

    Private Function RequiresCustom4Value() As Boolean
        If Not Me.tbCustom4.Visible Then Return False
        If Me.Text = "Sample Size - Log-rank Test" Then Return True
        If Me.Text = "Sample Size - Unpaired T-test" AndAlso SelectedComboValue() <> "Superiority" Then Return True
        If Me.Text = "Sample Size - Independent Proportions" AndAlso SelectedComboValue() <> "Superiority" Then Return True

        Return False   ' optional for Cox and hidden elsewhere
    End Function

    Private Function ParseOptionalUiDouble(textValue As String, defaultValue As Double) As Double
        If textValue Is Nothing OrElse textValue.Trim() = String.Empty Then
            Return defaultValue
        End If
        Return ParseUiDouble(textValue, "Optional value")
    End Function

    Private Sub SetDefaultIfBlank(tb As System.Windows.Forms.TextBox, defaultValue As String)
        If tb.Text Is Nothing OrElse tb.Text.Trim() = String.Empty Then
            tb.Text = defaultValue
        End If
    End Sub

    Private Sub AddUiError(ByRef sAllErrors As String, ByRef bFocusSet As Boolean, ctrl As Control, labelText As String, message As String)
        If Not bFocusSet Then
            ctrl.Select()
            bFocusSet = True
        End If

        sAllErrors &= labelText & ": " & message & vbLf
    End Sub

    Private Sub TryValidateOptionalNumeric(tb As System.Windows.Forms.TextBox, labelText As String,
                                       ByRef sAllErrors As String, ByRef bFocusSet As Boolean, ByRef bwait As Boolean)
        If tb.Visible AndAlso tb.Text.Trim() <> String.Empty Then
            Dim tmp As Double
            Dim sError As String = String.Empty
            If Not CheckNumeric(tb, tmp, sError) Then
                AddUiError(sAllErrors, bFocusSet, tb, labelText, sError)
                bwait = True
            End If
        End If
    End Sub

    Private Sub ApplyUnpairedTLayout()
        Me.lblBeta.Visible = True
        Me.spinBtnBeta.Visible = True
        Me.lblKappa.Visible = True
        Me.tbKappa.Visible = True

        If SelectedComboValue() = "Noninferiority" Then
            Me.lblMeanDiff.Text = "Expected Mean Difference"
            Me.lblSD.Text = "Noninferiority Margin"
            Me.lblKappa.Text = "Standard Deviation"
            Me.lblCustom4.Text = "Ratio of control to experimental subjects"
            Me.lblCustom4.Visible = True
            Me.tbCustom4.Visible = True

            SetDefaultIfBlank(Me.tbMeanDiff, "0")
            SetDefaultIfBlank(Me.tbSD, "5")
            SetDefaultIfBlank(Me.tbKappa, "10")
            SetDefaultIfBlank(Me.tbCustom4, "1")
            Me.lblAlpha.Text = "One-sided Alpha"

        ElseIf SelectedComboValue() = "Equivalence" Then
            Me.lblMeanDiff.Text = "Expected Mean Difference"
            Me.lblSD.Text = "Equivalence Margin"
            Me.lblKappa.Text = "Standard Deviation"
            Me.lblCustom4.Text = "Ratio of control to experimental subjects"
            Me.lblCustom4.Visible = True
            Me.tbCustom4.Visible = True

            SetDefaultIfBlank(Me.tbMeanDiff, "0")
            SetDefaultIfBlank(Me.tbSD, "5")
            SetDefaultIfBlank(Me.tbKappa, "10")
            SetDefaultIfBlank(Me.tbCustom4, "1")
            Me.lblAlpha.Text = "One-sided Alpha"

        Else
            Me.lblMeanDiff.Text = "Mean Difference"
            Me.lblSD.Text = "Standard Deviation"
            Me.lblKappa.Text = "Ratio of control to experimental subjects"
            Me.lblCustom4.Visible = False
            Me.tbCustom4.Visible = False

            SetDefaultIfBlank(Me.tbMeanDiff, "5")
            SetDefaultIfBlank(Me.tbSD, "10")
            SetDefaultIfBlank(Me.tbKappa, "1")
            Me.lblAlpha.Text = "Alpha"
        End If
    End Sub

    Private Sub ApplyIndependentProportionsLayout()
        Me.lblBeta.Visible = True
        Me.spinBtnBeta.Visible = True
        Me.lblKappa.Visible = True
        Me.tbKappa.Visible = True

        Me.lblMeanDiff.Text = "Control Group Proportion"
        Me.lblSD.Text = "Experimental Group Proportion"

        If SelectedComboValue() = "Noninferiority" Then
            Me.lblKappa.Text = "Noninferiority Margin"
            Me.lblCustom4.Text = "Ratio of control to experimental subjects"
            Me.lblCustom4.Visible = True
            Me.tbCustom4.Visible = True

            SetDefaultIfBlank(Me.tbMeanDiff, FormatUiDouble(0.4))
            SetDefaultIfBlank(Me.tbSD, FormatUiDouble(0.4))
            If ParseUiDouble(Me.tbKappa.Text) >= 1 Or ParseUiDouble(Me.tbKappa.Text) <= 0 Then Me.tbKappa.Text = ""
            SetDefaultIfBlank(Me.tbKappa, FormatUiDouble(0.1))
                SetDefaultIfBlank(Me.tbCustom4, "1")
            Me.lblAlpha.Text = "One-sided Alpha"

        ElseIf SelectedComboValue() = "Equivalence" Then
            Me.lblKappa.Text = "Equivalence Margin"
            Me.lblCustom4.Text = "Ratio of control to experimental subjects"
            Me.lblCustom4.Visible = True
            Me.tbCustom4.Visible = True

            SetDefaultIfBlank(Me.tbMeanDiff, FormatUiDouble(0.4))
            SetDefaultIfBlank(Me.tbSD, FormatUiDouble(0.4))
            If ParseUiDouble(Me.tbKappa.Text) >= 1 Or ParseUiDouble(Me.tbKappa.Text) <= 0 Then Me.tbKappa.Text = ""
            SetDefaultIfBlank(Me.tbKappa, FormatUiDouble(0.1))
            SetDefaultIfBlank(Me.tbCustom4, "1")
            Me.lblAlpha.Text = "One-sided Alpha"

        Else
            Me.lblKappa.Text = "Ratio of control to experimental subjects"
            Me.lblCustom4.Visible = False
            Me.tbCustom4.Visible = False

            SetDefaultIfBlank(Me.tbMeanDiff, FormatUiDouble(0.4))
            SetDefaultIfBlank(Me.tbSD, FormatUiDouble(0.5))
            SetDefaultIfBlank(Me.tbKappa, "1")
            Me.lblAlpha.Text = "Alpha"
        End If
    End Sub

    Private Sub ApplyCoxLayout()
        Me.lblBeta.Visible = True
        Me.spinBtnBeta.Visible = True
        Me.lblKappa.Visible = True
        Me.tbKappa.Visible = True
        Me.lblCustom4.Visible = True
        Me.tbCustom4.Visible = True
        Me.lblAlpha.Text = "Alpha"

        If SelectedComboValue() = "Continuous Covariate" Then
            Me.lblMeanDiff.Text = "Hazard Ratio per Unit"
            Me.lblSD.Text = "Covariate Standard Deviation"
            Me.lblKappa.Text = "Overall Event Proportion (optional)"
            Me.lblCustom4.Text = "R-squared with Other Covariates (optional)"

            SetDefaultIfBlank(Me.tbMeanDiff, FormatUiDouble(1.25))
            SetDefaultIfBlank(Me.tbSD, "1")
            SetDefaultIfBlank(Me.tbKappa, FormatUiDouble(0.4))
            SetDefaultIfBlank(Me.tbCustom4, "0")

        Else
            Me.lblMeanDiff.Text = "Hazard Ratio"
            Me.lblSD.Text = "Ratio of control to experimental subjects"
            Me.lblKappa.Text = "Overall Event Proportion (optional)"
            Me.lblCustom4.Text = "R-squared with Other Covariates (optional)"

            SetDefaultIfBlank(Me.tbMeanDiff, FormatUiDouble(0.75))
            SetDefaultIfBlank(Me.tbSD, "1")
            SetDefaultIfBlank(Me.tbKappa, FormatUiDouble(0.4))
            SetDefaultIfBlank(Me.tbCustom4, "0")
        End If
    End Sub

End Class