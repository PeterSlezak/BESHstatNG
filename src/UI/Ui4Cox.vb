Imports System.Drawing

Public Class Ui4Cox
    Private pWorksheet As Object
    Private pWorkbook As Object
    Private VariableColumnsInfo As Dictionary(Of Integer, Object()) 'information of variable/column names inported into the input listbox

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.TabControl1.Anchor = Windows.Forms.AnchorStyles.Left Or
                                Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right Or
                                Windows.Forms.AnchorStyles.Top
        Me.btCalculate.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right
        Me.btnHelp.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right
        Me.ProgressBar1.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right Or
                                Windows.Forms.AnchorStyles.Left
        Me.lblProgress.Anchor = Windows.Forms.AnchorStyles.Left Or
                                Windows.Forms.AnchorStyles.Bottom

        Me.lbAllColumns.Anchor = Windows.Forms.AnchorStyles.Left Or
                                 Windows.Forms.AnchorStyles.Bottom Or
                                 Windows.Forms.AnchorStyles.Top

        Me.lbTime.Anchor = Windows.Forms.AnchorStyles.Left Or
                        Windows.Forms.AnchorStyles.Right Or
                        Windows.Forms.AnchorStyles.Top
        Me.lbCensoring.Anchor = Windows.Forms.AnchorStyles.Left Or
                             Windows.Forms.AnchorStyles.Right Or
                             Windows.Forms.AnchorStyles.Top
        Me.lbStrata.Anchor = Windows.Forms.AnchorStyles.Left Or
                              Windows.Forms.AnchorStyles.Right Or
                              Windows.Forms.AnchorStyles.Top
        Me.lbXs.Anchor = Windows.Forms.AnchorStyles.Left Or
                         Windows.Forms.AnchorStyles.Right Or
                         Windows.Forms.AnchorStyles.Top Or
                         Windows.Forms.AnchorStyles.Bottom

        Me.lblNote.Anchor = Windows.Forms.AnchorStyles.Right Or
                            Windows.Forms.AnchorStyles.Bottom
        Me.cbSheetsList.Anchor = Windows.Forms.AnchorStyles.Top Or
                                 Windows.Forms.AnchorStyles.Right
        Me.btReload.Anchor = Windows.Forms.AnchorStyles.Top Or
                             Windows.Forms.AnchorStyles.Right

        Me.lbSelectedVariables.Anchor = Windows.Forms.AnchorStyles.Left Or
                                        Windows.Forms.AnchorStyles.Bottom Or
                                        Windows.Forms.AnchorStyles.Top
        Me.tbInitValues.Anchor = Windows.Forms.AnchorStyles.Left Or
                                 Windows.Forms.AnchorStyles.Bottom Or
                                 Windows.Forms.AnchorStyles.Top
        Me.lbSelectedEffectsList.Anchor = Windows.Forms.AnchorStyles.Left Or
                                          Windows.Forms.AnchorStyles.Right Or
                                          Windows.Forms.AnchorStyles.Top Or
                                          Windows.Forms.AnchorStyles.Bottom
        Me.tbRemoveSelectedEffects.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                            Windows.Forms.AnchorStyles.Right
        Me.btClearAllSelectedEffects.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                            Windows.Forms.AnchorStyles.Right

        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        Try
            Dim bWait As Boolean = False
            Dim strWarning As String = String.Empty

            'activate workbook we are working on (different may  be open if we re-running the analysis)
            Me.pWorkbook.activate

            validateInputs(bWait, strWarning)
            If bWait Then
                If strWarning <> String.Empty Then MsgBox(strWarning)
                Exit Sub
            End If

            Dim MyData = GetData()
            If MyData.bZeroValid Then 'check for zero valid data
                MsgBox("No valid observations")
                Exit Sub
            End If

            'do the calculation
            Dim cox = New CoxPH(MyData.SurvRecordsList, MyData.varNames, Int(Me.spinBtnMaxIter.Value), CDbl(Me.tbEps.Text))
            cox.bRobustVariance = Me.ckRobustVariance.Checked
            cox.bReturnCov = Me.ckCovarMatrix.Checked
            cox.bComputeAllResiduals = Me.ckAllResiduals.Checked
            cox.bComputePHScoreTest = Me.ckPHtest.Checked
            cox.bIterationDetails = Me.ckIterationsDetails.Checked
            cox.bTrace = Me.ckTrace.Checked
            Dim m As TieMethod
            If Me.optBreslow.Checked Then
                m = TieMethod.Breslow
            ElseIf Me.optEfron.Checked Then
                m = TieMethod.Efron
            ElseIf Me.optExact.Checked Then
                m = TieMethod.Exact
            End If
            Dim resu = cox.Fit(m, Me.ProgressBar1, Me.lblProgress)
            Dim res = cox.wrapResults(If(MyData.bStrata, MyData.StrataVarName, Nothing))

            'Dump results
            Dim WriteRes = New WriteResults
            WriteRes.wb = app.Workbooks.Add()
            app.ActiveWorkbook.ActiveSheet.name = "Cox Regression"
            WriteRes.ws = app.ActiveWorkbook.ActiveSheet

            Dim rr = New ProcessListofResultTables(res)
            rr.writeToSheet(WriteRes, True)


            ' baseline hazard from fitted model (matches R's survfit(coxph, newdata=...))
            Dim bh = cox.ComputeBaseline(bZeroBetas:=False)
            Dim strExpString As String = String.Empty
            With app.ActiveSheet
                For i = 1 To MyData.varNames.Length
                    Dim strCellAddress1 = .Cells(i + 1, 4 + 3 * bh.Keys.Count).Address(RowAbsolute:=True, ColumnAbsolute:=True) 'b
                    Dim strCellAddress2 = .Cells(i + 1, 5 + 3 * bh.Keys.Count).Address(RowAbsolute:=True, ColumnAbsolute:=True) 'predictor value
                    If i = 1 Then
                        strExpString = $"{strCellAddress1}*{strCellAddress2}"
                    Else
                        strExpString += $"+{strCellAddress1}*{strCellAddress2}" 'i.e. bi*xi
                    End If
                Next i
            End With

            app.Worksheets.Add()
            app.ActiveWorkbook.ActiveSheet.name = "Adjusted Curves"
            WriteRes.ws = app.ActiveWorkbook.ActiveSheet
            WriteRes.setRowPointer(1)
            WriteRes.setColumnPointer(1)
            Dim j As Integer = 0
            Dim strCellAddress(,) As Object = Nothing, Strata1Times() As Double = Nothing, Strata1SurvProb() As Double = Nothing
            For Each strId In bh.Keys
                Dim x(,) As Object = Array2objArray(cox.BaseSurvivalForPloting(bh(strId)))
                Dim tit(,) As Object = {{$"Time_{strId}", $"Baseline Survival_{strId}", $"Baseline Cumulative Hazard_{strId}"}}
                WriteRes.write(HorizontalStackArrays(tit, x))
                WriteRes.setRowPointer(1)
                WriteRes.shiftColumnPointer(3)

                If j = 0 Then
                    ReDim strCellAddress(UBound(x, 1), 0), Strata1Times(UBound(x, 1)), Strata1SurvProb(UBound(x, 1))
                    For i = 0 To UBound(x, 1)
                        Dim strAddr = app.ActiveSheet.Cells(i + 2, 2).Address(RowAbsolute:=False, ColumnAbsolute:=False)
                        strCellAddress(i, 0) = $"={strAddr}^exp({strExpString})"

                        Strata1Times(i) = x(i, 0)
                        Strata1SurvProb(i) = x(i, 1)
                    Next
                End If
                j += 1
            Next
            'Adjusted curve
            WriteRes.shiftColumnPointer()
            WriteRes.write(HorizontalStackArrays({{"Adjusted Survival"}}, strCellAddress))
            WriteRes.setRowPointer(1)
            WriteRes.shiftColumnPointer(1)
            app.ActiveSheet.Cells(2, 5 + 3 * bh.Keys.Count).AddComment
            With app.ActiveSheet.Cells(2, 5 + 3 * bh.Keys.Count).Comment
                .Visible = False
                .text("Enter values for adjusted survival curve here. If stratified analysis was performed then you need to adjust" + vbNewLine +
                     "formula in the Adjusted-Survival-column, to compute the adjusted survival curve for appropriate stratum." + vbNewLine +
                     "Note: The 1st stratum is selected as the default for baseline survival curve in the formula." + vbNewLine +
                     "Similarly, you need to change the time values (x-axis) in the chart for the respective stratum.")
                .Shape.TextFrame.AutoSize = True
            End With

            'model output
            Dim t = New ResultTable
            t.AddHeaderTopRow({"Variable", "b", "Covariate Value"})
            t.AddHeaderLeftRow(MyData.varNames)
            Dim tt(resu.Coefficients.Length - 1, 1) As Object
            For i = 0 To resu.Coefficients.Length - 1
                tt(i, 0) = resu.Coefficients(i)
            Next
            t.SetBody(tt)
            WriteRes.write(t)
            With app.ActiveSheet
                .Range(.Cells(2, 5 + 3 * bh.Keys.Count), .Cells(1 + MyData.varNames.Length, 5 + 3 * bh.Keys.Count)).Interior.Color = RGB(255, 255, 0)
            End With

            cox.PlotCox(WriteRes.ws, Strata1Times, Strata1SurvProb, 100, 200)
            'add new series to adjusted survival curve chart
            Dim strXChartDataAddr As String, strYChartDataAddr As String
            With app.ActiveSheet
                strXChartDataAddr = .Cells(2, 1).Address(RowAbsolute:=True, ColumnAbsolute:=True)
                strYChartDataAddr = .Cells(2, 2 + 3 * bh.Keys.Count).Address(RowAbsolute:=True, ColumnAbsolute:=True)
                strXChartDataAddr += ":" + .Cells(1 + Strata1Times.Length, 1).Address(RowAbsolute:=True, ColumnAbsolute:=True)
                strYChartDataAddr += ":" + .Cells(1 + Strata1SurvProb.Length, 2 + 3 * bh.Keys.Count).Address(RowAbsolute:=True, ColumnAbsolute:=True)
            End With

            With app.ActiveSheet.ChartObjects(1).Chart
                .SeriesCollection.NewSeries
                With .SeriesCollection(.SeriesCollection.count)
                    .Name = "=""Adjusted"""
                    .XValues = "='Adjusted Curves'!" & strXChartDataAddr
                    .Values = "='Adjusted Curves'!" & strYChartDataAddr
                End With
            End With

            'Residuals outputs
            app.Worksheets.Add()
            app.ActiveWorkbook.ActiveSheet.name = "Residuals"
            WriteRes.ws = app.ActiveWorkbook.ActiveSheet
            WriteRes.setRowPointer(1)
            WriteRes.setColumnPointer(1)

            Dim residualsList = cox.wrapResiduals()
            WriteRes.write({"", "Row ID"}, True)
            WriteRes.write(MyData.RowIds, True)
            WriteRes.setRowPointer(1)
            WriteRes.shiftColumnPointer(1)
            WriteRes.write({"Data", "Time"}, True)
            WriteRes.write(MyData.TimeData, True)
            WriteRes.setRowPointer(1)
            WriteRes.shiftColumnPointer(1)
            WriteRes.write({"", "Censorship"}, True)
            WriteRes.write(MyData.CensorData, True)
            WriteRes.setRowPointer(2)
            WriteRes.shiftColumnPointer(1)
            WriteRes.write(MyData.varNames)
            WriteRes.write(MyData.FinalData)
            WriteRes.setRowPointer(1)
            WriteRes.shiftColumnPointer(MyData.varNames.Length)

            For i = 0 To residualsList.Count - 1
                WriteRes.write(residualsList(i))
                WriteRes.setRowPointer(1)
                WriteRes.shiftColumnPointer(UBound(residualsList(i), 2) + 1)
            Next
        Catch ex As Exception
            BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub btReload_Click(sender As Object, e As System.EventArgs) Handles btReload.Click
        Dim newSheet As Object
        Me.lbAllColumns.Items.Clear()

        If Me.cbSheetsList.SelectedIndex <> -1 Then
            If pWorksheet.name <> Me.cbSheetsList.SelectedItem.ToString() Then 'new sheet selected clear all listboxes
                Me.lbTime.Items.Clear()
                Me.lbCensoring.Items.Clear()
                Me.lbStrata.Items.Clear()
                Me.lbXs.Items.Clear()
                Me.lbSelectedVariables.Items.Clear()
                Me.lbSelectedEffectsList.Items.Clear()
            End If
            newSheet = pWorkbook.worksheets(Me.cbSheetsList.SelectedItem.ToString())
            Me.Populate(newSheet)
        Else
            Me.Populate(pWorksheet)
        End If
    End Sub

    Sub Populate(ws As Object)
        Dim VarRng As Object, ws_temp As Object
        pWorksheet = ws
        pWorkbook = ws.parent
        Dim FinalCol = LastColumnInSheet(ws)
        Dim MaxRows = MaxRowsInSheet(ws)
        VarRng = ws.Range(ws.Cells(1, 1), ws.Cells(1, FinalCol)) 'Create range object to contain variable names
        Me.VariableColumnsInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns) 'Cycle through the range and add the variable names to the listbox

        'We may call this method multiple times so populate sheet combo box only once
        Me.cbSheetsList.Items.Clear()
        For Each ws_temp In pWorkbook.worksheets
            Me.cbSheetsList.Items.Add(ws_temp.name)
        Next
        Me.cbSheetsList.SelectedIndex = Me.cbSheetsList.FindStringExact(Me.pWorkbook.activesheet.name)
    End Sub

    Private Function GetData() As CoxPHData
        Dim ref As String
        Dim MyData = New CoxPHData

        'Find the response variable and assign the reference
        ref = "'" & pWorksheet.Name & "'!" & CreateReference(pWorksheet, Me.lbTime.Items(0), Me.VariableColumnsInfo)
        'Censorting
        ref = ref & ", " & CreateReference(pWorksheet, Me.lbCensoring.Items(0), Me.VariableColumnsInfo)

        'Stratum
        If Me.lbStrata.Items.Count > 0 Then
            If Me.lbStrata.Items(0) <> String.Empty Then
                MyData.bStrata = True
                ref = ref & ", " & CreateReference(pWorksheet, Me.lbStrata.Items(0), Me.VariableColumnsInfo)
            End If
        End If

        'X vars
        For i = 0 To lbSelectedEffectsList.Items.Count - 1
            ref = ref & ", " & CreateReference(pWorksheet, Me.lbSelectedEffectsList.Items(i), Me.VariableColumnsInfo)
        Next i
        Debug.Print(ref)

        MyData.DataInport(ref)
        Return MyData
    End Function

    Private Sub validateInputs(ByRef bWait As Boolean, ByRef strErr As String)
        Dim vals() As Double, bErr As Boolean

        'Initial parameter values
        If Me.tbInitValues.Text <> String.Empty Then
            setTextBoxProperties(Me.tbInitValues, Color.White, String.Empty) 'give the text box its usual background
            vals = GetNumbersFromStrList(Me.tbInitValues.Text, bErr)
            If bErr Then 'Error while converting to array
                strErr = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers."
                setTextBoxProperties(Me.tbInitValues, Color.Red, strErr)
                bWait = True
                Exit Sub
            End If
            If vals.Length <> Me.lbSelectedEffectsList.Items.Count + 1 Then '+1 because of intercept
                strErr = "Number of initial values does not match the number of estimated parameters." & vbNewLine &
                         "Initial value for the intercept should be the first one in the list."
                setTextBoxProperties(Me.tbInitValues, Color.Red, strErr)
                bWait = True
                Exit Sub
            End If
        End If

        'Input variables
        'Import data from listboxes
        If Me.lbTime.Items.Count = 0 Then
            strErr = "Time variable is not specified."
            bWait = True
            Exit Sub
        End If
        If Me.lbCensoring.Items.Count = 0 Then
            strErr = "Censoring variable is not specified."
            bWait = True
            Exit Sub
        End If
        If Me.lbSelectedEffectsList.Items.Count = 0 Then
            strErr = "No Intercept and Effects were specified."
            bWait = True
            Exit Sub
        End If
        If Me.lbSelectedEffectsList.Items.Count = 0 Then
            If MsgBox("Do you want to fit intercept only model?", vbYesNo + vbExclamation, gsAPP_TITLE) = vbNo Then
                bWait = True
                Exit Sub
            End If
        ElseIf Me.lbSelectedEffectsList.Items.Count = 0 Then
            strErr = "No Effects were specified."
            bWait = True
            Exit Sub
        End If
    End Sub

    Private Sub btAddTime_Click(sender As Object, e As System.EventArgs) Handles btAddTime.Click
        AddItemToListbox(Me.lbTime, Me.lbAllColumns, Me.lbXs, Me.lbCensoring, Me.lbStrata)
    End Sub

    Private Sub btRemoveTime_Click(sender As Object, e As System.EventArgs) Handles btRemoveTime.Click
        Remove_Item(Me.lbTime)
    End Sub

    Private Sub btAddCensorting_Click(sender As Object, e As System.EventArgs) Handles btAddCensorting.Click
        AddItemToListbox(Me.lbCensoring, Me.lbAllColumns, Me.lbXs, Me.lbTime, Me.lbStrata)
    End Sub

    Private Sub btRemoveCensoring_Click(sender As Object, e As System.EventArgs) Handles btRemoveCensoring.Click
        Remove_Item(Me.lbCensoring)
    End Sub

    Private Sub btAddStrata_Click(sender As Object, e As System.EventArgs) Handles btAddStrata.Click
        AddItemToListbox(Me.lbStrata, Me.lbAllColumns, Me.lbXs, Me.lbTime, Me.lbCensoring)
    End Sub

    Private Sub btRemoveStrata_Click(sender As Object, e As System.EventArgs) Handles btRemoveStrata.Click
        Remove_Item(Me.lbStrata)
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        AddItemsToListbox(Me.lbXs, Me.lbAllColumns, Me.lbTime, Me.lbCensoring, Me.lbStrata)
    End Sub

    Private Sub btRemoveX_Click(sender As Object, e As System.EventArgs) Handles btRemoveX.Click
        Remove_Item(Me.lbXs, "selected")
    End Sub

    Private Sub btAddEffect_Click(sender As Object, e As System.EventArgs) Handles btAddEffect.Click
        AddItemsToListbox(Me.lbSelectedEffectsList, Me.lbSelectedVariables, Me.lbTime, Me.lbCensoring, Me.lbStrata)
    End Sub

    Private Sub tbRemoveSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "selected")
    End Sub

    Private Sub btClearAllSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedEffects.Click
        Me.lbSelectedEffectsList.Items.Clear()
    End Sub

    Private Sub tbInitValues_Leave(sender As Object, e As System.EventArgs) Handles tbInitValues.Leave
        Dim vals() As Double, bErr As Boolean, tiptext As String

        setTextBoxProperties(Me.tbInitValues, Color.White, String.Empty) 'give the text box its usual background
        vals = GetNumbersFromStrList(Me.tbInitValues.Text, bErr)
        If bErr Then 'Error while converting to array
            tiptext = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers."
            setTextBoxProperties(Me.tbInitValues, Color.Red, tiptext)
        End If
        If vals.Length <> Me.lbSelectedEffectsList.Items.Count + 1 Then '+1 because of intercept
            tiptext = "Number of initial values does not match the number of estimated parameters." & vbNewLine &
                      "Initial value for the intercept should be the first one in the list."
            setTextBoxProperties(Me.tbInitValues, Color.Red, tiptext)
        End If
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles TabControl1.SelectedIndexChanged
        Dim i As Integer

        If Me.lbSelectedVariables.Items.Count > 0 Then
            If Not IsEqualListBox(Me.lbXs, Me.lbSelectedVariables) Then
                'values on 1st tab changed so refresh it with new values
                If Me.lbSelectedVariables.Items.Count > 0 Then Remove_Item(Me.lbSelectedVariables)
                For i = 0 To Me.lbXs.Items.Count - 1
                    Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
                Next i
                If Not IsSubsetListBox(Me.lbSelectedVariables, Me.lbSelectedEffectsList) Then
                    If MsgBox("There is a variable in selected effects list that was removed from the predictor variable(s) list." & vbNewLine & vbNewLine &
                "Clear selected effects list?", vbYesNo + vbExclamation, "Clear selected effects list?") = vbYes Then
                        'Selected item was removed from X vars
                        'TODO: this need to be updated when start using poly and interaction effects
                        If Me.lbSelectedEffectsList.Items.Count > 0 Then Remove_Item(Me.lbSelectedEffectsList)
                    End If
                End If
            End If
        Else 'load actual Xvars list for the 1st time
            For i = 0 To Me.lbXs.Items.Count - 1
                Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
            Next i
        End If
    End Sub

End Class