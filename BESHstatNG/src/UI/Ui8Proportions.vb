Imports System.Net
Imports System.Security.Cryptography
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui8Proportions
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)

        If Me.optSingle.Checked = True Then
            Me.lbl1.Text = "Total number of observations in the study"
            Me.lbl2.Text = "Number of responders"
            Me.lbl3.Visible = False
            Me.lbl4.Visible = False
            Me.spinBtnC.Visible = False
            Me.spinBtnD.Visible = False
            Me.spinBtnA.Select()
        End If
        Me.cbHypothesisType.Items.AddRange(New Object() {"Superiority", "Noninferiority", "Equivalence"})
        Me.cbHypothesisType.SelectedIndex = 0
        ' New independent-proportions NI / equivalence controls.
        Me.cbHypothesisType.Visible = False
        Me.lblMargin.Visible = False
        Me.spinBtnMargin_IndProp.Visible = False
        Me.lblMarginHint_IndProp.Visible = False
        AddHandler Me.cbHypothesisType.SelectedIndexChanged, AddressOf Me.HypothesisTypeChanged

        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim TotalN1 As Integer, RespondersN1 As Integer, TotalN2 As Integer, RespondersN2 As Integer
            Dim RespondersBoth As Integer, nres As Integer
            Dim res = New List(Of ResultTable), t = New ResultTable
            Dim alphaValue As Double = Me.spinBtnAlpha.Value '0.05
            Dim ciLabel As String = $"{100.0 * (1.0 - alphaValue):0.##}% CI"

            'get data
            If Me.optSingle.Checked Then
                TotalN1 = Me.spinBtnA.Value
                RespondersN1 = Me.spinBtnB.Value
            ElseIf Me.optIndependent.Checked Then
                TotalN1 = Me.spinBtnA.Value
                RespondersN1 = Me.spinBtnB.Value
                TotalN2 = Me.spinBtnC.Value
                RespondersN2 = Me.spinBtnD.Value
            ElseIf Me.optPaired.Checked Then
                TotalN1 = Me.spinBtnA.Value
                RespondersN1 = Me.spinBtnB.Value
                'for paired proportions we have different notation
                RespondersN2 = Me.spinBtnC.Value
                RespondersBoth = Me.spinBtnD.Value
            End If

            'do the calculations
            If Me.optSingle.Checked Then

                If TotalN1 < RespondersN1 Then 'total have to be less then # of responders
                    MsgBox("Total Number of observations is less then the Number of Responders", vbOKOnly, "Data input")
                    Me.spinBtnA.Select()
                    Exit Sub
                End If
                Dim SingleProp = contingencytable.SingleProportion(RespondersN1, TotalN1, alphaValue)
                If RespondersN1 * 2 > TotalN1 Then
                    nres = TotalN1 - RespondersN1
                Else
                    nres = RespondersN1
                End If
                Dim PtwoTAIL As Double = distributions.BinomDist(nres, TotalN1, 0.5, True) * 2.0
                If PtwoTAIL > 1 Then PtwoTAIL = 1.0

                t.AddHeaderTopRow({"Single proportion", ""})
                t.SetBody({{"Total Number of Subjects", TotalN1},
                            {"Number of Responders", RespondersN1},
                            {"Proportion", SingleProp.Estimate},
                            {ciLabel, SingleProp.strConfidenceInterval(CIformat.LL_to_UL)},
                            {"two-sided P-value", PtwoTAIL}})
                res.Add(t)

            ElseIf Me.optIndependent.Checked Then

                If TotalN1 < RespondersN1 Then
                    MsgBox("Total Number of observations is less then the Number of Responders", vbOKOnly, "Data input")
                    Me.spinBtnA.Select()
                    Exit Sub
                End If
                If TotalN2 < RespondersN2 Then
                    MsgBox("Total Number of observations is less then the Number of Responders", vbOKOnly, "Data input")
                    Me.spinBtnC.Select()
                    Exit Sub
                End If

                Select Case Me.cbHypothesisType.SelectedItem
                        Case "Noninferiority"
                            Dim margin As Double = CDbl(Me.spinBtnMargin_IndProp.Value)
                        Dim ni = equivalencetests.TestIndependentProportionsNonInferiority(
                                        controlResponders:=RespondersN1,
                                        controlTotal:=TotalN1,
                                        experimentalResponders:=RespondersN2,
                                        experimentalTotal:=TotalN2,
                                        nonInferiorityMargin:=margin,
                                        alphaOneSided:=alphaValue)

                        Dim niCiLabel As String = $"{100.0 * (1.0 - 2.0 * alphaValue):0.##}% CI for proportion difference"
                            t.AddHeaderTopRow({"Two independent proportions - Noninferiority", ""})
                        t.SetBody({
                                        {"Control / Reference sample size", ni.NumberOfControls},
                                        {"Control / Reference responders", ni.ControlResponders},
                                        {"Control / Reference proportion", ni.ControlProportion},
                                        {"Experimental / Test sample size", ni.NumberOfExperimental},
                                        {"Experimental / Test responders", ni.ExperimentalResponders},
                                        {"Experimental / Test proportion", ni.ExperimentalProportion},
                                        {"Difference (Experimental - Control)", ni.DifferenceExperimentalMinusControl},
                                        {"Noninferiority margin", ni.NonInferiorityMargin},
                                        {"Noninferiority limit", ni.NonInferiorityLimit},
                                        {niCiLabel, ni.TwoSidedEquivalentConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                                        {"Lower one-sided confidence limit", ni.LowerOneSidedConfidenceLimit},
                                        {"Z statistic", ni.ZStatistic},
                                        {"One-sided P-value", ni.PValue},
                                        {"Conclusion", ni.Conclusion}
                                    })
                        res.Add(t)

                        Case "Equivalence"
                            Dim margin As Double = CDbl(Me.spinBtnMargin_IndProp.Value)
                        Dim eq = equivalencetests.TestIndependentProportionsEquivalence(
                                    controlResponders:=RespondersN1,
                                    controlTotal:=TotalN1,
                                    experimentalResponders:=RespondersN2,
                                    experimentalTotal:=TotalN2,
                                    lowerMargin:=-margin,
                                    upperMargin:=margin,
                                    alphaOneSided:=alphaValue)

                        Dim eqCiLabel As String = $"{100.0 * (1.0 - 2.0 * alphaValue):0.##}% CI for proportion difference"
                            t.AddHeaderTopRow({"Two independent proportions - Equivalence", ""})
                        t.SetBody({
                                        {"Control / Reference sample size", eq.NumberOfControls},
                                        {"Control / Reference responders", eq.ControlResponders},
                                        {"Control / Reference proportion", eq.ControlProportion},
                                        {"Experimental / Test sample size", eq.NumberOfExperimental},
                                        {"Experimental / Test responders", eq.ExperimentalResponders},
                                        {"Experimental / Test proportion", eq.ExperimentalProportion},
                                        {"Difference (Experimental - Control)", eq.DifferenceExperimentalMinusControl},
                                        {"Lower equivalence margin", eq.LowerMargin},
                                        {"Upper equivalence margin", eq.UpperMargin},
                                        {eqCiLabel, eq.EquivalentConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                                        {"Lower TOST Z statistic", eq.LowerComponentStatistic},
                                        {"Lower TOST one-sided P-value", eq.LowerComponentPValue},
                                        {"Upper TOST Z statistic", eq.UpperComponentStatistic},
                                        {"Upper TOST one-sided P-value", eq.UpperComponentPValue},
                                        {"TOST P-value", eq.TostPValue},
                                        {"Conclusion", eq.Conclusion}
                                    })
                        res.Add(t)

                        Case Else
                            Dim TwoIdependent = contingencytable.TwoIndependentProportions(RespondersN1, TotalN1, RespondersN2, TotalN2, alphaValue)

                            ' get counts in format required by fisherexact
                            Dim R1C1 As Integer = RespondersN1
                            Dim R2C1 As Integer = TotalN1 - RespondersN1
                            Dim R1C2 As Integer = RespondersN2
                            Dim R2C2 As Integer = TotalN2 - RespondersN2
                            Dim Fisher = contingencytable.FisherExact2x2(R1C1, R1C2, R2C1, R2C2)

                            t.AddHeaderTopRow({"Two independent proportions", ""})
                        t.SetBody({{"Total Number of Subjects in Sample 1", TotalN1},
                                        {"Number of Responders in Sample 1", RespondersN1},
                                        {"Proportion in Sample 1", RespondersN1 / TotalN1},
                                        {"Total Number of Subjects in Sample 2", TotalN2},
                                        {"Number of Responders in Sample 2", RespondersN2},
                                        {"Proportion in Sample 2", RespondersN2 / TotalN2},
                                        {"Proportions Difference", TwoIdependent.Estimate},
                                        {ciLabel, TwoIdependent.strConfidenceInterval(CIformat.LL_to_UL)},
                                        {"Exact two-sided P-value", Fisher.Pvalue},
                                        {"Exact Mid two-sided P-value", Fisher.Pvalue2}})
                        res.Add(t)
                    End Select

                ElseIf Me.optPaired.Checked Then

                    If Math.Max(Math.Max(RespondersN1, RespondersN2), RespondersBoth) > TotalN1 Or
                   TotalN1 < RespondersN1 + RespondersBoth Or TotalN1 < RespondersN2 + RespondersBoth Then 'total have to be less then # of responders
                    MsgBox("Total Number of observations is less then the Number of Responders", vbOKOnly, "Data input")
                    Me.spinBtnA.Select()
                    Exit Sub
                End If
                Dim PairedProp = contingencytable.PairedProportions(TotalN1, RespondersN1, RespondersN2, RespondersBoth, alphaValue)

                'get counts in format required by liddel
                Dim Total As Integer = TotalN1
                Dim R1C1 As Integer = RespondersN1 - RespondersBoth
                Dim R2C1 As Integer = Total - RespondersN1 - RespondersN2 + RespondersBoth
                Dim R1C2 As Integer = RespondersBoth
                Dim R2C2 As Integer = RespondersN2 - RespondersBoth

                Dim table = {{R1C1, R1C2}, {R2C1, R2C2}}
                Dim Liddell = contingencytable.Liddell_McNemar(table)

                t.AddHeaderTopRow({"Two paired proportions", ""})
                t.SetBody({{"Total Number of Subjects", TotalN1},
                            {"Number of Responders in the 1st Category", RespondersN1},
                            {"Proportion in the 1st Category", (RespondersN1 + RespondersBoth) / TotalN1},
                            {"Total Number of Subjects in the 2nd Category", RespondersN2},
                            {"Number of Responders in the 2nd Category", (RespondersN2 + RespondersBoth) / TotalN1},
                            {"Number of Responders in Both Categories", RespondersBoth},
                            {"Proportions Difference", PairedProp.Estimate},
                            {ciLabel, PairedProp.strConfidenceInterval(CIformat.LL_to_UL)},
                            {"Two-sided P-value", Liddell.Item1.Pvalue}})
                res.Add(t)
            End If

            'Dump outputs
            Dim WriteRes = GetResultWriter() 'pass just table from the main test output
            Dim rr = New ProcessListofResultTables(res)
            Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
            Dim totcols As Integer = rr.TotCols
            If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
                If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                    Exit Sub
                End If
            End If

            rr.writeToSheet(WriteRes, True)
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Function GetResultWriter() As WriteResults
        Dim WriteRes = New WriteResults, rRange As Range
        If Me.optWorkbook.Checked Then
            WriteRes.wb = AppGlobals.app.Workbooks.Add()
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        ElseIf Me.optWorksheet.Checked Then
            WriteRes.wb = AppGlobals.app.ActiveWorkbook
            WriteRes.wb.Worksheets.Add()
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        Else
            WriteRes.wb = AppGlobals.app.ActiveWorkbook
            WriteRes.ws = WorksheetFromRefAdress(Me.RefEditOutput.Address)
            rRange = WriteRes.ws.Range(Me.RefEditOutput.Address)
            WriteRes.setRowPointer(rRange.Row)
            WriteRes.setColumnPointer(rRange.Column)
        End If

        Return WriteRes
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

    ' =============================================================================
    ' HELPERS
    ' =============================================================================
    Private Function ValidateIndependentMargin() As Boolean
        If Me.spinBtnMargin_IndProp Is Nothing Then Return True
        Dim mode As String = Me.cbHypothesisType.SelectedItem
        If mode = "Superiority" Then Return True

        If Me.spinBtnMargin_IndProp.Value <= 0D Then
            MsgBox("The margin must be greater than 0.", vbOKOnly, "Data input")
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
End Class