Imports System.Drawing
Imports BESHStatNG.AppInfrastructure

Public Class Ui4Cox

    Private pWorksheet As Object
    Private pWorkbook As Object
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information of variable/column names inported into the input listbox
    'Ui4Cox owns the TermSpecs dictionary; the shared EffectsController mutates this same
    'instance by reference so add/remove/clear operations remain synchronized.
    Private TermSpecs As Dictionary(Of String, TermSpec)
    Private ReadOnly EffectsController As RegressionEffectsController

    Private pRegressionCalculationRunning As Boolean = False
    Private pRegressionCancelRequested As Boolean = False
    Private pRegressionInterruptRequested As Boolean = False
    Private pRegressionCloseAfterCancel As Boolean = False

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        Me.btInterrupt.Enabled = False
        Me.tbEps.Text = FormatUiDouble(0.000001)

        ' Add any initialization after the InitializeComponent() call.
        Me.TabControl1.Anchor = Windows.Forms.AnchorStyles.Left Or
                                Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right Or
                                Windows.Forms.AnchorStyles.Top
        Me.btCalculate.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right
        Me.btnHelp.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                Windows.Forms.AnchorStyles.Right
        Me.btInterrupt.Anchor = Windows.Forms.AnchorStyles.Bottom Or
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

        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)

        'Term specifications for selected effects.
        'This dictionary remains owned by Ui4Cox and is passed into the shared controller
        'so both the form and the controller operate on the same backing state.
        Me.TermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)

        'Shared effect-authoring controller for Cox regression effect construction.
        Me.EffectsController = New RegressionEffectsController(Me.lbSelectedVariables,
                                                               Me.lbSelectedEffectsList,
                                                               Me.TermSpecs)
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        BeginRegressionComputation()

        Try
            Dim bWait As Boolean = False
            Dim strWarning As String = String.Empty
            Dim bInitialValues As Boolean = (Me.tbInitValues.Text <> String.Empty)
            Dim startVals() As Double = Nothing

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

            Dim fitX(,) As Double = Nothing
            Dim fitVarNames() As String = Nothing
            Dim fitRecords As List(Of survival.SurvivalRecord) = Nothing

            BuildExpandedCoxInputs(MyData, fitX, fitVarNames, fitRecords)

            If bInitialValues Then
                If Not ValidateExpandedInitialValuesCount(fitVarNames.Length) Then
                    Exit Sub
                End If

                startVals = GetNumbersFromStrList(Me.tbInitValues.Text, False)
            End If

            'do the calculation
            Dim cox = New CoxPH(fitRecords, fitVarNames, Int(Me.spinBtnMaxIter.Value), ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"))
            If bInitialValues Then cox.startParams = startVals

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

            Dim alphaValue As Double = Me.spinBtnAlpha.Value
            Dim resu = cox.Fit(m, Me.ProgressBar1, Me.lblProgress)
            Dim res = cox.wrapResults(If(MyData.bStrata, MyData.StrataVarName, Nothing), alphaValue)

            'Dump results
            Dim WriteRes = New WriteResults
            WriteRes.wb = AppGlobals.app.Workbooks.Add()
            AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Cox Regression"
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

            Dim rr = New ProcessListofResultTables(res)
            rr.writeToSheet(WriteRes, True)

            ' baseline hazard from fitted model (matches R's survfit(coxph, newdata=...))
            Dim bh = cox.ComputeBaseline(bZeroBetas:=False)
            Dim strExpString As String = String.Empty
            With AppGlobals.app.ActiveSheet
                For i = 1 To fitVarNames.Length
                    Dim strCellAddress1 = .Cells(i + 1, 4 + 3 * bh.Keys.Count).Address(RowAbsolute:=True, ColumnAbsolute:=True) 'b
                    Dim strCellAddress2 = .Cells(i + 1, 5 + 3 * bh.Keys.Count).Address(RowAbsolute:=True, ColumnAbsolute:=True) 'predictor value
                    If i = 1 Then
                        strExpString = $"{strCellAddress1}*{strCellAddress2}"
                    Else
                        strExpString += $"+{strCellAddress1}*{strCellAddress2}" 'i.e. bi*xi
                    End If
                Next i
            End With

            AppGlobals.app.Worksheets.Add()
            AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Adjusted Curves"
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
            WriteRes.setRowPointer(1)
            WriteRes.setColumnPointer(1)

            Dim j As Integer = 0
            Dim strCellAddress(,) As Object = Nothing, Strata1Times() As Double = Nothing, Strata1SurvProb() As Double = Nothing

            For Each strId In bh.Keys
                Dim x(,) As Object = Matrix.Array2objArray(cox.BaseSurvivalForPloting(bh(strId)))
                Dim tit(,) As Object = {{$"Time_{strId}", $"Baseline Survival_{strId}", $"Baseline Cumulative Hazard_{strId}"}}
                WriteRes.write(Matrix.HorizontalStackArrays(tit, x))
                WriteRes.setRowPointer(1)
                WriteRes.shiftColumnPointer(3)

                If j = 0 Then
                    ReDim strCellAddress(UBound(x, 1), 0), Strata1Times(UBound(x, 1)), Strata1SurvProb(UBound(x, 1))
                    For i = 0 To UBound(x, 1)
                        Dim strAddr = AppGlobals.app.ActiveSheet.Cells(i + 2, 2).Address(RowAbsolute:=False, ColumnAbsolute:=False)
                        strCellAddress(i, 0) = $"={strAddr}^exp({strExpString})"

                        Strata1Times(i) = x(i, 0)
                        Strata1SurvProb(i) = x(i, 1)
                    Next
                End If

                j += 1
            Next

            'Adjusted curve
            WriteRes.shiftColumnPointer()
            WriteRes.write(Matrix.HorizontalStackArrays({{"Adjusted Survival"}}, strCellAddress))
            WriteRes.setRowPointer(1)
            WriteRes.shiftColumnPointer(1)

            AppGlobals.app.ActiveSheet.Cells(2, 5 + 3 * bh.Keys.Count).AddComment
            With AppGlobals.app.ActiveSheet.Cells(2, 5 + 3 * bh.Keys.Count).Comment
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
            t.AddHeaderLeftRow(fitVarNames)

            Dim tt(resu.Coefficients.Length - 1, 1) As Object
            For i = 0 To resu.Coefficients.Length - 1
                tt(i, 0) = resu.Coefficients(i)
            Next
            t.SetBody(tt)
            WriteRes.write(t)

            With AppGlobals.app.ActiveSheet
                .Range(.Cells(2, 5 + 3 * bh.Keys.Count), .Cells(1 + fitVarNames.Length, 5 + 3 * bh.Keys.Count)).Interior.Color = RGB(255, 255, 0)
            End With

            cox.PlotCox(WriteRes.ws, Strata1Times, Strata1SurvProb, 100, 200)

            'add new series to adjusted survival curve chart
            Dim strXChartDataAddr As String, strYChartDataAddr As String
            With AppGlobals.app.ActiveSheet
                strXChartDataAddr = .Cells(2, 1).Address(RowAbsolute:=True, ColumnAbsolute:=True)
                strYChartDataAddr = .Cells(2, 2 + 3 * bh.Keys.Count).Address(RowAbsolute:=True, ColumnAbsolute:=True)
                strXChartDataAddr += ":" + .Cells(1 + Strata1Times.Length, 1).Address(RowAbsolute:=True, ColumnAbsolute:=True)
                strYChartDataAddr += ":" + .Cells(1 + Strata1SurvProb.Length, 2 + 3 * bh.Keys.Count).Address(RowAbsolute:=True, ColumnAbsolute:=True)
            End With

            With AppGlobals.app.ActiveSheet.ChartObjects(1).Chart
                .SeriesCollection.NewSeries
                With .SeriesCollection(.SeriesCollection.count)
                    .Name = "=""Adjusted"""
                    .XValues = "='Adjusted Curves'!" & strXChartDataAddr
                    .Values = "='Adjusted Curves'!" & strYChartDataAddr
                End With
            End With

            'Residuals outputs
            AppGlobals.app.Worksheets.Add()
            AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Residuals"
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
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
            WriteRes.write(fitVarNames)
            WriteRes.write(fitX)
            WriteRes.setRowPointer(1)
            WriteRes.shiftColumnPointer(fitVarNames.Length)

            For i = 0 To residualsList.Count - 1
                WriteRes.write(residualsList(i))
                WriteRes.setRowPointer(1)
                WriteRes.shiftColumnPointer(UBound(residualsList(i), 2) + 1)
            Next

        Catch ex As System.OperationCanceledException
            FinishRegressionComputation("Calculation cancelled.")
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        Finally
            EndRegressionComputation()
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
                Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
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
        Dim MyData = New CoxPHData
        Dim keys As New List(Of String)

        keys.Add(CStr(Me.lbTime.Items(0))) 'Time
        keys.Add(CStr(Me.lbCensoring.Items(0))) 'Censoring

        'Stratum
        If Me.lbStrata.Items.Count > 0 AndAlso Me.lbStrata.Items(0) <> String.Empty Then
            MyData.bStrata = True
            keys.Add(CStr(Me.lbStrata.Items(0)))
        End If

        'Only import required RAW predictors. The selected effects list may later
        'contain derived terms, but the raw import should remain stable.
        Dim rawXKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedEffectsList.Items, Me.TermSpecs)
        For Each xKey As String In rawXKeys
            keys.Add(xKey)
        Next

        Dim ref As String = BuildExcelRefList(pWorksheet, keys, Me.VariableColumnsInfo)
        MyData.DataImport(ref)
        Return MyData
    End Function

    Private Sub BeginRegressionComputation()
        pRegressionCancelRequested = False
        pRegressionInterruptRequested = False
        pRegressionCloseAfterCancel = False
        pRegressionCalculationRunning = True
        AppGlobals.SetRegressionComputationCallbacks(AddressOf IsRegressionCancellationRequested, AddressOf IsRegressionInterruptionRequested)

        Try
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = True
            Me.lblProgress.Text = "Preparing calculation..."
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

    Private Sub EndRegressionComputation()
        pRegressionCalculationRunning = False
        AppGlobals.ClearRegressionComputationCallbacks()

        Try
            If pRegressionInterruptRequested AndAlso Not pRegressionCancelRequested Then
                Me.lblProgress.Text = "Calculation interrupted; latest accepted estimates returned."
            End If
            Me.btCalculate.Enabled = True
            Me.btInterrupt.Enabled = False
            Windows.Forms.Application.DoEvents()
        Catch
        End Try

        If pRegressionCloseAfterCancel Then
            pRegressionCloseAfterCancel = False
            Try
                Me.Close()
            Catch
            End Try
        End If
    End Sub

    Private Function IsRegressionCancellationRequested() As Boolean
        Return pRegressionCancelRequested
    End Function

    Private Function IsRegressionInterruptionRequested() As Boolean
        Return pRegressionInterruptRequested AndAlso Not pRegressionCancelRequested
    End Function

    Private Sub FinishRegressionComputation(message As String)
        Try
            Me.ProgressBar1.Style = Windows.Forms.ProgressBarStyle.Continuous
            Me.lblProgress.Text = message
            Me.btCalculate.Enabled = True
            Me.btInterrupt.Enabled = False
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Builds the expanded predictor matrix, predictor names, and survival-record list
    ''' for the current Cox regression model.
    ''' </summary>
    ''' <param name="MyData">
    ''' Raw imported Cox data containing time, censoring, optional strata, and only
    ''' required raw predictors.
    ''' </param>
    ''' <param name="fitX">
    ''' Returns the expanded predictor matrix.
    ''' </param>
    ''' <param name="fitVarNames">
    ''' Returns the expanded predictor names aligned to <paramref name="fitX"/>.
    ''' </param>
    ''' <param name="fitRecords">
    ''' Returns the survival-record list rebuilt from the expanded predictors.
    ''' </param>
    Private Sub BuildExpandedCoxInputs(MyData As CoxPHData,
                                       ByRef fitX(,) As Double,
                                       ByRef fitVarNames() As String,
                                       ByRef fitRecords As List(Of survival.SurvivalRecord))

        'Rebuild the raw predictor key list from the selected effects.
        'Do not use MyData.varNames here: after CoxPHData.DataImport() those names are
        'the imported/stripped covariate column names and may not match the UI raw keys
        'used by the selected-effects list (for example, "SEX | VarD").
        Dim rawXKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedEffectsList.Items, Me.TermSpecs)

        'Even though the Cox model does not expose an intercept term, categorical
        'predictors should still use reference-level coding rather than all-k dummies.
        RegressionDesignCore.BuildExpandedPredictorMatrix(rawX:=MyData.DataDbl,
                                                  rawXKeys:=rawXKeys,
                                                  effectItems:=Me.lbSelectedEffectsList.Items,
                                                  termSpecs:=Me.TermSpecs,
                                                  omitCategoricalReference:=True,
                                                  outX:=fitX,
                                                  outPredictorNames:=fitVarNames)

        fitRecords = BuildExpandedSurvivalRecords(MyData, fitX)
    End Sub

    ''' <summary>
    ''' Rebuilds the Cox survival-record list using an expanded predictor matrix.
    ''' </summary>
    ''' <param name="MyData">
    ''' The imported Cox data object containing time, censoring, row ids, and optional strata.
    ''' </param>
    ''' <param name="fitX">
    ''' The expanded predictor matrix aligned to the rows of <paramref name="MyData"/>.
    ''' </param>
    ''' <returns>
    ''' A new list of <see cref="survival.SurvivalRecord"/> objects whose covariates
    ''' correspond to the expanded predictor matrix.
    ''' </returns>
    Private Function BuildExpandedSurvivalRecords(MyData As CoxPHData,
                                                  fitX(,) As Double) As List(Of survival.SurvivalRecord)

        Dim out As New List(Of survival.SurvivalRecord)
        Dim p As Integer = 0

        If fitX IsNot Nothing Then
            p = UBound(fitX, 2) + 1
        End If

        For i As Integer = 0 To MyData.nRows - 1
            Dim covars() As Double = {}

            If p > 0 Then
                ReDim covars(p - 1)
                For j As Integer = 0 To p - 1
                    covars(j) = fitX(i, j)
                Next
            End If

            Dim sr As New survival.SurvivalRecord
            sr.Censorship = MyData.CensorData(i)
            sr.Stratum = If(MyData.bStrata, MyData.StrataData(i), "0")
            sr.Time = MyData.TimeData(i)
            sr.Index = MyData.RowIds(i)
            sr.Covariates = covars
            out.Add(sr)
        Next

        Return out
    End Function

    ''' <summary>
    ''' Validates the exact number of user-supplied initial values after Cox predictor expansion.
    ''' </summary>
    ''' <param name="expectedCount">
    ''' The exact number of Cox regression coefficients expected by the fitted model.
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
            Dim msg As String = $"Number of initial values does not match the number of estimated parameters for Cox regression." &
                                vbNewLine &
                                $"Expected {expectedCount}, received {vals.Length}."

            setTextBoxProperties(Me.tbInitValues, Color.Red, msg)
            MsgBox(msg, vbExclamation, "Input Error!")
            Return False
        End If

        Return True
    End Function

    Private Sub validateInputs(ByRef bWait As Boolean, ByRef strErr As String)
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

            'Exact initial-value length validation is performed after predictor expansion
            'inside btCalculate_Click(), because the final Cox parameter count depends on
            'the expanded design matrix.
        End If

        'Input variables
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
        Me.EffectsController.AddMainEffectsFromSelectedVars()
    End Sub

    Private Sub btAddEffectCategoricalFactor_Click(sender As Object, e As System.EventArgs) Handles btAddEffectCategoricalFactor.Click
        Me.EffectsController.AddCategoricalEffectsFromSelectedVars()
    End Sub

    Private Sub btnPoly_Click(sender As Object, e As System.EventArgs) Handles btnPoly.Click
        Me.EffectsController.AddPolynomialEffectsFromSelectedVars(CInt(Me.spinBtnPoly.Value))
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

    Private Sub tbInitValues_Leave(sender As Object, e As System.EventArgs) Handles tbInitValues.Leave
        Dim vals() As Double, bErr As Boolean, tiptext As String

        setTextBoxProperties(Me.tbInitValues, Color.White, String.Empty) 'give the text box its usual background
        vals = GetNumbersFromStrList(Me.tbInitValues.Text, bErr)
        If bErr Then 'Error while converting to array
            tiptext = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers without thousands separators."
            setTextBoxProperties(Me.tbInitValues, Color.Red, tiptext)
            Exit Sub
        End If

        'Exact initial-value length validation is performed after predictor expansion
        'during model fitting, because the final Cox parameter count depends on the
        'expanded design matrix.
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
            Next i
        End If
    End Sub

    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs) Handles btInterrupt.Click
        If Not pRegressionCalculationRunning Then Exit Sub

        pRegressionInterruptRequested = True

        Try
            Me.lblProgress.Text = "Interrupting; latest accepted estimates will be returned..."
            Me.btInterrupt.Enabled = False
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

    Private Sub RegressionForm_FormClosing(sender As Object, e As Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not pRegressionCalculationRunning Then Exit Sub

        pRegressionCancelRequested = True
        pRegressionCloseAfterCancel = True
        e.Cancel = True

        Try
            Me.lblProgress.Text = "Cancelling calculation..."
            Me.ProgressBar1.Style = Windows.Forms.ProgressBarStyle.Marquee
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = False
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

End Class