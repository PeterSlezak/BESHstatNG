Imports System.Security.Cryptography
Imports Microsoft.Office.Interop.Excel

Public Class Ui4KMandLogRank

    Sub New(analysis As String)
        ' This call is required by the designer.
        InitializeComponent()

        Me.RefEdit1_SurvivalTime.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit2_Censor.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit3_GroupID.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit4_StrataID.ExcelConnector = BESHstatGlobals.app
        Me.RefEditOutput.ExcelConnector = BESHstatGlobals.app
        Me.Text = analysis

        With Me.cbXunits.Items
            .Add("days")
            .Add("weeks")
            .Add("months")
            .Add("years")
        End With
        Me.cbXunits.SelectedIndex = 0

        If Me.Text = "Kaplan-Meier Plot" Then
            Me.lblGroup.Text = "Group ID (Optional)"
            Me.lblStrata.Visible = False
            Me.RefEdit4_StrataID.Visible = False
        ElseIf Me.Text = "Logrank Test" Then
            Me.lblGroup.Text = "Group ID"
        End If
        Me.RefEdit1_SurvivalTime.txtAddress.Select()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim errText As String = String.Empty, data As DataObj

            'Validate Inputs
            If Me.checkInputs() Then Exit Sub

            'Get Data
            data = Me.getData(errText)
            If errText <> String.Empty Then
                MsgBox(errText, vbExclamation)
                Exit Sub
            End If

            If Me.Text = "Kaplan-Meier Plot" Then
                Me.RunKM(data)
            ElseIf Me.Text = "Logrank Test" Then
                Me.RunLogrank(data)
            End If
        Catch ex As Exception
            BESHstatGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunLogrank(data As DataObj)
        'We run the same analyses as in KM dialog but when starting with Logrank we allow for Stratified input
        'On the other hand when calling with KM we allow to not specify group ID. However, in either case
        'same set of analysis are applicable.
        Me.RunKM(data)
    End Sub

    Private Sub RunKM(data As DataObj)
        Dim strErr As String = String.Empty, strMethod As String = String.Empty
        Dim grpData() As String, strataData() As String
        Dim d = data.FinalData
        Dim WriteRes = New WriteResults
        Dim n As Integer = UBound(d, 1) + 1
        Dim colId As Integer = 0

        'Create data for GROUP ID if missing
        If Me.RefEdit3_GroupID.Address = String.Empty Then 'We have only One group
            ReDim grpData(n - 1)
            For i = 0 To n - 1
                grpData(i) = "0"
            Next
        Else
            grpData = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(d, 0))
            colId += 1
        End If
        Dim NoGroups As Integer = grpData.Distinct().Count()

        'Create data for STRATA ID
        If Me.RefEdit4_StrataID.Address = String.Empty Then 'We have only One group
            ReDim strataData(n - 1)
            For i = 0 To n - 1
                strataData(i) = "0"
            Next
        Else
            strataData = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(d, colId))
            colId += 1
        End If

        Dim SurvD = survival.CreatSurvivalData(Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d, colId)),
                                      Matrix.Array2intArray(Matrix.GetColumnFrom2Darray(d, colId + 1)),
                                      grpData, strataData, strErr)

        If strErr <> String.Empty Then
            MsgBox(strErr, vbExclamation)
            Exit Sub
        End If

        'compute Logrank test and KM related outputs
        Dim LRKM As New survival.Survival_KM_LR(SurvD)

        If Me.optLogRank.Checked Then
            strMethod = "Logrank"
        ElseIf Me.optTaroneWare.Checked Then
            strMethod = "Tarone-Ware"
        ElseIf Me.optGehanBreslow.Checked Then
            strMethod = "Gehan-Breslow"
        ElseIf Me.optPeto.Checked Then
            strMethod = "Peto"
        ElseIf Me.optModPeto.Checked Then
            strMethod = "modified Peto"
        End If

        If NoGroups > 1 Then LRKM.WeightedLogRankTest(strMethod)
        LRKM.BrookmeyerCrowleyMedianSurvivalCI() 'Median survival time and CI by group
        If Me.ckCIoutput.Checked Then LRKM.SurvivalCurveTabularOutput()
        If Me.ckBCtest.Checked And NoGroups > 1 Then LRKM.EqualityOfMedianTest()
        If NoGroups = 2 Then
            If Me.ckCSCatFTP.Checked Then LRKM.CompareCurveFixTimePoint()
        End If

        'wrap all computed results
        Dim res = LRKM.wrapResults()

        'Dump outputs
        WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        Dim strTitle = If(Me.ckDisplayTitle.Checked, Me.tbTitleText.Text, String.Empty)
        LRKM.AddKMplot(WriteRes.ws, Me.ckPlotCI.Checked, Me.ckShowLegend.Checked, strTitle, Me.cbXunits.Text)
    End Sub

    Private Function getData(ByRef strErr As String) As DataObj
        Dim byIdData = New DataObj
        Dim refTime As String, refCen As String, refFinal As String, refGroup As String = String.Empty, refStrata As String = String.Empty
        Dim CharCols As Integer = 0

        Dim wks As String = WorksheetNameFromRefAdress(Me.RefEdit1_SurvivalTime.Address, True)
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit2_Censor.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If

        refTime = prepareRef2D(Me.RefEdit1_SurvivalTime.Address)
        refCen = prepareRef2D(Me.RefEdit2_Censor.Address)

        If Me.RefEdit3_GroupID.Address <> String.Empty Then
            If wks <> WorksheetNameFromRefAdress(Me.RefEdit3_GroupID.Address, True) Then
                strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            End If
            CharCols += 1
            refGroup = prepareRef2D(Me.RefEdit3_GroupID.Address) 'reuse refData
        End If

        If Me.RefEdit4_StrataID.Address <> String.Empty Then
            If wks <> WorksheetNameFromRefAdress(Me.RefEdit4_StrataID.Address, True) Then
                strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            End If
            CharCols += 1
            refStrata = prepareRef2D(Me.RefEdit4_StrataID.Address)
        End If

        refFinal = refGroup & ", " & Replace(refStrata, wks & "!", String.Empty)
        If Trim$(refFinal) = "," Then 'no group, no strata. We need to make sure the refFinal starts with the sheet name
            refFinal = refTime & ", " &
                       Replace(refCen, wks & "!", String.Empty) 'Remove "Sheet1!" from string
        Else
            refFinal = refFinal & ", " & Replace(refTime, wks & "!", String.Empty) & ", " &
                       Replace(refCen, wks & "!", String.Empty)  'Remove "Sheet1!" from string
        End If

        byIdData.DataInport(refFinal, True, CharCols)

        Return byIdData

    End Function

    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If CheckRefEdit(Me.RefEdit1_SurvivalTime.Address, True) Then
            Me.TabMultipage.SelectedIndex = 0
            RefEditReset(Me.RefEdit1_SurvivalTime)
            bOut = True
        End If

        If CheckRefEdit(Me.RefEdit2_Censor.Address, True) Then
            Me.TabMultipage.SelectedIndex = 0
            RefEditReset(Me.RefEdit2_Censor)
            bOut = True
        End If

        'It's optional for KM plot
        If (Me.Text = "Kaplan-Meier Plot" And Me.RefEdit3_GroupID.Address <> String.Empty) Or
            Me.Text = "Logrank Test" Then
            If CheckRefEdit(Me.RefEdit3_GroupID.Address, True) Then
                Me.TabMultipage.SelectedIndex = 0
                RefEditReset(Me.RefEdit3_GroupID)
                bOut = True
            End If
        End If

        'It's optional for Logrank and not applicable for KM
        If Me.Text = "Logrank Test" And Me.RefEdit4_StrataID.Address <> String.Empty Then
            If CheckRefEdit(Me.RefEdit4_StrataID.Address, True) Then
                Me.TabMultipage.SelectedIndex = 0
                RefEditReset(Me.RefEdit4_StrataID)
                bOut = True
            End If
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

    Private Function GetResultWriter() As WriteResults
        Dim WriteRes = New WriteResults, rRange As Range
        If Me.optWorkbook.Checked Then
            WriteRes.wb = BESHstatGlobals.app.Workbooks.Add()
            WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet
        ElseIf Me.optWorksheet.Checked Then
            WriteRes.wb = BESHstatGlobals.app.ActiveWorkbook
            WriteRes.wb.Worksheets.Add()
            WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet
        Else
            WriteRes.wb = BESHstatGlobals.app.ActiveWorkbook
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
End Class