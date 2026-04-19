Imports System.Drawing
Imports BESHStatNG.AppInfrastructure

Public Class Ui13GEE

    Private pWorksheet As Object
    Private pWorkbook As Object
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information of variable/column names inported into the input listbox
    'Ui13GEE owns the TermSpecs dictionary; the shared EffectsController mutates this same
    'instance by reference so add/remove/clear operations remain synchronized.
    Private TermSpecs As Dictionary(Of String, TermSpec)
    Private ReadOnly EffectsController As RegressionEffectsController

    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()
        Me.tbEps.Text = FormatUiDouble(0.000001)
        Me.Text = analysis
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)

        ' Add any initialization after the InitializeComponent() call.
        If Me.Text = "Generalized Estimating Equations" Then

            For Each sFam In regression.Family.FamiliesList
                Me.cbFamily.Items.Add(sFam)
            Next
            For Each sCovStruct In regression.GEEcovStruct.CovStructsList
                Me.cbCovarStruct.Items.Add(sCovStruct)
            Next
            For Each sSE In {"Robust", "Naive", "Bias Reduced"}
                Me.cbStandardErr.Items.Add(sSE)
            Next
            Me.cbFamily.SelectedIndex = 0
            RefreshLinkOptionsForSelectedFamily(regression.GetCanonicalLinkFromDisplayName(Me.cbFamily.SelectedItem.ToString()))
            Me.cbCovarStruct.SelectedIndex = 0
            Me.cbStandardErr.SelectedIndex = 0
            UpdateClassificationOptionsState(False)
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


        'Term specifications for selected effects.
        'This dictionary remains owned by Ui13GEE and is passed into the shared controller
        'so both the form and the controller operate on the same backing state.
        Me.TermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)

        'Shared effect-authoring controller for GEE model construction.
        Me.EffectsController = New RegressionEffectsController(Me.lbSelectedVariables,
                                                               Me.lbSelectedEffectsList,
                                                               Me.TermSpecs)
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

    Private Shared Function GetFamilyCodeFromDisplayName(familyDisplayName As String) As String
        Select Case familyDisplayName
            Case "Binomial"
                Return "Binomial"
            Case "Poisson"
                Return "Poisson"
            Case "Negative Binomial"
                Return "NegativeBinomial"
            Case "Gaussian"
                Return "Gaussian"
            Case "Gamma"
                Return "Gamma"
            Case Else
                Return String.Empty
        End Select
    End Function

    Private Sub RefreshLinkOptionsForSelectedFamily(Optional preferredLink As String = Nothing)
        Dim selectedFamilyName As String = String.Empty
        If Me.cbFamily.SelectedItem IsNot Nothing Then
            selectedFamilyName = Me.cbFamily.SelectedItem.ToString()
        End If

        Dim linkToSelect As String = preferredLink
        If String.IsNullOrWhiteSpace(linkToSelect) AndAlso Me.cbLink.SelectedItem IsNot Nothing Then
            linkToSelect = Me.cbLink.SelectedItem.ToString()
        End If

        Dim familyCode As String = GetFamilyCodeFromDisplayName(selectedFamilyName)

        Me.cbLink.BeginUpdate()
        Try
            Me.cbLink.Items.Clear()

            If String.IsNullOrWhiteSpace(familyCode) Then
                Me.cbLink.SelectedIndex = -1
                UpdatePowerLinkState()
                UpdateClassificationOptionsState()
                Return
            End If

            Dim fam As regression.Family = regression.createFamily(familyCode)

            For Each sLink As String In regression.Link.LinkList.Values
                If fam.testLink(sLink) Then
                    Me.cbLink.Items.Add(sLink)
                End If
            Next

            If Not String.IsNullOrWhiteSpace(linkToSelect) Then
                Dim existingIndex As Integer = Me.cbLink.FindStringExact(linkToSelect)
                If existingIndex >= 0 Then
                    Me.cbLink.SelectedIndex = existingIndex
                End If
            End If

            If Me.cbLink.SelectedIndex = -1 AndAlso Me.cbLink.Items.Count > 0 Then
                Me.cbLink.SelectedIndex = 0
            End If

            UpdatePowerLinkState()
            UpdateClassificationOptionsState()

        Finally
            Me.cbLink.EndUpdate()
        End Try
    End Sub

    Private Sub UpdatePowerLinkState()
        Dim usePowerLink As Boolean =
            Me.cbLink.SelectedItem IsNot Nothing AndAlso
            Me.cbLink.SelectedItem.ToString() = "Power"

        Me.lblPower.Enabled = usePowerLink
        Me.tbPower.Enabled = usePowerLink
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
                Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
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
            tiptext = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers without thousands separators."
            setTextBoxProperties(Me.tbInitValues, Color.Red, tiptext)
            Exit Sub
        End If

        'Do not validate the exact number of initial values here.
        'The exact parameter count depends on the expanded design matrix and is
        'validated after effect expansion inside RunGEE().
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles TabControl1.SelectedIndexChanged

        If Me.lbSelectedVariables.Items.Count > 0 Then
            If Not IsEqualListBox(Me.lbXs, Me.lbSelectedVariables) Then
                'values on 1st tab changed so refresh it with new values
                If Me.lbSelectedVariables.Items.Count > 0 Then Remove_Item(Me.lbSelectedVariables)
                For i = 0 To Me.lbXs.Items.Count - 1
                    Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
                Next
                If Not IsSubsetListBox(Me.lbSelectedVariables, Me.lbSelectedEffectsList, bOnlyMain:=True) Then
                    If MsgBox("There is a variable in selected effects list that was removed from the predictor variable(s) list." & vbNewLine & vbNewLine &
                              "Clear selected effects list?", vbYesNo + vbExclamation, "Clear selected effects list?") = vbYes Then
                        'Selected item was removed from X vars
                        If Me.lbSelectedEffectsList.Items.Count > 0 Then Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
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
                strErr = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers without thousands separators."
                setTextBoxProperties(Me.tbInitValues, Color.Red, strErr)
                bWait = True
                Exit Sub
            End If

            'Do not validate parameter count here.
            'The exact count depends on the expanded design matrix and is validated
            'after expansion inside RunGEE().
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
            If MsgBox("Do you want to fit intercept only model?", vbYesNo + vbExclamation, AppGlobals.gsAPP_TITLE) = vbNo Then
                bWait = True
                Exit Sub
            End If
        ElseIf Me.lbSelectedEffectsList.Items.Count = 0 Then
            strErr = "No Effects were specified."
            bWait = True
            Exit Sub
        End If

        If IsCurrentBinomialGeeFamily() Then
            setTextBoxProperties(Me.tbClassificationTreshold, Color.White, String.Empty)
            Try
                Dim threshold As Double = GetClassificationThresholdUiValue()
            Catch ex As Exception
                strErr = ex.Message
                setTextBoxProperties(Me.tbClassificationTreshold, Color.Red, strErr)
                bWait = True
                Exit Sub
            End Try
        End If
    End Sub

    Private Function GetData() As geeData
        Dim MyData As geeData = New geeData
        Dim keys As New List(Of String)

        'Response variable always first
        keys.Add(CStr(Me.lbY.Items(0)))

        'Only import required RAW predictors. The selected effects list may later
        'contain derived terms, but the raw import should remain stable.
        Dim rawXKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedEffectsList.Items, Me.TermSpecs)
        For Each xKey As String In rawXKeys
            keys.Add(xKey)
        Next

        'Cluster ID
        If Me.lbClusterID.Items(0) <> String.Empty Then keys.Add(CStr(Me.lbClusterID.Items(0)))

        'Time / within-cluster ordering variable
        If Me.lbTime.Items.Count > 0 AndAlso Me.lbTime.Items(0) <> vbNullString Then
            MyData.bTime = True
            keys.Add(CStr(Me.lbTime.Items(0)))
        End If

        'Offset
        If Me.lbOffset.Items.Count > 0 AndAlso Me.lbOffset.Items(0) <> String.Empty Then
            MyData.bOffset = True
            keys.Add(CStr(Me.lbOffset.Items(0)))
        End If

        'Weights
        If Me.lbWeights.Items.Count > 0 AndAlso Me.lbWeights.Items(0) <> String.Empty Then
            MyData.bWeights = True
            keys.Add(CStr(Me.lbWeights.Items(0)))
        End If

        Dim ref As String = BuildExcelRefList(pWorksheet, keys, Me.VariableColumnsInfo)
        MyData.DataInport(ref)
        Return MyData
    End Function

    ''' <summary>
    ''' Builds the expanded regression matrix and aligned variable names for GEE.
    ''' </summary>
    ''' <param name="MyData">
    ''' Raw imported GEE data containing Y in column 0 and only required raw predictors thereafter.
    ''' Cluster id, time, offset, and weights are stored separately by <see cref="geeData"/>.
    ''' </param>
    ''' <param name="fitData">
    ''' Returns the expanded matrix in the form [Y | expanded X].
    ''' </param>
    ''' <param name="fitVarNames">
    ''' Returns variable names aligned to <paramref name="fitData"/>.
    ''' </param>
    Private Sub BuildExpandedRegressionInputs(MyData As geeData,
                                              ByRef fitData(,) As Double,
                                              ByRef fitVarNames() As String)

        'The current GEE engine always includes an intercept internally, so categorical
        'predictors should always omit their reference level when expanded.
        RegressionDesignCore.BuildExpandedRegressionDataMatrix(raw:=MyData,
                                                       yKey:=CStr(Me.lbY.Items(0)),
                                                       effectItems:=Me.lbSelectedEffectsList.Items,
                                                       termSpecs:=Me.TermSpecs,
                                                       omitCategoricalReference:=True,
                                                       outData:=fitData,
                                                       outVarNames:=fitVarNames)
    End Sub

    ''' <summary>
    ''' Validates the exact number of user-supplied initial values after effect expansion.
    ''' </summary>
    ''' <param name="expectedCount">
    ''' The exact number of mean-model parameters expected by the fitted GEE.
    ''' </param>
    ''' <returns>
    ''' <see langword="True"/> when the supplied initial values are valid; otherwise <see langword="False"/>.
    ''' </returns>
    Private Function ValidateExpandedInitialValuesCount(expectedCount As Integer) As Boolean
        If Me.tbInitValues.Text = String.Empty Then Return True

        setTextBoxProperties(Me.tbInitValues, Color.White, String.Empty)

        Dim bErr As Boolean = False
        Dim vals() As Double = GetNumbersFromStrList(Me.tbInitValues.Text, bErr)

        If bErr Then
            Dim msg As String = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers without thousands separators."
            setTextBoxProperties(Me.tbInitValues, Color.Red, msg)
            MsgBox(msg, vbExclamation, "Input Error!")
            Return False
        End If

        If vals.Length <> expectedCount Then
            Dim msg As String = $"Number of initial values does not match the number of estimated parameters for Generalized Estimating Equations." &
                                vbNewLine &
                                $"Expected {expectedCount}, received {vals.Length}." &
                                vbNewLine &
                                "Initial value for the intercept should be the first one in the list."

            setTextBoxProperties(Me.tbInitValues, Color.Red, msg)
            MsgBox(msg, vbExclamation, "Input Error!")
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Returns <c>True</c> when the currently selected GEE family is binomial.
    ''' </summary>
    Private Function IsCurrentBinomialGeeFamily() As Boolean
        If Not String.Equals(Me.Text, "Generalized Estimating Equations", StringComparison.Ordinal) Then Return False
        If Me.cbFamily.SelectedItem Is Nothing Then Return False
        Return String.Equals(GetFamilyCodeFromDisplayName(Me.cbFamily.SelectedItem.ToString()), "Binomial", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Reads and validates the classification threshold entered in the UI.
    ''' </summary>
    ''' <returns>
    ''' Threshold on the closed interval [0,1]. Defaults to 0.5 when the text box is blank.
    ''' </returns>
    Private Function GetClassificationThresholdUiValue() As Double
        Dim txt As String = Me.tbClassificationTreshold.Text.Trim()
        If txt = String.Empty Then
            Me.tbClassificationTreshold.Text = FormatUiDouble(0.5R)
            Return 0.5R
        End If

        Dim threshold As Double = ParseUiDouble(txt, "classification threshold")
        If threshold < 0.0R OrElse threshold > 1.0R Then
            Throw New FormatException("Classification threshold must be between 0 and 1.")
        End If
        Return threshold
    End Function

    ''' <summary>
    ''' Enables or disables the classification output group according to the currently
    ''' selected family. Classification reporting is available only for binomial GEE.
    ''' </summary>
    ''' <param name="bAlreadyInit">
    ''' When <c>True</c>, invalid highlight state on the threshold text box may be cleared
    ''' when the classification group is disabled.
    ''' </param>
    Private Sub UpdateClassificationOptionsState(Optional bAlreadyInit As Boolean = True)
        Dim enabledForFamily As Boolean = IsCurrentBinomialGeeFamily()

        Me.grpClassification.Enabled = enabledForFamily
        Me.cbPerformClasification.Enabled = enabledForFamily
        Me.lblCallibrationBinsN.Enabled = enabledForFamily AndAlso Me.cbOutputCalibrationTable.Checked
        Me.spinBtnCallibrationBinsN.Enabled = enabledForFamily AndAlso Me.cbOutputCalibrationTable.Checked

        If Not enabledForFamily Then
            If bAlreadyInit Then setTextBoxProperties(Me.tbClassificationTreshold, Color.White, String.Empty)
        ElseIf String.IsNullOrWhiteSpace(Me.tbClassificationTreshold.Text) Then
            Me.tbClassificationTreshold.Text = FormatUiDouble(0.5R)
        End If

        If Me.cbPerformClasification.Checked And enabledForFamily Then
            Me.grpClassification.Enabled = True
        ElseIf Not Me.cbPerformClasification.Checked And enabledForFamily Then
            Me.grpClassification.Enabled = False
        End If
    End Sub

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
                    AppGlobals.BSlogg.Log("Cannot extract initial parameter values. They will be ignored.")
                    MsgBox("Cannot extract initial parameter values. They will be ignored.")
                Else
                    bInitialValues = True
                End If
            End If

            If Me.Text = "Generalized Estimating Equations" Then
                Me.RunGEE(MyData, bInitialValues)
            End If
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunGEE(MyData As geeData, bInitialValues As Boolean)
        Dim fitGEE As GEE
        Try
            Dim alphaValue As Double = Me.spinBtnAlpha.Value
            Dim fitData(,) As Double = Nothing
            Dim fitVarNames() As String = Nothing

            BuildExpandedRegressionInputs(MyData, fitData, fitVarNames)

            If bInitialValues Then
                'GEE currently always includes an intercept internally.
                Dim expectedCount As Integer = fitVarNames.Length

                If Not ValidateExpandedInitialValuesCount(expectedCount) Then
                    Exit Sub
                End If
            End If

            'create family
            Dim fam = regression.createFamily(regression.Family.FamiliesCodes(Me.cbFamily.SelectedIndex))
            If Me.tbDispersionParameterNB2.Text <> String.Empty Then
                Try
                    Dim dispParam As Double = ParseUiDouble(Me.tbDispersionParameterNB2.Text, "Dispersion parameter")
                    If dispParam > 0 Then fam.pdAlpha = dispParam
                Catch
                End Try
            End If

            'create link
            Dim lnk As regression.Link
            If Me.cbLink.SelectedItem = "Power" Then
                lnk = regression.createLink(Me.cbLink.SelectedItem, ParseUiDouble(Me.tbPower.Text, "Power link parameter"))
            Else
                lnk = regression.createLink(Me.cbLink.SelectedItem)
            End If

            'create Covariance structure
            Dim covStr = regression.createGEEcovMat(regression.GEEcovStruct.CovStructsList(Me.cbCovarStruct.SelectedIndex))
            fitGEE = New GEE(fam, lnk, covStr, Me.cbStandardErr.SelectedItem)

            fitGEE.data(fitData, MyData.ClusterIdData, MyData.RowIds,
                    If(MyData.bOffset, MyData.OffsetData, Nothing),
                    If(MyData.bWeights, MyData.WeightData, Nothing),
                    If(MyData.bTime, MyData.TimeData, Nothing))

            fitGEE.setVarNames(fitVarNames, MyData.ClusterIdVarName,
                           If(MyData.bOffset, MyData.OffsetVarName, Nothing),
                           If(MyData.bWeights, MyData.WeightVarName, Nothing),
                           If(MyData.bTime, MyData.TimeVarName, Nothing))

            fitGEE.bComputeResiduals = Me.ckResiduals.Checked
            fitGEE.bIterationDetails = Me.ckIterationsDetails.Checked
            fitGEE.settingInputs(alphaValue,
                                 ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations"),
                                 ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"),
                                 Me.ckUseP.Checked)

            If bInitialValues Then fitGEE.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'validated above

            fitGEE.Fit(bInitialValues, , Me.ProgressBar1, Me.lblProgress)

            ''Dump results
            Dim WriteRes As WriteResults = New WriteResults
            WriteRes.wb = AppGlobals.app.Workbooks.Add()
            AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
            WriteRes.write({"Row ID"})
            WriteRes.setRowPointer(2)
            WriteRes.write(MyData.RowIds, bTall:=True)
            WriteRes.setRowPointer()
            WriteRes.setColumnPointer(2)
            WriteRes.write(fitVarNames)
            WriteRes.write(fitData)
            WriteRes.setRowPointer()
            WriteRes.shiftColumnPointer(fitVarNames.Length)

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
            AppGlobals.app.ActiveWorkbook.Worksheets.Add()
            AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "GEE"
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

            Dim rr = New ProcessListofResultTables(res)
            rr.writeToSheet(WriteRes, True)

            If IsCurrentBinomialGeeFamily() AndAlso Me.grpClassification.Enabled And cbPerformClasification.Checked Then
                Dim y() As Double = fitGEE.ObservedResponses()
                Dim p() As Double = fitGEE.PredictedResponses()
                Dim weights() As Double = fitGEE.ObservationWeights()
                Dim threshold As Double = GetClassificationThresholdUiValue()

                regression.BinaryClassificationReporting.ValidateBinaryInputs(y, p, weights)

                Dim summary As regression.BinaryClassificationSummary =
                    regression.BinaryClassificationReporting.ComputeBinarySummary(y, p, threshold, weights)

                Dim thresholdRows As List(Of regression.BinaryThresholdRow) = Nothing
                If Me.cbOutputTresholdTable.Checked Then
                    thresholdRows = regression.BinaryClassificationReporting.BuildThresholdTable(y, p, Nothing, weights)
                End If

                Dim calibrationRows As List(Of regression.CalibrationBinSummary) = Nothing
                If Me.cbOutputCalibrationTable.Checked Then
                    calibrationRows = regression.BinaryClassificationReporting.BuildCalibrationBins(
                        y, p, CInt(Me.spinBtnCallibrationBinsN.Value), weights, "quantile")


                End If

                Dim brier As Double = Double.NaN
                Dim eventRate As Double = Double.NaN
                If Me.cbBrierScore.Checked Then
                    brier = regression.BinaryClassificationReporting.ComputeBrierScore(y, p, weights)
                    eventRate = BESHStatNG.WorksheetFunctions.ComputeWeightedEventRate(y, weights)
                End If

                Dim clsRes As List(Of ResultTable) = regression.BinaryClassificationReporting.WrapResults(
                    summary, thresholdRows, calibrationRows, brier, eventRate, "GEE Binary Classification")

                If clsRes IsNot Nothing AndAlso clsRes.Count > 0 Then
                    WriteRes = New WriteResults
                    AppGlobals.app.ActiveWorkbook.Worksheets.Add(After:=AppGlobals.app.ActiveWorkbook.Worksheets(AppGlobals.app.ActiveWorkbook.Worksheets.Count))
                    AppGlobals.app.ActiveWorkbook.ActiveSheet.Name = "GEE Classification"
                    WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

                    Dim rrClass As New ProcessListofResultTables(clsRes)
                    rrClass.writeToSheet(WriteRes, True)

                    regression.BinaryClassificationReporting.AddRocResultsAndPlotToClassificationSheet(WriteRes, y, p, CDbl(Me.spinBtnAlpha.Value))

                    If calibrationRows IsNot Nothing Then
                        Dim cp As New graphics.CalibrationPlot(calibrationRows)
                        Dim cht = cp.addCalibrationPlot(WriteRes.ws)
                    End If
                End If
            End If

        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True, "Failed to write GEE results to the workbook")
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
        Me.EffectsController.AddMainEffectsFromSelectedVars()
    End Sub

    Private Sub btAddEffectCategoricalFactor_Click(sender As Object, e As System.EventArgs) Handles btAddEffectCategoricalFactor.Click
        Me.EffectsController.AddCategoricalEffectsFromSelectedVars()
    End Sub

    Private Sub btn2Interactions_Click(sender As Object, e As System.EventArgs) Handles btn2Interactions.Click
        Me.EffectsController.AddTwoWayInteractionsFromSelectedVars()
    End Sub

    Private Sub btnCustomInteraction_Click(sender As Object, e As System.EventArgs) Handles btnCustomInteraction.Click
        Me.EffectsController.AddCustomInteractionFromSelectedVars()
    End Sub

    Private Sub tbRemoveSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "selected", Me.TermSpecs)
    End Sub

    Private Sub btClearAllSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        AddItemsToListbox(Me.lbXs, Me.lbAllColumns, Me.lbY, Me.lbOffset, Me.lbWeights, Me.lbClusterID)
    End Sub

    Private Sub btnPoly_Click(sender As Object, e As System.EventArgs) Handles btnPoly.Click
        Me.EffectsController.AddPolynomialEffectsFromSelectedVars(CInt(Me.spinBtnPoly.Value))
    End Sub

    Private Sub cbFamily_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbFamily.SelectedIndexChanged
        RefreshLinkOptionsForSelectedFamily(regression.GetCanonicalLinkFromDisplayName(Me.cbFamily.SelectedItem.ToString()))
    End Sub

    Private Sub cbLink_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbLink.SelectedIndexChanged
        UpdatePowerLinkState()
    End Sub

    Private Sub cbOutputCalibrationTable_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbOutputCalibrationTable.CheckedChanged
        UpdateClassificationOptionsState()
    End Sub

    Private Sub cbPerformClasification_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbPerformClasification.CheckedChanged
        If Me.cbPerformClasification.Checked And IsCurrentBinomialGeeFamily() Then
            Me.grpClassification.Enabled = True
        ElseIf Not Me.cbPerformClasification.Checked And IsCurrentBinomialGeeFamily() Then
            Me.grpClassification.Enabled = False
        End If
    End Sub
End Class