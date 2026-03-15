Imports System.Security.Cryptography
Imports BESHStatNG.Agreement.Agreement
Imports Microsoft.Office.Interop.Excel

Public Class Ui9ANOVA2nested
    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Text = analysis

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit1_Group.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit2_Nested.ExcelConnector = BESHstatGlobals.app
        Me.RefEdit3_Data.ExcelConnector = BESHstatGlobals.app
        Me.RefEditOutput.ExcelConnector = BESHstatGlobals.app

        If Me.Text = "Passing-Bablok Regression" Then
            Me.lblRefedit1_Group.Text = "Group (optional)"
            Me.lblRefedit2_Nested.Text = "Reference method (X)"
            Me.lblRefedit3_Data.Text = "Test method (Y)"
        End If

        Me.RefEdit1_Group.txtAddress.Select()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If Me.Text = "Two-Way Nested ANOVA" Then
            If CheckRefEdit(Me.RefEdit1_Group.Address, True) Then
                RefEditReset(Me.RefEdit1_Group)
                bOut = True
            End If
        ElseIf Me.Text = "Passing-Bablok Regression" And Me.RefEdit1_Group.Address <> String.empty Then
            If CheckRefEdit(Me.RefEdit1_Group.Address, True) Then
                RefEditReset(Me.RefEdit1_Group)
                bOut = True
            End If
        End If

        If CheckRefEdit(Me.RefEdit2_Nested.Address, True) Then
            RefEditReset(Me.RefEdit2_Nested)
            bOut = True
        End If

        Dim bOneColumn As Boolean = True
        If Me.Text = "Two-Way Nested ANOVA" Then
            bOneColumn = False
        ElseIf Me.Text = "Passing-Bablok Regression" Then
            bOneColumn = True
        End If

        If CheckRefEdit(Me.RefEdit3_Data.Address, bOneColumn) Then
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
            BESHstatGlobals.BSlogg.Log("Zero valid data!", BESHstatGlobals.LogMsgType.Warn)
            Return Nothing
        End If

        out.X = byIdData.FinalData
        out.varNames = byIdData.varNames

        Return out
    End Function

    Private Function getDataPB(ByRef strErr As String) As MultiGroupsPairedDataObj
        'get data for Passing-Bablok regression

        Dim out = New MultiGroupsPairedDataObj
        Dim byIdData = New DataObj
        Dim refGrp As String, refX As String, refFinal As String, refY As String

        Dim wks As String = WorksheetNameFromRefAdress(Me.RefEdit2_Nested.Address, True)
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit3_Data.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If

        refX = prepareRef2D(Me.RefEdit2_Nested.Address)
        refY = prepareRef2D(Me.RefEdit3_Data.Address)

        If Me.RefEdit1_Group.Address <> String.Empty Then 'optional
            If wks <> WorksheetNameFromRefAdress(Me.RefEdit1_Group.Address, True) Then
                strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            End If

            refGrp = prepareRef2D(Me.RefEdit1_Group.Address)

            refFinal = refGrp & ", " &
                       Replace(refX, wks & "!", String.Empty) & ", " &
                       Replace(refY, wks & "!", String.Empty)  'Remove "Sheet1!" from string
            byIdData.DataInport(refFinal, True, 1) 'first column can be character
        Else
            refFinal = refX & ", " &
                       Replace(refY, wks & "!", String.Empty)
            byIdData.DataInport(refFinal, True)
        End If


        If byIdData.varNames.Length = 0 Then
            strErr = "Zero valid data!"
            BESHstatGlobals.BSlogg.Log("Zero valid data!", BESHstatGlobals.LogMsgType.Warn)
            Return Nothing
        End If

        out.X = byIdData.FinalData
        out.varNames = byIdData.varNames

        Return out
    End Function

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim errText As String = String.Empty
            Dim Data As MultiGroupsPairedDataObj = Nothing

            'Validate Inputs
            If Me.checkInputs() Then Exit Sub

            'Get Data
            If Me.Text = "Two-Way Nested ANOVA" Then
                Data = Me.getData(errText)
            ElseIf Me.Text = "Passing-Bablok Regression" Then
                Data = Me.getDataPB(errText)
            End If

            If errText <> String.Empty Then
                MsgBox(errText, vbExclamation)
                Exit Sub
            End If

            If Me.Text = "Two-Way Nested ANOVA" Then
                Me.Run2WayNested(Data)
            ElseIf Me.Text = "Passing-Bablok Regression" Then
                Me.RunPassingBablok(Data)
            End If

        Catch ex As Exception
            BESHstatGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunPassingBablok(d As MultiGroupsPairedDataObj)
        Dim WriteRes = New WriteResults
        Dim bGrouped As Boolean = False
        Dim x() As Double = Nothing, y() As Double = Nothing, grp() As Object = Nothing
        Dim pb As PassinbBablok = Nothing

        If d.X.GetLength(1) = 2 Then
            bGrouped = False
        Else
            bGrouped = True
        End If

        If bGrouped Then
            grp = Matrix.GetColumnFrom2Darray(d.X, 0)
            x = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 1))
            y = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 2))
            pb = New PassinbBablok(x, y, d.varNames(1), d.varNames(2), grp, d.varNames(0))
            pb.GroupeDBlockPassingBablok()
        Else
            x = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 0))
            y = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 1))
            pb = New PassinbBablok(x, y, d.varNames(0), d.varNames(1))
            pb.PassingBablokCI()
        End If

        Dim res = pb.wrapResults()
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

        rr.writeToSheet(WriteRes, True)
        pb.AddPlot(WriteRes.ws)
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