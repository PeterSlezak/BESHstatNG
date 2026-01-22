Imports Microsoft.Office.Interop.Excel

Public Class Ui11PCA

    Private pWorksheet As Worksheet
    Private pWorkbook As Workbook
    Private VariableColumnsInfo As Dictionary(Of Integer, Object()) 'information of variable/column names inported into the input listbox

    Sub New(analysis As String, ws As Worksheet)
        ' This call is required by the designer.
        InitializeComponent()
        pWorksheet = ws
        pWorkbook = ws.Parent

        ' Add any initialization after the InitializeComponent() call.
        Me.Text = analysis

        If Me.Text = "Scatter Plot Matrix" Then
            Me.TabPageOptionsPCA.Parent = Nothing
        ElseIf Me.Text = "Principal Component Analysis" Then
            Me.TabPageOptionsSPM.Parent = Nothing
        ElseIf Me.Text = "Multiple Correspondence Analysis" Then
            Me.ckFirstRow.Visible = True
            Me.TabPageOptionsPCA.Parent = Nothing
            Me.TabPageOptionsSPM.Parent = Nothing
        End If

        Me.WireHelp(Me.btnHelp)
        Me.Populate()
    End Sub

    Private Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        Try
            Dim MyData As DataObj
            'activate workbook we are working on (different may  be open if we re-running the analysis)
            Me.pWorkbook.Activate()

            MyData = GetData()
            If MyData.bZeroValid Then 'check for zero valid data
                MsgBox("No valid observations")
                Exit Sub
            End If

            If Me.Text = "Scatter Plot Matrix" Then
                Me.RunSPM(MyData)
            ElseIf Me.Text = "Principal Component Analysis" Then
                Me.RunPCA(MyData)
            ElseIf Me.Text = "Multiple Correspondence Analysis" Then
                Me.RunMCA(MyData)
            End If
        Catch ex As Exception
            BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunMCA(MyData As DataObj)
        Dim mca As New Multivariate.CA

        If Me.ckFirstRow.Checked Then 'remove first data row
            Dim ids As New Dictionary(Of Integer, Integer)
            For i = 1 To MyData.nRows - 1
                ids.Add(i, MyData.RowIds(i))
            Next
            MyData.SubsetByRowIdValues(ids)
        End If

        mca.DataMultiple(Array2strArray(MyData.FinalData), MyData.varNames)
        mca.Calculate()

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = app.Workbooks.Add()
        app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(3) 'second row is blank because of Design matrix having two header rows
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
        WriteRes.write(MyData.varNames)
        WriteRes.setRowPointer(3) 'second row is blank
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        WriteRes.write({"Design Matrix:"})
        WriteRes.setRowPointer()
        WriteRes.shiftColumnPointer(1)
        WriteRes.write(mca.BurtVarNames)
        WriteRes.write(mca.rowNames)
        WriteRes.write(mca.DesignMatrix)

        'MCA numerical results
        Dim res = mca.wrapResults()
        WriteRes = New WriteResults
        app.ActiveWorkbook.Worksheets.Add()
        app.ActiveWorkbook.ActiveSheet.name = "MCA results"
        WriteRes.ws = app.ActiveWorkbook.ActiveSheet
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

        'Add figures
        mca.plot()
        mca.contribPlot(0, False)
        mca.contribPlot(1, False)
    End Sub

    Private Sub RunPCA(MyData As DataObj)
        Dim strExtractMethod As String = String.Empty, Extractcoef As Double = 0.0, strExtractMethodLong As String = String.Empty
        Dim strMatrix As String = If(Me.optCorr.Checked, "Correlation", "Covariance")

        If Me.optExtractEigen.Checked Then
            strExtractMethod = "Eigenvalue"
            Extractcoef = CDbl(Me.spinBtnExtractEigen.Value)
            strExtractMethodLong = "Eigenvalue > " & CStr(Me.spinBtnExtractEigen.Value)

        ElseIf Me.optExtractFixed.checked Then
            strExtractMethod = "Fixed"
            Extractcoef = CDbl(Me.spinBtnExtractComp.Value)
            strExtractMethodLong = "Fixed Number of Components = " & CStr(Me.spinBtnExtractComp.Value)

        ElseIf Me.optExtractVariance.checked Then
            strExtractMethod = "Variance"
            Extractcoef = CDbl(Me.spinBtnExtractVariance.Value)
            strExtractMethodLong = "Explained Variance > " & CStr(Me.spinBtnExtractVariance.Value)

        End If

        Dim objPCA As New Multivariate.PCA
        With objPCA
            .dataInputs(MyData.DataDbl, MyData.RowIds, MyData.varNames, strExtractMethodLong)
            .settingsInputs(CInt(Me.tbMaxIter.Text), CDbl(Me.tbEps.Text), strMatrix)
            .Calculate(strExtractMethod, Extractcoef)
        End With

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = app.Workbooks.Add()
        app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(2)
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
        WriteRes.write(MyData.varNames)
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        'Standardized values
        WriteRes.write(objPCA.StandardizedVarNames(MyData.varNames))
        WriteRes.write(objPCA.StandardizedData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        'Final Reduced Dataset
        WriteRes.write(objPCA.StandardizedVarNames(objPCA.PCnames("Reduced_Data_PC")))
        WriteRes.write(objPCA.ReducedDataset)

        'PCA numerical results
        Dim res = objPCA.wrapResults()
        WriteRes = New WriteResults
        app.ActiveWorkbook.Worksheets.Add()
        app.ActiveWorkbook.ActiveSheet.name = "PCA results"
        WriteRes.ws = app.ActiveWorkbook.ActiveSheet
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

        'Figures
        objPCA.screePlot()
        objPCA.scorePlot2D()
        objPCA.loadingPlot2D()
        objPCA.biplot(0.0)
        objPCA.biplot(0.5)
        objPCA.biplot(1.0)
        objPCA.scorePlot3D()
        objPCA.loadingPlot3D()

    End Sub

    Private Sub RunSPM(MyData As DataObj)
        Dim spm = New graphics.ScatterPlotMatrix(MyData.DataDbl, MyData.varNames, pWorkbook)
        spm.settingInputs(Me.ckDisplayCorrelCoef.Checked, Me.ckShowRegressionLines.Checked)
        spm.compute()
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        AddItemsToListbox(Me.lbXs, Me.lbAllColumns)
    End Sub
    Private Sub btRemoveX_Click(sender As Object, e As System.EventArgs) Handles btRemoveX.Click
        Remove_Item(Me.lbXs, "selected")
    End Sub

    Private Sub btReload_Click(sender As Object, e As System.EventArgs) Handles btReload.Click
        Dim newSheet As Object
        Me.lbAllColumns.Items.Clear()

        If Me.cbSheetsList.SelectedIndex <> -1 Then
            If pWorksheet.name <> Me.cbSheetsList.SelectedItem.ToString() Then 'new sheet selected clear all listboxes
                Me.lbXs.Items.Clear()
            End If
            newSheet = pWorkbook.worksheets(Me.cbSheetsList.SelectedItem.ToString())
            Me.Populate(newSheet)
        Else
            Me.Populate(pWorksheet)
        End If
    End Sub

    Private Function GetData() As DataObj
        Dim ref As String = String.Empty
        Dim MyData As DataObj = New DataObj

        'X vars
        For i = 0 To lbXs.Items.Count - 1
            If i = 0 Then
                ref = "'" & pWorksheet.Name & "'!" & CreateReference(pWorksheet, Me.lbXs.Items(i), Me.VariableColumnsInfo)
            Else
                ref &= ", " & CreateReference(pWorksheet, Me.lbXs.Items(i), Me.VariableColumnsInfo)
            End If
        Next i

        'Prepare Data from references
        If Me.Text = "Multiple Correspondence Analysis" Then 'accept Text data
            MyData.DataInport(ref, False, 20000) 'just some large number as all data can be character
        Else
            MyData.DataInport(ref)
        End If

        Return MyData
    End Function

    Sub Populate(Optional ws As Object = Nothing)
        Dim VarRng As Object, ws_temp As Object
        If ws IsNot Nothing Then
            pWorksheet = ws
            pWorkbook = ws.parent
        End If

        Dim FinalCol = LastColumnInSheet(pWorksheet)
        Dim MaxRows = MaxRowsInSheet(pWorksheet)
        VarRng = pWorksheet.Range(pWorksheet.Cells(1, 1), pWorksheet.Cells(1, FinalCol)) 'Create range object to contain variable names
        If Me.Text = "Multiple Correspondence Analysis" Then
            'we accept character columns
            Me.VariableColumnsInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns, False) 'Cycle through the range and add the variable names to the listbox
        Else
            Me.VariableColumnsInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns)
        End If

        'We may call this method multiple times so populate sheet combo box only once
        Me.cbSheetsList.Items.Clear()
        For Each ws_temp In pWorkbook.Worksheets
            Me.cbSheetsList.Items.Add(ws_temp.name)
        Next
        Me.cbSheetsList.SelectedIndex = Me.cbSheetsList.FindStringExact(Me.pWorkbook.ActiveSheet.name)
    End Sub
End Class