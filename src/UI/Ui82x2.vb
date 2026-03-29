Imports System.Drawing
Imports System.Security.Cryptography
Imports System.Security.Policy
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui82x2

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit1_WorksheetData.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim R1C1 As Integer, R1C2 As Integer, R2C1 As Integer, R2C2 As Integer, bSelectedOption As Boolean = False
            Dim table(1, 1) As Integer, res = New List(Of ResultTable), t = New ResultTable
            Dim alphaValue As Double = CDbl(Me.spinBtnAlpha.Value)

            'get data
            If Me.optWorksheetData.Checked Then
                Dim d = Me.getDataMultipleGroups()
                If d.X.GetLength(0) <> 2 Or d.X.GetLength(1) <> 2 Then
                    MsgBox("Wrong dimensions of input table, 2x2 is expected.", vbOKOnly, "2x2 table analysis")
                    Exit Sub
                End If
                table = Matrix.Array2intArray(d.X)
                R1C1 = table(0, 0)
                R1C2 = table(0, 1)
                R2C1 = table(1, 0)
                R2C2 = table(1, 1)
            ElseIf Me.optScreenData.Checked Then
                R1C1 = Me.spinBtnA.Value
                R1C2 = Me.spinBtnB.Value
                R2C1 = Me.spinBtnC.Value
                R2C2 = Me.spinBtnD.Value
                table = {{R1C1, R1C2}, {R2C1, R2C2}}
            End If
            t.SetBody(table)
            t.AddHeaderTopRow({"Analyzed 2x2 table", ""})
            res.Add(t)

            'do the analysis
            If Me.ckFisher.Checked Then
                If R1C1 + R1C2 + R2C1 + R2C2 > 1000 Then
                    MsgBox("Too large sample size for exact computation.", vbOKOnly, "Fisher's exact test")
                Else
                    Dim Fisher = contingencytable.FisherExact2x2(R1C1, R1C2, R2C1, R2C2)
                    t = New ResultTable
                    t.AddHeaderTopRow({"Fisher's Exact Test", ""})
                    t.SetBody({{"one-sided P-value", Fisher.PvalueLowerSide},
                               {"two-sided P-value", Fisher.Pvalue},
                               {"one-sided mid P-value", Fisher.pValueExactLowerSide},
                               {"two-sided mid P-value", Fisher.Pvalue2}})
                    res.Add(t)
                End If
            End If

            If Me.ckChi2.Checked Or Me.ckAssociation.Checked Then
                Dim chi2indep = contingencytable.Chi2TESTindependence(table)
                t = New ResultTable
                t.AddHeaderTopRow({"Pearson's Chi-squared Test", ""})
                t.SetBody({{"Chi-Square", chi2indep.Item1.TestStatistics1}, {"two-sided P-value", chi2indep.Item1.Pvalue}})
                res.Add(t)

                t = New ResultTable
                t.AddHeaderTopRow({"Measures of Nominal Association", ""})
                t.SetBody({{"Cramer's V", chi2indep.Item2},
                           {"Pearson's contingency coefficient", chi2indep.Item3},
                           {"Phi", chi2indep.Item4}})
                res.Add(t)
            End If
            If Me.ckLiddel.Checked Then
                Dim Liddel = contingencytable.Liddell_McNemar(table, alphaValue)
                t = New ResultTable
                t.AddHeaderTopRow({"Liddell's Test", ""})
                t.SetBody({{"P-value", Liddel.Item1.Pvalue},
                            {"Risk ratio", Liddel.Item2.Estimate},
                            {Liddel.Item2.CIlabel, Liddel.Item2.strConfidenceInterval(CIformat.LL_to_UL)}})
                res.Add(t)
            End If
            If Me.ckOR.Checked Then
                Dim Odds = contingencytable.OddsRatio(table, alphaValue)
                t = New ResultTable
                t.AddHeaderTopRow({"Odds Ratio", ""})
                t.SetBody({{"OR", Odds.Item1.Estimate},
                          {Odds.Item1.CIlabel & " (Woolf)", Odds.Item1.strConfidenceInterval(CIformat.LL_to_UL)},
                          {Odds.Item2.CIlabel & " (Cornfield)", Odds.Item2.strConfidenceInterval(CIformat.LL_to_UL)}})
                res.Add(t)
            End If
            If Me.ckRR.Checked Then
                Dim Risk = contingencytable.RiskRatio(table, alphaValue)
                t = New ResultTable
                t.AddHeaderTopRow({"Risk Ratio", ""})
                t.SetBody({{"RR", Risk.Estimate},
                            {Risk.CIlabel, Risk.strConfidenceInterval(CIformat.LL_to_UL)}})
                res.Add(t)
            End If

            'chcek wheter at least one option was selected
            If res.Count <= 1 Then
                MsgBox("Select statistic(s) to compute.", vbOKOnly, "2x2 table analysis")
                Exit Sub
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

    Private Function getDataMultipleGroups() As MultiGroupsPairedData
        Dim out = New MultiGroupsPairedData
        Dim columData = New DataObj
        Dim ref As String = prepareRef2D(Me.RefEdit1_WorksheetData.Address)

        columData.DataInport(ref, True)
        out.X = columData.DataDbl()
        out.varNames = columData.varNames

        Return out
    End Function

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