Imports System.Net
Imports System.Security.Cryptography
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui8Proportions
    Sub New(tagn As Integer)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Tag = tagn
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
                MsgBox(issue.Message, vbOKOnly, "Data input")
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
            CoreServices.Errors.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Function GetResultWriter() As ExcelDnaResultWriter
        Dim WriteRes = New ExcelDnaResultWriter, rRange As Range
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
        If mode <> "Noninferiority" AndAlso mode <> "Equivalence" Then Return True

        Dim hypothesis = If(mode = "Noninferiority", ExcelCommon.IndependentProportionsHypothesis.Noninferiority, ExcelCommon.IndependentProportionsHypothesis.Equivalence)
        Dim report = ExcelCommon.ProportionsInputPolicy.ValidateMargin(hypothesis, CDbl(Me.spinBtnMargin_IndProp.Value))
        If report.HasErrors Then
            MsgBox(report.Issues(0).Message, vbOKOnly, "Data input")
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
