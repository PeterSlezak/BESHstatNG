Imports System.Drawing
Imports System.Security.Cryptography
Imports System.Security.Policy
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui82x2

    Sub New(tagn As Integer)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Tag = tagn
        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit1_WorksheetData.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)
        Me.WireHelp(Me.btnHelp)
    End Sub

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
                    MsgBox(input.Validation.Issues.First(Function(i) i.Severity = ExcelCommon.SpreadsheetValidationSeverity.Error).Message, vbOKOnly, "2x2 table analysis")
                    Exit Sub
                End If
                report = ExcelCommon.ContingencyReportPolicy.TwoByTwo(input.Data, options)
            ElseIf Me.optScreenData.Checked Then
                report = WindowsContingencyInputAdapter.AnalyzeDialog(CDbl(Me.spinBtnA.Value), CDbl(Me.spinBtnB.Value),
                    CDbl(Me.spinBtnC.Value), CDbl(Me.spinBtnD.Value), options)
            Else
                MsgBox("Select a count input source.", vbOKOnly, "2x2 table analysis")
                Exit Sub
            End If
            For Each issue In report.Validation.Issues
                If issue.Severity = ExcelCommon.SpreadsheetValidationSeverity.Warning Then
                    MsgBox(issue.Message, vbOKOnly, "Fisher's exact test")
                End If
            Next
            If Not report.Success Then
                MsgBox(report.Validation.Issues.First(Function(i) i.Severity = ExcelCommon.SpreadsheetValidationSeverity.Error).Message, vbOKOnly, "2x2 table analysis")
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

    Private Function getDataMultipleGroups() As DataObj
        Dim imported As New DataObj
        Dim reference As String = prepareRef2D(Me.RefEdit1_WorksheetData.Address)
        ExcelDnaDataImporter.ImportInto(imported, reference, True)
        Return imported
    End Function

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