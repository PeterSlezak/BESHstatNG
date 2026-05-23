Option Explicit On

Imports BESHStatNG.AppInfrastructure
Imports System.Threading.Tasks

Public Class Ui18MMRM
    Private pWorksheet As Object
    Private pWorkbook As Object
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information of variable/column names inported into the input listbox
    Private TermSpecs As Dictionary(Of String, TermSpec)
    Private ReadOnly EffectsController As RegressionEffectsController
    Private pMmrmCalculationRunning As Boolean = False
    Private pMmrmCancelRequested As Boolean = False
    Private pMmrmInterruptRequested As Boolean = False
    Private pMmrmCloseAfterCancel As Boolean = False
    Private pMmrmProgressStopwatch As System.Diagnostics.Stopwatch = Nothing
    Private pMmrmProgressRefreshActive As Boolean = False
    Private pLastMmrmProgressRefreshTimestamp As Long = 0

    Private Const MMRM_PROGRESS_REFRESH_INTERVAL_MS As Double = 100.0
    Private Const MMRM_GROUP_AUTO As String = "(Auto)"
    Private Const MMRM_GROUP_NONE As String = "(None)"
    Private Const MMRM_BASELINE_SMALLEST As String = "(Smallest)"
    Private Const MMRM_CONTROL_FIRST As String = "(First)"
    Private Const MMRM_MODE_NONE As String = "None"
    Private Const MMRM_MODE_PAIRWISE As String = "Pairwise among group levels"
    Private Const MMRM_MODE_CONTROL As String = "Each group vs control"
    Private Const MMRM_MODE_SELECTED As String = "Selected comparison only"
    Private Const MMRM_DIR_HIGHER_MINUS_LOWER As String = "Higher level - lower level"
    Private Const MMRM_DIR_TREATMENT_MINUS_CONTROL As String = "Treatment - control"
    Private Const MMRM_DIR_CONTROL_MINUS_TREATMENT As String = "Control - treatment"
    Private Const MMRM_LSMEANS_OBSERVED_GRID As String = "Observed design grid"
    Private Const MMRM_LSMEANS_REFERENCE_GRID As String = "Reference grid"
    Private Const MMRM_RG_WEIGHT_EQUAL As String = "Equal class-cell weights"
    Private Const MMRM_RG_WEIGHT_OBSERVED As String = "Observed class-cell weights"
    Private Const MMRM_RG_COVARIATE_MEANS As String = "Continuous covariates at observed means"
    Private Const MMRM_RG_COVARIATE_ZERO As String = "Continuous covariates at 0"
    Private Const MMRM_MULT_NONE As String = "None"
    Private Const MMRM_MULT_BONFERRONI As String = "Bonferroni"
    Private Const MMRM_MULT_HOLM As String = "Holm"
    Private Const MMRM_MULT_SIDAK As String = "Sidak"
    Private Const MMRM_OPT_AI As String = "AI/Fisher scoring (default)"
    Private Const MMRM_OPT_BFGS_AUTO As String = "Projected BFGS (auto gradient)"
    Private Const MMRM_OPT_BFGS_ANALYTIC As String = "Projected BFGS (analytic gradient)"
    Private Const MMRM_OPT_BFGS_NUMERICAL As String = "Projected BFGS (finite-difference gradient)"
    Private Const MMRM_GRAD_AUTO As String = "Auto (analytic where available)"
    Private Const MMRM_GRAD_ANALYTIC As String = "Analytic score"
    Private Const MMRM_GRAD_VALIDATE As String = "Analytic score + finite-difference validation"
    Private Const MMRM_GRAD_NUMERICAL As String = "Numerical finite difference"

    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()
        Me.tbEps.Text = FormatUiDouble(0.000001)
        Me.Text = analysis
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)
        InitializeMMRMControls()
        Me.btInterrupt.Enabled = False

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
        Me.lbSelectedEffectsList.Anchor = Windows.Forms.AnchorStyles.Left Or
                                          Windows.Forms.AnchorStyles.Right Or
                                          Windows.Forms.AnchorStyles.Top Or
                                          Windows.Forms.AnchorStyles.Bottom
        Me.tbRemoveSelectedEffects.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                            Windows.Forms.AnchorStyles.Right
        Me.btClearAllSelectedEffects.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                            Windows.Forms.AnchorStyles.Right

        'Term specifications for selected effects.
        'This dictionary remains owned by Ui18MMRM and is passed into the shared controller
        'so both the form and the controller operate on the same backing state.
        Me.TermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)

        'Shared effect-authoring controller for model construction.
        Me.EffectsController = New RegressionEffectsController(Me.lbSelectedVariables, Me.lbSelectedEffectsList, Me.TermSpecs)

        Me.WireHelp(Me.btnHelp)
    End Sub

    Sub Populate(ws As Object)
        Dim VarRng As Object, ws_temp As Object
        pWorksheet = ws
        pWorkbook = ws.parent
        Dim FinalCol = LastColumnInSheet(ws)
        Dim MaxRows = MaxRowsInSheet(ws)
        VarRng = ws.Range(ws.Cells(1, 1), ws.Cells(1, FinalCol)) 'Create range object to contain variable names
        Me.VariableColumnsInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns, bNumeric_only:=False) 'Cycle through the range and add all non-empty variable names to the listbox

        'We may call this method multiple times so populate sheet combo box only once
        Me.cbSheetsList.Items.Clear()
        For Each ws_temp In pWorkbook.worksheets
            Me.cbSheetsList.Items.Add(ws_temp.name)
        Next
        Me.cbSheetsList.SelectedIndex = Me.cbSheetsList.FindStringExact(Me.pWorkbook.activesheet.name)
    End Sub

    Private Sub ValidateInputs(ByRef bWait As Boolean, ByRef strWarning As String)

        bWait = False
        strWarning = String.Empty

        If Me.lbY.Items.Count <> 1 Then
            strWarning = "Please select exactly one continuous response variable."
            bWait = True
            Exit Sub
        End If

        If Me.lbClusterID.Items.Count <> 1 Then
            strWarning = "Please select a Subject ID variable."
            bWait = True
            Exit Sub
        End If

        If Me.lbTime.Items.Count <> 1 Then
            strWarning = "Please select a Visit / Time variable for MMRM."
            bWait = True
            Exit Sub
        End If

        If Not TryRequireNumericColumn(CStr(Me.lbY.Items(0)), "Response variable", strWarning) Then
            bWait = True
            Exit Sub
        End If

        If Not TryRequireNumericColumn(CStr(Me.lbTime.Items(0)), "Visit / Time variable", strWarning) Then
            bWait = True
            Exit Sub
        End If

        Dim rawFixedKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedEffectsList.Items, Me.TermSpecs)
        For Each rawKey As String In rawFixedKeys
            If Not TryRequireNumericColumn(rawKey, "Fixed-effect variable", strWarning) Then
                bWait = True
                Exit Sub
            End If
        Next

        If Me.lbSelectedEffectsList.Items.Count = 0 AndAlso Not Me.cbIntercept.Checked Then
            strWarning = "No fixed effects were specified and the intercept is disabled."
            bWait = True
            Exit Sub
        End If

        If Me.cbCovarStruct.SelectedItem Is Nothing Then
            strWarning = "Please select a residual covariance structure."
            bWait = True
            Exit Sub
        End If

        If Me.cbFitMethod.SelectedItem Is Nothing Then
            strWarning = "Please select ML or REML."
            bWait = True
            Exit Sub
        End If

        If Me.cbMMRMCovOptimizerMode IsNot Nothing AndAlso Me.cbMMRMCovOptimizerMode.SelectedItem Is Nothing Then
            strWarning = "Please select a covariance optimizer mode."
            bWait = True
            Exit Sub
        End If

        If Me.cbMMRMCovGradientMode IsNot Nothing AndAlso Me.cbMMRMCovGradientMode.SelectedItem Is Nothing Then
            strWarning = "Please select a covariance gradient mode."
            bWait = True
            Exit Sub
        End If

        Dim eps As Double = ParseUiDouble(Me.tbEps.Text, "Convergence epsilon")
        If eps <= 0 Then
            strWarning = "Convergence epsilon must be positive."
            bWait = True
            Exit Sub
        End If

        Dim maxIter As Integer = ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations")
        If maxIter <= 0 Then
            strWarning = "Maximum iterations must be positive."
            bWait = True
            Exit Sub
        End If

    End Sub

    Private Function TryRequireNumericColumn(variableKey As String, roleDescription As String, ByRef warning As String) As Boolean
        If IsAvailableColumnNumeric(variableKey) Then Return True

        warning = roleDescription & " must be numeric. The selected column '" &
                  If(variableKey, String.Empty) &
                  "' contains no numeric observations in the current worksheet scan. Character columns are allowed only for the Subject ID."
        Return False
    End Function

    Private Function IsAvailableColumnNumeric(variableKey As String) As Boolean
        If String.IsNullOrWhiteSpace(variableKey) Then Return False
        If Me.VariableColumnsInfo Is Nothing Then Return False

        Dim info As VarColumnInfo = Nothing
        If Not Me.VariableColumnsInfo.TryGetValue(variableKey, info) OrElse info Is Nothing Then Return False
        If Me.pWorksheet Is Nothing Then Return False

        Dim maxRows As Integer = MaxRowsInSheet(Me.pWorksheet)
        Dim r As Object = Me.pWorksheet.Range(Me.pWorksheet.Cells(1, info.ColumnNumber), Me.pWorksheet.Cells(maxRows, info.ColumnNumber))
        Return CountNonmissing(r, True) > 0
    End Function

    Private Function GetData() As MmrmData

        Dim keys As New List(Of String)

        ' Subject first so character subject IDs can be imported with CharCols:=0.
        keys.Add(CStr(Me.lbClusterID.Items(0)))

        ' Response second.
        keys.Add(CStr(Me.lbY.Items(0)))

        ' Visit third.
        keys.Add(CStr(Me.lbTime.Items(0)))

        ' Required raw fixed-effect variables.
        Dim rawXKeys As List(Of String) =
        RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedEffectsList.Items, Me.TermSpecs)

        For Each xKey As String In rawXKeys
            If Not keys.Contains(xKey) Then keys.Add(xKey)
        Next

        Dim ref As String = BuildExcelRefList(pWorksheet, keys, Me.VariableColumnsInfo)

        Dim d As New DataObj()
        ExcelDnaDataImporter.ImportInto(d, ref, True, CharCols:=0)

        Return New MmrmData With {
            .Raw = d,
            .SubjectKey = CStr(Me.lbClusterID.Items(0)),
            .ResponseKey = CStr(Me.lbY.Items(0)),
            .VisitKey = CStr(Me.lbTime.Items(0))
        }

    End Function

    Private Sub BuildExpandedFixedInputs(mmrmData As MmrmData,
                                     ByRef y() As Double,
                                     ByRef x(,) As Double,
                                     ByRef fixedNames() As String)

        If mmrmData Is Nothing OrElse mmrmData.Raw Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(mmrmData)))

        Dim raw As DataObj = mmrmData.Raw
        Dim nRows As Integer = raw.nRows

        If nRows <= 0 Then
            CoreServices.Errors.LogAndThrow(New ApplicationException("No valid observations are available for MMRM."))
        End If

        ' y is extracted explicitly because the MMRM import order is Subject, Response, Visit, predictors.
        Dim yCol As Integer = ResolveDataColumnIndex(raw, mmrmData.ResponseKey, "response")
        y = ExtractNumericColumnFromData(raw, yCol)

        ' Build expanded fixed-effect predictors from the authored effects list.
        Dim rawXKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedEffectsList.Items, Me.TermSpecs)
        Dim expandedX(,) As Double = Nothing
        Dim expandedNames() As String = New String() {}

        If rawXKeys.Count > 0 Then
            Dim rawX(,) As Double = ExtractRawNumericMatrix(raw, rawXKeys)

            RegressionDesignCore.BuildExpandedPredictorMatrix(rawX:=rawX,
                                                          rawXKeys:=rawXKeys,
                                                          effectItems:=Me.lbSelectedEffectsList.Items,
                                                          termSpecs:=Me.TermSpecs,
                                                          omitCategoricalReference:=True,
                                                          outX:=expandedX,
                                                          outPredictorNames:=expandedNames)
        End If

        If expandedNames Is Nothing Then expandedNames = New String() {}

        If Me.cbIntercept.Checked Then
            x = regression.MixedModelFrontEndHelpers.AddInterceptColumn(expandedX, nRows)
            fixedNames = CombineNamesWithIntercept(expandedNames)
        Else
            If expandedX Is Nothing OrElse expandedNames.Length = 0 Then
                CoreServices.Errors.LogAndThrow(New ApplicationException("MMRM fixed-effects design contains no columns. Add at least one fixed effect or enable the intercept."))
            End If

            x = expandedX
            fixedNames = DirectCast(expandedNames.Clone(), String())
        End If

    End Sub

    Private Async Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        Try
            Dim bWait As Boolean, strWarning As String
            'activate workbook we are working on (different may  be open if we re-running the analysis)
            Me.pWorkbook.activate

            strWarning = String.Empty
            ValidateInputs(bWait, strWarning)
            If bWait Then
                If strWarning <> String.Empty Then MsgBox(strWarning)
                Exit Sub
            End If

            Dim MyData = GetData()
            If MyData.Raw.bZeroValid Then 'check for zero valid data
                MsgBox("No valid observations")
                Exit Sub
            End If

            If Me.Text = "Mixed Models for Repeated Measures (MMRM)" Then
                Await Me.RunMMRMAsync(MyData)
            End If

        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Async Function RunMMRMAsync(mmrmGuiData As MmrmData) As Task

        Dim y() As Double = Nothing
        Dim x(,) As Double = Nothing
        Dim fixedNames() As String = Nothing

        BuildExpandedFixedInputs(mmrmGuiData, y, x, fixedNames)

        Dim raw As DataObj = mmrmGuiData.Raw
        Dim subjectCol As Integer = ResolveDataColumnIndex(raw, mmrmGuiData.SubjectKey, "subject")
        Dim visitCol As Integer = ResolveDataColumnIndex(raw, mmrmGuiData.VisitKey, "visit/time")

        Dim subjectId() As Object = regression.MixedModelFrontEndHelpers.ExtractObjectColumnFromData(raw, subjectCol)
        Dim visit() As Double = ExtractNumericColumnFromData(raw, visitCol)

        Dim blockData As regression.MixedModelBlockData =
            regression.MixedModelBlockData.FromArrays(y:=y,
                                                      x:=x,
                                                      subjectId:=subjectId,
                                                      z:=Nothing,
                                                      visit:=visit,
                                                      sortWithinSubjectByVisit:=True,
                                                      rowNumbers:=raw.RowIds)

        Dim rStruct As regression.MixedModelRStruct =
            regression.MixedModelRStructUtils.createMixedModelRStruct(CStr(Me.cbCovarStruct.SelectedItem))

        Dim selectedInferenceMethod As String = CStr(Me.cbInferenceMethod.SelectedItem)
        Dim krInferenceRequested As Boolean = String.Equals(selectedInferenceMethod, "Kenward-Roger", StringComparison.OrdinalIgnoreCase)

        Dim fitMethod As regression.MixedModelFitMethod
        If String.Equals(CStr(Me.cbFitMethod.SelectedItem), "ML", StringComparison.OrdinalIgnoreCase) Then
            fitMethod = regression.MixedModelFitMethod.ML
        Else
            fitMethod = regression.MixedModelFitMethod.REML
        End If

        If krInferenceRequested AndAlso fitMethod = regression.MixedModelFitMethod.ML Then
            fitMethod = regression.MixedModelFitMethod.REML
            Try
                Me.cbFitMethod.SelectedItem = "REML"
            Catch
                ' Non-fatal: the request itself is still forced to REML.
            End Try

            MsgBox("Kenward-Roger inference requires REML. The MMRM fit method has been changed to REML for this analysis.",
                   MsgBoxStyle.Information,
                   "MMRM Kenward-Roger inference")
        End If

        Dim req As regression.MixedModelFitRequest = regression.MixedModelFitRequest.CreateMMRM(blockData, rStruct, fitMethod)

        req.ResponseVarName = RegressionDesignCore.GetCoefBaseName(mmrmGuiData.ResponseKey)
        req.SubjectVarName = RegressionDesignCore.GetCoefBaseName(mmrmGuiData.SubjectKey)
        req.VisitVarName = RegressionDesignCore.GetCoefBaseName(mmrmGuiData.VisitKey)
        req.FixedEffectNames = fixedNames
        req.FixedFormulaText = BuildSelectedEffectsText()
        req.RandomFormulaText = String.Empty
        req.RequestLabel = "MMRM"
        Select Case selectedInferenceMethod
            Case "Large-sample normal"
                req.FixedInferenceMethod = regression.MixedModelFixedInferenceMethod.WaldNormal
            Case "Residual DF"
                req.FixedInferenceMethod = regression.MixedModelFixedInferenceMethod.ResidualDF
            Case "Satterthwaite"
                req.FixedInferenceMethod = regression.MixedModelFixedInferenceMethod.Satterthwaite
                req.UseSatterthwaite = True
            Case "Kenward-Roger"
                req.EnableFullKenwardRogerForMmrm()
            Case Else
                req.FixedInferenceMethod = regression.MixedModelFixedInferenceMethod.BetweenWithin
        End Select

        ' MixedModelControl is a Structure.  A property access returns a copy, so assign to a
        ' local variable, update it, then assign the whole structure back to req.Control.
        Dim ctl As regression.MixedModelControl = req.Control
        Dim uiEps As Double = ParseUiDouble(Me.tbEps.Text, "Convergence epsilon")

        ctl.MaxIter = ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations")

        ' Keep one UI field for now, but apply it consistently to all three optimizer stopping tolerances.
        ctl.Epsilon = uiEps
        ctl.StepTolerance = uiEps
        ctl.FunctionTolerance = uiEps
        ApplyMMRMCovarianceOptimizerSelections(ctl)

        ctl.Trace = Me.ckTrace.Checked OrElse Me.ckIterationsDetails.Checked
        req.Control = ctl

        ResetMMRMProgress()
        pMmrmCancelRequested = False
        pMmrmInterruptRequested = False
        pMmrmCloseAfterCancel = False
        pMmrmCalculationRunning = True
        req.ProgressReporter = AddressOf ReportMMRMProgressFromAnyThread
        req.CancellationRequested = AddressOf IsMMRMCancellationRequested
        req.InterruptionRequested = AddressOf IsMMRMInterruptionRequested

        Dim model As regression.MMRM = Nothing
        Dim result As regression.MixedModelResult = Nothing

        Try
            model = New regression.MMRM(req)

            Try
                result = Await Task.Run(Function() model.Fit())
                FinishMMRMProgress(result, result IsNot Nothing AndAlso result.Converged)
                If result IsNot Nothing AndAlso result.Cancelled Then
                    Return
                End If
            Catch ex As System.OperationCanceledException
                pMmrmCancelRequested = True
                FinishMMRMProgress(result, False)
                Return
            Catch
                FinishMMRMProgress(result, False)
                Throw
            Finally
                CompleteMMRMRunOnUiThread()
            End Try

            InvokeMMRMUi(Sub()
                             AppendMMRMEstimatedMeanTables(result, x, fixedNames, visit, mmrmGuiData)
                             WriteMMRMResults(mmrmGuiData, result, y, x, fixedNames, subjectId, visit)
                         End Sub)
        Finally
            ReleaseMMRMLargeRunReferences(result, model, req)

            result = Nothing
            model = Nothing
            blockData = Nothing
            rStruct = Nothing
            raw = Nothing
            y = Nothing
            x = Nothing
            fixedNames = Nothing
            subjectId = Nothing
            visit = Nothing
            mmrmGuiData = Nothing

            RequestMMRMManagedCleanup()
        End Try
    End Function

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles TabControl1.SelectedIndexChanged

        If Me.lbSelectedVariables.Items.Count > 0 Then
            If Not IsEqualListBox(Me.lbXs, Me.lbSelectedVariables) Then
                Remove_Item(Me.lbSelectedVariables)

                For i As Integer = 0 To Me.lbXs.Items.Count - 1
                    Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
                Next

                If Not IsSubsetListBox(Me.lbSelectedVariables, Me.lbSelectedEffectsList, bOnlyMain:=True) Then
                    If MsgBox("There is a variable in selected effects list that was removed from the fixed-effect source variable(s) list." & vbNewLine & vbNewLine &
                          "Clear selected fixed-effects list?", vbYesNo + vbExclamation, "Clear selected effects list?") = vbYes Then
                        If Me.lbSelectedEffectsList.Items.Count > 0 Then Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
                    End If
                End If
            End If
        Else
            For i As Integer = 0 To Me.lbXs.Items.Count - 1
                Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
            Next
        End If

        RefreshMMRMContrastControls()

        Try
            If TabControl1.SelectedTab Is Me.TabPageOptions Then
                TryRefreshMMRMContrastControlsFromCurrentSelections()
            End If
        Catch ex As Exception
            CoreServices.Logger.Debug("MMRM Options tab refresh skipped: " & ex.Message)
        End Try

    End Sub

    Private Sub btReload_Click(sender As Object, e As System.EventArgs) Handles btReload.Click
        Dim newSheet As Object
        Me.lbAllColumns.Items.Clear()

        If Me.cbSheetsList.SelectedIndex <> -1 Then
            If pWorksheet.Name <> Me.cbSheetsList.SelectedItem.ToString() Then
                Me.lbY.Items.Clear()
                Me.lbClusterID.Items.Clear()
                Me.lbTime.Items.Clear()
                Me.lbXs.Items.Clear()
                Me.lbSelectedVariables.Items.Clear()
                Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
            End If

            newSheet = pWorkbook.Worksheets(Me.cbSheetsList.SelectedItem.ToString())
            Me.Populate(newSheet)
        Else
            Me.Populate(pWorksheet)
        End If

        RefreshMMRMContrastControls()
    End Sub

    Private Sub btRemoveY_Click(sender As Object, e As System.EventArgs) Handles btRemoveY.Click
        Remove_Item(Me.lbY)
    End Sub

    Private Sub btRemoveClusterID_Click(sender As Object, e As System.EventArgs) Handles btRemoveClusterID.Click
        Remove_Item(Me.lbClusterID)
    End Sub

    Private Sub btRemoveTime_Click(sender As Object, e As System.EventArgs) Handles btRemoveTime.Click
        Remove_Item(Me.lbTime)
        RefreshMMRMContrastControls()
    End Sub

    Private Sub btRemoveX_Click(sender As Object, e As System.EventArgs) Handles btRemoveX.Click
        Remove_Item(Me.lbXs, "selected")
        RefreshMMRMContrastControls()
    End Sub

    Private Sub btAddY_Click(sender As Object, e As System.EventArgs) Handles btAddY.Click
        AddItemToListbox(Me.lbY, Me.lbAllColumns, Me.lbXs, Me.lbClusterID, Me.lbTime)
    End Sub

    Private Sub btAddClusterID_Click(sender As Object, e As System.EventArgs) Handles btAddClusterID.Click
        AddItemToListbox(Me.lbClusterID, Me.lbAllColumns, Me.lbXs, Me.lbY, Me.lbTime)
    End Sub

    Private Sub btAddTime_Click(sender As Object, e As System.EventArgs) Handles btAddTime.Click
        ' Do not check lbXs here: visit/time is commonly also used as a fixed effect in MMRM.
        AddItemToListbox(Me.lbTime, Me.lbAllColumns, Me.lbY, Me.lbClusterID)
        RefreshMMRMContrastControls()
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        ' Do not check lbTime here: visit/time is commonly also used as a fixed effect in MMRM.
        AddItemsToListbox(Me.lbXs, Me.lbAllColumns, Me.lbY, Me.lbClusterID)
        RefreshMMRMContrastControls()
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

    Private Sub btnPoly_Click(sender As Object, e As System.EventArgs) Handles btnPoly.Click
        Me.EffectsController.AddPolynomialEffectsFromSelectedVars(CInt(Me.spinBtnPoly.Value))
    End Sub

    Private Sub tbRemoveSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "selected", Me.TermSpecs)
    End Sub

    Private Sub btClearAllSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
    End Sub

    Private Sub cbMMRMControlLevel_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbMMRMControlLevel.SelectedIndexChanged
        Try
            UpdateMMRMContrastControlEnabledState()
        Catch ex As Exception
            CoreServices.Logger.Warn("cbMMRMControlLevel_SelectedIndexChanged failed: " & ex.ToString())
        End Try
    End Sub

    Private Sub cbMMRMContrastMode_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbMMRMContrastMode.SelectedIndexChanged
        Try
            UpdateMMRMContrastControlEnabledState()
        Catch ex As Exception
            CoreServices.Logger.Warn("cbMMRMContrastMode_SelectedIndexChanged failed: " & ex.ToString())
        End Try
    End Sub

    Private Sub MMRMReferenceGridControlChanged(sender As Object, e As System.EventArgs) Handles cbMMRMLSMeansMode.SelectedIndexChanged,
        cbMMRMRefGridWeighting.SelectedIndexChanged,
        cbMMRMRefGridCovariates.SelectedIndexChanged

        Try
            UpdateMMRMContrastControlEnabledState()
        Catch ex As Exception
            CoreServices.Logger.Warn("MMRMReferenceGridControlChanged failed: " & ex.Message)
        End Try
    End Sub

    Private Sub MMRMContrastControl_CheckedOrSelectedChanged(sender As Object, e As System.EventArgs) Handles ckMMRMClassInfo.CheckedChanged,
        ckMMRMEstimatedMeans.CheckedChanged,
        ckMMRMDiffInChange.CheckedChanged,
        ckMMRMChangeFromBaseline.CheckedChanged,
        cbMMRMGroupingFactor.SelectedIndexChanged,
        cbMMRMContrastDirection.SelectedIndexChanged,
        cbMMRMComparisonLevel.SelectedIndexChanged,
        cbMMRMBaselineVisit.SelectedIndexChanged,
        cbMMRMContrastMode.SelectedIndexChanged,
        cbMMRMControlLevel.SelectedIndexChanged

        Try
            UpdateMMRMContrastControlEnabledState()

            ' When the grouping factor changes, observed group levels must be refreshed.
            ' For other controls this is harmless and keeps the UI current.
            If sender Is Me.cbMMRMGroupingFactor OrElse sender Is Me.cbMMRMContrastMode Then
                TryRefreshMMRMContrastControlsFromCurrentSelections()
            End If

        Catch ex As Exception
            CoreServices.Logger.Warn("MMRMContrastControl_CheckedOrSelectedChanged failed: " & ex.Message)
        End Try
    End Sub

    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs) Handles btInterrupt.Click
        If Not pMmrmCalculationRunning Then Exit Sub

        pMmrmInterruptRequested = True

        Try
            Me.lblProgress.Text = "Interrupting MMRM; latest accepted estimates will be returned..."
            Me.btInterrupt.Enabled = False
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' Helpers
    '--------------------------------------------------------------------------
    Private Sub WriteMMRMResults(mmrmGuiData As MmrmData,
                                 result As regression.MixedModelResult,
                                 y() As Double,
                                 x(,) As Double,
                                 fixedNames() As String,
                                 subjectId() As Object,
                                 visit() As Double)

        If result Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(result)))
        If mmrmGuiData Is Nothing OrElse mmrmGuiData.Raw Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(mmrmGuiData)))

        Dim wb As Object = AppGlobals.app.Workbooks.Add()

        WriteMMRMDataSheet(wb, mmrmGuiData, result, y, x, fixedNames, subjectId, visit)
        WriteMMRMModelSheet(wb, result)

        If (Me.ckTrace.Checked OrElse Me.ckIterationsDetails.Checked) AndAlso Not String.IsNullOrWhiteSpace(result.strTrace) Then
            WriteMMRMTraceSheet(wb, result.strTrace)
        End If

    End Sub

    Private Sub WriteMMRMDataSheet(wb As Object,
                                   mmrmGuiData As MmrmData,
                                   result As regression.MixedModelResult,
                                   y() As Double,
                                   x(,) As Double,
                                   fixedNames() As String,
                                   subjectId() As Object,
                                   visit() As Double)

        Dim writeRes As New ExcelDnaResultWriter
        writeRes.wb = wb
        writeRes.ws = wb.ActiveSheet
        writeRes.ws.Name = "Data"

        writeRes.write({"Row ID"})
        writeRes.setRowPointer(2)
        writeRes.write(mmrmGuiData.Raw.RowIds, bTall:=True)
        writeRes.setRowPointer()
        writeRes.shiftColumnPointer(1)

        writeRes.write({RegressionDesignCore.GetCoefBaseName(mmrmGuiData.ResponseKey)})
        writeRes.setRowPointer(2)
        writeRes.write(y, bTall:=True)
        writeRes.setRowPointer()
        writeRes.shiftColumnPointer(1)

        If fixedNames IsNot Nothing AndAlso fixedNames.Length > 0 Then
            writeRes.write(fixedNames)
            writeRes.setRowPointer(2)
            writeRes.write(x)
            writeRes.setRowPointer()
            writeRes.shiftColumnPointer(fixedNames.Length)
        End If

        writeRes.write({RegressionDesignCore.GetCoefBaseName(mmrmGuiData.SubjectKey)})
        writeRes.setRowPointer(2)
        writeRes.write(subjectId, bTall:=True)
        writeRes.setRowPointer()
        writeRes.shiftColumnPointer(1)

        writeRes.write({RegressionDesignCore.GetCoefBaseName(mmrmGuiData.VisitKey)})
        writeRes.setRowPointer(2)
        writeRes.write(visit, bTall:=True)
        writeRes.setRowPointer()
        writeRes.shiftColumnPointer(1)

        If result.FittedMarginal IsNot Nothing AndAlso result.FittedMarginal.Length = y.Length Then
            writeRes.write({"Fitted marginal"})
            writeRes.setRowPointer(2)
            writeRes.write(result.FittedMarginal, bTall:=True)
            writeRes.setRowPointer()
            writeRes.shiftColumnPointer(1)
        End If

        If Me.ckResiduals.Checked AndAlso result.ResidualRaw IsNot Nothing AndAlso result.ResidualRaw.Length = y.Length Then
            writeRes.write({"Residual"})
            writeRes.setRowPointer(2)
            writeRes.write(result.ResidualRaw, bTall:=True)
            writeRes.setRowPointer()
            writeRes.shiftColumnPointer(1)
        End If

    End Sub

    Private Sub WriteMMRMModelSheet(wb As Object, result As regression.MixedModelResult)
        Dim alphaValue As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim tables As List(Of ResultTable) = result.wrapResults(alphaValue,
                                                                includeOptimizerTrace:=Me.ckIterationsDetails.Checked,
                                                                includeDiagnostics:=Me.cbDiagnostic.Checked)
        Dim writeRes As New ExcelDnaResultWriter
        wb.Worksheets.Add(After:=wb.Worksheets(wb.Worksheets.Count))
        wb.ActiveSheet.Name = "MMRM"
        writeRes.wb = wb
        writeRes.ws = wb.ActiveSheet

        Dim rr As New ProcessListofResultTables(tables)
        rr.writeToSheet(writeRes, True)
    End Sub

    Private Sub WriteMMRMTraceSheet(wb As Object, traceText As String)
        Dim writeRes As New ExcelDnaResultWriter
        wb.Worksheets.Add(After:=wb.Worksheets(wb.Worksheets.Count))
        wb.ActiveSheet.Name = "MMRM Trace"
        writeRes.wb = wb
        writeRes.ws = wb.ActiveSheet

        writeRes.write(regression.MixedModelFrontEndHelpers.TraceTextToMatrix(traceText))
    End Sub

    Private Function BuildSelectedEffectsText() As String
        Return regression.MixedModelFrontEndHelpers.BuildEffectsText(If(Me.lbSelectedEffectsList Is Nothing, Nothing, Me.lbSelectedEffectsList.Items),
                     Me.cbIntercept.Checked, "Intercept only")
    End Function

    Private Function ResolveDataColumnIndex(raw As DataObj, key As String, role As String) As Integer
        Return regression.MixedModelFrontEndHelpers.ResolveDataColumnIndex(raw, key, role, "MMRM")
    End Function

    Private Function ExtractNumericColumnFromData(raw As DataObj, columnIndex As Integer) As Double()
        Return regression.MixedModelFrontEndHelpers.ExtractNumericColumnFromData(raw, columnIndex, "MMRM")
    End Function

    Private Function ExtractRawNumericMatrix(raw As DataObj, rawXKeys As List(Of String)) As Double(,)
        Return regression.MixedModelFrontEndHelpers.ExtractRawNumericMatrix(raw, rawXKeys, "fixed-effect", "MMRM")
    End Function

    Private Function CombineNamesWithIntercept(expandedNames() As String) As String()
        Return regression.MixedModelFrontEndHelpers.AddInterceptName(expandedNames)
    End Function

    ''' <summary>
    ''' Adds first-pass MMRM estimated marginal mean and contrast tables to the fitted result.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This method currently uses the observed fixed-effect design rows as the LS-means reference grid.
    ''' It therefore computes linear estimates from averages of fitted design rows.  This is intentional
    ''' for the first GUI release because it avoids adding a separate reference-grid editor while still
    ''' producing clinically useful visit, group, and change-from-baseline summaries.
    ''' </para>
    ''' <para>
    ''' If a simple numeric grouping variable is detected among the fixed-effect source variables, the
    ''' method also adds visit-by-group LS-means and pairwise group contrast tables.
    ''' </para>
    ''' </remarks>
    Private Sub AppendMMRMEstimatedMeanTables(result As regression.MixedModelResult,
                                          x(,) As Double,
                                          fixedNames() As String,
                                          visit() As Double,
                                          mmrmData As MmrmData)
        Try
            If result Is Nothing Then Exit Sub
            If result.AdditionalResultTables Is Nothing Then result.AdditionalResultTables = New List(Of ResultTable)()

            CoreServices.Logger.Debug("AppendMMRMEstimatedMeanTables start. classInfo=" & Me.ckMMRMClassInfo.Checked.ToString() &
                                "; estimatedMeans=" & Me.ckMMRMEstimatedMeans.Checked.ToString() &
                                "; changeFromBaseline=" & Me.ckMMRMChangeFromBaseline.Checked.ToString() &
                                "; diffInChange=" & Me.ckMMRMDiffInChange.Checked.ToString() &
                                "; existingAdditionalTables=" & result.AdditionalResultTables.Count.ToString())

            If x Is Nothing Then
                CoreServices.Logger.Warn("AppendMMRMEstimatedMeanTables skipped: x design matrix is Nothing.")
                Exit Sub
            End If

            If visit Is Nothing Then
                CoreServices.Logger.Warn("AppendMMRMEstimatedMeanTables skipped: visit vector is Nothing.")
                Exit Sub
            End If

            If result.Beta Is Nothing OrElse result.Beta.Length = 0 Then
                CoreServices.Logger.Warn("AppendMMRMEstimatedMeanTables skipped: result.Beta is empty.")
                Exit Sub
            End If

            If result.VarBeta Is Nothing Then
                CoreServices.Logger.Warn("AppendMMRMEstimatedMeanTables skipped: result.VarBeta is Nothing.")
                Exit Sub
            End If

            If x.GetLength(0) <> visit.Length Then
                CoreServices.Logger.Warn("AppendMMRMEstimatedMeanTables skipped: row mismatch. x rows=" & x.GetLength(0).ToString() &
                                    "; visit length=" & visit.Length.ToString())
                Exit Sub
            End If

            Dim alphaValue As Double = CDbl(Me.spinBtnAlpha.Value)

            Dim groupKey As String = Nothing
            Dim groupValues() As Double = Nothing
            Dim hasGroup As Boolean = TryResolveMMRMGroupingVariableFromControls(mmrmData, groupKey, groupValues)

            CoreServices.Logger.Debug("AppendMMRMEstimatedMeanTables grouping resolved. hasGroup=" & hasGroup.ToString() &
                                "; groupKey='" & If(groupKey, String.Empty) & "'")

            ' Refresh data-dependent choices such as observed visit levels and group levels.
            ' This is safe even when no group is available.
            Try
                RefreshMMRMContrastControlsFromData(mmrmData, visit, If(hasGroup, groupValues, Nothing))
            Catch ex As Exception
                CoreServices.Logger.Warn("RefreshMMRMContrastControlsFromData failed inside AppendMMRMEstimatedMeanTables: " & ex.Message)
            End Try

            If Me.ckMMRMClassInfo.Checked Then
                AddOptionalMMRMTable(result,
                                 Function() BuildMMRMClassLevelInformationTable(mmrmData, If(hasGroup, groupKey, Nothing)),
                                 "Class Level Information")
            End If

            If Not Me.ckMMRMEstimatedMeans.Checked Then
                CoreServices.Logger.Debug("AppendMMRMEstimatedMeanTables finished after class-level output because estimated means are disabled.")
                Exit Sub
            End If

            Dim baselineVisit As Double = ResolveSelectedBaselineVisit(visit)
            Dim contrastMode As String = SelectedComboText(Me.cbMMRMContrastMode, MMRM_MODE_PAIRWISE)
            Dim contrastDirection As String = SelectedComboText(Me.cbMMRMContrastDirection, MMRM_DIR_HIGHER_MINUS_LOWER)
            Dim controlLevel As Double = If(hasGroup, ResolveSelectedControlLevel(groupValues), Double.NaN)

            If IsMMRMReferenceGridModeSelected() Then
                AppendMMRMReferenceGridTables(result:=result,
                                  fixedNames:=fixedNames,
                                  visit:=visit,
                                  mmrmData:=mmrmData,
                                  hasGroup:=hasGroup,
                                  groupKey:=groupKey,
                                  groupValues:=groupValues,
                                  alphaValue:=alphaValue,
                                  contrastMode:=contrastMode,
                                  controlLevel:=controlLevel,
                                  contrastDirection:=contrastDirection)

                CoreServices.Logger.Debug("AppendMMRMEstimatedMeanTables used reference-grid mode; observed-design-grid tables skipped.")
                Exit Sub
            End If

            CoreServices.Logger.Debug("AppendMMRMEstimatedMeanTables options. baseline=" & baselineVisit.ToString(Globalization.CultureInfo.InvariantCulture) &
                                "; contrastMode='" & contrastMode & "'; direction='" & contrastDirection &
                                "'; controlLevel=" & controlLevel.ToString(Globalization.CultureInfo.InvariantCulture))

            AddOptionalMMRMTable(result,
                             Function() regression.MMRMPostEstimation.BuildEstimatedMeansByVisitTable(result, x, visit, alphaValue),
                             "Estimated marginal means by visit")

            If Me.ckMMRMChangeFromBaseline.Checked Then
                AddOptionalMMRMTable(result,
                                 Function() regression.MMRMPostEstimation.BuildChangeFromBaselineTableControlled(result, x, visit, baselineVisit, alphaValue),
                                 "MMRM change from baseline by visit")
            End If

            If hasGroup Then
                AddOptionalMMRMTable(result,
                                 Function() regression.MMRMPostEstimation.BuildEstimatedMeansByVisitAndGroupTable(result, x, visit, groupValues, groupKey, alphaValue),
                                 "Estimated marginal means by visit and group")

                If Not String.Equals(contrastMode, MMRM_MODE_NONE, StringComparison.OrdinalIgnoreCase) Then
                    AddOptionalMMRMTable(result,
                                     Function() BuildMMRMVisitGroupDifferencesTableControlled(result, x, visit, groupValues, groupKey, alphaValue, contrastMode, controlLevel, contrastDirection),
                                     "MMRM group differences by visit")
                End If

                If Me.ckMMRMChangeFromBaseline.Checked Then
                    AddOptionalMMRMTable(result,
                                     Function() regression.MMRMPostEstimation.BuildChangeFromBaselineByGroupTableControlled(result, x, visit, groupValues, groupKey, baselineVisit, alphaValue),
                                     "MMRM change from baseline by visit and group")

                    If Me.ckMMRMDiffInChange.Checked AndAlso Not String.Equals(contrastMode, MMRM_MODE_NONE, StringComparison.OrdinalIgnoreCase) Then
                        AddOptionalMMRMTable(result,
                                         Function() BuildMMRMDifferenceInChangeFromBaselineTableControlled(result, x, visit, groupValues, groupKey, baselineVisit, alphaValue, contrastMode, controlLevel, contrastDirection),
                                         "MMRM difference in change from baseline")
                    End If
                End If
            Else
                CoreServices.Logger.Debug("AppendMMRMEstimatedMeanTables: no grouping factor resolved; group-specific LS-means/contrast tables skipped.")
            End If

            CoreServices.Logger.Debug("AppendMMRMEstimatedMeanTables complete. additionalTables=" & result.AdditionalResultTables.Count.ToString())

        Catch ex As Exception
            CoreServices.Logger.Warn("AppendMMRMEstimatedMeanTables failed: " & ex.ToString())
        End Try
    End Sub

    ''' <summary>
    ''' Chooses a simple numeric grouping variable for an optional visit-by-group mean table.
    ''' </summary>
    ''' <remarks>
    ''' The first implementation is conservative: among selected fixed-effect source variables,
    ''' exclude the visit/time variable and choose the variable with the smallest number of
    ''' finite distinct numeric levels between 2 and 6.  This works well for examples such as
    ''' SexCode in the Orthodont data while avoiding continuous variables such as age when a
    ''' two-level grouping variable is present.
    ''' </remarks>
    Private Function TryChooseMMRMGroupingVariable(mmrmData As MmrmData,
                                               ByRef groupKey As String,
                                               ByRef groupValues() As Double) As Boolean
        groupKey = Nothing
        groupValues = Nothing

        If mmrmData Is Nothing OrElse mmrmData.Raw Is Nothing Then Return False

        Dim factorKeys As List(Of String) = GetMMRMCategoricalFactorBaseKeys()
        If factorKeys Is Nothing OrElse factorKeys.Count = 0 Then Return False

        Dim bestKey As String = Nothing
        Dim bestValues() As Double = Nothing
        Dim bestLevelCount As Integer = Integer.MaxValue

        For Each key As String In factorKeys
            Try
                Dim col As Integer = ResolveDataColumnIndex(mmrmData.Raw, key, "MMRM grouping factor")
                Dim values() As Double = ExtractNumericColumnFromData(mmrmData.Raw, col)
                Dim levels() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(values)

                If levels IsNot Nothing AndAlso levels.Length >= 2 Then
                    If levels.Length < bestLevelCount Then
                        bestLevelCount = levels.Length
                        bestKey = key
                        bestValues = values
                    End If
                End If
            Catch ex As Exception
                CoreServices.Logger.Debug("MMRM auto grouping factor skipped '" & key & "': " & ex.Message)
            End Try
        Next

        If String.IsNullOrWhiteSpace(bestKey) OrElse bestValues Is Nothing Then Return False

        groupKey = bestKey
        groupValues = bestValues
        Return True
    End Function

    ''' <summary>
    ''' Builds a SAS-like Class Level Information table for the current MMRM output.
    ''' </summary>
    ''' <param name="mmrmData">Imported MMRM GUI data.</param>
    ''' <param name="groupKey">
    ''' Optional grouping factor used by LS-means/contrasts.  When supplied, that
    ''' variable is shown in addition to the subject identifier.
    ''' </param>
    ''' <returns>
    ''' Result table with columns Class, Levels, Values; or Nothing when no class
    ''' variables can be resolved.
    ''' </returns>
    Private Function BuildMMRMClassLevelInformationTable(mmrmData As MmrmData,
                                                     Optional groupKey As String = Nothing) As ResultTable
        If mmrmData Is Nothing OrElse mmrmData.Raw Is Nothing Then Return Nothing

        Dim rows As New List(Of Object())

        ' IMPORTANT: use HashSet, not List(comparer).  List(Of String) has no
        ' comparer constructor; passing StringComparer to List creates the runtime
        ' InvalidCastException seen in the log.
        Dim addedKeys As New System.Collections.Generic.HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        AddClassLevelRow(rows, addedKeys, mmrmData.Raw, mmrmData.SubjectKey)

        If Not String.IsNullOrWhiteSpace(groupKey) Then
            AddClassLevelRow(rows, addedKeys, mmrmData.Raw, groupKey)
        End If

        If rows.Count = 0 Then Return Nothing

        Dim body(rows.Count - 1, 2) As Object
        For i As Integer = 0 To rows.Count - 1
            body(i, 0) = rows(i)(0)
            body(i, 1) = rows(i)(1)
            body(i, 2) = rows(i)(2)
        Next

        Dim t As New ResultTable
        t.AddTitle("Class Level Information")
        t.SetBody(body)
        t.AddHeaderTopRow({"Class", "Levels", "Values"})
        t.AddFootnote("Class levels are shown for the subject ID and, when available, the grouping factor used by MMRM LS-means/contrast output.")
        Return t
    End Function


    ''' <summary>
    ''' Adds one row to the Class Level Information table.
    ''' </summary>
    Private Sub AddClassLevelRow(rows As List(Of Object()),
                             addedKeys As System.Collections.Generic.HashSet(Of String),
                             raw As DataObj,
                             key As String)
        If rows Is Nothing OrElse addedKeys Is Nothing OrElse raw Is Nothing Then Exit Sub
        If String.IsNullOrWhiteSpace(key) Then Exit Sub

        Dim baseName As String = RegressionDesignCore.GetCoefBaseName(key)
        If String.IsNullOrWhiteSpace(baseName) Then baseName = key.Trim()

        If addedKeys.Contains(baseName) Then Exit Sub

        Try
            Dim col As Integer = ResolveDataColumnIndex(raw, key, "class-level variable")
            Dim values() As Object = regression.MixedModelFrontEndHelpers.ExtractObjectColumnFromData(raw, col)
            Dim uniqueValues() As Object = regression.MixedModelFrontEndHelpers.UniqueSortedClassValues(values)

            If uniqueValues Is Nothing OrElse uniqueValues.Length = 0 Then Exit Sub

            rows.Add(New Object() {baseName, uniqueValues.Length, regression.MixedModelFrontEndHelpers.JoinClassValues(uniqueValues)})
            addedKeys.Add(baseName)
        Catch ex As Exception
            CoreServices.Logger.Warn("Class-level row skipped for '" & key & "': " & ex.Message)
        End Try
    End Sub

    Private Sub AppendMMRMReferenceGridTables(result As regression.MixedModelResult,
                                          fixedNames() As String,
                                          visit() As Double,
                                          mmrmData As MmrmData,
                                          hasGroup As Boolean,
                                          groupKey As String,
                                          groupValues() As Double,
                                          alphaValue As Double,
                                          contrastMode As String,
                                          controlLevel As Double,
                                          contrastDirection As String)
        If result Is Nothing OrElse fixedNames Is Nothing OrElse visit Is Nothing OrElse mmrmData Is Nothing Then Exit Sub

        Dim spec As regression.MixedModelReferenceGridSpec =
        BuildMMRMReferenceGridSpec(result:=result,
                                   fixedNames:=fixedNames,
                                   visit:=visit,
                                   mmrmData:=mmrmData,
                                   hasGroup:=hasGroup,
                                   groupKey:=groupKey,
                                   groupValues:=groupValues,
                                   alphaValue:=alphaValue)

        If spec Is Nothing Then
            CoreServices.Logger.Warn("Reference-grid LS-means skipped: specification could not be built.")
            Exit Sub
        End If

        Dim rows As List(Of regression.MixedModelReferenceGridRow) =
        regression.MixedModelReferenceGridService.BuildReferenceGridRows(spec)

        If rows Is Nothing OrElse rows.Count = 0 Then
            CoreServices.Logger.Warn("Reference-grid LS-means skipped: no reference-grid rows were created.")
            Exit Sub
        End If

        AddOptionalMMRMTable(result,
                         Function() regression.MixedModelReferenceGridService.BuildEstimatedMeansTable(
                             title:="Reference-grid estimated marginal means",
                             rows:=rows,
                             result:=result,
                             spec:=spec),
                         "Reference-grid estimated marginal means")

        If hasGroup AndAlso Not String.IsNullOrWhiteSpace(groupKey) AndAlso
       Not String.Equals(contrastMode, MMRM_MODE_NONE, StringComparison.OrdinalIgnoreCase) Then

            AddOptionalMMRMTable(result,
                             Function() BuildMMRMReferenceGridGroupContrastsTable(result,
                                                                                 rows,
                                                                                 spec,
                                                                                 groupKey,
                                                                                 contrastMode,
                                                                                 controlLevel,
                                                                                 contrastDirection),
                             "Reference-grid group contrasts")
        End If
    End Sub

    ''' <summary>
    ''' Initializes MMRM-specific model, inference, and LS-means/contrast controls.
    ''' </summary>
    Private Sub InitializeMMRMControls()

        Me.cbFitMethod.Items.Clear()
        Me.cbFitMethod.Items.AddRange(New Object() {"ML", "REML"})
        Me.cbFitMethod.SelectedIndex = 1

        Me.cbInferenceMethod.Items.Clear()
        Me.cbInferenceMethod.Items.AddRange(New Object() {"Large-sample normal", "Residual DF", "Between-within DF", "Satterthwaite", "Kenward-Roger"})
        Me.cbInferenceMethod.SelectedIndex = 4

        Me.cbCovarStruct.Items.Clear()
        For Each s As String In regression.MixedModelRStruct.RStructsList
            Me.cbCovarStruct.Items.Add(s)
        Next
        Me.cbCovarStruct.SelectedItem = "Unstructured"
        If Me.cbCovarStruct.SelectedIndex < 0 AndAlso Me.cbCovarStruct.Items.Count > 0 Then Me.cbCovarStruct.SelectedIndex = 0

        Me.ckMMRMClassInfo.Checked = True
        Me.ckMMRMEstimatedMeans.Checked = True
        Me.ckMMRMChangeFromBaseline.Checked = True
        Me.ckMMRMDiffInChange.Checked = True

        Me.cbMMRMGroupingFactor.Items.Clear()
        Me.cbMMRMGroupingFactor.Items.Add(MMRM_GROUP_AUTO)
        Me.cbMMRMGroupingFactor.Items.Add(MMRM_GROUP_NONE)
        Me.cbMMRMGroupingFactor.SelectedIndex = 0

        Me.cbMMRMBaselineVisit.Items.Clear()
        Me.cbMMRMBaselineVisit.Items.Add(MMRM_BASELINE_SMALLEST)
        Me.cbMMRMBaselineVisit.SelectedIndex = 0

        Me.cbMMRMContrastMode.Items.Clear()
        Me.cbMMRMContrastMode.Items.AddRange(New Object() {MMRM_MODE_NONE, MMRM_MODE_PAIRWISE, MMRM_MODE_CONTROL, MMRM_MODE_SELECTED})
        Me.cbMMRMContrastMode.SelectedItem = MMRM_MODE_PAIRWISE

        Me.cbMMRMControlLevel.Items.Clear()
        Me.cbMMRMControlLevel.Items.Add(MMRM_CONTROL_FIRST)
        Me.cbMMRMControlLevel.SelectedIndex = 0

        Me.cbMMRMContrastDirection.Items.Clear()
        Me.cbMMRMContrastDirection.Items.AddRange(New Object() {MMRM_DIR_HIGHER_MINUS_LOWER, MMRM_DIR_TREATMENT_MINUS_CONTROL, MMRM_DIR_CONTROL_MINUS_TREATMENT})
        Me.cbMMRMContrastDirection.SelectedItem = MMRM_DIR_HIGHER_MINUS_LOWER

        Me.cbMMRMComparisonLevel.Items.Clear()
        Me.cbMMRMComparisonLevel.Items.AddRange(New Object() {MMRM_GROUP_AUTO})
        Me.cbMMRMComparisonLevel.SelectedIndex = 0

        Me.cbMMRMLSMeansMode.Items.Clear()
        Me.cbMMRMLSMeansMode.Items.AddRange(New Object() {MMRM_LSMEANS_OBSERVED_GRID, MMRM_LSMEANS_REFERENCE_GRID})
        Me.cbMMRMLSMeansMode.SelectedItem = MMRM_LSMEANS_OBSERVED_GRID

        Me.cbMMRMRefGridWeighting.Items.Clear()
        Me.cbMMRMRefGridWeighting.Items.AddRange(New Object() {MMRM_RG_WEIGHT_EQUAL, MMRM_RG_WEIGHT_OBSERVED})
        Me.cbMMRMRefGridWeighting.SelectedItem = MMRM_RG_WEIGHT_EQUAL

        Me.cbMMRMRefGridCovariates.Items.Clear()
        Me.cbMMRMRefGridCovariates.Items.AddRange(New Object() {MMRM_RG_COVARIATE_MEANS, MMRM_RG_COVARIATE_ZERO})
        Me.cbMMRMRefGridCovariates.SelectedItem = MMRM_RG_COVARIATE_MEANS

        Me.cbMMRMMultiplicity.Items.Clear()
        Me.cbMMRMMultiplicity.Items.AddRange(New Object() {MMRM_MULT_NONE, MMRM_MULT_BONFERRONI, MMRM_MULT_HOLM, MMRM_MULT_SIDAK})
        Me.cbMMRMMultiplicity.SelectedItem = MMRM_MULT_NONE

        If Me.cbMMRMCovOptimizerMode IsNot Nothing Then
            Me.cbMMRMCovOptimizerMode.Items.Clear()
            Me.cbMMRMCovOptimizerMode.Items.AddRange(New Object() {MMRM_OPT_AI, MMRM_OPT_BFGS_AUTO, MMRM_OPT_BFGS_ANALYTIC, MMRM_OPT_BFGS_NUMERICAL})
            Me.cbMMRMCovOptimizerMode.SelectedItem = MMRM_OPT_AI
        End If

        If Me.cbMMRMCovGradientMode IsNot Nothing Then
            Me.cbMMRMCovGradientMode.Items.Clear()
            Me.cbMMRMCovGradientMode.Items.AddRange(New Object() {MMRM_GRAD_AUTO, MMRM_GRAD_ANALYTIC, MMRM_GRAD_VALIDATE, MMRM_GRAD_NUMERICAL})
            Me.cbMMRMCovGradientMode.SelectedItem = MMRM_GRAD_AUTO
        End If

        Me.tbEps.Text = FormatUiDouble(0.000001)

        RefreshMMRMContrastControls()

    End Sub

    ''' <summary>
    ''' Refreshes LS-means/contrast combo-box candidates from currently selected variables.
    ''' This method performs a lightweight structural refresh first, then attempts a
    ''' quiet data-dependent refresh so observed visit and group levels are available
    ''' before Fit is clicked.
    ''' </summary>
    Private Sub RefreshMMRMContrastControls()
        Try
            RefreshMMRMGroupingFactorItems()
            RefreshMMRMBaselineVisitItems(Nothing)
            RefreshMMRMControlLevelItems(Nothing)
            RefreshMMRMComparisonLevelCombo(Nothing, Nothing)
            UpdateMMRMContrastControlEnabledState()

            ' Best-effort pre-fit refresh.  This populates baseline/control/comparison
            ' level lists from the current worksheet selections.  It should not display
            ' errors while the user is still building the dialog.
            TryRefreshMMRMContrastControlsFromCurrentSelections()

        Catch ex As Exception
            CoreServices.Logger.Warn("RefreshMMRMContrastControls failed: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Refreshes data-dependent baseline, control-level, and comparison-level combo
    ''' boxes after the data are imported.
    ''' </summary>
    Private Sub RefreshMMRMContrastControlsFromData(mmrmData As MmrmData, visit() As Double, groupValues() As Double)
        Try
            RefreshMMRMBaselineVisitItems(visit)
            RefreshMMRMControlLevelItems(groupValues)

            Dim controlLevel As Double = Double.NaN
            If groupValues IsNot Nothing Then
                controlLevel = ResolveSelectedControlLevel(groupValues)
            End If

            RefreshMMRMComparisonLevelCombo(groupValues, controlLevel)
            UpdateMMRMContrastControlEnabledState()

        Catch ex As Exception
            CoreServices.Logger.Warn("RefreshMMRMContrastControlsFromData failed: " & ex.Message)
        End Try
    End Sub

    Private Sub RefreshMMRMGroupingFactorItems()
        Dim oldSelection As String = If(Me.cbMMRMGroupingFactor.SelectedItem Is Nothing,
                                    MMRM_GROUP_AUTO,
                                    CStr(Me.cbMMRMGroupingFactor.SelectedItem))

        Me.cbMMRMGroupingFactor.Items.Clear()
        Me.cbMMRMGroupingFactor.Items.Add(MMRM_GROUP_AUTO)
        Me.cbMMRMGroupingFactor.Items.Add(MMRM_GROUP_NONE)

        Dim factorKeys As List(Of String) = GetMMRMCategoricalFactorBaseKeys()

        For Each key As String In factorKeys
            If Me.cbMMRMGroupingFactor.FindStringExact(key) < 0 Then
                Me.cbMMRMGroupingFactor.Items.Add(key)
            End If
        Next

        Dim idx As Integer = Me.cbMMRMGroupingFactor.FindStringExact(oldSelection)
        If idx >= 0 Then
            Me.cbMMRMGroupingFactor.SelectedIndex = idx
        Else
            Me.cbMMRMGroupingFactor.SelectedIndex = 0
        End If
    End Sub

    Private Sub RefreshMMRMBaselineVisitItems(visit() As Double)
        Dim oldSelection As String = If(Me.cbMMRMBaselineVisit.SelectedItem Is Nothing, MMRM_BASELINE_SMALLEST, CStr(Me.cbMMRMBaselineVisit.SelectedItem))

        Me.cbMMRMBaselineVisit.Items.Clear()
        Me.cbMMRMBaselineVisit.Items.Add(MMRM_BASELINE_SMALLEST)

        If visit IsNot Nothing Then
            Dim visits() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            If visits IsNot Nothing Then
                For Each v As Double In visits
                    Me.cbMMRMBaselineVisit.Items.Add(regression.MixedModelPostEstimation.FormatProfileValue(v))
                Next
            End If
        End If

        Dim idx As Integer = Me.cbMMRMBaselineVisit.FindStringExact(oldSelection)
        If idx >= 0 Then
            Me.cbMMRMBaselineVisit.SelectedIndex = idx
        Else
            Me.cbMMRMBaselineVisit.SelectedIndex = 0
        End If
    End Sub

    Private Sub RefreshMMRMControlLevelItems(groupValues() As Double)
        Dim oldSelection As String = If(Me.cbMMRMControlLevel.SelectedItem Is Nothing, MMRM_CONTROL_FIRST, CStr(Me.cbMMRMControlLevel.SelectedItem))

        Me.cbMMRMControlLevel.Items.Clear()
        Me.cbMMRMControlLevel.Items.Add(MMRM_CONTROL_FIRST)

        If groupValues IsNot Nothing Then
            Dim groups() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)
            If groups IsNot Nothing Then
                For Each g As Double In groups
                    Me.cbMMRMControlLevel.Items.Add(regression.MixedModelPostEstimation.FormatProfileValue(g))
                Next
            End If
        End If

        Dim idx As Integer = Me.cbMMRMControlLevel.FindStringExact(oldSelection)
        If idx >= 0 Then
            Me.cbMMRMControlLevel.SelectedIndex = idx
        Else
            Me.cbMMRMControlLevel.SelectedIndex = 0
        End If
    End Sub

    Private Sub UpdateMMRMContrastControlEnabledState()
        Dim meansOn As Boolean = Me.ckMMRMEstimatedMeans.Checked
        Dim classOn As Boolean = Me.ckMMRMClassInfo.Checked

        Dim groupSelection As String = If(Me.cbMMRMGroupingFactor.SelectedItem Is Nothing,
                                      MMRM_GROUP_AUTO,
                                      CStr(Me.cbMMRMGroupingFactor.SelectedItem))

        Dim groupPossible As Boolean =
        Not String.Equals(groupSelection, MMRM_GROUP_NONE, StringComparison.OrdinalIgnoreCase)

        Dim contrastMode As String = If(Me.cbMMRMContrastMode.SelectedItem Is Nothing,
                                    MMRM_MODE_NONE,
                                    CStr(Me.cbMMRMContrastMode.SelectedItem))

        Dim isNoneMode As Boolean =
        String.Equals(contrastMode, MMRM_MODE_NONE, StringComparison.OrdinalIgnoreCase)

        Dim isControlMode As Boolean =
        String.Equals(contrastMode, MMRM_MODE_CONTROL, StringComparison.OrdinalIgnoreCase)

        Dim isSelectedMode As Boolean =
        String.Equals(contrastMode, MMRM_MODE_SELECTED, StringComparison.OrdinalIgnoreCase)

        Dim needsControlLevel As Boolean = isControlMode OrElse isSelectedMode
        Dim needsComparisonLevel As Boolean = isSelectedMode
        Dim needsDirection As Boolean = (isControlMode OrElse isSelectedMode)

        Me.cbMMRMGroupingFactor.Enabled = classOn OrElse meansOn
        Me.cbMMRMBaselineVisit.Enabled = meansOn AndAlso Me.ckMMRMChangeFromBaseline.Checked
        Me.cbMMRMContrastMode.Enabled = meansOn AndAlso groupPossible
        Me.cbMMRMControlLevel.Enabled = meansOn AndAlso groupPossible AndAlso needsControlLevel
        Me.cbMMRMComparisonLevel.Enabled = meansOn AndAlso groupPossible AndAlso needsComparisonLevel
        Me.lblMMRMComparisonLevel.Enabled = Me.cbMMRMComparisonLevel.Enabled
        Me.cbMMRMContrastDirection.Enabled = meansOn AndAlso groupPossible AndAlso needsDirection

        Me.ckMMRMChangeFromBaseline.Enabled = meansOn
        Me.ckMMRMDiffInChange.Enabled = meansOn AndAlso Me.ckMMRMChangeFromBaseline.Checked AndAlso groupPossible AndAlso Not isNoneMode

        Dim referenceGridControlsOn As Boolean = meansOn AndAlso IsMMRMReferenceGridModeSelected()
        Me.cbMMRMLSMeansMode.Enabled = meansOn
        Me.lblMMRMLSMeansMode.Enabled = meansOn
        Me.cbMMRMRefGridWeighting.Enabled = referenceGridControlsOn
        Me.lblMMRMRefGridWeighting.Enabled = referenceGridControlsOn
        Me.cbMMRMRefGridCovariates.Enabled = referenceGridControlsOn
        Me.lblMMRMRefGridCovariates.Enabled = referenceGridControlsOn
        Me.cbMMRMMultiplicity.Enabled = meansOn AndAlso groupPossible AndAlso Not isNoneMode
        Me.lblMMRMMultiplicity.Enabled = meansOn AndAlso groupPossible AndAlso Not isNoneMode

    End Sub

    Private Sub ApplyMMRMCovarianceOptimizerSelections(ByRef ctl As regression.MixedModelControl)
        Dim optimizerText As String = SelectedComboText(Me.cbMMRMCovOptimizerMode, MMRM_OPT_AI)
        Dim gradientText As String = SelectedComboText(Me.cbMMRMCovGradientMode, MMRM_GRAD_AUTO)

        ctl.CovarianceOptimizerMode = ParseMMRMCovarianceOptimizerMode(optimizerText)
        ctl.CovarianceGradientMode = ParseMMRMCovarianceGradientMode(gradientText)

        If String.Equals(optimizerText, MMRM_OPT_BFGS_ANALYTIC, StringComparison.OrdinalIgnoreCase) Then
            ctl.CovarianceOptimizerMode = regression.MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient
            ctl.CovarianceGradientMode = regression.MixedModelCovarianceGradientMode.AnalyticScore
        ElseIf String.Equals(optimizerText, MMRM_OPT_BFGS_NUMERICAL, StringComparison.OrdinalIgnoreCase) Then
            ctl.CovarianceOptimizerMode = regression.MixedModelCovarianceOptimizerMode.ProjectedBfgs
            ctl.CovarianceGradientMode = regression.MixedModelCovarianceGradientMode.NumericalFiniteDifference
        End If
    End Sub

    Private Function ParseMMRMCovarianceOptimizerMode(selection As String) As regression.MixedModelCovarianceOptimizerMode
        If String.IsNullOrWhiteSpace(selection) Then Return regression.MixedModelCovarianceOptimizerMode.AverageInformationReml
        Return regression.MixedModelFrontEndHelpers.ParseCovarianceOptimizerMode(selection, regression.MixedModelCovarianceOptimizerMode.ProjectedBfgs)
    End Function

    Private Function ParseMMRMCovarianceGradientMode(selection As String) As regression.MixedModelCovarianceGradientMode
        Return regression.MixedModelFrontEndHelpers.ParseCovarianceGradientMode(selection, regression.MixedModelCovarianceGradientMode.Auto)
    End Function

    Private Function SelectedComboText(cb As Windows.Forms.ComboBox, fallback As String) As String
        If cb Is Nothing OrElse cb.SelectedItem Is Nothing Then Return fallback
        Dim s As String = CStr(cb.SelectedItem)
        If String.IsNullOrWhiteSpace(s) Then Return fallback
        Return s
    End Function

    Private Function IsMMRMReferenceGridModeSelected() As Boolean
        Return Me.cbMMRMLSMeansMode IsNot Nothing AndAlso
           String.Equals(SelectedComboText(Me.cbMMRMLSMeansMode, MMRM_LSMEANS_OBSERVED_GRID),
                         MMRM_LSMEANS_REFERENCE_GRID,
                         StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Returns the base variable keys for selected fixed effects that are explicitly
    ''' authored as categorical main effects.
    ''' </summary>
    Private Function GetMMRMCategoricalFactorBaseKeys() As List(Of String)
        Dim out As New List(Of String)

        If Me.lbSelectedEffectsList Is Nothing OrElse Me.TermSpecs Is Nothing Then Return out

        Dim visitBase As String = Nothing
        If Me.lbTime IsNot Nothing AndAlso Me.lbTime.Items.Count = 1 Then
            visitBase = RegressionDesignCore.GetCoefBaseName(CStr(Me.lbTime.Items(0)))
        End If

        For Each it As Object In Me.lbSelectedEffectsList.Items
            Dim effKey As String = CStr(it)
            If String.IsNullOrWhiteSpace(effKey) Then Continue For
            If Not Me.TermSpecs.ContainsKey(effKey) Then Continue For

            Dim spec As TermSpec = Me.TermSpecs(effKey)
            If spec Is Nothing Then Continue For
            If Not String.Equals(spec.Kind, "MainEffect", StringComparison.OrdinalIgnoreCase) Then Continue For
            If spec.Scale <> PredictorScale.Categorical Then Continue For
            If spec.BaseVarKeys Is Nothing OrElse spec.BaseVarKeys.Count <> 1 Then Continue For

            Dim baseKey As String = spec.BaseVarKeys(0)
            Dim baseName As String = RegressionDesignCore.GetCoefBaseName(baseKey)

            If Not String.IsNullOrWhiteSpace(visitBase) AndAlso
           String.Equals(baseName, visitBase, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            If Not out.Contains(baseKey) Then out.Add(baseKey)
        Next

        Return out
    End Function

    ''' <summary>
    ''' Resolves the grouping variable using the explicit combo-box selection.
    ''' </summary>
    Private Function TryResolveMMRMGroupingVariableFromControls(mmrmData As MmrmData,
                                                                ByRef groupKey As String,
                                                                ByRef groupValues() As Double) As Boolean
        groupKey = Nothing
        groupValues = Nothing

        If mmrmData Is Nothing OrElse mmrmData.Raw Is Nothing Then Return False

        Dim sel As String = SelectedComboText(Me.cbMMRMGroupingFactor, MMRM_GROUP_AUTO)

        If String.Equals(sel, MMRM_GROUP_NONE, StringComparison.OrdinalIgnoreCase) Then Return False

        If String.Equals(sel, MMRM_GROUP_AUTO, StringComparison.OrdinalIgnoreCase) Then
            Return TryChooseMMRMGroupingVariable(mmrmData, groupKey, groupValues)
        End If

        Try
            Dim col As Integer = ResolveDataColumnIndex(mmrmData.Raw, sel, "LS-means grouping variable")
            Dim values() As Double = ExtractNumericColumnFromData(mmrmData.Raw, col)
            Dim levels() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(values)
            If levels Is Nothing OrElse levels.Length < 2 Then Return False

            groupKey = sel
            groupValues = values
            Return True
        Catch ex As Exception
            CoreServices.Logger.Warn("Selected MMRM grouping factor could not be used: " & ex.Message)
            Return False
        End Try
    End Function

    Private Function ResolveSelectedBaselineVisit(visit() As Double) As Double
        Dim visits() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
        If visits Is Nothing OrElse visits.Length = 0 Then Return Double.NaN

        Dim sel As String = SelectedComboText(Me.cbMMRMBaselineVisit, MMRM_BASELINE_SMALLEST)
        If String.Equals(sel, MMRM_BASELINE_SMALLEST, StringComparison.OrdinalIgnoreCase) Then Return visits(0)

        For Each v As Double In visits
            If String.Equals(regression.MixedModelPostEstimation.FormatProfileValue(v), sel, StringComparison.OrdinalIgnoreCase) Then Return v
        Next

        Dim parsed As Double
        If Double.TryParse(sel, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, parsed) Then Return parsed
        If Double.TryParse(sel, parsed) Then Return parsed

        Return visits(0)
    End Function

    Private Function ResolveSelectedControlLevel(groupValues() As Double) As Double
        Dim groups() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)
        If groups Is Nothing OrElse groups.Length = 0 Then Return Double.NaN

        Dim sel As String = SelectedComboText(Me.cbMMRMControlLevel, MMRM_CONTROL_FIRST)
        If String.Equals(sel, MMRM_CONTROL_FIRST, StringComparison.OrdinalIgnoreCase) Then Return groups(0)

        For Each g As Double In groups
            If String.Equals(regression.MixedModelPostEstimation.FormatProfileValue(g), sel, StringComparison.OrdinalIgnoreCase) Then Return g
        Next

        Dim parsed As Double
        If Double.TryParse(sel, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, parsed) Then Return parsed
        If Double.TryParse(sel, parsed) Then Return parsed

        Return groups(0)
    End Function

    Private Function BuildMMRMVisitGroupDifferencesTableControlled(result As regression.MixedModelResult,
                                                               x(,) As Double,
                                                               visit() As Double,
                                                               groupValues() As Double,
                                                               groupName As String,
                                                               alpha As Double,
                                                               contrastMode As String,
                                                               controlLevel As Double,
                                                               direction As String) As ResultTable
        Dim groups() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)

        Dim comparisonLevel As Double = Double.NaN
        If String.Equals(contrastMode, MMRM_MODE_SELECTED, StringComparison.OrdinalIgnoreCase) Then
            If Not ResolveSelectedComparisonLevel(groups, controlLevel, comparisonLevel) Then
                Throw New ApplicationException("Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level.")
            End If
        End If

        Return regression.MMRMPostEstimation.BuildVisitGroupDifferencesTableControlled(result:=result,
                                                                                   x:=x,
                                                                                   visit:=visit,
                                                                                   groupValues:=groupValues,
                                                                                   groupName:=groupName,
                                                                                   alpha:=alpha,
                                                                                   contrastMode:=contrastMode,
                                                                                   controlLevel:=controlLevel,
                                                                                   comparisonLevel:=comparisonLevel,
                                                                                   direction:=direction)
    End Function

    Private Function BuildMMRMDifferenceInChangeFromBaselineTableControlled(result As regression.MixedModelResult,
                                                                        x(,) As Double,
                                                                        visit() As Double,
                                                                        groupValues() As Double,
                                                                        groupName As String,
                                                                        baseline As Double,
                                                                        alpha As Double,
                                                                        contrastMode As String,
                                                                        controlLevel As Double,
                                                                        direction As String) As ResultTable
        Dim groups() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)

        Dim comparisonLevel As Double = Double.NaN
        If String.Equals(contrastMode, MMRM_MODE_SELECTED, StringComparison.OrdinalIgnoreCase) Then
            If Not ResolveSelectedComparisonLevel(groups, controlLevel, comparisonLevel) Then
                Throw New ApplicationException("Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level.")
            End If
        End If

        Return regression.MMRMPostEstimation.BuildDifferenceInChangeFromBaselineTableControlled(result:=result,
                                                                                           x:=x,
                                                                                           visit:=visit,
                                                                                           groupValues:=groupValues,
                                                                                           groupName:=groupName,
                                                                                           baseline:=baseline,
                                                                                           alpha:=alpha,
                                                                                           contrastMode:=contrastMode,
                                                                                           controlLevel:=controlLevel,
                                                                                           comparisonLevel:=comparisonLevel,
                                                                                           direction:=direction)
    End Function

    Private Function BuildMMRMReferenceGridSpec(result As regression.MixedModelResult,
                                            fixedNames() As String,
                                            visit() As Double,
                                            mmrmData As MmrmData,
                                            hasGroup As Boolean,
                                            groupKey As String,
                                            groupValues() As Double,
                                            alphaValue As Double) As regression.MixedModelReferenceGridSpec
        If result Is Nothing OrElse mmrmData Is Nothing OrElse mmrmData.Raw Is Nothing Then Return Nothing

        Dim spec As New regression.MixedModelReferenceGridSpec With {
        .FixedEffectNames = If(fixedNames Is Nothing, result.FixedEffectNames, fixedNames),
        .Weighting = SelectedReferenceGridWeighting(),
        .MultiplicityAdjustment = SelectedMultiplicityAdjustment(),
        .Alpha = alphaValue
    }

        Dim visitProfileName As String = ReferenceGridProfileName(mmrmData.VisitKey)
        Dim visitLevels() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
        If visitLevels IsNot Nothing AndAlso visitLevels.Length > 0 Then
            spec.AddByFactor(visitProfileName, visitLevels, visit)
        End If

        Dim groupProfileName As String = Nothing
        If hasGroup AndAlso Not String.IsNullOrWhiteSpace(groupKey) AndAlso groupValues IsNot Nothing Then
            groupProfileName = ReferenceGridProfileName(groupKey)
            Dim groupLevels() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)
            If groupLevels IsNot Nothing AndAlso groupLevels.Length > 1 Then
                spec.AddByFactor(groupProfileName, groupLevels, groupValues)
            End If
        End If

        AddReferenceGridMarginalFactors(spec, mmrmData, visitProfileName, groupProfileName)
        AddReferenceGridContinuousCovariates(spec, mmrmData, visitProfileName, groupProfileName)

        Return spec
    End Function

    Private Function UseObservedMeanReferenceCovariates() As Boolean
        Return Not String.Equals(SelectedComboText(Me.cbMMRMRefGridCovariates, MMRM_RG_COVARIATE_MEANS),
                             MMRM_RG_COVARIATE_ZERO,
                             StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function SelectedMultiplicityAdjustment() As regression.MixedModelMultiplicityAdjustment
        Dim txt As String = SelectedComboText(Me.cbMMRMMultiplicity, MMRM_MULT_NONE)

        If String.Equals(txt, MMRM_MULT_BONFERRONI, StringComparison.OrdinalIgnoreCase) Then
            Return regression.MixedModelMultiplicityAdjustment.Bonferroni
        End If

        If String.Equals(txt, MMRM_MULT_HOLM, StringComparison.OrdinalIgnoreCase) Then
            Return regression.MixedModelMultiplicityAdjustment.Holm
        End If

        If String.Equals(txt, MMRM_MULT_SIDAK, StringComparison.OrdinalIgnoreCase) Then
            Return regression.MixedModelMultiplicityAdjustment.Sidak
        End If

        Return regression.MixedModelMultiplicityAdjustment.None
    End Function

    Private Function SelectedReferenceGridWeighting() As regression.MixedModelReferenceGridWeighting
        Dim txt As String = SelectedComboText(Me.cbMMRMRefGridWeighting, MMRM_RG_WEIGHT_EQUAL)

        If String.Equals(txt, MMRM_RG_WEIGHT_OBSERVED, StringComparison.OrdinalIgnoreCase) Then
            Return regression.MixedModelReferenceGridWeighting.ObservedCellFrequency
        End If

        Return regression.MixedModelReferenceGridWeighting.EqualCells
    End Function

    Private Sub AddReferenceGridMarginalFactors(spec As regression.MixedModelReferenceGridSpec,
                                            mmrmData As MmrmData,
                                            visitProfileName As String,
                                            groupProfileName As String)
        Dim factorKeys As List(Of String) = GetMMRMCategoricalFactorBaseKeys()
        If factorKeys Is Nothing Then Exit Sub

        For Each key As String In factorKeys
            Dim profileName As String = ReferenceGridProfileName(key)

            If String.Equals(profileName, visitProfileName, StringComparison.OrdinalIgnoreCase) Then Continue For
            If Not String.IsNullOrWhiteSpace(groupProfileName) AndAlso
           String.Equals(profileName, groupProfileName, StringComparison.OrdinalIgnoreCase) Then Continue For

            Try
                Dim col As Integer = ResolveDataColumnIndex(mmrmData.Raw, key, "reference-grid marginal factor")
                Dim values() As Double = ExtractNumericColumnFromData(mmrmData.Raw, col)
                Dim levels() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(values)

                If levels IsNot Nothing AndAlso levels.Length > 1 Then
                    spec.AddMarginalFactor(profileName, levels, values)
                End If

            Catch ex As Exception
                CoreServices.Logger.Debug("Reference-grid marginal factor skipped '" & key & "': " & ex.Message)
            End Try
        Next
    End Sub


    Private Sub AddReferenceGridContinuousCovariates(spec As regression.MixedModelReferenceGridSpec,
                                                 mmrmData As MmrmData,
                                                 visitProfileName As String,
                                                 groupProfileName As String)
        Dim covariateKeys As List(Of String) = GetMMRMContinuousMainEffectBaseKeys()
        If covariateKeys Is Nothing Then Exit Sub

        For Each key As String In covariateKeys
            Dim profileName As String = ReferenceGridProfileName(key)

            If String.Equals(profileName, visitProfileName, StringComparison.OrdinalIgnoreCase) Then Continue For
            If Not String.IsNullOrWhiteSpace(groupProfileName) AndAlso
           String.Equals(profileName, groupProfileName, StringComparison.OrdinalIgnoreCase) Then Continue For

            Try
                Dim col As Integer = ResolveDataColumnIndex(mmrmData.Raw, key, "reference-grid continuous covariate")
                Dim values() As Double = ExtractNumericColumnFromData(mmrmData.Raw, col)

                If UseObservedMeanReferenceCovariates() Then
                    spec.AddCovariateMean(profileName, values)
                Else
                    spec.AddCovariateValue(profileName, values, 0.0)
                End If

            Catch ex As Exception
                CoreServices.Logger.Debug("Reference-grid continuous covariate skipped '" & key & "': " & ex.Message)
            End Try
        Next
    End Sub

    Private Function BuildMMRMReferenceGridGroupContrastsTable(result As regression.MixedModelResult,
                                                          rows As List(Of regression.MixedModelReferenceGridRow),
                                                          spec As regression.MixedModelReferenceGridSpec,
                                                          groupKey As String,
                                                          contrastMode As String,
                                                          controlLevel As Double,
                                                          contrastDirection As String) As ResultTable
        If rows Is Nothing OrElse rows.Count = 0 Then Return Nothing

        Dim groupProfileName As String = ReferenceGridProfileName(groupKey)

        If String.Equals(contrastMode, MMRM_MODE_PAIRWISE, StringComparison.OrdinalIgnoreCase) Then
            Return regression.MixedModelReferenceGridService.BuildPairwiseContrastsByFactor(rows,
                                                                                       result,
                                                                                       spec,
                                                                                       groupProfileName,
                                                                                       "Reference-grid group contrasts")
        End If

        Dim groupLevels() As Double =
        regression.MixedModelReferenceGridService.GetLevelsFromRows(rows, groupProfileName)

        Dim comparisonLevel As Double = Double.NaN
        Dim useSingleComparison As Boolean = False

        If String.Equals(contrastMode, MMRM_MODE_SELECTED, StringComparison.OrdinalIgnoreCase) Then
            If Not ResolveSelectedComparisonLevel(groupLevels, controlLevel, comparisonLevel) Then
                Throw New ApplicationException("Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level.")
            End If

            useSingleComparison = True
        End If

        Return regression.MixedModelReferenceGridService.BuildContrastsAgainstControlByFactor(
                    rows:=rows,
                    result:=result,
                    spec:=spec,
                    factorName:=groupProfileName,
                    controlLevel:=controlLevel,
                    comparisonLevel:=comparisonLevel,
                    useSingleComparison:=useSingleComparison,
                    direction:=contrastDirection,
                    treatmentMinusControlText:=MMRM_DIR_TREATMENT_MINUS_CONTROL,
                    controlMinusTreatmentText:=MMRM_DIR_CONTROL_MINUS_TREATMENT,
                    title:="Reference-grid group contrasts")
    End Function

    Private Function IsSameProfileGroupComparisonCandidate(candidate As regression.MixedModelReferenceGridRow,
                                                       rows As List(Of regression.MixedModelReferenceGridRow),
                                                       groupProfileName As String) As Boolean
        If candidate Is Nothing OrElse candidate.Profile Is Nothing Then Return False
        If Not candidate.Profile.ContainsKey(groupProfileName) Then Return False
        Return True
    End Function


    Private Function FindMatchingReferenceGridRow(rows As List(Of regression.MixedModelReferenceGridRow),
                                              profile As Dictionary(Of String, Double),
                                              groupProfileName As String,
                                              targetGroupLevel As Double) As regression.MixedModelReferenceGridRow
        For Each row As regression.MixedModelReferenceGridRow In rows
            If row Is Nothing OrElse row.Profile Is Nothing Then Continue For
            If Not row.Profile.ContainsKey(groupProfileName) Then Continue For
            If Not regression.MixedModelPostEstimation.NearlyEqual(row.Profile(groupProfileName), targetGroupLevel) Then Continue For

            Dim same As Boolean = True
            For Each kvp As KeyValuePair(Of String, Double) In profile
                If String.Equals(kvp.Key, groupProfileName, StringComparison.OrdinalIgnoreCase) Then Continue For
                If Not row.Profile.ContainsKey(kvp.Key) Then same = False : Exit For
                If Not regression.MixedModelPostEstimation.NearlyEqual(row.Profile(kvp.Key), kvp.Value) Then same = False : Exit For
            Next

            If same Then Return row
        Next

        Return Nothing
    End Function


    Private Function GetLevelsFromReferenceGridRows(rows As List(Of regression.MixedModelReferenceGridRow),
                                                factorName As String) As Double()
        Dim vals As New List(Of Double)()

        For Each row As regression.MixedModelReferenceGridRow In rows
            If row Is Nothing OrElse row.Profile Is Nothing Then Continue For
            If Not row.Profile.ContainsKey(factorName) Then Continue For

            Dim v As Double = row.Profile(factorName)
            Dim found As Boolean = False

            For Each existing As Double In vals
                If regression.MixedModelPostEstimation.NearlyEqual(existing, v) Then
                    found = True
                    Exit For
                End If
            Next

            If Not found Then vals.Add(v)
        Next

        vals.Sort()
        Return vals.ToArray()
    End Function


    Private Function ReferenceGridOtherProfileSuffix(profile As Dictionary(Of String, Double),
                                                 groupProfileName As String) As String
        If profile Is Nothing Then Return String.Empty

        Dim parts As New List(Of String)()

        For Each kvp As KeyValuePair(Of String, Double) In profile
            If String.Equals(kvp.Key, groupProfileName, StringComparison.OrdinalIgnoreCase) Then Continue For
            parts.Add(kvp.Key & "=" & regression.MixedModelPostEstimation.FormatProfileValue(kvp.Value))
        Next

        If parts.Count = 0 Then Return String.Empty
        Return " | " & String.Join(", ", parts)
    End Function

    Private Function ReferenceGridProfileName(rawKey As String) As String
        Return regression.MixedModelReferenceGridService.NormalizeProfileName(rawKey)
    End Function

    Private Function GetMMRMContinuousMainEffectBaseKeys() As List(Of String)
        Dim out As New List(Of String)

        If Me.lbSelectedEffectsList Is Nothing OrElse Me.TermSpecs Is Nothing Then Return out

        For Each it As Object In Me.lbSelectedEffectsList.Items
            Dim effKey As String = CStr(it)
            If String.IsNullOrWhiteSpace(effKey) Then Continue For
            If Not Me.TermSpecs.ContainsKey(effKey) Then Continue For

            Dim spec As TermSpec = Me.TermSpecs(effKey)
            If spec Is Nothing Then Continue For
            If Not String.Equals(spec.Kind, "MainEffect", StringComparison.OrdinalIgnoreCase) Then Continue For
            If spec.Scale <> PredictorScale.Continuous Then Continue For
            If spec.BaseVarKeys Is Nothing OrElse spec.BaseVarKeys.Count <> 1 Then Continue For

            If Not out.Contains(spec.BaseVarKeys(0)) Then out.Add(spec.BaseVarKeys(0))
        Next

        Return out
    End Function

    ''' <summary>
    ''' Builds and appends one optional MMRM table.  Failures are logged but do not
    ''' stop the rest of the MMRM output pipeline.
    ''' </summary>
    Private Sub AddOptionalMMRMTable(result As regression.MixedModelResult,
                                 tableFactory As Func(Of ResultTable),
                                 tableLabel As String)
        If result Is Nothing OrElse tableFactory Is Nothing Then Exit Sub
        If result.AdditionalResultTables Is Nothing Then result.AdditionalResultTables = New List(Of ResultTable)()

        Try
            Dim t As ResultTable = tableFactory.Invoke()
            If t Is Nothing Then
                CoreServices.Logger.Debug("MMRM optional table not created: " & tableLabel)
                Exit Sub
            End If

            result.AdditionalResultTables.Add(t)
            CoreServices.Logger.Debug("MMRM optional table added: " & tableLabel)

        Catch ex As Exception
            CoreServices.Logger.Warn("MMRM optional table failed: " & tableLabel & ": " & ex.ToString())
        End Try
    End Sub

    ''' <summary>
    ''' Populates comparison-level ComboBox from observed numeric group levels.
    ''' </summary>
    Private Sub RefreshMMRMComparisonLevelCombo(groupLevels As Double(),
                                           Optional selectedControlLevel As Nullable(Of Double) = Nothing)

        Dim oldSelection As String = If(Me.cbMMRMComparisonLevel.SelectedItem Is Nothing,
                                    MMRM_GROUP_AUTO,
                                    CStr(Me.cbMMRMComparisonLevel.SelectedItem))

        Me.cbMMRMComparisonLevel.Items.Clear()
        Me.cbMMRMComparisonLevel.Items.Add(MMRM_GROUP_AUTO)

        If groupLevels IsNot Nothing Then
            Dim groups() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupLevels)
            If groups IsNot Nothing Then
                For Each g As Double In groups
                    If Double.IsNaN(g) OrElse Double.IsInfinity(g) Then Continue For
                    Me.cbMMRMComparisonLevel.Items.Add(regression.MixedModelPostEstimation.FormatProfileValue(g))
                Next
            End If
        End If

        Dim idx As Integer = Me.cbMMRMComparisonLevel.FindStringExact(oldSelection)
        If idx >= 0 Then
            Me.cbMMRMComparisonLevel.SelectedIndex = idx
        Else
            Me.cbMMRMComparisonLevel.SelectedIndex = 0
        End If

        UpdateMMRMContrastControlEnabledState()
    End Sub

    ''' <summary>
    ''' Resolves selected comparison/treatment level. If cbMMRMComparisonLevel is absent
    ''' or set to Auto, returns the first observed level different from controlLevel.
    ''' </summary>
    Private Function ResolveSelectedComparisonLevel(groupLevels As Double(),
                                                    controlLevel As Double,
                                                    ByRef comparisonLevel As Double) As Boolean

        comparisonLevel = Double.NaN

        If groupLevels Is Nothing OrElse groupLevels.Length = 0 Then Return False

        Dim cb = Me.cbMMRMComparisonLevel
        Dim selectedText As String = If(cb Is Nothing OrElse cb.SelectedItem Is Nothing,
                                        "(Auto)",
                                        CStr(cb.SelectedItem).Trim())

        If selectedText.Length = 0 OrElse selectedText.Equals("(Auto)", StringComparison.OrdinalIgnoreCase) Then
            For Each g As Double In groupLevels
                If Not regression.MixedModelPostEstimation.NearlyEqual(g, controlLevel) Then
                    comparisonLevel = g
                    Return True
                End If
            Next
            Return False
        End If

        Dim parsed As Double
        If Double.TryParse(selectedText, Globalization.NumberStyles.Any, Globalization.CultureInfo.CurrentCulture, parsed) _
           OrElse Double.TryParse(selectedText, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, parsed) Then

            If regression.MixedModelPostEstimation.NearlyEqual(parsed, controlLevel) Then Return False

            comparisonLevel = parsed
            Return True
        End If

        For Each g As Double In groupLevels
            If String.Equals(regression.MixedModelPostEstimation.FormatProfileValue(g), selectedText, StringComparison.OrdinalIgnoreCase) Then
                If regression.MixedModelPostEstimation.NearlyEqual(g, controlLevel) Then Return False
                comparisonLevel = g
                Return True
            End If
        Next

        Return False
    End Function

    ''' <summary>
    ''' Resolves contrast direction into first-second levels.
    ''' </summary>
    Private Sub ResolveDirectedGroupPair(controlLevel As Double,
                                         comparisonLevel As Double,
                                         ByRef firstLevel As Double,
                                         ByRef secondLevel As Double)

        Dim direction As String = If(Me.cbMMRMContrastDirection Is Nothing OrElse
                                     Me.cbMMRMContrastDirection.SelectedItem Is Nothing,
                                     "Higher level - lower level",
                                     CStr(Me.cbMMRMContrastDirection.SelectedItem).Trim())

        If direction.Equals("Treatment - control", StringComparison.OrdinalIgnoreCase) Then
            firstLevel = comparisonLevel
            secondLevel = controlLevel
            Return
        End If

        If direction.Equals("Control - treatment", StringComparison.OrdinalIgnoreCase) Then
            firstLevel = controlLevel
            secondLevel = comparisonLevel
            Return
        End If

        If comparisonLevel >= controlLevel Then
            firstLevel = comparisonLevel
            secondLevel = controlLevel
        Else
            firstLevel = controlLevel
            secondLevel = comparisonLevel
        End If
    End Sub

    ''' <summary>
    ''' Attempts to populate data-dependent LS-means/contrast controls from the current
    ''' worksheet selections before the model is fitted.
    ''' </summary>
    ''' <remarks>
    ''' This routine is intentionally non-throwing.  During dialog editing the user may
    ''' not yet have selected response, subject, visit, or fixed-effect variables.  In
    ''' that case the method leaves the placeholder values in place.
    ''' </remarks>
    Private Sub TryRefreshMMRMContrastControlsFromCurrentSelections()
        Try
            If Not HasEnoughMMRMSelectionsForLevelRefresh() Then
                UpdateMMRMContrastControlEnabledState()
                Exit Sub
            End If

            Dim mmrmData As MmrmData = GetData()
            If mmrmData Is Nothing OrElse mmrmData.Raw Is Nothing Then Exit Sub
            If mmrmData.Raw.bZeroValid Then Exit Sub

            Dim raw As DataObj = mmrmData.Raw

            Dim visitCol As Integer = ResolveDataColumnIndex(raw, mmrmData.VisitKey, "visit/time")
            Dim visitValues() As Double = ExtractNumericColumnFromData(raw, visitCol)

            Dim groupKey As String = Nothing
            Dim groupValues() As Double = Nothing
            Dim hasGroup As Boolean = TryResolveMMRMGroupingVariableFromControls(mmrmData, groupKey, groupValues)

            RefreshMMRMContrastControlsFromData(mmrmData, visitValues, If(hasGroup, groupValues, Nothing))

            CoreServices.Logger.Debug("MMRM contrast controls pre-fit refresh completed. hasGroup=" &
                                    hasGroup.ToString() & "; groupKey='" & If(groupKey, String.Empty) & "'.")

        Catch ex As Exception
            ' Non-fatal.  The same controls will be refreshed again after successful
            ' data import during Fit.
            CoreServices.Logger.Debug("MMRM contrast controls pre-fit refresh skipped: " & ex.Message)
            UpdateMMRMContrastControlEnabledState()
        End Try
    End Sub

    ''' <summary>
    ''' Returns True when the current dialog selections are sufficient to import the
    ''' small working data block needed to populate visit/group level controls.
    ''' </summary>
    Private Function HasEnoughMMRMSelectionsForLevelRefresh() As Boolean
        If Me.lbY Is Nothing OrElse Me.lbY.Items.Count <> 1 Then Return False
        If Me.lbClusterID Is Nothing OrElse Me.lbClusterID.Items.Count <> 1 Then Return False
        If Me.lbTime Is Nothing OrElse Me.lbTime.Items.Count <> 1 Then Return False
        If Me.lbSelectedEffectsList Is Nothing OrElse Me.lbSelectedEffectsList.Items.Count = 0 Then Return False
        If Me.VariableColumnsInfo Is Nothing OrElse Me.VariableColumnsInfo.Count = 0 Then Return False
        If Me.pWorksheet Is Nothing Then Return False
        Return True
    End Function

    Private Function IsMMRMCancellationRequested() As Boolean
        Return pMmrmCancelRequested
    End Function

    Private Function IsMMRMInterruptionRequested() As Boolean
        Return pMmrmInterruptRequested AndAlso Not pMmrmCancelRequested
    End Function

    Private Sub Ui18MMRM_FormClosing(sender As Object, e As Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not pMmrmCalculationRunning Then Exit Sub

        pMmrmCancelRequested = True
        pMmrmCloseAfterCancel = True
        e.Cancel = True

        Try
            Me.lblProgress.Text = "Cancelling MMRM..."
            Me.ProgressBar1.Style = Windows.Forms.ProgressBarStyle.Marquee
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = False
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

    Private Sub ResetMMRMProgress()
        Try
            Me.ProgressBar1.Minimum = 0
            Me.ProgressBar1.Maximum = 100
            Me.ProgressBar1.Style = Windows.Forms.ProgressBarStyle.Continuous
            Me.ProgressBar1.Value = 0
            pMmrmProgressStopwatch = System.Diagnostics.Stopwatch.StartNew()
            pLastMmrmProgressRefreshTimestamp = 0
            Me.lblProgress.Text = "Preparing MMRM..."
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = True
            RefreshMMRMProgressControls(True)
        Catch
        End Try
    End Sub

    Private Sub ReportMMRMProgressFromAnyThread(info As regression.MixedModelProgressInfo)
        Try
            If Me.IsDisposed Then Exit Sub

            If Me.InvokeRequired Then
                Me.BeginInvoke(New Action(Of regression.MixedModelProgressInfo)(AddressOf UpdateMMRMProgress), info)
            Else
                UpdateMMRMProgress(info)
            End If
        Catch
            ' Progress reporting must never interrupt the background fit.
        End Try
    End Sub

    Private Sub UpdateMMRMProgress(info As regression.MixedModelProgressInfo)
        Try
            If info Is Nothing Then Exit Sub

            Dim value As Integer = Math.Max(Me.ProgressBar1.Minimum, Math.Min(Me.ProgressBar1.Maximum, info.Percent))
            Me.ProgressBar1.Value = value

            Dim msg As String = If(pMmrmCancelRequested, "Cancelling MMRM", If(pMmrmInterruptRequested, "Interrupting MMRM", info.Stage))
            Dim elapsedSecondsText As String = UIprocedures.FormatProgressElapsedSeconds(Me.pMmrmProgressStopwatch)
            If info.Iteration >= 0 AndAlso info.MaxIterations > 0 Then
                If elapsedSecondsText.Length > 0 Then msg &= " " & elapsedSecondsText
                msg &= " (" & info.Iteration.ToString() & "/" & info.MaxIterations.ToString() & ")"
            ElseIf elapsedSecondsText.Length > 0 Then
                msg &= " " & elapsedSecondsText
            End If

            Dim convergenceParts As New List(Of String)
            If IsFinite(info.Objective) Then convergenceParts.Add("f=" & FormatProgressDouble(info.Objective))
            If IsFinite(info.FunctionChange) Then convergenceParts.Add("Δf=" & FormatProgressDouble(info.FunctionChange))
            If IsFinite(info.GradNorm) Then convergenceParts.Add("|g|=" & FormatProgressDouble(info.GradNorm))

            If convergenceParts.Count > 0 Then
                msg &= " | " & String.Join("; ", convergenceParts.ToArray())
            End If

            If Not String.IsNullOrWhiteSpace(info.Message) Then
                msg &= " - " & info.Message
            End If

            Me.lblProgress.Text = msg
            RefreshMMRMProgressControls(False)
        Catch
        End Try
    End Sub

    Private Sub RefreshMMRMProgressControls(Optional force As Boolean = False)
        If pMmrmProgressRefreshActive Then Exit Sub

        Try
            Dim nowTicks As Long = System.Diagnostics.Stopwatch.GetTimestamp()

            If Not force AndAlso pLastMmrmProgressRefreshTimestamp <> 0 Then
                Dim elapsedMs As Double = (CDbl(nowTicks - pLastMmrmProgressRefreshTimestamp) * 1000.0) / CDbl(System.Diagnostics.Stopwatch.Frequency)
                If elapsedMs < MMRM_PROGRESS_REFRESH_INTERVAL_MS Then Exit Sub
            End If

            pLastMmrmProgressRefreshTimestamp = nowTicks
            pMmrmProgressRefreshActive = True

            If Me.lblProgress IsNot Nothing AndAlso Not Me.lblProgress.IsDisposed Then
                Me.lblProgress.Invalidate()
                Me.lblProgress.Update()
            End If

            If Me.ProgressBar1 IsNot Nothing AndAlso Not Me.ProgressBar1.IsDisposed Then
                Me.ProgressBar1.Invalidate()
                Me.ProgressBar1.Update()
            End If
        Catch
            ' Progress painting must not interrupt fitting.
        Finally
            pMmrmProgressRefreshActive = False
        End Try
    End Sub

    Private Sub InvokeMMRMUi(action As Action)
        If action Is Nothing Then Exit Sub

        If Me.IsDisposed Then Exit Sub

        If Me.InvokeRequired Then
            If Not Me.IsHandleCreated Then Exit Sub
            Me.Invoke(action)
        Else
            action()
        End If
    End Sub

    Private Sub CompleteMMRMRunOnUiThread()
        Try
            InvokeMMRMUi(Sub()
                             pMmrmCalculationRunning = False
                             pMmrmProgressStopwatch = Nothing
                             Me.btCalculate.Enabled = True
                             Me.btInterrupt.Enabled = False

                             If pMmrmCloseAfterCancel Then
                                 pMmrmCloseAfterCancel = False
                                 Me.Close()
                             End If
                         End Sub)
        Catch
            ' Cleanup must not mask the original fit/output exception.
        End Try
    End Sub

    Private Sub FinishMMRMProgress(result As regression.MixedModelResult, success As Boolean)
        Try
            InvokeMMRMUi(Sub()
                             Me.ProgressBar1.Style = Windows.Forms.ProgressBarStyle.Continuous
                             Me.ProgressBar1.Value = If(success, 100, Math.Min(Me.ProgressBar1.Value, 99))

                             If result IsNot Nothing AndAlso result.Cancelled Then
                                 Me.lblProgress.Text = "MMRM cancelled."
                             ElseIf result IsNot Nothing AndAlso result.Interrupted Then
                                 Me.lblProgress.Text = "MMRM interrupted; latest estimates returned."
                             ElseIf result IsNot Nothing AndAlso Not Double.IsNaN(result.ExecutionTimeMs) AndAlso Not Double.IsInfinity(result.ExecutionTimeMs) Then
                                 Me.lblProgress.Text = "Elapsed Time: " & (result.ExecutionTimeMs / 1000.0).ToString("0.000", Globalization.CultureInfo.InvariantCulture) & " s"
                             Else
                                 Me.lblProgress.Text = If(success, "Completed", "Failed")
                             End If

                             Me.btCalculate.Enabled = True
                             Me.btInterrupt.Enabled = False
                             pMmrmProgressStopwatch = Nothing
                             RefreshMMRMProgressControls(True)
                         End Sub)
        Catch
            Try
                InvokeMMRMUi(Sub()
                                 Me.btCalculate.Enabled = True
                                 Me.btInterrupt.Enabled = False
                                 pMmrmProgressStopwatch = Nothing
                             End Sub)
            Catch
            End Try
        End Try
    End Sub

    Private Shared Sub ReleaseMMRMLargeRunReferences(result As regression.MixedModelResult,
                                                    model As regression.MMRM,
                                                    req As regression.MixedModelFitRequest)
        Try
            If req IsNot Nothing Then
                req.ProgressReporter = Nothing
                req.CancellationRequested = Nothing
                req.InterruptionRequested = Nothing
                req.Data = Nothing
            End If

            If result IsNot Nothing Then
                result.ReleaseLargePostEstimationWorkspaces()
            End If

            If model IsNot Nothing Then
                model.ReleaseFitState(releaseResultWorkspaces:=True,
                                      clearRequestRuntimeReferences:=True)
            End If
        Catch
            ' Memory-release helpers must never affect an already completed analysis.
        End Try
    End Sub

    Private Sub Ui18MMRM_FormClosed(sender As Object, e As Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ClearMMRMDialogReferences()
        RequestMMRMManagedCleanup()
    End Sub

    Private Sub ClearMMRMDialogReferences()
        Try
            pWorksheet = Nothing
            pWorkbook = Nothing

            If VariableColumnsInfo IsNot Nothing Then VariableColumnsInfo.Clear()
            If TermSpecs IsNot Nothing Then TermSpecs.Clear()

            pMmrmProgressStopwatch = Nothing
            pMmrmProgressRefreshActive = False
            pLastMmrmProgressRefreshTimestamp = 0
        Catch
            ' Best-effort cleanup only.
        End Try
    End Sub

    Private Shared Sub RequestMMRMManagedCleanup()
        Try
            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
            GC.Collect(GC.MaxGeneration, System.GCCollectionMode.Forced, blocking:=True, compacting:=True)
            GC.WaitForPendingFinalizers()
            GC.Collect(GC.MaxGeneration, System.GCCollectionMode.Forced, blocking:=True, compacting:=True)
        Catch
            Try
                GC.Collect()
            Catch
            End Try
        End Try
    End Sub
End Class