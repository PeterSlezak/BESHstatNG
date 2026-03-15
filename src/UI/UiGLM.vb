Option Explicit On
Imports System
Imports System.Diagnostics.Tracing
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports System.Windows.Forms.LinkLabel

Public Class UiGLM

    Private pWorksheet As Object
    Private pWorkbook As Object
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information of variable/column names inported into the input listbox
    'key = effects list item string (e.g. Age | VarA, or "Age | VarA"^2)
    Private TermSpecs As Dictionary(Of String, UIprocedures.TermSpec)

    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()

        Me.Text = analysis

        ' Add any initialization after the InitializeComponent() call.
        If Me.Text = "Generalized Linear Models" Then
            Me.TabPageLogisticModel.Parent = Nothing
            Me.TabPageOptions_LinearModel.Parent = Nothing
            Me.grpReference.Visible = False

            For Each sFam In regression.Family.FamiliesList
                Me.cbFamily.Items.Add(sFam)
            Next
            For Each sLink In regression.Link.LinkList.Values
                Me.cbLink.Items.Add(sLink)
            Next
            Me.cbFamily.SelectedIndex = 0
            Me.cbLink.SelectedIndex = 0

        ElseIf Me.Text = "Negative Binomial Regression (NB2)" Then
            Me.TabPageLogisticModel.Parent = Nothing
            Me.TabPageOptions_LinearModel.Parent = Nothing
            Me.grpReference.Visible = False

            Me.cbFamily.Items.Add("Negative Binomial")
            For Each sLink In regression.Link.PoissonLinkList.Keys
                Me.cbLink.Items.Add(regression.Link.PoissonLinkList(sLink))
            Next
            Me.cbFamily.SelectedIndex = 0
            Me.cbLink.SelectedIndex = 0

        ElseIf Me.Text = "Zero-Inflated Poisson Regression" Then
            Me.TabPageOptions_LinearModel.Parent = Nothing
            Me.grpModelSpecification.Visible = False
            Me.grpReference.Visible = False
            Me.TabPageLogisticModel.Parent = Me.TabControl1
            Me.TabPageBuildModel.Text = "Build Model - Poisson"
            Me.lblEMiterations.Enabled = True
            Me.tbEMiterations.Enabled = True

        ElseIf Me.Text = "Multinomial Logistic Regression" Or Me.Text = "Ordinal Logistic Regression" Then
            Me.TabPageLogisticModel.Parent = Nothing
            Me.TabPageOptions_LinearModel.Parent = Nothing
            Me.grpModelSpecification.Visible = False
            Me.grpReference.Visible = True
            Me.grpReference.Enabled = True
            If Me.Text = "Ordinal Logistic Regression" Then Me.ckIntercept.Visible = False

        ElseIf Me.Text = "Multiple Linear Regression (LM)" Then
            Me.TabPageLogisticModel.Parent = Nothing
            Me.TabPageOptions.Parent = Nothing
            Me.lbOffset.Visible = False
            Me.lblOffset.Visible = False
            Me.btAddOffset.Visible = False
            Me.btRemoveOffset.Visible = False
            Me.lblInitValues.Visible = False
            Me.tbInitValues.Visible = False

            Me.btnPoly.Visible = True
            Me.spinBtnPoly.Visible = True
            Me.btn2Interactions.Visible = True
            Me.btnCustomInteraction.Visible = True

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

        'Term specifications for selected effects (main, polynomial, interaction)
        Me.TermSpecs = New Dictionary(Of String, UIprocedures.TermSpec)(StringComparer.Ordinal)
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

    Private Function GetData(Optional bZip As Boolean = False) As glmData
        Dim MyData As New glmData
        Dim keys As New List(Of String)
        '--- Response variable always first ---
        Dim yKey As String = CStr(Me.lbY.Items(0))
        keys.Add(yKey)

        If bZip Then
            'ZIP Logistic tab: effects are raw vars (no poly/interaction here)
            For i As Integer = 0 To Me.lbSelectedEffectsListLogistic.Items.Count - 1
                Dim xKey As String = CStr(Me.lbSelectedEffectsListLogistic.Items(i))
                keys.Add(xKey)
            Next

        Else
            '--- Build refs only from required RAW predictors ---
            Dim rawXKeys As List(Of String) = UIprocedures.GetRequiredRawVarKeys(Me.lbSelectedEffectsList.Items, Me.TermSpecs)
            For Each xKey As String In rawXKeys
                keys.Add(xKey)
            Next

            '--- Offset (must be appended before weights so glmData parses correctly) ---
            If Me.lbOffset.Items.Count > 0 AndAlso CStr(Me.lbOffset.Items(0)) <> String.Empty Then
                MyData.bOffset = True
                Dim offKey As String = CStr(Me.lbOffset.Items(0))
                keys.Add(offKey)
            End If

            '--- Weights (must be last column in the import ref) ---
            If Me.lbWeights.Items.Count > 0 AndAlso CStr(Me.lbWeights.Items(0)) <> String.Empty Then
                MyData.bWeights = True
                Dim wKey As String = CStr(Me.lbWeights.Items(0))
                keys.Add(wKey)
            End If
        End If

        '--- Import ---
        Dim ref As String = BuildExcelRefList(pWorksheet, keys, Me.VariableColumnsInfo)
        MyData.DataInport(ref)
        Return MyData
    End Function

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
            If vals.Length <> Me.lbSelectedEffectsList.Items.Count + If(Me.ckIntercept.Checked, 1, 0) And
               (Me.Text = "Generalized Linear Models" Or Me.Text = "Negative Binomial Regression (NB2)") Then '+1 because of intercept

                strErr = "Number of initial values does not match the number of estimated parameters." & vbNewLine &
                         "Initial value for the intercept should be the first one in the list."
                setTextBoxProperties(Me.tbInitValues, Color.Red, strErr)
                bWait = True
                Exit Sub
            End If
            If vals.Length < Me.lbSelectedEffectsList.Items.Count + 2 And Me.Text = "Ordinal Logistic Regression" Then '+1 because of intercept
                'this is just a lower estimate. Exact value depends on the number of categories. Precise check will be done during the fit.

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
            strErr = "Dependent variable is missing, or independent variables and effects were not specified."
            bWait = True
            Exit Sub
        End If
        If Me.lbSelectedEffectsList.Items.Count = 0 And Not Me.ckIntercept.Checked Then
            strErr = "No Intercept and Effects were specified."
            bWait = True
            Exit Sub
        End If
        If Me.lbSelectedEffectsList.Items.Count = 0 And Me.ckIntercept.Checked Then
            If MsgBox("Do you want to fit intercept only model?", vbYesNo + vbExclamation, BESHstatGlobals.gsAPP_TITLE) = vbNo Then
                bWait = True
                Exit Sub
            End If
        ElseIf Me.lbSelectedEffectsList.Items.Count = 0 Then
            strErr = "No Effects were specified."
            bWait = True
            Exit Sub
        End If
    End Sub

    Private Sub btReload_Click(sender As Object, e As System.EventArgs) Handles btReload.Click
        Dim newSheet As Object
        Me.lbAllColumns.Items.Clear()

        If Me.cbSheetsList.SelectedIndex <> -1 Then
            If pWorksheet.name <> Me.cbSheetsList.SelectedItem.ToString() Then 'new sheet selected clear all listboxes
                Me.lbY.Items.Clear()
                Me.lbOffset.Items.Clear()
                Me.lbWeights.Items.Clear()
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

    Private Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        Try
            Dim bWait As Boolean, strWarning As String, LogisticData As glmData = Nothing, bLogisticInitialValues As Boolean = False
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
                    BESHstatGlobals.BSlogg.Log("Cannot extract initial parameter values. They will be ignored.")
                    MsgBox("Cannot extract initial parameter values. They will be ignored.")
                Else
                    bInitialValues = True
                End If
            End If

            If Me.Text = "Zero-Inflated Poisson Regression" Then
                'we need to import Logistic related data and init values
                LogisticData = GetData(True)
                If LogisticData.bZeroValid Then 'check for zero valid data
                    MsgBox("No valid observations")
                    Exit Sub
                End If

                If Me.tbInitValuesLogistic.Text <> String.Empty Then
                    Dim bErr As Boolean = False
                    Dim initVals = GetNumbersFromStrList(Me.tbInitValuesLogistic.Text, bErr)
                    If bErr Then
                        BESHstatGlobals.BSlogg.Log("Cannot extract initial parameter values. They will be ignored.")
                        MsgBox("Cannot extract initial parameter values. They will be ignored.")
                    Else
                        bLogisticInitialValues = True
                    End If
                End If

                'Get Common Poisson and Logistic model records only
                Dim commonRows() As Integer = LogisticData.RowIds.Intersect(MyData.RowIds).ToArray()
                LogisticData.SubsetByRowIdValues(CommonItems(LogisticData.RowIds, commonRows))
                MyData.SubsetByRowIdValues(CommonItems(MyData.RowIds, commonRows))
            End If

            If Me.Text = "Generalized Linear Models" Then
                Me.RunGLM(MyData, bInitialValues)
            ElseIf Me.Text = "Negative Binomial Regression (NB2)" Then
                Me.RunGLMNB2(MyData, bInitialValues)
            ElseIf Me.Text = "Zero-Inflated Poisson Regression" Then
                Me.RunZIP(MyData, bInitialValues, LogisticData, bLogisticInitialValues)
            ElseIf Me.Text = "Multinomial Logistic Regression" Then
                Me.RunMultiLogit(MyData, bInitialValues)
            ElseIf Me.Text = "Ordinal Logistic Regression" Then
                Me.RunOrdLogit(MyData, bInitialValues)
            ElseIf Me.Text = "Multiple Linear Regression (LM)" Then
                Me.RunOLS(MyData, bInitialValues)
            End If
        Catch ex As Exception
            BESHstatGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunOLS(MyData As glmData, bInitialValues As Boolean)
        Dim lm As New regression.LinearModel()
        Dim expanded(,) As Double = Nothing
        Dim expandedNames() As String = Nothing
        UIprocedures.BuildExpandedLmDataMatrix(raw:=MyData, yKey:=CStr(Me.lbY.Items(0)),
                                               effectItems:=Me.lbSelectedEffectsList.Items, termSpecs:=Me.TermSpecs,
                                               outData:=expanded, outVarNames:=expandedNames)
        lm.Data(expanded, expandedNames, MyData.RowIds, If(MyData.bWeights, MyData.WeightData, Nothing))
        ' lm.Data(MyData.DataDbl, MyData.varNames, MyData.RowIds, If(MyData.bWeights, MyData.WeightData, Nothing))

        lm.bReturnCov = Me.ckCovarMatrixLM.Checked
        lm.bComputeResiduals = Me.ckResidualsLM.Checked
        Dim ss As regression.TermSumOfSquaresType
        If Me.optTypeISS.Checked Then
            ss = regression.TermSumOfSquaresType.TypeI
        ElseIf Me.optTypeIIISS.checked Then
            ss = regression.TermSumOfSquaresType.TypeIII
        End If
        Dim customGroups As Dictionary(Of String, Integer()) = UIprocedures.BuildCustomTermGroupsForLm(Me.lbSelectedEffectsList.Items, Me.TermSpecs, Me.ckIntercept.Checked)
        lm.Fit(Me.ckIntercept.Checked, customGroups, ss)   'lm.Fit(Me.ckIntercept.Checked,, ss)

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = BESHstatGlobals.app.Workbooks.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(2)
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
        'WriteRes.write(MyData.varNames)
        'WriteRes.write(MyData.FinalData)
        'WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.write(expandedNames)
        WriteRes.write(expanded)
        WriteRes.shiftColumnPointer(expandedNames.Length)
        WriteRes.setRowPointer()

        'Weights
        If MyData.bWeights Then
            WriteRes.write({MyData.WeightVarName})
            WriteRes.setRowPointer(2)
            WriteRes.write(MyData.WeightData, bTall:=True)
            WriteRes.setRowPointer()
            WriteRes.shiftColumnPointer(1)
        End If

        'Residuals
        If lm.bComputeResiduals Then
            WriteRes.write(lm.AllResiduals_toPrint)
        End If

        'Create new worksheet in workbook. It will automaticaly be an activesheet
        'We need to start new writer to start writing on this new sheet
        Dim res = lm.wrapResults()
        WriteRes = New WriteResults
        BESHstatGlobals.app.ActiveWorkbook.Worksheets.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "LM"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

    End Sub

    Private Sub RunOrdLogit(MyData As glmData, bInitialValues As Boolean)
        Dim ordL = New regression.OrdinalLogitModel()
        ordL.Data(MyData.DataDbl, MyData.varNames, MyData.RowIds,
                   If(MyData.bOffset, MyData.OffsetData, Nothing),
                   If(MyData.bWeights, MyData.WeightData, Nothing))
        ordL.bReturnCov = Me.ckCovarMatrix.Checked
        ordL.bComputeResiduals = Me.ckResiduals.Checked
        ordL.bIterationDetails = Me.ckIterationsDetails.Checked
        ordL.SettingInputs(0.05, CInt(Me.tbMaxIter.Text), CDbl(Me.tbEps.Text))
        If bInitialValues Then ordL.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'we tested already that they are correct
        Dim refCat = If(Me.optFirst.Checked, regression.ReferenceCategory.First, regression.ReferenceCategory.Last)
        ordL.Fit(refCat, bInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = BESHstatGlobals.app.Workbooks.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(2)
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
        WriteRes.write(MyData.varNames)
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()

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

        'Residuals
        If ordL.bComputeResiduals Then
            WriteRes.write(ordL.wrapResiduals())
        End If

        'Create new worksheet in workbook. It will automaticaly be an activesheet
        'We need to start new writer to start writing on this new sheet
        Dim res = ordL.wrapResults(If(MyData.bOffset, MyData.OffsetVarName, Nothing),
                                     If(MyData.bWeights, MyData.WeightVarName, Nothing))
        WriteRes = New WriteResults
        BESHstatGlobals.app.ActiveWorkbook.Worksheets.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Ordinal_LR"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunMultiLogit(MyData As glmData, bInitialValues As Boolean)
        Dim multL = New regression.MultinomialLogitModel()
        multL.data(MyData.DataDbl, MyData.varNames, MyData.RowIds,
                   If(MyData.bOffset, MyData.OffsetData, Nothing),
                   If(MyData.bWeights, MyData.WeightData, Nothing))
        multL.bReturnCov = Me.ckCovarMatrix.Checked
        multL.bComputeResiduals = Me.ckResiduals.Checked
        multL.bIterationDetails = Me.ckIterationsDetails.Checked
        multL.settingInputs(0.05, CInt(Me.tbMaxIter.Text), CDbl(Me.tbEps.Text))
        Dim lIntercept As Integer = If(Me.ckIntercept.Checked, 1, 0)
        If bInitialValues Then multL.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'we tested already that they are correct
        Dim refCat = If(Me.optFirst.Checked, regression.ReferenceCategory.First, regression.ReferenceCategory.Last)
        multL.Fit(lIntercept, refCat, bInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = BESHstatGlobals.app.Workbooks.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(2)
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
        WriteRes.write(MyData.varNames)
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()

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

        'Residuals
        If multL.bComputeResiduals Then
            WriteRes.write(multL.wrapResiduals())
        End If

        'Create new worksheet in workbook. It will automaticaly be an activesheet
        'We need to start new writer to start writing on this new sheet
        Dim res = multL.wrapResults(If(MyData.bOffset, MyData.OffsetVarName, Nothing),
                                     If(MyData.bWeights, MyData.WeightVarName, Nothing))
        WriteRes = New WriteResults
        BESHstatGlobals.app.ActiveWorkbook.Worksheets.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Multinomial_LR"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunZIP(PoissonData As glmData, bPoissonInitialValues As Boolean,
                       LogisticData As glmData, bLogisticInitialValues As Boolean)
        Dim zipFit = New ZeroInflatedPoisson
        zipFit.dataInputs(PoissonData.DataDbl, LogisticData.DataDbl, PoissonData.varNames, LogisticData.varNames, PoissonData.RowIds)
        zipFit.bComputeResiduals = Me.ckResiduals.Checked
        zipFit.bIterationDetails = Me.ckIterationsDetails.Checked
        zipFit.bReturnCov = Me.ckCovarMatrix.Checked
        zipFit.settingInputs(0.05, CInt(Me.tbMaxIter.Text), CInt(Me.tbEMiterations.Text), CDbl(Me.tbEps.Text))
        If bPoissonInitialValues Then zipFit.startParamsPois = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'we tested already that they are correct
        If bLogisticInitialValues Then zipFit.startParamsLog = GetNumbersFromStrList(Me.tbInitValuesLogistic.Text, False) 'we tested already that they are correct
        zipFit.Fit(If(Me.ckIntercept.Checked, 1, 0), If(Me.ckInterceptLogistic.Checked, 1, 0),
                         bPoissonInitialValues, bLogisticInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = BESHstatGlobals.app.Workbooks.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Poisson"})
        WriteRes.write({"Row ID"})
        WriteRes.write(PoissonData.RowIds, bTall:=True)
        WriteRes.setRowPointer(2)
        WriteRes.setColumnPointer(2)
        WriteRes.write(PoissonData.varNames)
        WriteRes.write(PoissonData.FinalData)
        WriteRes.shiftColumnPointer(PoissonData.varNames.Length)
        WriteRes.setRowPointer(1)
        WriteRes.write({"Logistic"})
        WriteRes.write(LogisticData.varNames)
        WriteRes.write(LogisticData.FinalData)
        WriteRes.shiftColumnPointer(LogisticData.varNames.Length)
        WriteRes.setRowPointer(2)
        'Offset
        If PoissonData.bOffset Then
            WriteRes.write({PoissonData.OffsetVarName})
            WriteRes.setRowPointer(2)
            WriteRes.write(PoissonData.OffsetData, bTall:=True)
            WriteRes.setRowPointer(2)
            WriteRes.shiftColumnPointer(1)
        End If
        'Prediction
        WriteRes.write({"Prediction"})
        WriteRes.write(zipFit.Predicted, bTall:=True)
        WriteRes.setRowPointer(2)
        WriteRes.shiftColumnPointer(1)
        'Residuals
        If zipFit.bComputeResiduals Then
            WriteRes.write(zipFit.AllResiduals())
        End If

        'Create new worksheet in workbook. It will automaticaly be an activesheet
        'We need to start new writer to start writing on this new sheet
        Dim res = zipFit.wrapResults(If(PoissonData.bOffset, PoissonData.OffsetVarName, Nothing),
                                     If(PoissonData.bWeights, PoissonData.WeightVarName, Nothing))
        WriteRes = New WriteResults
        BESHstatGlobals.app.ActiveWorkbook.Worksheets.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Zero-Inflated Poisson"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunGLMNB2(MyData As glmData, bInitialValues As Boolean)
        Dim lnk As regression.Link
        If Me.cbLink.SelectedItem = "Power" Then
            lnk = regression.createLink(Me.cbLink.SelectedItem, CDbl(Me.tbPower.Text))
        Else
            lnk = regression.createLink(Me.cbLink.SelectedItem)
        End If

        Dim nb2 = New GLM_NB(lnk)
        nb2.data(MyData.DataDbl, MyData.RowIds,
                 If(MyData.bOffset, MyData.OffsetData, Nothing),
                 If(MyData.bWeights, MyData.WeightData, Nothing))
        nb2.setVarNames(MyData.varNames)
        nb2.bReturnCov = Me.ckCovarMatrix.Checked
        nb2.bComputeResiduals = Me.ckResiduals.Checked
        nb2.bIterationDetails = Me.ckIterationsDetails.Checked
        nb2.settingInputs(0.05, CInt(Me.tbMaxIter.Text), CDbl(Me.tbEps.Text))
        Dim lIntercept As Integer = If(Me.ckIntercept.Checked, 1, 0)
        If bInitialValues Then nb2.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'we tested already that they are correct
        nb2.Fit(lIntercept, bInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = BESHstatGlobals.app.Workbooks.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(2)
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
        WriteRes.write(MyData.varNames)
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(UBound(MyData.FinalData, 2) + 1)
        WriteRes.setRowPointer()

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

        'Prediction
        WriteRes.write({"Prediction"})
        WriteRes.setRowPointer(2)
        WriteRes.write(nb2.PredictedResponses, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.shiftColumnPointer(1)

        'Residuals
        If nb2.bComputeResiduals Then
            WriteRes.write(nb2.AllResiduals())
        End If

        'Create new worksheet in workbook. It will automaticaly be an activesheet
        'We need to start new writer to start writing on this new sheet
        Dim res = nb2.wrapResults(If(MyData.bOffset, MyData.OffsetVarName, Nothing),
                                  If(MyData.bWeights, MyData.WeightVarName, Nothing))
        WriteRes = New WriteResults
        BESHstatGlobals.app.ActiveWorkbook.Worksheets.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "GLM NB2"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunGLM(MyData As glmData, bInitialValues As Boolean)
        Dim fitGlm As GLM
        Dim fam = regression.createFamily(regression.Family.FamiliesCodes(Me.cbFamily.SelectedIndex))
        If Me.tbDispersionParameterNB2.Text <> String.Empty Then
            Try
                Dim dispParam As Double = CDbl(Me.tbDispersionParameterNB2.Text)
                If dispParam > 0 Then fam.pdAlpha = dispParam
            Catch
            End Try
        End If
        Dim lnk As regression.Link
        If Me.cbLink.SelectedItem = "Power" Then
            lnk = regression.createLink(Me.cbLink.SelectedItem, CDbl(Me.tbPower.Text))
        Else
            lnk = regression.createLink(Me.cbLink.SelectedItem)
        End If
        fitGlm = New GLM(fam, lnk)
        fitGlm.data(MyData.DataDbl, MyData.RowIds,
                        If(MyData.bOffset, MyData.OffsetData, Nothing),
                        If(MyData.bWeights, MyData.WeightData, Nothing))
        fitGlm.setVarNames(MyData.varNames)
        fitGlm.bReturnCov = Me.ckCovarMatrix.Checked
        fitGlm.bComputeResiduals = Me.ckResiduals.Checked
        fitGlm.bIterationDetails = Me.ckIterationsDetails.Checked
        fitGlm.settingInputs(0.05, CInt(Me.tbMaxIter.Text), CDbl(Me.tbEps.Text))
        Dim lIntercept As Integer = If(Me.ckIntercept.Checked, 1, 0)
        If bInitialValues Then fitGlm.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'we tested already that they are correct
        fitGlm.Fit(lIntercept, bInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = BESHstatGlobals.app.Workbooks.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet
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
            WriteRes.setRowPointer()
            WriteRes.write({MyData.OffsetVarName})
            WriteRes.setRowPointer(2)
            WriteRes.write(MyData.OffsetData, bTall:=True)
            WriteRes.setRowPointer()
            WriteRes.shiftColumnPointer(1)
        End If

        'Weights
        If MyData.bWeights Then
            WriteRes.setRowPointer()
            WriteRes.write({MyData.WeightVarName})
            WriteRes.setRowPointer(2)
            WriteRes.write(MyData.WeightData, bTall:=True)
            WriteRes.setRowPointer()
            WriteRes.shiftColumnPointer(1)
        End If

        'Prediction
        WriteRes.setRowPointer()
        WriteRes.write({"Prediction"})
        WriteRes.setRowPointer(2)
        WriteRes.write(fitGlm.PredictedResponses, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.shiftColumnPointer(1)

        'Residuals
        If fitGlm.bComputeResiduals Then
            WriteRes.write(fitGlm.AllResiduals())
        End If

        'Create new worksheet in workbook. It will automaticaly be an activesheet
        'We need to start new writer to start writing on this new sheet
        Dim res = fitGlm.wrapResults(If(MyData.bOffset, MyData.OffsetVarName, Nothing),
                                          If(MyData.bWeights, MyData.WeightVarName, Nothing))
        WriteRes = New WriteResults
        BESHstatGlobals.app.ActiveWorkbook.Worksheets.Add()
        BESHstatGlobals.app.ActiveWorkbook.ActiveSheet.name = "GLM"
        WriteRes.ws = BESHstatGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
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
                Next i
                If Not IsSubsetListBox(Me.lbSelectedVariables, Me.lbSelectedEffectsList, bOnlyMain:=True) Then
                    If MsgBox("There is a variable in selected effects list that was removed from the predictor variable(s) list." & vbNewLine & vbNewLine &
                              "Clear selected effects list?", vbYesNo + vbExclamation, "Clear selected effects list?") = vbYes Then
                        'Selected item was removed from X vars
                        If Me.lbSelectedEffectsList.Items.Count > 0 Then Remove_Item(Me.lbSelectedEffectsList)
                    End If
                End If
            End If
            If Me.Text = "Zero-Inflated Poisson Regression" Then
                If Not IsEqualListBox(Me.lbXs, Me.lbSelectedVariablesLogistic) Then
                    'values on 1st tab changed so refresh it with new values
                    If Me.lbSelectedVariablesLogistic.Items.Count > 0 Then Remove_Item(Me.lbSelectedVariablesLogistic)
                    For i = 0 To Me.lbXs.Items.Count - 1
                        Me.lbSelectedVariablesLogistic.Items.Add(Me.lbXs.Items(i))
                    Next i
                    If Not IsSubsetListBox(Me.lbSelectedVariablesLogistic, Me.lbSelectedEffectsListLogistic) Then
                        If MsgBox("There is a variable in selected effects list that was removed from the predictor variable(s) list." & vbNewLine & vbNewLine &
                              "Clear selected effects list?", vbYesNo + vbExclamation, "Clear selected effects list?") = vbYes Then
                            'Selected item was removed from X vars
                            'TODO: this need to be updated when start using poly and interaction effects
                            If Me.lbSelectedEffectsList.Items.Count > 0 Then Remove_Item(Me.lbSelectedEffectsList)
                        End If
                    End If
                End If
            End If
        Else 'load actual Xvars list for the 1st time
            For i = 0 To Me.lbXs.Items.Count - 1
                Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
                If Me.Text = "Zero-Inflated Poisson Regression" Then Me.lbSelectedVariablesLogistic.Items.Add(Me.lbXs.Items(i))
            Next
        End If
    End Sub

    Private Sub btAddY_Click(sender As Object, e As System.EventArgs) Handles btAddY.Click
        AddItemToListbox(Me.lbY, Me.lbAllColumns, Me.lbXs, Me.lbOffset, Me.lbWeights)
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

    Private Sub btRemoveX_Click(sender As Object, e As System.EventArgs) Handles btRemoveX.Click
        Remove_Item(Me.lbXs, "selected")
    End Sub

    Private Sub btAddOffset_Click(sender As Object, e As System.EventArgs) Handles btAddOffset.Click
        AddItemToListbox(Me.lbOffset, Me.lbAllColumns, Me.lbXs, Me.lbY, Me.lbWeights)
    End Sub

    Private Sub btAddWeights_Click(sender As Object, e As System.EventArgs) Handles btAddWeights.Click
        AddItemToListbox(Me.lbWeights, Me.lbAllColumns, Me.lbXs, Me.lbY, Me.lbOffset)
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        AddItemsToListbox(Me.lbXs, Me.lbAllColumns, Me.lbY, Me.lbOffset, Me.lbWeights)
    End Sub

    Private Sub btClearAllSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedEffects.Click
        Me.lbSelectedEffectsList.Items.Clear()
        'Keep TermSpecs in sync when clearing/removing effects
        If Me.TermSpecs IsNot Nothing Then Me.TermSpecs.Clear()
    End Sub

    Private Sub tbRemoveSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffects.Click
        Dim removed As New List(Of String)
        For Each it As Object In Me.lbSelectedEffectsList.SelectedItems
            removed.Add(CStr(it))
        Next

        Remove_Item(Me.lbSelectedEffectsList, "selected")

        'Keep TermSpecs in sync when clearing/removing effects
        If Me.TermSpecs IsNot Nothing Then
            For Each k As String In removed
                If Me.TermSpecs.ContainsKey(k) Then Me.TermSpecs.Remove(k)
            Next
        End If

        UIprocedures.RefreshTermSpecOrders(Me.lbSelectedEffectsList.Items, Me.TermSpecs)
    End Sub

    Private Sub btAddEffect_Click(sender As Object, e As System.EventArgs) Handles btAddEffect.Click
        'AddItemsToListbox(Me.lbSelectedEffectsList, Me.lbSelectedVariables, Me.lbY, Me.lbOffset, Me.lbWeights)
        AddMainEffectsFromSelectedVars()
    End Sub

    Private Sub btAddEffectLogistic_Click(sender As Object, e As System.EventArgs) Handles btAddEffectLogistic.Click
        AddItemsToListbox(Me.lbSelectedEffectsListLogistic, Me.lbSelectedVariablesLogistic, Me.lbY, Me.lbOffset, Me.lbWeights)
    End Sub

    Private Sub tbRemoveSelectedEffectsLogistic_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffectsLogistic.Click
        Remove_Item(Me.lbSelectedEffectsListLogistic, "selected")
    End Sub

    Private Sub btClearAllSelectedEffectsLogistic_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedEffectsLogistic.Click
        Me.lbSelectedEffectsListLogistic.Items.Clear()
    End Sub

    Private Sub btnPoly_Click(sender As Object, e As System.EventArgs) Handles btnPoly.Click
        AddPolynomialEffectsFromSelectedVars(CInt(Me.spinBtnPoly.Value))
    End Sub

    Private Sub btn2Interactions_Click(sender As Object, e As System.EventArgs) Handles btn2Interactions.Click
        AddTwoWayInteractionsFromSelectedVars()
    End Sub

    Private Sub btnCustomInteraction_Click(sender As Object, e As System.EventArgs) Handles btnCustomInteraction.Click
        AddCustomInteractionFromSelectedVars()
    End Sub

    Private Sub AddMainEffectsFromSelectedVars()
        If Me.lbSelectedVariables.SelectedItems.Count = 0 Then
            MsgBox("Please select variable(s).", vbExclamation, "Input Error!")
            Exit Sub
        End If

        If Me.TermSpecs Is Nothing Then
            Me.TermSpecs = New Dictionary(Of String, UIprocedures.TermSpec)(StringComparer.Ordinal)
        End If

        For Each it As Object In Me.lbSelectedVariables.SelectedItems
            Dim baseKey As String = CStr(it)

            'Skip duplicates in the effects list
            If Me.lbSelectedEffectsList.Items.Contains(baseKey) Then Continue For

            Me.lbSelectedEffectsList.Items.Add(baseKey)

            Dim spec As New UIprocedures.TermSpec With {
                    .Kind = "MainEffect",
                    .BaseVarKeys = New List(Of String) From {baseKey},
                    .Degree = 1,
                    .DisplayNameForCoef = UIprocedures.GetCoefBaseName(baseKey),
                    .Order = Me.lbSelectedEffectsList.Items.Count - 1}
            Me.TermSpecs(baseKey) = spec
        Next
    End Sub

    Private Sub AddPolynomialEffectsFromSelectedVars(degree As Integer)
        If degree < 2 Then
            MsgBox("Polynomial degree must be >= 2.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        If Me.lbSelectedVariables.SelectedItems.Count = 0 Then
            MsgBox("Please select variable(s).", vbExclamation, "Input Error!")
            Exit Sub
        End If

        If Me.TermSpecs Is Nothing Then
            Me.TermSpecs = New Dictionary(Of String, UIprocedures.TermSpec)(StringComparer.Ordinal)
        End If

        For Each it As Object In Me.lbSelectedVariables.SelectedItems
            Dim baseKey As String = CStr(it)

            'Listbox display term:  "Age | VarA"^2   (quotes included)
            Dim termKey As String = UIprocedures.MakePolynomialEffectKey(baseKey, degree)

            'Skip duplicates in the effects list
            If Me.lbSelectedEffectsList.Items.Contains(termKey) Then Continue For

            Me.lbSelectedEffectsList.Items.Add(termKey)

            Dim spec As New UIprocedures.TermSpec With {
                    .Kind = "Polynomial",
                    .BaseVarKeys = New List(Of String) From {baseKey},
                    .Degree = degree,
                    .DisplayNameForCoef = UIprocedures.GetCoefBaseName(baseKey) & "^" & CStr(degree),
                    .Order = Me.lbSelectedEffectsList.Items.Count - 1}
            Me.TermSpecs(termKey) = spec
        Next
    End Sub

    Private Sub AddTwoWayInteractionsFromSelectedVars()
        If Me.lbSelectedVariables.SelectedItems.Count < 2 Then
            MsgBox("Please select at least two variables.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        If Me.TermSpecs Is Nothing Then
            Me.TermSpecs = New Dictionary(Of String, UIprocedures.TermSpec)(StringComparer.Ordinal)
        End If

        'Use SelectedIndices sorted -> deterministic order based on listbox order
        Dim idxs As New List(Of Integer)
        For Each ix As Integer In Me.lbSelectedVariables.SelectedIndices
            idxs.Add(ix)
        Next
        idxs.Sort()

        For a As Integer = 0 To idxs.Count - 2
            Dim v1 As String = CStr(Me.lbSelectedVariables.Items(idxs(a)))

            For b As Integer = a + 1 To idxs.Count - 1
                Dim v2 As String = CStr(Me.lbSelectedVariables.Items(idxs(b)))

                'Listbox display: "Var1":"Var2"
                Dim termKey As String = UIprocedures.MakeInteractionEffectKey(New List(Of String) From {v1, v2})

                'Skip duplicates
                If Me.lbSelectedEffectsList.Items.Contains(termKey) Then Continue For

                Me.lbSelectedEffectsList.Items.Add(termKey)

                Dim spec As New UIprocedures.TermSpec With {
                        .Kind = "Interaction",
                        .BaseVarKeys = New List(Of String) From {v1, v2},
                        .Degree = 1,
                        .DisplayNameForCoef = UIprocedures.MakeInteractionCoefName(New List(Of String) From {v1, v2}),
                        .Order = Me.lbSelectedEffectsList.Items.Count - 1}

                Me.TermSpecs(termKey) = spec
            Next
        Next
    End Sub

    Private Sub AddCustomInteractionFromSelectedVars()
        If Me.lbSelectedVariables.SelectedItems.Count < 2 Then
            MsgBox("Please select at least two variables.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        If Me.TermSpecs Is Nothing Then
            Me.TermSpecs = New Dictionary(Of String, UIprocedures.TermSpec)(StringComparer.Ordinal)
        End If

        'Deterministic order: by listbox order (not click order)
        Dim idxs As New List(Of Integer)
        For Each ix As Integer In Me.lbSelectedVariables.SelectedIndices
            idxs.Add(ix)
        Next
        idxs.Sort()

        Dim baseKeys As New List(Of String)
        For Each ix As Integer In idxs
            Dim v As String = CStr(Me.lbSelectedVariables.Items(ix))
            baseKeys.Add(v)
        Next

        'Listbox display: "Var1":"Var2":"Var3"... (quotes around each var)
        Dim termKey As String = UIprocedures.MakeInteractionEffectKey(baseKeys)

        'Skip duplicates
        If Me.lbSelectedEffectsList.Items.Contains(termKey) Then Exit Sub

        Me.lbSelectedEffectsList.Items.Add(termKey)

        Dim spec As New UIprocedures.TermSpec With {
                .Kind = "Interaction",
                .BaseVarKeys = baseKeys,
                .Degree = 1,
                .DisplayNameForCoef = UIprocedures.MakeInteractionCoefName(baseKeys),
                .Order = Me.lbSelectedEffectsList.Items.Count - 1}

        Me.TermSpecs(termKey) = spec
    End Sub

End Class