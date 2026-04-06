Imports System.IO
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

        ' Add any initialization after the InitializeComponent() call.
        If Me.Text = "Sample Size - Paired T-test" Then
            Me.lblKappa.Visible = False
            Me.tbKappa.Visible = False
        ElseIf Me.Text = "Sample Size - Unpaired T-test" Then

        ElseIf Me.Text = "Sample Size - Single Proportion" Then
            Me.lblKappa.Visible = False
            Me.tbKappa.Visible = False

            Me.lblMeanDiff.Text = "Proportion"
            Me.lblSD.Text = "Null Hypothesis Proportion"
            Me.tbMeanDiff.Text = String.Empty
            Me.tbSD.Text = FormatUiDouble(0.5)
        ElseIf Me.Text = "Sample Size - Independent Proportions" Then
            Me.lblMeanDiff.Text = "Control Group Proportion"
            Me.lblSD.Text = "Experimental Group Proportion"
            Me.tbMeanDiff.Text = FormatUiDouble(0.4)
            Me.tbSD.Text = FormatUiDouble(0.5)
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
            End If
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunIndependentProp()
        Dim bFocusSet As Boolean, sAllErrors As String = String.Empty

        'Check input values
        Dim CProp As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Control Group Proportion")
        If CProp < 0 Or CProp > 1 Then
            If Not bFocusSet Then
                Me.tbMeanDiff.Select()
                bFocusSet = True
            End If
            sAllErrors += "Control Group Proportion should be 0 < Proportion < 1. " + vbLf
        End If

        Dim TProp As Double = ParseUiDouble(Me.tbSD.Text, "Experimental Group Proportion")
        If TProp < 0 Or TProp > 1 Then
            If Not bFocusSet Then
                Me.tbSD.Select()
                bFocusSet = True
            End If
            sAllErrors += "Experimental Group Proportion should be 0 < Proportion < 1. " + vbLf
        End If

        If CProp = TProp Then
            If Not bFocusSet Then
                Me.tbMeanDiff.Select()
                bFocusSet = True
            End If
            sAllErrors += "Proportion: Proportions should not be equal. " + vbLf
        End If

        If Len(sAllErrors) > 0 Then
            MsgBox(sAllErrors, vbExclamation, AppGlobals.gsAPP_TITLE)
            Exit Sub 'Display any error we got in the main call
        End If

        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim Kappa As Double = ParseUiDouble(Me.tbKappa.Text, "Ratio of control to experimental subjects")
        Dim result As IndependentProportionsSampleSizeResult = SampleSizeCalculator.CalculateIndependentProportions(CProp, TProp, Kappa, Alpha, Beta)

        Dim out As String = $"Inputs: Control Group Proportion={CProp}; Experimental Group Proportion={TProp}; Ratio of control to experimental subjects={Kappa}. {vbNewLine}" &
                        $"alpha={Alpha}; beta={Beta}. {vbNewLine} For uncorrected chi-square test: {vbNewLine}" &
                        $"Estimated Number of Controls:{result.UncorrectedNumberOfControls} {vbNewLine}" &
                        $"Est. Number of Experimental subjects:{result.UncorrectedNumberOfExperimental} {vbNewLine} For corrected chi-square or Fisher's exact test: {vbNewLine}" &
                        $"Estimated Number of Controls:{result.CorrectedNumberOfControls} {vbNewLine}" &
                        $"Est. Number of Experimental subjects:{result.CorrectedNumberOfExperimental} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
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
        Dim result As SingleProportionSampleSizeResult = SampleSizeCalculator.CalculateSingleProportion(prop, H0Prop, Alpha, Beta)

        Dim out As String = $"Inputs: Proportion={prop}; Null Hypothesis Proportion={H0Prop}; alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                        $"Est. Number of Subjects:{result.NumberOfSubjects} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunPTtest()
        Dim diff As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Mean difference")
        Dim sd As Double = ParseUiDouble(Me.tbSD.Text, "Standard deviation")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim result As PairedTTestSampleSizeResult = SampleSizeCalculator.CalculatePairedTTest(diff, sd, Alpha, Beta)

        Dim out As String = $"Inputs: Mean difference={diff}; SD={sd}; alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                        $"Est. Number of Paires:{result.NumberOfPairs} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunUPTtest()
        Dim diff As Double = ParseUiDouble(Me.tbMeanDiff.Text, "Mean difference")
        Dim sd As Double = ParseUiDouble(Me.tbSD.Text, "Standard deviation")
        Dim Kappa As Double = ParseUiDouble(Me.tbKappa.Text, "Ratio of control to experimental subjects")
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)
        Dim result As UnpairedTTestSampleSizeResult = SampleSizeCalculator.CalculateUnpairedTTest(diff, sd, Kappa, Alpha, Beta)

        Dim out As String = $"Inputs: Mean difference={diff}; SD={sd}; Ratio of control to experimental subjects={Kappa}. {vbNewLine}" &
                        $"alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                        $"Estimated Number of Controls:{result.NumberOfControls} {vbNewLine}" &
                        $"Est. Number of Experimental subjects:{result.NumberOfExperimental} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub CheckData(ByRef sAllErrors As String, ByRef bwait As Boolean)
        Dim sError As String = String.Empty, bFocusSet As Boolean
        Dim diff As Double, sd As Double, Kappa As Double

        'Check input values
        If Not CheckNumeric(tbMeanDiff, diff, sError) Then
            'set the focus to the 1st control with an error
            If Not bFocusSet Then
                Me.tbMeanDiff.Select()
                bFocusSet = True
            End If
            'build an error string, so we display all errors on the UserForm in one error message
            sAllErrors += Me.lblMeanDiff.Text & ":" & sError & vbLf
            bwait = True
        End If

        If Not CheckNumeric(tbSD, sd, sError) Then
            If Not bFocusSet Then
                Me.tbSD.Select()
                bFocusSet = True
            End If
            sAllErrors += Me.lblSD.Text & ":" & sError & vbLf
            bwait = True
        End If

        If Me.Text = "Sample Size - Unpaired T-test" Then
            If Not CheckNumeric(tbKappa, Kappa, sError) Then
                If Not bFocusSet Then
                    Me.tbKappa.Select()
                    bFocusSet = True
                End If
                sAllErrors += "Ratio of controls/experimental subjects:" + sError + vbLf
                bwait = True
            End If
        End If
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

End Class