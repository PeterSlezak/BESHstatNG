Imports System.Security.Cryptography
Imports Microsoft.Office.Interop.Excel

Public Class Ui9ANOVA2nested
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit1_Group.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit2_Nested.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit3_Data.ExcelConnector = BESHstatGlobals.app
        Me.RefEditOutput.ExcelConnector = BESHstatGlobals.app
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If CheckRefEdit(Me.RefEdit1_Group.Address, True) Then
            RefEditReset(Me.RefEdit1_Group)
            bOut = True
        End If

        If CheckRefEdit(Me.RefEdit2_Nested.Address, True) Then
            RefEditReset(Me.RefEdit2_Nested)
            bOut = True
        End If

        If CheckRefEdit(Me.RefEdit3_Data.Address) Then
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

    Private Function getData(ByRef strErr As String) As MultiGroupsPairedDataObj
        Dim out = New MultiGroupsPairedDataObj
        Dim byIdData = New DataObj
        Dim refGrp As String, refNest As String, refFinal As String, refData As String

        Dim wks As String = WorksheetNameFromRefAdress(Me.RefEdit1_Group.Address, True)
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit2_Nested.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit3_Data.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If

        refGrp = prepareRef2D(Me.RefEdit1_Group.Address)
        refNest = prepareRef2D(Me.RefEdit2_Nested.Address)
        refData = prepareRef2D(Me.RefEdit3_Data.Address)

        refFinal = refGrp & ", " &
               Replace(refNest, wks & "!", String.Empty) & ", " &
               Replace(refData, wks & "!", String.Empty)  'Remove "Sheet1!" from string

        byIdData.DataInport(refFinal, True, 1)

        If byIdData.varNames.Length = 0 Then
            strErr = "Zero valid data!"
            BSlogg.Log("Zero valid data!", LogMsgType.Warn)
            Return Nothing
        End If

        out.X = byIdData.FinalData
        out.varNames = byIdData.varNames

        Return out
    End Function

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim errText As String = String.Empty

            'Validate Inputs
            If Me.checkInputs() Then Exit Sub

            'Get Data
            Dim Data As MultiGroupsPairedDataObj = Me.getData(errText)
            If errText <> String.Empty Then
                MsgBox(errText, vbExclamation)
                Exit Sub
            End If

            Me.Run2WayNested(Data)
        Catch ex As Exception
            BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub Run2WayNested(d As MultiGroupsPairedDataObj)
        Dim WriteRes = New WriteResults
        Dim nest = New parametric.TwoWayNestedANOVA(d.X, d.varNames)
        nest.compute()
        Dim res = nest.wrapResults()
        Dim rr = New ProcessListofResultTables(res)

        'Dump outputs
        WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim totrows As Integer = rr.TotRows + res.Count + res.Count - 1
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        WriteRes.write({{"Two-Way Nested ANOVA"}, {$"Balanced design = {nest.balancedDesign}"}})
        Dim sep As New List(Of Object(,))
        sep.Add({{"", ""}, {"Satterthwaite approximation", ""}})
        rr.SetSeparators(sep)
        rr.writeToSheet(WriteRes)

    End Sub

    Private Function GetResultWriter() As WriteResults
        Dim WriteRes = New WriteResults, rRange As Range
        If Me.optWorkbook.Checked Then
            WriteRes.wb = app.Workbooks.Add()
            WriteRes.ws = app.ActiveWorkbook.ActiveSheet
        ElseIf Me.optWorksheet.Checked Then
            WriteRes.wb = app.ActiveWorkbook
            WriteRes.wb.Worksheets.Add()
            WriteRes.ws = app.ActiveWorkbook.ActiveSheet
        Else
            WriteRes.wb = app.ActiveWorkbook
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