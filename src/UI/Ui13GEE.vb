Imports System.Drawing

Public Class Ui13GEE

    Private pWorksheet As Object
    Private pWorkbook As Object
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information of variable/column names inported into the input listbox

    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Text = analysis

        ' Add any initialization after the InitializeComponent() call.
        If Me.Text = "Generalized Estimating Equations" Then

            For Each sFam In regression.Family.FamiliesList
                Me.cbFamily.Items.Add(sFam)
            Next
            For Each sLink In regression.Link.LinkList.Values
                Me.cbLink.Items.Add(sLink)
            Next
            For Each sCovStruct In regression.GEEcovStruct.CovStructsList
                Me.cbCovarStruct.Items.Add(sCovStruct)
            Next
            For Each sSE In {"Robust", "Naive", "Bias Reduced"}
                Me.cbStandardErr.Items.Add(sSE)
            Next
            Me.cbFamily.SelectedIndex = 0
            Me.cbLink.SelectedIndex = 0
            Me.cbCovarStruct.SelectedIndex = 0
            Me.cbStandardErr.SelectedIndex = 0
        End If


        Me.TabControl1.Anchor = Windows.Forms.AnchorStyles.Left Or
                                Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right Or
                                Windows.Forms.AnchorStyles.Top
        Me.btCalculate.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right
        Me.btnHelp.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right
        Me.lblProgress.Anchor = Windows.Forms.AnchorStyles.Left Or
                                Windows.Forms.AnchorStyles.Bottom
        Me.ProgressBar1.Anchor = Windows.Forms.AnchorStyles.Left Or
                                 Windows.Forms.AnchorStyles.Right Or
                                 Windows.Forms.AnchorStyles.Bottom
        Me.lbAllColumns.Anchor = Windows.Forms.AnchorStyles.Left Or
                                 Windows.Forms.AnchorStyles.Bottom Or
                                 Windows.Forms.AnchorStyles.Top
        'Me.lbWeights.Anchor = Windows.Forms.AnchorStyles.Right Or Windows.Forms.AnchorStyles.Left
        Me.lbY.Anchor = Windows.Forms.AnchorStyles.Left Or
                        Windows.Forms.AnchorStyles.Right Or
                        Windows.Forms.AnchorStyles.Top
        Me.lbOffset.Anchor = Windows.Forms.AnchorStyles.Left Or
                             Windows.Forms.AnchorStyles.Right Or
                             Windows.Forms.AnchorStyles.Top
        Me.lbWeights.Anchor = Windows.Forms.AnchorStyles.Left Or
                              Windows.Forms.AnchorStyles.Right Or
                              Windows.Forms.AnchorStyles.Top
        Me.lbClusterID.Anchor = Windows.Forms.AnchorStyles.Left Or
                                Windows.Forms.AnchorStyles.Right Or
                                Windows.Forms.AnchorStyles.Top
        Me.lbTime.Anchor = Windows.Forms.AnchorStyles.Left Or
                           Windows.Forms.AnchorStyles.Right Or
                           Windows.Forms.AnchorStyles.Top
        Me.lbXs.Anchor = Windows.Forms.AnchorStyles.Left Or
                         Windows.Forms.AnchorStyles.Right Or
                         Windows.Forms.AnchorStyles.Top Or
                         Windows.Forms.AnchorStyles.Bottom

        Me.lblNote.Anchor = Windows.Forms.AnchorStyles.Top Or
                            Windows.Forms.AnchorStyles.Right Or
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

    Private Sub btReload_Click(sender As Object, e As System.EventArgs) Handles btReload.Click
        Dim newSheet As Object
        Me.lbAllColumns.Items.Clear()

        If Me.cbSheetsList.SelectedIndex <> -1 Then
            If pWorksheet.name <> Me.cbSheetsList.SelectedItem.ToString() Then 'new sheet selected clear all listboxes
                Me.lbY.Items.Clear()
                Me.lbOffset.Items.Clear()
                Me.lbWeights.Items.Clear()
                Me.lbClusterID.Items.Clear()
                Me.lbTime.Items.Clear()
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

        If Me.lbSelectedVariables.Items.Count > 0 Then
            If Not IsEqualListBox(Me.lbXs, Me.lbSelectedVariables) Then
                'values on 1st tab changed so refresh it with new values
                If Me.lbSelectedVariables.Items.Count > 0 Then Remove_Item(Me.lbSelectedVariables)
                For i = 0 To Me.lbXs.Items.Count - 1
                    Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
                Next
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
            Next
        End If
    End Sub

    Private Sub valiateInputs(ByRef bWait As Boolean, ByRef strErr As String)
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
        If Me.lbY.Items.Count = 0 Then
            strErr = "Dependent variable is missing."
            bWait = True
            Exit Sub
        End If
        If Me.lbClusterID.Items.Count = 0 Then
            strErr = "Cluster ID variable is missing."
            bWait = True
            Exit Sub
        End If
        If Me.lbSelectedEffectsList.Items.Count = 0 And Not Me.cbIntercept.Checked Then
            strErr = "No Intercept and Effects were specified."
            bWait = True
            Exit Sub
        End If
        If Me.lbSelectedEffectsList.Items.Count = 0 And Me.cbIntercept.Checked Then
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

    Private Function GetData() As geeData
        Dim ref As String
        Dim MyData As geeData = New geeData

        'Find the response variable and assign the reference
        ref = "'" & pWorksheet.Name & "'!" & CreateReference(pWorksheet, Me.lbY.Items(0), Me.VariableColumnsInfo)

        'X vars
        For i = 0 To lbSelectedEffectsList.Items.Count - 1
            ref = ref & ", " & CreateReference(pWorksheet, Me.lbSelectedEffectsList.Items(i), Me.VariableColumnsInfo)
        Next
        'Cluster ID
        If Me.lbClusterID.Items(0) <> String.Empty Then
            ref = ref & ", " & CreateReference(pWorksheet, Me.lbClusterID.Items(0), Me.VariableColumnsInfo)
        End If
        'Time/Withing cluster ordering variable
        If Me.lbTime.Items.Count > 0 Then
            If Me.lbTime.Items(0) <> vbNullString Then
                MyData.bTime = True
                ref = ref & ", " & CreateReference(pWorksheet, Me.lbTime.Items(0), Me.VariableColumnsInfo)
            End If
        End If
        'Offset
        If Me.lbOffset.Items.Count > 0 Then
            If Me.lbOffset.Items(0) <> String.Empty Then
                MyData.bOffset = True
                ref = ref & ", " & CreateReference(pWorksheet, Me.lbOffset.Items(0), Me.VariableColumnsInfo)
            End If
        End If
        'Weights
        If Me.lbWeights.Items.Count > 0 Then
            If Me.lbWeights.Items(0) <> String.Empty Then
                MyData.bWeights = True
                ref = ref & ", " & CreateReference(pWorksheet, Me.lbWeights.Items(0), Me.VariableColumnsInfo)
            End If
        End If

        'Prepare Data from references
        MyData.DataInport(ref)
        Return MyData
    End Function

    Private Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        Try
            Dim bWait As Boolean, strWarning As String
            'activate workbook we are working on (different may  be open if we re-running the analysis)
            Me.pWorkbook.activate

            strWarning = String.Empty
            valiateInputs(bWait, strWarning)
            If bWait Then
                If strWarning <> String.Empty Then MsgBox(strWarning)
                Exit Sub
            End If

            Dim MyData = GetData()
            If MyData.bZeroValid Then 'check for zero valid data
                MsgBox("No valid observations")
                Exit Sub
            End If

            'Initialization values
            Dim bInitialValues = False
            If Me.tbInitValues.Text <> String.Empty Then
                Dim bErr As Boolean = False
                Dim initVals = GetNumbersFromStrList(Me.tbInitValues.Text, bErr)
                If bErr Then
                    BSlogg.Log("Cannot extract initial parameter values. They will be ignored.")
                    MsgBox("Cannot extract initial parameter values. They will be ignored.")
                Else
                    bInitialValues = True
                End If
            End If

            If Me.Text = "Generalized Estimating Equations" Then
                Me.RunGEE(MyData, bInitialValues)
            End If
        Catch ex As Exception
            BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunGEE(MyData As geeData, bInitialValues As Boolean)
        Dim fitGEE As GEE
        Try
            'create family
            Dim fam = regression.createFamily(regression.Family.FamiliesCodes(Me.cbFamily.SelectedIndex))
            If Me.tbDispersionParameterNB2.Text <> String.Empty Then
                Try
                    Dim dispParam As Double = CDbl(Me.tbDispersionParameterNB2.Text)
                    If dispParam > 0 Then fam.pdAlpha = dispParam
                Catch
                End Try
            End If

            'create link
            Dim lnk As regression.Link
            If Me.cbLink.SelectedItem = "Power" Then
                lnk = regression.createLink(Me.cbLink.SelectedItem, CDbl(Me.tbPower.Text))
            Else
                lnk = regression.createLink(Me.cbLink.SelectedItem)
            End If

            'create Covariance structure
            Dim covStr = regression.createGEEcovMat(regression.GEEcovStruct.CovStructsList(Me.cbCovarStruct.SelectedIndex))
            fitGEE = New GEE(fam, lnk, covStr, Me.cbStandardErr.SelectedItem)
            fitGEE.data(MyData.DataDbl, MyData.ClusterIdData, MyData.RowIds,
                    If(MyData.bOffset, MyData.OffsetData, Nothing),
                    If(MyData.bWeights, MyData.WeightData, Nothing),
                    If(MyData.bTime, MyData.TimeData, Nothing))
            fitGEE.setVarNames(MyData.varNames, MyData.ClusterIdVarName,
                           If(MyData.bOffset, MyData.OffsetVarName, Nothing),
                           If(MyData.bWeights, MyData.WeightVarName, Nothing),
                           If(MyData.bTime, MyData.TimeVarName, Nothing))
            fitGEE.bComputeResiduals = Me.ckResiduals.Checked
            fitGEE.bIterationDetails = Me.ckIterationsDetails.Checked
            fitGEE.settingInputs(0.05, CInt(Me.tbMaxIter.Text), CDbl(Me.tbEps.Text), Me.ckUseP.Checked)
            If bInitialValues Then fitGEE.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'we tested already that they are correct
            fitGEE.Fit(bInitialValues, , Me.ProgressBar1, Me.lblProgress)

            ''Dump results
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
            WriteRes.setRowPointer()
            WriteRes.shiftColumnPointer(UBound(MyData.FinalData, 2) + 1)

            'Offset
            If MyData.bOffset Then
                WriteRes.write({MyData.OffsetVarName})
                WriteRes.setRowPointer(2)
                WriteRes.write(MyData.OffsetData, bTall:=True)
                WriteRes.setRowPointer()
                WriteRes.shiftColumnPointer(1)
            End If
            'Weights
            If MyData.bWeights Then
                WriteRes.write({MyData.WeightVarName})
                WriteRes.setRowPointer(2)
                WriteRes.write(MyData.WeightData, bTall:=True)
                WriteRes.setRowPointer()
                WriteRes.shiftColumnPointer(1)
            End If
            'Time
            If MyData.bTime Then
                WriteRes.write({MyData.TimeVarName})
                WriteRes.setRowPointer(2)
                WriteRes.write(MyData.TimeData, bTall:=True)
                WriteRes.setRowPointer()
                WriteRes.shiftColumnPointer(1)
            End If
            'Cluster ID
            WriteRes.write({MyData.ClusterIdVarName})
            WriteRes.setRowPointer(2)
            WriteRes.write(MyData.ClusterIdData, bTall:=True)
            WriteRes.setRowPointer()
            WriteRes.shiftColumnPointer(1)

            'Prediction
            WriteRes.write({"Prediction"})
            WriteRes.setRowPointer(2)
            WriteRes.write(fitGEE.PredictedResponses, bTall:=True)
            WriteRes.setRowPointer()
            WriteRes.shiftColumnPointer(1)

            'Residuals
            If fitGEE.bComputeResiduals Then WriteRes.write(fitGEE.AllResiduals())

            'Create new worksheet in workbook. It will automaticaly be an activesheet
            'We need to start new writer to start writing on this new sheet
            Dim res = fitGEE.wrapResults()
            WriteRes = New WriteResults
            app.ActiveWorkbook.Worksheets.Add()
            app.ActiveWorkbook.ActiveSheet.name = "GEE"
            WriteRes.ws = app.ActiveWorkbook.ActiveSheet

            Dim rr = New ProcessListofResultTables(res)
            rr.writeToSheet(WriteRes, True)
        Catch ex As Exception
            Debug.Print(ex.Message)
        End Try
    End Sub

    Private Sub btRemoveY_Click(sender As Object, e As System.EventArgs) Handles btRemoveY.Click
        Remove_Item(Me.lbY)
    End Sub

    Private Sub btRemoveOffset_Click(sender As Object, e As System.EventArgs) Handles btRemoveOffset.Click
        Remove_Item(Me.lbOffset)
    End Sub

    Private Sub btRemoveWeights_Click(sender As Object, e As System.EventArgs) Handles btRemoveWeights.Click
        Remove_Item(Me.lbWeights)
    End Sub

    Private Sub btRemoveClusterID_Click(sender As Object, e As System.EventArgs) Handles btRemoveClusterID.Click
        Remove_Item(Me.lbClusterID)
    End Sub

    Private Sub btRemoveTime_Click(sender As Object, e As System.EventArgs) Handles btRemoveTime.Click
        Remove_Item(Me.lbTime)
    End Sub

    Private Sub btRemoveX_Click(sender As Object, e As System.EventArgs) Handles btRemoveX.Click
        Remove_Item(Me.lbXs, "selected")
    End Sub

    Private Sub btAddY_Click(sender As Object, e As System.EventArgs) Handles btAddY.Click
        AddItemToListbox(Me.lbY, Me.lbAllColumns, Me.lbXs, Me.lbOffset, Me.lbWeights, Me.lbClusterID, Me.lbTime)
    End Sub

    Private Sub btAddOffset_Click(sender As Object, e As System.EventArgs) Handles btAddOffset.Click
        AddItemToListbox(Me.lbOffset, Me.lbAllColumns, Me.lbXs, Me.lbY, Me.lbWeights, Me.lbClusterID, Me.lbTime)
    End Sub

    Private Sub btAddWeights_Click(sender As Object, e As System.EventArgs) Handles btAddWeights.Click
        AddItemToListbox(Me.lbWeights, Me.lbAllColumns, Me.lbXs, Me.lbY, Me.lbOffset, Me.lbClusterID, Me.lbTime)
    End Sub

    Private Sub btAddClusterID_Click(sender As Object, e As System.EventArgs) Handles btAddClusterID.Click
        AddItemToListbox(Me.lbClusterID, Me.lbAllColumns, Me.lbXs, Me.lbY, Me.lbOffset, Me.lbWeights, Me.lbTime) '
    End Sub

    Private Sub btAddTime_Click(sender As Object, e As System.EventArgs) Handles btAddTime.Click
        AddItemToListbox(Me.lbTime, Me.lbAllColumns, Me.lbY, Me.lbOffset, Me.lbWeights, Me.lbClusterID) ', Me.lbXs
    End Sub

    Private Sub btAddEffect_Click(sender As Object, e As System.EventArgs) Handles btAddEffect.Click
        AddItemsToListbox(Me.lbSelectedEffectsList, Me.lbSelectedVariables, Me.lbY, Me.lbOffset, Me.lbWeights, Me.lbClusterID)
    End Sub

    Private Sub tbRemoveSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "selected")
    End Sub

    Private Sub btClearAllSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedEffects.Click
        Me.lbSelectedEffectsList.Items.Clear()
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        AddItemsToListbox(Me.lbXs, Me.lbAllColumns, Me.lbY, Me.lbOffset, Me.lbWeights, Me.lbClusterID)
    End Sub
End Class