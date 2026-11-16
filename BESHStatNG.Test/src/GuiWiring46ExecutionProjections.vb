Option Strict On
Option Explicit On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon
Imports Control = BESHStatNG.Test.Harness46.FakeControl46
Imports ExcelDnaResultWriter = BESHStatNG.Test.Harness46.FakeResultWriter46
Imports Range = BESHStatNG.Test.Harness46.FakeRange46
Imports ProcessListofResultTables = BESHStatNG.Test.Harness46.FakeTables46
Namespace Harness46
Partial Friend Class Ui12SampleSizeTtestSinglePropProjection46
 Friend WithEvents btCompute As New FakeControl46()
 Friend WithEvents cbHypothesisType As New FakeControl46()
 Friend WithEvents lblAlpha As New FakeControl46()
 Friend WithEvents lblBeta As New FakeControl46()
 Friend WithEvents lblCustom4 As New FakeControl46()
 Friend WithEvents lblKappa As New FakeControl46()
 Friend WithEvents lblMeanDiff As New FakeControl46()
 Friend WithEvents lblSD As New FakeControl46()
 Friend WithEvents spinBtnAlpha As New FakeControl46()
 Friend WithEvents spinBtnBeta As New FakeControl46()
 Friend WithEvents tbCustom4 As New FakeControl46()
 Friend WithEvents tbKappa As New FakeControl46()
 Friend WithEvents tbMeanDiff As New FakeControl46()
 Friend WithEvents tbOutput As New FakeControl46()
 Friend WithEvents tbSD As New FakeControl46()
    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim strErr As String = String.Empty, wait As Boolean = False

            Me.CheckData(strErr, wait)
            If strErr <> String.Empty Then
                FakeMessages46.Record(strErr, vbExclamation, FakeGlobals46.gsAPP_TITLE)
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
            FakeFaults46.Record(ex, False, True)
        End Try
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
    Private Sub RunLogRank()
        Dim hazardRatio As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Hazard ratio")
        Dim controlEventProportion As Double = ParseUiDouble(Me.tbSD.Text, "Control event proportion")
        Dim experimentalEventProportion As Double = ParseUiDouble(Me.tbKappa.Text, "Experimental event proportion")
        Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        Dim requestLogRankSampleSize As New SampleSizePlanningRequest(SampleSizePlanningMethod.LogRankSampleSize, Alpha, Beta, New Double() {hazardRatio, controlEventProportion, experimentalEventProportion, kappa})
        Me.tbOutput.AppendText(FakePlanning46.Calculate(requestLogRankSampleSize).Text)
    End Sub
    Private Sub RunCox()
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim rSquaredWithOtherCovariates As Double = ParseOptionalUiDouble(Me.tbCustom4.Text, 0.0)
        Dim overallEventProportion As Double = ParseOptionalUiDouble(Me.tbKappa.Text, Double.NaN)

        If SelectedComboValue() = "Continuous Covariate" Then
            Dim hazardRatioPerUnit As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Hazard ratio per unit")
            Dim covariateSd As Double = ParseUiDouble(Me.tbSD.Text, "Covariate standard deviation")

            Dim requestCoxEventCountContinuousCovariate As New SampleSizePlanningRequest(SampleSizePlanningMethod.CoxEventCountContinuousCovariate, Alpha, Beta, New Double() {hazardRatioPerUnit, covariateSd, rSquaredWithOtherCovariates, overallEventProportion}, overallEventProvided:=Me.tbKappa.Text.Trim() <> String.Empty, rSquaredProvided:=Me.tbCustom4.Text.Trim() <> String.Empty)
            Me.tbOutput.AppendText(FakePlanning46.Calculate(requestCoxEventCountContinuousCovariate).Text)
            Exit Sub
        End If

        Dim hazardRatio As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Hazard ratio")
        Dim kappa As Double = ParseUiDouble(Me.tbSD.Text, "Ratio of control to experimental subjects")

        Dim requestCoxEventCountBinaryCovariate As New SampleSizePlanningRequest(SampleSizePlanningMethod.CoxEventCountBinaryCovariate, Alpha, Beta, New Double() {hazardRatio, kappa, rSquaredWithOtherCovariates, overallEventProportion}, overallEventProvided:=Me.tbKappa.Text.Trim() <> String.Empty, rSquaredProvided:=Me.tbCustom4.Text.Trim() <> String.Empty)
        Me.tbOutput.AppendText(FakePlanning46.Calculate(requestCoxEventCountBinaryCovariate).Text)
    End Sub
    Private Sub RunICC()
        Dim nullIcc As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Null ICC")
        Dim alternativeIcc As Double = ParseUiDouble(Me.tbSD.Text, "Alternative ICC")
        Dim observationsPerSubjectValue As Double = ParseUiDouble(Me.tbKappa.Text, "Observations per subject")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        If observationsPerSubjectValue < 2 OrElse observationsPerSubjectValue <> Math.Truncate(observationsPerSubjectValue) Then
            FakeMessages46.Record("Observations per subject must be an integer greater than or equal to 2.", vbExclamation, FakeGlobals46.gsAPP_TITLE)
            Exit Sub
        End If

        Dim requestIccHypothesisTestSampleSize As New SampleSizePlanningRequest(SampleSizePlanningMethod.IccHypothesisTestSampleSize, Alpha, Beta, New Double() {nullIcc, alternativeIcc, observationsPerSubjectValue}, observationsPerSubject:=CInt(observationsPerSubjectValue))
        Me.tbOutput.AppendText(FakePlanning46.Calculate(requestIccHypothesisTestSampleSize).Text)
    End Sub
    Private Sub RunBlandAltman()
        Dim sdDifference As Double = ParseUiDouble(Me.tbMeanDiff.Text, "SD of differences")
        Dim desiredHalfWidth As Double = ParseUiDouble(Me.tbSD.Text, "Desired LoA CI half-width")
        Dim loaMultiplier As Double = ParseUiDouble(Me.tbKappa.Text, "LoA multiplier")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)

        Dim requestBlandAltmanLoASampleSize As New SampleSizePlanningRequest(SampleSizePlanningMethod.BlandAltmanLoASampleSize, Alpha, Double.NaN, New Double() {sdDifference, desiredHalfWidth, loaMultiplier})
        Me.tbOutput.AppendText(FakePlanning46.Calculate(requestBlandAltmanLoASampleSize).Text)
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
            FakeMessages46.Record(sAllErrors, vbExclamation, FakeGlobals46.gsAPP_TITLE)
            Exit Sub
        End If

        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        If SelectedComboValue() = "Noninferiority" Then
            Dim marginAbsolute As Double = ParseUiDouble(Me.tbKappa.Text, "Noninferiority margin")
            Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")

            Dim requestNonInferiorityIndependentProportions As New SampleSizePlanningRequest(SampleSizePlanningMethod.NonInferiorityIndependentProportions, Alpha, Beta, New Double() {controlProp, experimentalProp, marginAbsolute, kappa})
            Me.tbOutput.AppendText(FakePlanning46.Calculate(requestNonInferiorityIndependentProportions).Text)
            Exit Sub
        End If

        If SelectedComboValue() = "Equivalence" Then
            Dim marginAbsolute As Double = ParseUiDouble(Me.tbKappa.Text, "Equivalence margin")
            Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")

            Dim requestEquivalenceIndependentProportions As New SampleSizePlanningRequest(SampleSizePlanningMethod.EquivalenceIndependentProportions, Alpha, Beta, New Double() {controlProp, experimentalProp, marginAbsolute, kappa})
            Me.tbOutput.AppendText(FakePlanning46.Calculate(requestEquivalenceIndependentProportions).Text)
            Exit Sub
        End If

        If controlProp = experimentalProp Then
            FakeMessages46.Record("Control and experimental proportions should not be equal for superiority planning.", vbExclamation, FakeGlobals46.gsAPP_TITLE)
            Exit Sub
        End If

        Dim kappaSuperiority As Double = ParseUiDouble(Me.tbKappa.Text, "Ratio of control to experimental subjects")
        Dim requestIndependentProportions As New SampleSizePlanningRequest(SampleSizePlanningMethod.IndependentProportions, Alpha, Beta, New Double() {controlProp, experimentalProp, kappaSuperiority})
        Me.tbOutput.AppendText(FakePlanning46.Calculate(requestIndependentProportions).Text)
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
            FakeMessages46.Record(sAllErrors, vbExclamation, FakeGlobals46.gsAPP_TITLE)
            Exit Sub 'Display any error we got in the main call
        End If

        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim requestSingleProportion As New SampleSizePlanningRequest(SampleSizePlanningMethod.SingleProportion, Alpha, Beta, New Double() {prop, H0Prop})
        Me.tbOutput.AppendText(FakePlanning46.Calculate(requestSingleProportion).Text)
    End Sub
    Private Sub RunPTtest()
        Dim diff As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Mean difference")
        Dim sd As Double = ParseUiDouble(Me.tbSD.Text, "Standard deviation")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim requestPairedTTest As New SampleSizePlanningRequest(SampleSizePlanningMethod.PairedTTest, Alpha, Beta, New Double() {diff, sd})
        Me.tbOutput.AppendText(FakePlanning46.Calculate(requestPairedTTest).Text)
    End Sub
    Private Sub RunUPTtest()
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        If SelectedComboValue() = "Noninferiority" Then
            Dim expectedDifference As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Expected mean difference")
            Dim marginAbsolute As Double = ParseUiDouble(Me.tbSD.Text, "Noninferiority margin")
            Dim sd As Double = ParseUiDouble(Me.tbKappa.Text, "Standard deviation")
            Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")

            Dim requestNonInferiorityUnpairedTTest As New SampleSizePlanningRequest(SampleSizePlanningMethod.NonInferiorityUnpairedTTest, Alpha, Beta, New Double() {expectedDifference, marginAbsolute, sd, kappa})
            Me.tbOutput.AppendText(FakePlanning46.Calculate(requestNonInferiorityUnpairedTTest).Text)
            Exit Sub
        End If

        If SelectedComboValue() = "Equivalence" Then
            Dim expectedDifference As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Expected mean difference")
            Dim marginAbsolute As Double = ParseUiDouble(Me.tbSD.Text, "Equivalence margin")
            Dim sd As Double = ParseUiDouble(Me.tbKappa.Text, "Standard deviation")
            Dim kappa As Double = ParseUiDouble(Me.tbCustom4.Text, "Ratio of control to experimental subjects")

            Dim requestEquivalenceUnpairedTTest As New SampleSizePlanningRequest(SampleSizePlanningMethod.EquivalenceUnpairedTTest, Alpha, Beta, New Double() {expectedDifference, marginAbsolute, sd, kappa})
            Me.tbOutput.AppendText(FakePlanning46.Calculate(requestEquivalenceUnpairedTTest).Text)
            Exit Sub
        End If

        Dim diff As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Mean difference")
        Dim sdSuperiority As Double = ParseUiDouble(Me.tbSD.Text, "Standard deviation")
        Dim kappaSuperiority As Double = ParseUiDouble(Me.tbKappa.Text, "Ratio of control to experimental subjects")
        Dim requestUnpairedTTest As New SampleSizePlanningRequest(SampleSizePlanningMethod.UnpairedTTest, Alpha, Beta, New Double() {diff, sdSuperiority, kappaSuperiority})
        Me.tbOutput.AppendText(FakePlanning46.Calculate(requestUnpairedTTest).Text)
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
    Private Sub SetDefaultIfBlank(tb As FakeControl46, defaultValue As String)
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
    Private Sub TryValidateOptionalNumeric(tb As FakeControl46, labelText As String,
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
Partial Friend Class Ui82x2Projection46
 Friend WithEvents btCompute As New FakeControl46()
 Friend WithEvents ckAssociation As New FakeControl46()
 Friend WithEvents ckChi2 As New FakeControl46()
 Friend WithEvents ckFisher As New FakeControl46()
 Friend WithEvents ckLiddel As New FakeControl46()
 Friend WithEvents ckOR As New FakeControl46()
 Friend WithEvents ckRR As New FakeControl46()
 Friend WithEvents spinBtnAlpha As New FakeControl46()
 Private Function getDataMultipleGroups() As DataObj
  FakeOutput46.ImportCalls += 1
  Return FakeOutput46.Imported
 End Function
    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim options As New ExcelCommon.TwoByTwoReportOptions With {
                .Alpha = CDbl(Me.spinBtnAlpha.Value), .Fisher = Me.ckFisher.Checked,
                .ChiSquare = Me.ckChi2.Checked, .Association = Me.ckAssociation.Checked,
                .Liddell = Me.ckLiddel.Checked, .OddsRatio = Me.ckOR.Checked, .RiskRatio = Me.ckRR.Checked}
            Dim report As ExcelCommon.ContingencyReportResult
            If Me.optWorksheetData.Checked Then
                Dim input = WindowsContingencyInputAdapter.ParseWorksheet(Me.getDataMultipleGroups(),
                    New ExcelCommon.SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders = False, .HasRowLabels = False}, True)
                If Not input.Success Then
                    FakeMessages46.Record(input.Validation.Issues.First(Function(i) i.Severity = ExcelCommon.SpreadsheetValidationSeverity.Error).Message, vbOKOnly, "2x2 table analysis")
                    Exit Sub
                End If
                report = ExcelCommon.ContingencyReportPolicy.TwoByTwo(input.Data, options)
            ElseIf Me.optScreenData.Checked Then
                report = WindowsContingencyInputAdapter.AnalyzeDialog(CDbl(Me.spinBtnA.Value), CDbl(Me.spinBtnB.Value),
                    CDbl(Me.spinBtnC.Value), CDbl(Me.spinBtnD.Value), options)
            Else
                FakeMessages46.Record("Select a count input source.", vbOKOnly, "2x2 table analysis")
                Exit Sub
            End If
            For Each issue In report.Validation.Issues
                If issue.Severity = ExcelCommon.SpreadsheetValidationSeverity.Warning Then
                    FakeMessages46.Record(issue.Message, vbOKOnly, "Fisher's exact test")
                End If
            Next
            If Not report.Success Then
                FakeMessages46.Record(report.Validation.Issues.First(Function(i) i.Severity = ExcelCommon.SpreadsheetValidationSeverity.Error).Message, vbOKOnly, "2x2 table analysis")
                Exit Sub
            End If
            Dim res = report.ToLegacyTables()

            'Dump outputs
            Dim WriteRes = GetResultWriter() 'pass just table from the main test output
            Dim rr = New FakeTables46(res)
            Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
            Dim totcols As Integer = rr.TotCols
            If FakeOutput46.AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
                If FakeMessages46.Record("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                    Exit Sub
                End If
            End If

            rr.writeToSheet(WriteRes, True)
        Catch ex As Exception
            FakeFaults46.Record(ex, False, True)
        End Try
    End Sub
    Private Function GetResultWriter() As FakeResultWriter46
        Dim WriteRes = New FakeResultWriter46, rRange As FakeRange46
        If Me.optWorkbook.Checked Then
            WriteRes.wb = FakeGlobals46.app.Workbooks.Add()
            WriteRes.ws = FakeGlobals46.app.ActiveWorkbook.ActiveSheet
        ElseIf Me.optWorksheet.Checked Then
            WriteRes.wb = FakeGlobals46.app.ActiveWorkbook
            WriteRes.wb.Worksheets.Add()
            WriteRes.ws = FakeGlobals46.app.ActiveWorkbook.ActiveSheet
        Else
            WriteRes.wb = FakeGlobals46.app.ActiveWorkbook
            WriteRes.ws = FakeOutput46.WorksheetFromRefAdress(Me.RefEditOutput.Address)
            rRange = WriteRes.ws.Range(Me.RefEditOutput.Address)
            WriteRes.setRowPointer(rRange.Row)
            WriteRes.setColumnPointer(rRange.Column)
        End If

        Return WriteRes
    End Function
End Class
Partial Friend Class Ui8ProportionsProjection46
 Friend WithEvents btCompute As New FakeControl46()
 Friend WithEvents spinBtnAlpha As New FakeControl46()
 Friend WithEvents spinBtnB As New FakeControl46()
    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim mode = ExcelCommon.ProportionsInputPolicy.ModeFromChoices(Me.optSingle.Checked, Me.optIndependent.Checked, Me.optPaired.Checked)
            Dim hypothesis As ExcelCommon.IndependentProportionsHypothesis = ExcelCommon.IndependentProportionsHypothesis.Superiority
            Select Case Me.cbHypothesisType.SelectedItem
                Case "Noninferiority" : hypothesis = ExcelCommon.IndependentProportionsHypothesis.Noninferiority
                Case "Equivalence" : hypothesis = ExcelCommon.IndependentProportionsHypothesis.Equivalence
            End Select
            If mode = ExcelCommon.ProportionsMode.Independent AndAlso Not Me.ValidateIndependentMargin() Then Exit Sub
            Dim report = WindowsContingencyInputAdapter.AnalyzeProportions(mode, hypothesis,
                CDbl(Me.spinBtnA.Value), CDbl(Me.spinBtnB.Value), If(mode = ExcelCommon.ProportionsMode.Independent, CDbl(Me.spinBtnC.Value), 0.0),
                If(mode = ExcelCommon.ProportionsMode.SingleSample, 0.0, If(mode = ExcelCommon.ProportionsMode.Independent, CDbl(Me.spinBtnD.Value), CDbl(Me.spinBtnC.Value))),
                If(mode = ExcelCommon.ProportionsMode.Paired, CDbl(Me.spinBtnD.Value), 0.0), CDbl(Me.spinBtnAlpha.Value), CDbl(Me.spinBtnMargin_IndProp.Value))
            If Not report.Success Then
                Dim issue = report.Validation.Issues.First(Function(i) i.Severity = ExcelCommon.SpreadsheetValidationSeverity.Error)
                FakeMessages46.Record(issue.Message, vbOKOnly, "Data input")
                If issue.FieldPath = "counts.total2" OrElse issue.FieldPath = "counts.responders2" Then
                    Me.spinBtnC.Select()
                ElseIf issue.FieldPath = "options.margin" Then
                    Me.spinBtnMargin_IndProp.Select()
                Else
                    Me.spinBtnA.Select()
                End If
                Exit Sub
            End If
            Dim res = report.ToLegacyTables()

            'Dump outputs
            Dim WriteRes = GetResultWriter() 'pass just table from the main test output
            Dim rr = New FakeTables46(res)
            Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
            Dim totcols As Integer = rr.TotCols
            If FakeOutput46.AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
                If FakeMessages46.Record("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                    Exit Sub
                End If
            End If

            rr.writeToSheet(WriteRes, True)
        Catch ex As Exception
            FakeFaults46.Record(ex, False, True)
        End Try
    End Sub
    Private Function GetResultWriter() As FakeResultWriter46
        Dim WriteRes = New FakeResultWriter46, rRange As FakeRange46
        If Me.optWorkbook.Checked Then
            WriteRes.wb = FakeGlobals46.app.Workbooks.Add()
            WriteRes.ws = FakeGlobals46.app.ActiveWorkbook.ActiveSheet
        ElseIf Me.optWorksheet.Checked Then
            WriteRes.wb = FakeGlobals46.app.ActiveWorkbook
            WriteRes.wb.Worksheets.Add()
            WriteRes.ws = FakeGlobals46.app.ActiveWorkbook.ActiveSheet
        Else
            WriteRes.wb = FakeGlobals46.app.ActiveWorkbook
            WriteRes.ws = FakeOutput46.WorksheetFromRefAdress(Me.RefEditOutput.Address)
            rRange = WriteRes.ws.Range(Me.RefEditOutput.Address)
            WriteRes.setRowPointer(rRange.Row)
            WriteRes.setColumnPointer(rRange.Column)
        End If

        Return WriteRes
    End Function
End Class
End Namespace
