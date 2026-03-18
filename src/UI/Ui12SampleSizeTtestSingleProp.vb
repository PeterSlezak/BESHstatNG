Imports System.IO
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui12SampleSizeTtestSingleProp

    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()

        Me.Text = analysis
        Me.lblSettings.Text = "Use " & Chr(34) & AppGlobals.app.DecimalSeparator & Chr(34) & " as a decimal separator and " &
                                      Chr(34) & AppGlobals.app.ThousandsSeparator & Chr(34) & " as a thousands separator."

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
            Me.tbSD.Text = "0.5"
        ElseIf Me.Text = "Sample Size - Independent Proportions" Then
            Me.lblMeanDiff.Text = "Control Group Proportion"
            Me.lblSD.Text = "Experimental Group Proportion"
            Me.tbMeanDiff.Text = "0.4"
            Me.tbSD.Text = "0.5"
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
        Dim CProp As Double = CDbl(Me.tbMeanDiff.Text)
        If CProp < 0 Or CProp > 1 Then
            If Not bFocusSet Then
                Me.tbMeanDiff.Select()
                bFocusSet = True
            End If
            sAllErrors += "Control Group Proportion should be 0 < Proportion < 1. " + vbLf
        End If

        Dim TProp As Double = CDbl(Me.tbSD.Text)
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
        Dim Kappa As Double = CDbl(Me.tbKappa.Text)

        'get the estimate based on the normal distribution
        Dim Pciara As Double = (CProp + TProp / Kappa) / (1 + 1 / Kappa)
        Dim UncorrNest As Double = distributions.NormSInv(1.0 - Alpha / 2.0) * Math.Sqrt((1.0 + Kappa) * Pciara * (1.0 - Pciara))
        UncorrNest = (UncorrNest + (distributions.NormSInv(1.0 - Beta) * Math.Sqrt(CProp * (1.0 - CProp) + Kappa * TProp * (1.0 - TProp)))) ^ 2
        UncorrNest = (UncorrNest / (TProp - CProp) ^ 2) / Kappa
        UncorrNest = RoundUp(UncorrNest, 0)
        Dim UncorrNt As Integer = Int(UncorrNest)

        Dim CorrNest As Double = (UncorrNt / 4.0) * (1.0 + Math.Sqrt(1.0 + (2.0 * (Kappa + 1.0)) / (CDbl(UncorrNt) * Kappa * Math.Abs(CProp - TProp)))) ^ 2
        Dim CorrNt As Integer = Int(RoundUp(CorrNest, 0))

        Dim out As String = $"Inputs: Control Group Proportion={CProp}; Experimental Group Proportion={TProp}; Ratio of control to experimental subjects={Kappa}. {vbNewLine}" &
                            $"alpha={Alpha}; beta={Beta}. {vbNewLine} For uncorrected chi-square test: {vbNewLine}" &
                            $"Estimated Number of Controls:{Int(UncorrNt * Kappa)} {vbNewLine}" &
                            $"Est. Number of Experimental subjects:{UncorrNt} {vbNewLine} For corrected chi-square or Fisher's exact test: {vbNewLine}" &
                            $"Estimated Number of Controls:{Int(CorrNt * Kappa)} {vbNewLine}" &
                            $"Est. Number of Experimental subjects:{CorrNt} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunSingleProp()
        Dim bFocusSet As Boolean, sAllErrors As String = String.Empty

        'Check input values
        Dim prop As Double = CDbl(Me.tbMeanDiff.Text)
        If prop < 0 Or prop > 1 Then
            If Not bFocusSet Then
                Me.tbMeanDiff.Select()
                bFocusSet = True
            End If
            sAllErrors += "Proportion: Proportion should be 0 < Proportion < 1. " + vbLf
        End If

        Dim H0Prop As Double = CDbl(Me.tbSD.Text)
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

        'get the estimate based on the normal distribution
        Dim Nest As Double = prop * (1.0 - prop) * ((distributions.NormSInv(1.0 - Alpha / 2.0) + distributions.NormSInv(1.0 - Beta)) / (prop - H0Prop)) ^ 2
        Nest = RoundUp(Nest, 0)
        Dim n As Integer = Int(Nest) 'final sample size estimate

        Dim out As String = $"Inputs: Proportion={prop}; Null Hypothesis Proportion={H0Prop}; alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                            $"Est. Number of Subjects:{n} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunPTtest()
        Dim Crit As Double
        Dim diff As Double = CDbl(Me.tbMeanDiff.Text)
        Dim sd As Double = CDbl(Me.tbSD.Text)
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        'get 1st estimate based on the normal distribution
        Dim Nest As Double = (sd * (distributions.NormSInv(1.0 - Alpha / 2.0) + distributions.NormSInv(1.0 - Beta)) / diff) ^ 2
        Nest = RoundUp(Nest, 0)
        Dim n As Integer = Int(Nest)

        If n > 1 Then 'Iterate to get the final estimate
            For i = 0 To 1000
                Crit = (distributions.T_Inv(Alpha / 2, n - 1) + distributions.T_Inv(Beta, n - 1)) ^ 2 / (diff / sd) ^ 2
                If CDbl(n) > Crit Then Exit For
                n += 1
            Next
        End If

        Dim out As String = $"Inputs: Mean difference={diff}; SD={sd}; alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                            $"Est. Number of Paires:{n} {vbNewLine} {vbNewLine}"

        Me.tbOutput.AppendText(out)
    End Sub

    Private Sub RunUPTtest()
        Dim Crit As Double
        Dim diff As Double = CDbl(Me.tbMeanDiff.Text)
        Dim sd As Double = CDbl(Me.tbSD.Text)
        Dim Kappa As Double = CDbl(Me.tbKappa.Text)
        Dim Alpha As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim Beta As Double = CDbl(Me.spinBtnBeta.Value)

        'get 1st estimate based on the normal distribution
        Dim Nest As Double = (1.0 + 1.0 / Kappa) * (sd * (distributions.NormSInv(1.0 - Alpha / 2.0) + distributions.NormSInv(1.0 - Beta)) / diff) ^ 2
        Nest = RoundUp(Nest, 0)
        Dim nt As Integer = Int(Nest) 'final sample size estimates

        If nt > 1 Then
            'Iterate to get the final estimate
            For i = 0 To 1000
                Crit = (1 + 1 / Kappa) * (distributions.T_Inv(Alpha / 2, nt * (Kappa + 1) - 2) + distributions.T_Inv(Beta, nt * (Kappa + 1) - 2)) ^ 2 / (diff / sd) ^ 2
                If CDbl(nt) > Crit Then Exit For
                nt += 1
            Next
        End If

        Dim out As String = $"Inputs: Mean difference={diff}; SD={sd}; Ratio of control to experimental subjects={Kappa}. {vbNewLine}" &
                            $"alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                            $"Estimated Number of Controls:{Int(nt * Kappa)} {vbNewLine}" &
                            $"Est. Number of Experimental subjects:{nt} {vbNewLine} {vbNewLine}"

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
            sAllErrors += "Mean Difference:" + sError + vbLf
            bwait = True
        End If

        If Not CheckNumeric(tbSD, sd, sError) Then
            If Not bFocusSet Then
                Me.tbSD.Select()
                bFocusSet = True
            End If
            sAllErrors += "Standard Deviation:" + sError + vbLf
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