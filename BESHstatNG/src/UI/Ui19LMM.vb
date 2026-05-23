Option Explicit On

Imports BESHStatNG.AppInfrastructure
Imports System.Threading.Tasks

Public Class Ui19LMM
    Private pWorksheet As Object
    Private pWorkbook As Object
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information of variable/column names inported into the input listbox
    Private FixedTermSpecs As Dictionary(Of String, TermSpec)
    Private RandomTermSpecs As Dictionary(Of String, TermSpec)

    Private FixedEffectsController As RegressionEffectsController
    Private RandomEffectsController As RegressionEffectsController

    Private pLmmCalculationRunning As Boolean = False
    Private pLmmCancelRequested As Boolean = False
    Private pLmmInterruptRequested As Boolean = False
    Private pLmmCloseAfterCancel As Boolean = False
    Private pLmmProgressStopwatch As System.Diagnostics.Stopwatch = Nothing
    Private pLmmProgressRefreshActive As Boolean = False
    Private pLastLmmProgressRefreshTimestamp As Long = 0

    Private Const LMM_PROGRESS_REFRESH_INTERVAL_MS As Double = 100.0
    Private Const LMM_OPT_AI As String = "AI/Fisher scoring (default)"
    Private Const LMM_OPT_BFGS_AUTO As String = "Projected BFGS (auto gradient)"
    Private Const LMM_OPT_BFGS_ANALYTIC As String = "Projected BFGS (analytic gradient)"
    Private Const LMM_OPT_BFGS_NUMERICAL As String = "Projected BFGS (finite-difference gradient)"
    Private Const LMM_GRAD_AUTO As String = "Auto (analytic where available)"
    Private Const LMM_GRAD_ANALYTIC As String = "Analytic score"
    Private Const LMM_GRAD_VALIDATE As String = "Analytic score + finite-difference validation"
    Private Const LMM_GRAD_NUMERICAL As String = "Numerical finite difference"

    Private Class LmmGuiData
        Public Raw As DataObj
        Public SubjectKey As String
        Public ResponseKey As String
        Public VisitKey As String
    End Class

    Sub New(analysis As String)
        InitializeComponent()
        Me.tbEps.Text = FormatUiDouble(0.000001)
        Me.Text = analysis
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)
        InitializeLMMControls()
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
        Me.lbSelectedFixedEffectsList.Anchor = Windows.Forms.AnchorStyles.Left Or
                                          Windows.Forms.AnchorStyles.Right Or
                                          Windows.Forms.AnchorStyles.Top
        Me.tbRemoveSelectedFixedEffects.Anchor = Windows.Forms.AnchorStyles.Top Or
                                            Windows.Forms.AnchorStyles.Right
        Me.btClearAllSelectedFixedEffects.Anchor = Windows.Forms.AnchorStyles.Top Or
                                            Windows.Forms.AnchorStyles.Right
        Me.lbSelectedRandomEffectsList.Anchor = Windows.Forms.AnchorStyles.Left Or
                                          Windows.Forms.AnchorStyles.Right Or
                                          Windows.Forms.AnchorStyles.Top Or
                                          Windows.Forms.AnchorStyles.Bottom
        Me.tbRemoveSelectedRandomEffects.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                            Windows.Forms.AnchorStyles.Right
        Me.btClearAllSelectedRandomEffects.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                            Windows.Forms.AnchorStyles.Right

        'Term specifications for selected effects.
        'This dictionary remains owned by Ui19LMM and is passed into the shared controller
        'so both the form and the controller operate on the same backing state.
        Me.FixedTermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
        Me.RandomTermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)

        'Shared effect-authoring controller for model construction.
        Me.FixedEffectsController = New RegressionEffectsController(Me.lbSelectedVariables, Me.lbSelectedFixedEffectsList, Me.FixedTermSpecs)
        Me.RandomEffectsController = New RegressionEffectsController(Me.lbSelectedVariables, Me.lbSelectedRandomEffectsList, Me.RandomTermSpecs)

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

    Private Sub InitializeLMMControls()

        'Keep labels aligned with the LMM validation rules. Subject is mandatory; model source
        'variables are optional when intercept-only fixed/random parts are requested.
        Me.cbFitMethod.Items.Clear()
        Me.cbFitMethod.Items.AddRange(New Object() {"ML", "REML"})
        Me.cbFitMethod.SelectedIndex = 1

        Me.cbInferenceMethod.Items.Clear()
        Me.cbInferenceMethod.Items.AddRange(New Object() {"Large-sample normal", "Residual DF", "Satterthwaite", "Kenward-Roger"})
        Me.cbInferenceMethod.SelectedIndex = 3

        Me.cbCovarStruct.Items.Clear()
        For Each s As String In regression.MixedModelRStruct.RStructsList
            Me.cbCovarStruct.Items.Add(s)
        Next
        Me.cbCovarStruct.SelectedItem = "Identity"
        If Me.cbCovarStruct.SelectedIndex < 0 AndAlso Me.cbCovarStruct.Items.Count > 0 Then Me.cbCovarStruct.SelectedIndex = 0

        Me.cbRandomCovarStruct.Items.Clear()
        For Each s As String In regression.MixedModelGStruct.GStructsList
            If Not String.Equals(s, "None", StringComparison.OrdinalIgnoreCase) Then
                Me.cbRandomCovarStruct.Items.Add(s)
            End If
        Next
        If Me.cbRandomCovarStruct.SelectedIndex < 0 AndAlso Me.cbRandomCovarStruct.Items.Count > 0 Then Me.cbRandomCovarStruct.SelectedIndex = 0

        If Me.cbLMMCovOptimizerMode IsNot Nothing Then
            Me.cbLMMCovOptimizerMode.Items.Clear()
            Me.cbLMMCovOptimizerMode.Items.AddRange(New Object() {LMM_OPT_AI, LMM_OPT_BFGS_AUTO, LMM_OPT_BFGS_ANALYTIC, LMM_OPT_BFGS_NUMERICAL})
            Me.cbLMMCovOptimizerMode.SelectedItem = LMM_OPT_AI
        End If

        If Me.cbLMMCovGradientMode IsNot Nothing Then
            Me.cbLMMCovGradientMode.Items.Clear()
            Me.cbLMMCovGradientMode.Items.AddRange(New Object() {LMM_GRAD_AUTO, LMM_GRAD_ANALYTIC, LMM_GRAD_VALIDATE, LMM_GRAD_NUMERICAL})
            Me.cbLMMCovGradientMode.SelectedItem = LMM_GRAD_AUTO
        End If

        Me.cbFixedIntercept.Checked = True
        Me.cbRandomIntercept.Checked = True
        Me.ckResiduals.Checked = True
        Me.ckLMMGCovarianceMatrix.Checked = True
        Me.ckLMMRCovarianceMatrix.Checked = False
        Me.ckLMMRandomEffects.Checked = False
        Me.ckLMMClassInfo.Checked = True
        Me.tbEps.Text = FormatUiDouble(0.000001)

        UpdateLMMCovarianceDependentControls()
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

        If Not TryRequireNumericColumn(CStr(Me.lbY.Items(0)), "Response variable", strWarning) Then
            bWait = True
            Exit Sub
        End If

        If Me.lbTime.Items.Count = 1 AndAlso Not TryRequireNumericColumn(CStr(Me.lbTime.Items(0)), "Visit / Time / Ordering variable", strWarning) Then
            bWait = True
            Exit Sub
        End If

        Dim rawFixedKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedFixedEffectsList.Items, Me.FixedTermSpecs)
        For Each rawKey As String In rawFixedKeys
            If Not TryRequireNumericColumn(rawKey, "Fixed-effect variable", strWarning) Then
                bWait = True
                Exit Sub
            End If
        Next

        Dim rawRandomKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedRandomEffectsList.Items, Me.RandomTermSpecs)
        For Each rawKey As String In rawRandomKeys
            If Not TryRequireNumericColumn(rawKey, "Random-effect variable", strWarning) Then
                bWait = True
                Exit Sub
            End If
        Next

        If Me.lbSelectedFixedEffectsList.Items.Count = 0 AndAlso Not Me.cbFixedIntercept.Checked Then
            strWarning = "No fixed effects were specified and the fixed intercept is disabled."
            bWait = True
            Exit Sub
        End If

        If Me.lbSelectedRandomEffectsList.Items.Count = 0 AndAlso Not Me.cbRandomIntercept.Checked Then
            strWarning = "No random effects were specified and the random intercept is disabled."
            bWait = True
            Exit Sub
        End If

        If Me.cbCovarStruct.SelectedItem Is Nothing Then
            strWarning = "Please select an R-side residual covariance structure."
            bWait = True
            Exit Sub
        End If

        If regression.MixedModelFrontEndHelpers.ResidualStructureRequiresVisit(CStr(Me.cbCovarStruct.SelectedItem)) AndAlso Me.lbTime.Items.Count <> 1 Then
            strWarning = "Please select a Visit / Time / Ordering variable for the selected visit-indexed residual covariance structure."
            bWait = True
            Exit Sub
        End If

        If Me.cbRandomCovarStruct.SelectedItem Is Nothing Then
            strWarning = "Please select a G-side random-effects covariance structure."
            bWait = True
            Exit Sub
        End If

        If Me.cbFitMethod.SelectedItem Is Nothing Then
            strWarning = "Please select ML or REML."
            bWait = True
            Exit Sub
        End If

        If Me.cbLMMCovOptimizerMode IsNot Nothing AndAlso Me.cbLMMCovOptimizerMode.SelectedItem Is Nothing Then
            strWarning = "Please select a covariance optimizer mode."
            bWait = True
            Exit Sub
        End If

        If Me.cbLMMCovGradientMode IsNot Nothing AndAlso Me.cbLMMCovGradientMode.SelectedItem Is Nothing Then
            strWarning = "Please select a covariance gradient mode."
            bWait = True
            Exit Sub
        End If

        Dim qAuthoring As Integer = Me.lbSelectedRandomEffectsList.Items.Count + If(Me.cbRandomIntercept.Checked, 1, 0)
        Dim gStructText As String = CStr(Me.cbRandomCovarStruct.SelectedItem)

        If String.Equals(gStructText, "Random Intercept", StringComparison.OrdinalIgnoreCase) AndAlso
           (qAuthoring <> 1 OrElse Not Me.cbRandomIntercept.Checked OrElse Me.lbSelectedRandomEffectsList.Items.Count <> 0) Then
            strWarning = "Random Intercept covariance requires Random Intercepts enabled and no authored random slopes/effects. For slope-only, categorical, interaction, polynomial, or multiple random effects, choose Variance Components (VC/Diag) or Unstructured Random Effects."
            bWait = True
            Exit Sub
        End If

        If String.Equals(gStructText, "Random Intercept + Slope", StringComparison.OrdinalIgnoreCase) AndAlso
            (qAuthoring <> 2 OrElse Not Me.cbRandomIntercept.Checked OrElse Me.lbSelectedRandomEffectsList.Items.Count <> 1) Then
            strWarning = "Random Intercept + Slope covariance requires Random Intercepts enabled plus exactly one authored random slope/effect. For multiple random effects or interactions, choose Variance Components (VC/Diag) or Unstructured Random Effects."
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

    Private Function GetData() As LmmGuiData

        Dim keys As New List(Of String)

        ' Subject first so character subject IDs can be imported with CharCols:=0.
        keys.Add(CStr(Me.lbClusterID.Items(0)))

        ' Response second.
        keys.Add(CStr(Me.lbY.Items(0)))

        ' Optional visit/order third.
        Dim visitKey As String = String.Empty
        If Me.lbTime.Items.Count = 1 Then
            visitKey = CStr(Me.lbTime.Items(0))
            If Not keys.Contains(visitKey) Then keys.Add(visitKey)
        End If

        Dim fixedRawKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedFixedEffectsList.Items, Me.FixedTermSpecs)
        Dim randomRawKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedRandomEffectsList.Items, Me.RandomTermSpecs)

        For Each xKey As String In fixedRawKeys
            If Not keys.Contains(xKey) Then keys.Add(xKey)
        Next

        For Each zKey As String In randomRawKeys
            If Not keys.Contains(zKey) Then keys.Add(zKey)
        Next

        Dim ref As String = BuildExcelRefList(pWorksheet, keys, Me.VariableColumnsInfo)

        Dim d As New DataObj()
        ExcelDnaDataImporter.ImportInto(d, ref, True, CharCols:=0)

        Return New LmmGuiData With {
            .Raw = d,
            .SubjectKey = CStr(Me.lbClusterID.Items(0)),
            .ResponseKey = CStr(Me.lbY.Items(0)),
            .visitKey = visitKey
        }

    End Function

    Private Sub BuildExpandedModelInputs(lmmData As LmmGuiData,
                                         ByRef y() As Double,
                                         ByRef x(,) As Double,
                                         ByRef fixedNames() As String,
                                         ByRef z(,) As Double,
                                         ByRef randomNames() As String)

        If lmmData Is Nothing OrElse lmmData.Raw Is Nothing Then
            CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(lmmData)))
        End If

        Dim raw As DataObj = lmmData.Raw
        Dim nRows As Integer = raw.nRows

        If nRows <= 0 Then
            CoreServices.Errors.LogAndThrow(New ApplicationException("No valid observations are available for LMM."))
        End If

        Dim yCol As Integer = ResolveDataColumnIndex(raw, lmmData.ResponseKey, "response")
        y = regression.MixedModelFrontEndHelpers.ExtractNumericColumnFromData(raw, yCol)

        BuildExpandedDesignFromEffects(raw:=raw,
                                       effectItems:=Me.lbSelectedFixedEffectsList.Items,
                                       termSpecs:=Me.FixedTermSpecs,
                                       includeIntercept:=Me.cbFixedIntercept.Checked,
                                       role:="fixed-effect",
                                       design:=x,
                                       designNames:=fixedNames)

        BuildExpandedDesignFromEffects(raw:=raw,
                                       effectItems:=Me.lbSelectedRandomEffectsList.Items,
                                       termSpecs:=Me.RandomTermSpecs,
                                       includeIntercept:=Me.cbRandomIntercept.Checked,
                                       role:="random-effect",
                                       design:=z,
                                       designNames:=randomNames)

    End Sub

    Private Sub BuildExpandedDesignFromEffects(raw As DataObj,
                                               effectItems As IEnumerable,
                                               termSpecs As Dictionary(Of String, TermSpec),
                                               includeIntercept As Boolean,
                                               role As String,
                                               ByRef design(,) As Double,
                                               ByRef designNames() As String)

        regression.MixedModelFrontEndHelpers.BuildExpandedDesignFromEffectSpecs(raw:=raw,
                                                                                effectItems:=effectItems,
                                                                                termSpecs:=termSpecs,
                                                                                includeIntercept:=includeIntercept,
                                                                                role:=role,
                                                                                design:=design,
                                                                                designNames:=designNames,
                                                                                analysisLabel:="LMM")

    End Sub

    Private Async Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        Try
            Dim bWait As Boolean, strWarning As String
            Me.pWorkbook.activate

            strWarning = String.Empty
            ValidateInputs(bWait, strWarning)
            If bWait Then
                If strWarning <> String.Empty Then MsgBox(strWarning)
                Exit Sub
            End If

            Dim myData As LmmGuiData = GetData()
            If myData.Raw.bZeroValid Then
                MsgBox("No valid observations")
                Exit Sub
            End If

            Await Me.RunLMMAsync(myData)

        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Async Function RunLMMAsync(lmmGuiData As LmmGuiData) As Task

        Dim y() As Double = Nothing
        Dim x(,) As Double = Nothing
        Dim z(,) As Double = Nothing
        Dim fixedNames() As String = Nothing
        Dim randomNames() As String = Nothing

        BuildExpandedModelInputs(lmmGuiData, y, x, fixedNames, z, randomNames)

        Dim raw As DataObj = lmmGuiData.Raw
        Dim subjectCol As Integer = ResolveDataColumnIndex(raw, lmmGuiData.SubjectKey, "subject")
        Dim visitCol As Integer = -1
        If Not String.IsNullOrWhiteSpace(lmmGuiData.VisitKey) Then
            visitCol = ResolveDataColumnIndex(raw, lmmGuiData.VisitKey, "visit/time/order")
        End If

        Dim subjectId() As Object = regression.MixedModelFrontEndHelpers.ExtractObjectColumnFromData(raw, subjectCol)
        Dim visit() As Double = Nothing
        If visitCol >= 0 Then visit = regression.MixedModelFrontEndHelpers.ExtractNumericColumnFromData(raw, visitCol)

        ValidateRandomStructureAgainstExpandedDesign(CStr(Me.cbRandomCovarStruct.SelectedItem), z, randomNames)

        Dim blockData As regression.MixedModelBlockData = regression.MixedModelBlockData.FromArrays(y:=y,
                                                        x:=x,
                                                        subjectId:=subjectId,
                                                        z:=z,
                                                        visit:=visit,
                                                        sortWithinSubjectByVisit:=(visit IsNot Nothing),
                                                        rowNumbers:=raw.RowIds)

        Dim rStruct As regression.MixedModelRStruct = regression.MixedModelRStructUtils.createMixedModelRStruct(CStr(Me.cbCovarStruct.SelectedItem))
        Dim gStruct As regression.MixedModelGStruct = regression.MixedModelGStructUtils.createMixedModelGStruct(CStr(Me.cbRandomCovarStruct.SelectedItem))

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
            End Try

            MsgBox("Kenward-Roger inference requires REML. The LMM fit method has been changed to REML for this analysis.",
                   MsgBoxStyle.Information,
                   "LMM Kenward-Roger inference")
        End If

        Dim req As regression.MixedModelFitRequest = regression.MixedModelFitRequest.CreateLMM(blockData, rStruct, gStruct, fitMethod)

        req.ResponseVarName = RegressionDesignCore.GetCoefBaseName(lmmGuiData.ResponseKey)
        req.SubjectVarName = RegressionDesignCore.GetCoefBaseName(lmmGuiData.SubjectKey)
        req.VisitVarName = If(String.IsNullOrWhiteSpace(lmmGuiData.VisitKey), String.Empty, RegressionDesignCore.GetCoefBaseName(lmmGuiData.VisitKey))
        req.FixedEffectNames = fixedNames
        req.RandomEffectNames = randomNames
        req.FixedFormulaText = BuildSelectedEffectsText(Me.lbSelectedFixedEffectsList, Me.cbFixedIntercept.Checked, "Intercept only")
        req.RandomFormulaText = BuildSelectedEffectsText(Me.lbSelectedRandomEffectsList, Me.cbRandomIntercept.Checked, "Random intercept only")
        req.RequestLabel = "LMM"

        Select Case selectedInferenceMethod
            Case "Large-sample normal"
                req.FixedInferenceMethod = regression.MixedModelFixedInferenceMethod.WaldNormal
            Case "Residual DF"
                req.FixedInferenceMethod = regression.MixedModelFixedInferenceMethod.ResidualDF
            Case "Satterthwaite"
                req.FixedInferenceMethod = regression.MixedModelFixedInferenceMethod.Satterthwaite
                req.UseSatterthwaite = True
            Case "Kenward-Roger"
                req.EnableFullKenwardRogerForLmm()
            Case Else
                req.FixedInferenceMethod = regression.MixedModelFixedInferenceMethod.WaldNormal
        End Select

        Dim ctl As regression.MixedModelControl = req.Control
        Dim uiEps As Double = ParseUiDouble(Me.tbEps.Text, "Convergence epsilon")

        ctl.MaxIter = ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations")
        ctl.Epsilon = uiEps
        ctl.StepTolerance = uiEps
        ctl.FunctionTolerance = uiEps
        ApplyLMMCovarianceOptimizerSelections(ctl)
        ctl.Trace = Me.ckTrace.Checked OrElse Me.ckIterationsDetails.Checked
        req.Control = ctl

        ResetLMMProgress()
        pLmmCancelRequested = False
        pLmmInterruptRequested = False
        pLmmCloseAfterCancel = False
        pLmmCalculationRunning = True
        req.ProgressReporter = AddressOf ReportLMMProgressFromAnyThread
        req.CancellationRequested = AddressOf IsLMMCancellationRequested
        req.InterruptionRequested = AddressOf IsLMMInterruptionRequested

        Dim model As regression.LMM = Nothing
        Dim result As regression.MixedModelResult = Nothing

        Try
            model = New regression.LMM(req)

            Try
                result = Await Task.Run(Function() model.Fit())
                FinishLMMProgress(result, result IsNot Nothing AndAlso result.Converged)
                If result IsNot Nothing AndAlso result.Cancelled Then Return
            Catch ex As System.OperationCanceledException
                pLmmCancelRequested = True
                FinishLMMProgress(result, False)
                Return
            Catch
                FinishLMMProgress(result, False)
                Throw
            Finally
                CompleteLMMRunOnUiThread()
            End Try

            InvokeLMMUi(Sub()
                            AppendLMMClassInfoTable(result, lmmGuiData)
                            WriteLMMResults(lmmGuiData, result, y, x, fixedNames, z, randomNames, subjectId, visit)
                        End Sub)
        Finally
            ReleaseLMMLargeRunReferences(result, model, req)

            result = Nothing
            model = Nothing
            blockData = Nothing
            rStruct = Nothing
            gStruct = Nothing
            raw = Nothing
            y = Nothing
            x = Nothing
            z = Nothing
            fixedNames = Nothing
            randomNames = Nothing
            subjectId = Nothing
            visit = Nothing
            lmmGuiData = Nothing

            RequestLMMManagedCleanup()
        End Try
    End Function

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
                Remove_Item(Me.lbSelectedFixedEffectsList, "all", Me.FixedTermSpecs)
                Remove_Item(Me.lbSelectedRandomEffectsList, "all", Me.RandomTermSpecs)
            End If

            newSheet = pWorkbook.Worksheets(Me.cbSheetsList.SelectedItem.ToString())
            Me.Populate(newSheet)
        Else
            Me.Populate(pWorksheet)
        End If
    End Sub

    Private Sub btRemoveY_Click(sender As Object, e As System.EventArgs) Handles btRemoveY.Click
        Remove_Item(Me.lbY)
    End Sub

    Private Sub btRemoveClusterID_Click(sender As Object, e As System.EventArgs) Handles btRemoveClusterID.Click
        Remove_Item(Me.lbClusterID)
    End Sub

    Private Sub btRemoveTime_Click(sender As Object, e As System.EventArgs) Handles btRemoveTime.Click
        Remove_Item(Me.lbTime)
    End Sub

    Private Sub btRemoveX_Click(sender As Object, e As System.EventArgs) Handles btRemoveX.Click
        Remove_Item(Me.lbXs, "selected")
        RefreshSelectedVariablesFromSourceList()
    End Sub

    Private Sub btAddY_Click(sender As Object, e As System.EventArgs) Handles btAddY.Click
        AddItemToListbox(Me.lbY, Me.lbAllColumns, Me.lbXs, Me.lbClusterID, Me.lbTime)
    End Sub

    Private Sub btAddClusterID_Click(sender As Object, e As System.EventArgs) Handles btAddClusterID.Click
        AddItemToListbox(Me.lbClusterID, Me.lbAllColumns, Me.lbXs, Me.lbY, Me.lbTime)
    End Sub

    Private Sub btAddTime_Click(sender As Object, e As System.EventArgs) Handles btAddTime.Click
        AddItemToListbox(Me.lbTime, Me.lbAllColumns, Me.lbY, Me.lbClusterID)
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        AddItemsToListbox(Me.lbXs, Me.lbAllColumns, Me.lbY, Me.lbClusterID)
        RefreshSelectedVariablesFromSourceList()
    End Sub

    Private Sub btAddFixedEffect_Click(sender As Object, e As System.EventArgs) Handles btAddFixedEffect.Click
        Me.FixedEffectsController.AddMainEffectsFromSelectedVars()
    End Sub

    Private Sub btAddFixedEffectCategoricalFactor_Click(sender As Object, e As System.EventArgs) Handles btAddFixedEffectCategoricalFactor.Click
        Me.FixedEffectsController.AddCategoricalEffectsFromSelectedVars()
    End Sub

    Private Sub btnFixed2Interactions_Click(sender As Object, e As System.EventArgs) Handles btnFixed2Interactions.Click
        Me.FixedEffectsController.AddTwoWayInteractionsFromSelectedVars()
    End Sub

    Private Sub btnFixedCustomInteraction_Click(sender As Object, e As System.EventArgs) Handles btnFixedCustomInteraction.Click
        Me.FixedEffectsController.AddCustomInteractionFromSelectedVars()
    End Sub

    Private Sub btnFixedPoly_Click(sender As Object, e As System.EventArgs) Handles btnFixedPoly.Click
        Me.FixedEffectsController.AddPolynomialEffectsFromSelectedVars(CInt(Me.spinBtnFixedPoly.Value))
    End Sub

    Private Sub tbRemoveSelectedFixedEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedFixedEffects.Click
        Remove_Item(Me.lbSelectedFixedEffectsList, "selected", Me.FixedTermSpecs)
    End Sub

    Private Sub btClearAllSelectedFixedEffects_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedFixedEffects.Click
        Remove_Item(Me.lbSelectedFixedEffectsList, "all", Me.FixedTermSpecs)
    End Sub

    Private Sub btAddRandomEffect_Click(sender As Object, e As System.EventArgs) Handles btAddRandomEffect.Click
        Me.RandomEffectsController.AddMainEffectsFromSelectedVars()
        PromoteRandomCovarianceForMultipleRandomEffects()
    End Sub

    Private Sub btAddRandomEffectCategoricalFactor_Click(sender As Object, e As System.EventArgs) Handles btAddRandomEffectCategoricalFactor.Click
        PromoteRandomCovarianceForGeneralRandomEffects()
        Me.RandomEffectsController.AddCategoricalEffectsFromSelectedVars()
        PromoteRandomCovarianceForMultipleRandomEffects()
    End Sub

    Private Sub btnRandom2Interactions_Click(sender As Object, e As System.EventArgs) Handles btnRandom2Interactions.Click
        PromoteRandomCovarianceForGeneralRandomEffects()
        Me.RandomEffectsController.AddTwoWayInteractionsFromSelectedVars()
        PromoteRandomCovarianceForMultipleRandomEffects()
    End Sub

    Private Sub btnRandomCustomInteraction_Click(sender As Object, e As System.EventArgs) Handles btnRandomCustomInteraction.Click
        PromoteRandomCovarianceForGeneralRandomEffects()
        Me.RandomEffectsController.AddCustomInteractionFromSelectedVars()
        PromoteRandomCovarianceForMultipleRandomEffects()
    End Sub

    Private Sub btnRandomPoly_Click(sender As Object, e As System.EventArgs) Handles btnRandomPoly.Click
        PromoteRandomCovarianceForGeneralRandomEffects()
        Me.RandomEffectsController.AddPolynomialEffectsFromSelectedVars(CInt(Me.spinBtnRandomPoly.Value))
        PromoteRandomCovarianceForMultipleRandomEffects()
    End Sub

    Private Sub tbRemoveSelectedRandomEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedRandomEffects.Click
        Remove_Item(Me.lbSelectedRandomEffectsList, "selected", Me.RandomTermSpecs)
    End Sub

    Private Sub btClearAllSelectedRandomEffects_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedRandomEffects.Click
        Remove_Item(Me.lbSelectedRandomEffectsList, "all", Me.RandomTermSpecs)
    End Sub

    Private Sub cbCovarStruct_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbCovarStruct.SelectedIndexChanged
        UpdateLMMCovarianceDependentControls()
    End Sub

    Private Sub cbRandomCovarStruct_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbRandomCovarStruct.SelectedIndexChanged
        UpdateLMMCovarianceDependentControls()
    End Sub

    Private Sub cbRandomIntercept_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbRandomIntercept.CheckedChanged
        PromoteRandomCovarianceForMultipleRandomEffects()
        UpdateLMMCovarianceDependentControls()
    End Sub

    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs) Handles btInterrupt.Click
        If Not pLmmCalculationRunning Then Exit Sub

        plmmInterruptRequested = True

        Try
            Me.lblProgress.Text = "Interrupting LMM; latest accepted estimates will be returned..."
            Me.btInterrupt.Enabled = False
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' Helpers
    '--------------------------------------------------------------------------
    Private Sub RefreshSelectedVariablesFromSourceList()
        If Me.lbSelectedVariables Is Nothing OrElse Me.lbXs Is Nothing Then Exit Sub

        Dim changed As Boolean = Not IsEqualListBox(Me.lbXs, Me.lbSelectedVariables)
        If Not changed AndAlso Me.lbSelectedVariables.Items.Count > 0 Then Exit Sub

        If changed Then
            Remove_Item(Me.lbSelectedVariables)
            For i As Integer = 0 To Me.lbXs.Items.Count - 1
                Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
            Next

            If Not TermSpecsUseOnlySelectedVariables(Me.lbSelectedFixedEffectsList, Me.FixedTermSpecs) Then
                If MsgBox("There is a variable in selected fixed effects that was removed from the model source variable(s) list." & vbNewLine & vbNewLine &
                      "Clear selected fixed-effects list?", vbYesNo + vbExclamation, "Clear selected fixed effects?") = vbYes Then
                    Remove_Item(Me.lbSelectedFixedEffectsList, "all", Me.FixedTermSpecs)
                End If
            End If

            If Not TermSpecsUseOnlySelectedVariables(Me.lbSelectedRandomEffectsList, Me.RandomTermSpecs) Then
                If MsgBox("There is a variable in selected random effects that was removed from the model source variable(s) list." & vbNewLine & vbNewLine &
                      "Clear selected random-effects list?", vbYesNo + vbExclamation, "Clear selected random effects?") = vbYes Then
                    Remove_Item(Me.lbSelectedRandomEffectsList, "all", Me.RandomTermSpecs)
                End If
            End If
        Else
            For i As Integer = 0 To Me.lbXs.Items.Count - 1
                Me.lbSelectedVariables.Items.Add(Me.lbXs.Items(i))
            Next
        End If
    End Sub

    Private Function TermSpecsUseOnlySelectedVariables(effectList As Windows.Forms.ListBox, termSpecs As Dictionary(Of String, TermSpec)) As Boolean
        Dim required As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(effectList.Items, termSpecs)
        For Each key As String In required
            If Not Me.lbSelectedVariables.Items.Contains(key) Then Return False
        Next
        Return True
    End Function

    Private Sub PromoteRandomCovarianceForMultipleRandomEffects()
        Try
            Dim authoredCount As Integer = Me.lbSelectedRandomEffectsList.Items.Count
            Dim qAuthoring As Integer = authoredCount + If(Me.cbRandomIntercept.Checked, 1, 0)
            Dim current As String = SelectedComboText(Me.cbRandomCovarStruct, "Random Intercept")
            If regression.MixedModelFrontEndHelpers.RandomAuthoringRequiresGeneralGSideStructure(Me.RandomTermSpecs,
                                                                                                authoredCount,
                                                                                                Me.cbRandomIntercept.Checked) Then
                If String.Equals(current, "Random Intercept", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(current, "Random Intercept + Slope", StringComparison.OrdinalIgnoreCase) Then
                    Me.cbRandomCovarStruct.SelectedItem = "Variance Components (VC/Diag)"
                End If
            ElseIf qAuthoring = 2 AndAlso Me.cbRandomIntercept.Checked AndAlso
                String.Equals(current, "Random Intercept", StringComparison.OrdinalIgnoreCase) Then
                Me.cbRandomCovarStruct.SelectedItem = "Random Intercept + Slope"
            End If
        Catch
        End Try
    End Sub

    Private Sub PromoteRandomCovarianceForGeneralRandomEffects()
        Try
            Dim current As String = SelectedComboText(Me.cbRandomCovarStruct, "Random Intercept")
            If String.Equals(current, "Random Intercept", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(current, "Random Intercept + Slope", StringComparison.OrdinalIgnoreCase) Then
                Me.cbRandomCovarStruct.SelectedItem = "Variance Components (VC/Diag)"
            End If
        Catch
        End Try
    End Sub

    Private Sub UpdateLMMCovarianceDependentControls()
        Try
            Dim needsVisit As Boolean = Me.cbCovarStruct.SelectedItem IsNot Nothing AndAlso regression.MixedModelFrontEndHelpers.ResidualStructureRequiresVisit(CStr(Me.cbCovarStruct.SelectedItem))

            'Keep the general random-effect authoring controls enabled. Buttons that can expand
            'to multiple random-effect columns promote the G-side covariance to VC/Diag before adding the term.
            Me.btnRandom2Interactions.Enabled = True
            Me.btnRandomCustomInteraction.Enabled = True
            Me.btAddRandomEffectCategoricalFactor.Enabled = True
            Me.btnRandomPoly.Enabled = True
            Me.spinBtnRandomPoly.Enabled = True
        Catch
        End Try
    End Sub

    Private Sub ValidateRandomStructureAgainstExpandedDesign(randomStructName As String, z(,) As Double, randomNames() As String)
        regression.MixedModelFrontEndHelpers.ValidateRandomStructureAgainstDesign(randomStructName:=randomStructName,
                                                                                 z:=z,
                                                                                 randomNames:=randomNames,
                                                                                 randomInterceptChecked:=Me.cbRandomIntercept.Checked,
                                                                                 authoredRandomEffectCount:=Me.lbSelectedRandomEffectsList.Items.Count,
                                                                                 enforceUiInterceptSemantics:=True)
    End Sub

    Private Sub ApplyLMMCovarianceOptimizerSelections(ByRef ctl As regression.MixedModelControl)
        Dim optimizerText As String = SelectedComboText(Me.cbLMMCovOptimizerMode, LMM_OPT_AI)
        Dim gradientText As String = SelectedComboText(Me.cbLMMCovGradientMode, LMM_GRAD_AUTO)

        ctl.CovarianceOptimizerMode = regression.MixedModelFrontEndHelpers.ParseCovarianceOptimizerMode(optimizerText)
        ctl.CovarianceGradientMode = regression.MixedModelFrontEndHelpers.ParseCovarianceGradientMode(gradientText)

        If String.Equals(optimizerText, LMM_OPT_BFGS_ANALYTIC, StringComparison.OrdinalIgnoreCase) Then
            ctl.CovarianceOptimizerMode = regression.MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient
            ctl.CovarianceGradientMode = regression.MixedModelCovarianceGradientMode.AnalyticScore
        ElseIf String.Equals(optimizerText, LMM_OPT_BFGS_NUMERICAL, StringComparison.OrdinalIgnoreCase) Then
            ctl.CovarianceOptimizerMode = regression.MixedModelCovarianceOptimizerMode.ProjectedBfgs
            ctl.CovarianceGradientMode = regression.MixedModelCovarianceGradientMode.NumericalFiniteDifference
        End If
    End Sub

    Private Function SelectedComboText(cb As Windows.Forms.ComboBox, fallback As String) As String
        If cb Is Nothing OrElse cb.SelectedItem Is Nothing Then Return fallback
        Dim s As String = CStr(cb.SelectedItem)
        If String.IsNullOrWhiteSpace(s) Then Return fallback
        Return s
    End Function

    Private Function BuildSelectedEffectsText(effectList As Windows.Forms.ListBox, includeIntercept As Boolean, interceptOnlyText As String) As String
        Return regression.MixedModelFrontEndHelpers.BuildEffectsText(If(effectList Is Nothing, Nothing, effectList.Items),
                                                                     includeIntercept,
                                                                     interceptOnlyText)
    End Function

    Private Function ResolveDataColumnIndex(raw As DataObj, key As String, role As String) As Integer
        Return regression.MixedModelFrontEndHelpers.ResolveDataColumnIndex(raw, key, role, "LMM")
    End Function

    Private Function CombineNamesWithIntercept(expandedNames() As String) As String()
        Return regression.MixedModelFrontEndHelpers.AddInterceptName(expandedNames)
    End Function

    Private Sub AppendLMMClassInfoTable(result As regression.MixedModelResult, lmmGuiData As LmmGuiData)
        Try
            If result Is Nothing OrElse Not Me.ckLMMClassInfo.Checked Then Exit Sub
            If result.AdditionalResultTables Is Nothing Then result.AdditionalResultTables = New List(Of ResultTable)()

            Dim rows As New List(Of Object())()
            AppendClassInfoRows(rows, lmmGuiData.Raw, "Fixed", Me.FixedTermSpecs)
            AppendClassInfoRows(rows, lmmGuiData.Raw, "Random", Me.RandomTermSpecs)

            If rows.Count = 0 Then Exit Sub

            Dim body(rows.Count - 1, 4) As Object
            For i As Integer = 0 To rows.Count - 1
                For j As Integer = 0 To 4
                    body(i, j) = rows(i)(j)
                Next
            Next

            Dim t As New ResultTable()
            t.AddTitle("Class level information")
            t.SetBody(body)
            t.AddHeaderTopRow({"Model part", "Variable", "Term kind", "Levels", "No. levels"})
            t.AddFootnote("Levels are observed numeric/coded values in the cleaned analysis data for variables used as categorical effects.")
            result.AdditionalResultTables.Add(t)
        Catch ex As Exception
            CoreServices.Logger.Warn("AppendLMMClassInfoTable failed: " & ex.Message)
        End Try
    End Sub

    Private Sub AppendClassInfoRows(rows As List(Of Object()), raw As DataObj, modelPart As String, specs As Dictionary(Of String, TermSpec))
        If rows Is Nothing OrElse raw Is Nothing OrElse specs Is Nothing Then Exit Sub

        Dim categoricalKeys As List(Of String) = GetCategoricalMainEffectBaseKeys(specs)
        Dim seen As New Dictionary(Of String, Boolean)(StringComparer.Ordinal)

        For Each baseKey As String In categoricalKeys
            Dim rowKey As String = modelPart & "|" & baseKey
            If seen.ContainsKey(rowKey) Then Continue For
            seen(rowKey) = True

            Dim col As Integer = ResolveDataColumnIndex(raw, baseKey, "categorical class")
            Dim values() As Double = regression.MixedModelFrontEndHelpers.ExtractNumericColumnFromData(raw, col)
            Dim levels As List(Of Double) = regression.MixedModelFrontEndHelpers.UniqueSortedFiniteValues(values)
            Dim levelText As String = String.Join(", ", levels.Select(Function(v) v.ToString("G15", Globalization.CultureInfo.InvariantCulture)).ToArray())
            rows.Add(New Object() {modelPart, RegressionDesignCore.GetCoefBaseName(baseKey), "Categorical main effect", levelText, levels.Count})
        Next
    End Sub

    Private Function GetCategoricalMainEffectBaseKeys(specs As Dictionary(Of String, TermSpec)) As List(Of String)
        Dim out As New List(Of String)()
        If specs Is Nothing Then Return out

        For Each kvp In specs
            Dim spec As TermSpec = kvp.Value
            If spec Is Nothing OrElse spec.BaseVarKeys Is Nothing Then Continue For
            If spec.Scale <> PredictorScale.Categorical Then Continue For
            If Not String.Equals(spec.Kind, "MainEffect", StringComparison.OrdinalIgnoreCase) Then Continue For

            For Each baseKey As String In spec.BaseVarKeys
                If Not out.Contains(baseKey) Then out.Add(baseKey)
            Next
        Next

        Return out
    End Function

    Private Sub WriteLMMResults(lmmGuiData As LmmGuiData,
                                result As regression.MixedModelResult,
                                y() As Double,
                                x(,) As Double,
                                fixedNames() As String,
                                z(,) As Double,
                                randomNames() As String,
                                subjectId() As Object,
                                visit() As Double)

        If result Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(result)))
        If lmmGuiData Is Nothing OrElse lmmGuiData.Raw Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(lmmGuiData)))

        Dim wb As Object = AppGlobals.app.Workbooks.Add()

        WriteLMMDataSheet(wb, lmmGuiData, result, y, x, fixedNames, z, randomNames, subjectId, visit)
        WriteLMMModelSheet(wb, result)

        If (Me.ckTrace.Checked OrElse Me.ckIterationsDetails.Checked) AndAlso Not String.IsNullOrWhiteSpace(result.strTrace) Then
            WriteLMMTraceSheet(wb, result.strTrace)
        End If

    End Sub

    Private Sub WriteLMMDataSheet(wb As Object,
                                  lmmGuiData As LmmGuiData,
                                  result As regression.MixedModelResult,
                                  y() As Double,
                                  x(,) As Double,
                                  fixedNames() As String,
                                  z(,) As Double,
                                  randomNames() As String,
                                  subjectId() As Object,
                                  visit() As Double)
        Dim writeRes As New ExcelDnaResultWriter
        writeRes.wb = wb
        writeRes.ws = wb.ActiveSheet
        writeRes.ws.Name = "Data"

        writeRes.write({"Row ID"})
        writeRes.setRowPointer(2)
        writeRes.write(lmmGuiData.Raw.RowIds, bTall:=True)
        writeRes.setRowPointer()
        writeRes.shiftColumnPointer(1)

        writeRes.write({RegressionDesignCore.GetCoefBaseName(lmmGuiData.ResponseKey)})
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

        If randomNames IsNot Nothing AndAlso randomNames.Length > 0 Then
            Dim zHeaders(randomNames.Length - 1) As String
            For j As Integer = 0 To randomNames.Length - 1
                zHeaders(j) = "Z: " & randomNames(j)
            Next

            writeRes.write(zHeaders)
            writeRes.setRowPointer(2)
            writeRes.write(z)
            writeRes.setRowPointer()
            writeRes.shiftColumnPointer(randomNames.Length)
        End If

        writeRes.write({RegressionDesignCore.GetCoefBaseName(lmmGuiData.SubjectKey)})
        writeRes.setRowPointer(2)
        writeRes.write(subjectId, bTall:=True)
        writeRes.setRowPointer()
        writeRes.shiftColumnPointer(1)

        If visit IsNot Nothing Then
            writeRes.write({RegressionDesignCore.GetCoefBaseName(lmmGuiData.VisitKey)})
            writeRes.setRowPointer(2)
            writeRes.write(visit, bTall:=True)
            writeRes.setRowPointer()
            writeRes.shiftColumnPointer(1)
        End If

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

    Private Sub WriteLMMModelSheet(wb As Object, result As regression.MixedModelResult)
        Dim alphaValue As Double = CDbl(Me.spinBtnAlpha.Value)

        If Not Me.ckLMMGCovarianceMatrix.Checked Then
            result.RandomCovarianceUserScale = Nothing
            result.RandomCorrelationUserScale = Nothing
        End If

        If Not Me.ckLMMRCovarianceMatrix.Checked Then
            result.ResidualCovarianceUserScale = Nothing
            result.ResidualCorrelationUserScale = Nothing
        End If

        If Not Me.ckLMMRandomEffects.Checked AndAlso result.RandomEffects IsNot Nothing Then
            result.RandomEffects.Clear()
        End If

        Dim tables As List(Of ResultTable) = result.wrapResults(alphaValue,
                                                                includeOptimizerTrace:=Me.ckIterationsDetails.Checked,
                                                                includeKenwardRogerTermTests:=True,
                                                                includeDiagnostics:=Me.cbDiagnostic.Checked)
        Dim writeRes As New ExcelDnaResultWriter
        wb.Worksheets.Add(After:=wb.Worksheets(wb.Worksheets.Count))
        wb.ActiveSheet.Name = "LMM"
        writeRes.wb = wb
        writeRes.ws = wb.ActiveSheet

        Dim rr As New ProcessListofResultTables(tables)
        rr.writeToSheet(writeRes)
    End Sub

    Private Sub WriteLMMTraceSheet(wb As Object, traceText As String)
        Dim writeRes As New ExcelDnaResultWriter
        wb.Worksheets.Add(After:=wb.Worksheets(wb.Worksheets.Count))
        wb.ActiveSheet.Name = "LMM Trace"
        writeRes.wb = wb
        writeRes.ws = wb.ActiveSheet

        writeRes.write(regression.MixedModelFrontEndHelpers.TraceTextToMatrix(traceText))
    End Sub

    Private Function IsLMMCancellationRequested() As Boolean
        Return pLmmCancelRequested
    End Function

    Private Function IsLMMInterruptionRequested() As Boolean
        Return pLmmInterruptRequested AndAlso Not pLmmCancelRequested
    End Function

    Private Sub Ui19LMM_FormClosing(sender As Object, e As Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not pLmmCalculationRunning Then Exit Sub

        pLmmCancelRequested = True
        pLmmCloseAfterCancel = True
        e.Cancel = True

        Try
            Me.lblProgress.Text = "Cancelling LMM..."
            Me.ProgressBar1.Style = Windows.Forms.ProgressBarStyle.Marquee
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = False
            Windows.Forms.Application.DoEvents()
        Catch
        End Try
    End Sub

    Private Sub ResetLMMProgress()
        Try
            Me.ProgressBar1.Minimum = 0
            Me.ProgressBar1.Maximum = 100
            Me.ProgressBar1.Style = Windows.Forms.ProgressBarStyle.Continuous
            Me.ProgressBar1.Value = 0
            pLmmProgressStopwatch = System.Diagnostics.Stopwatch.StartNew()
            pLastLmmProgressRefreshTimestamp = 0
            Me.lblProgress.Text = "Preparing LMM..."
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = True
            RefreshLMMProgressControls(True)
        Catch
        End Try
    End Sub

    Private Sub ReportLMMProgressFromAnyThread(info As regression.MixedModelProgressInfo)
        Try
            If Me.IsDisposed Then Exit Sub

            If Me.InvokeRequired Then
                Me.BeginInvoke(New Action(Of regression.MixedModelProgressInfo)(AddressOf UpdateLMMProgress), info)
            Else
                UpdateLMMProgress(info)
            End If
        Catch
        End Try
    End Sub

    Private Sub UpdateLMMProgress(info As regression.MixedModelProgressInfo)
        Try
            If info Is Nothing Then Exit Sub

            Dim value As Integer = Math.Max(Me.ProgressBar1.Minimum, Math.Min(Me.ProgressBar1.Maximum, info.Percent))
            Me.ProgressBar1.Value = value

            Dim msg As String = If(pLmmCancelRequested, "Cancelling LMM", If(pLmmInterruptRequested, "Interrupting LMM", info.Stage))
            Dim elapsedSecondsText As String = UIprocedures.FormatProgressElapsedSeconds(Me.pLmmProgressStopwatch)
            If info.Iteration >= 0 AndAlso info.MaxIterations > 0 Then
                If elapsedSecondsText.Length > 0 Then msg &= " " & elapsedSecondsText
                msg &= " (" & info.Iteration.ToString() & "/" & info.MaxIterations.ToString() & ")"
            ElseIf elapsedSecondsText.Length > 0 Then
                msg &= " " & elapsedSecondsText
            End If

            Dim convergenceParts As New List(Of String)
            If AppInfrastructure.IsFinite(info.Objective) Then convergenceParts.Add("f=" & UIprocedures.FormatProgressDouble(info.Objective))
            If AppInfrastructure.IsFinite(info.FunctionChange) Then convergenceParts.Add("Δf=" & UIprocedures.FormatProgressDouble(info.FunctionChange))
            If AppInfrastructure.IsFinite(info.GradNorm) Then convergenceParts.Add("|g|=" & UIprocedures.FormatProgressDouble(info.GradNorm))

            If convergenceParts.Count > 0 Then
                msg &= " | " & String.Join("; ", convergenceParts.ToArray())
            End If

            If Not String.IsNullOrWhiteSpace(info.Message) Then
                msg &= " - " & info.Message
            End If

            Me.lblProgress.Text = msg
            RefreshLMMProgressControls(False)
        Catch
        End Try
    End Sub

    Private Sub RefreshLMMProgressControls(Optional force As Boolean = False)
        If pLmmProgressRefreshActive Then Exit Sub

        Try
            Dim nowTicks As Long = System.Diagnostics.Stopwatch.GetTimestamp()

            If Not force AndAlso pLastLmmProgressRefreshTimestamp <> 0 Then
                Dim elapsedMs As Double = (CDbl(nowTicks - pLastLmmProgressRefreshTimestamp) * 1000.0) / CDbl(System.Diagnostics.Stopwatch.Frequency)
                If elapsedMs < LMM_PROGRESS_REFRESH_INTERVAL_MS Then Exit Sub
            End If

            pLastLmmProgressRefreshTimestamp = nowTicks
            pLmmProgressRefreshActive = True

            If Me.lblProgress IsNot Nothing AndAlso Not Me.lblProgress.IsDisposed Then
                Me.lblProgress.Invalidate()
                Me.lblProgress.Update()
            End If

            If Me.ProgressBar1 IsNot Nothing AndAlso Not Me.ProgressBar1.IsDisposed Then
                Me.ProgressBar1.Invalidate()
                Me.ProgressBar1.Update()
            End If
        Catch
        Finally
            pLmmProgressRefreshActive = False
        End Try
    End Sub

    Private Sub InvokeLMMUi(action As Action)
        If action Is Nothing Then Exit Sub
        If Me.IsDisposed Then Exit Sub

        If Me.InvokeRequired Then
            If Not Me.IsHandleCreated Then Exit Sub
            Me.Invoke(action)
        Else
            action()
        End If
    End Sub

    Private Sub CompleteLMMRunOnUiThread()
        Try
            InvokeLMMUi(Sub()
                            pLmmCalculationRunning = False
                            pLmmProgressStopwatch = Nothing
                            Me.btCalculate.Enabled = True
                            Me.btInterrupt.Enabled = False

                            If pLmmCloseAfterCancel Then
                                pLmmCloseAfterCancel = False
                                Me.Close()
                            End If
                        End Sub)
        Catch
        End Try
    End Sub

    Private Sub FinishLMMProgress(result As regression.MixedModelResult, success As Boolean)
        Try
            InvokeLMMUi(Sub()
                            Me.ProgressBar1.Style = Windows.Forms.ProgressBarStyle.Continuous
                            Me.ProgressBar1.Value = If(success, 100, Math.Min(Me.ProgressBar1.Value, 99))

                            If result IsNot Nothing AndAlso result.Cancelled Then
                                Me.lblProgress.Text = "LMM cancelled."
                            ElseIf result IsNot Nothing AndAlso result.Interrupted Then
                                Me.lblProgress.Text = "LMM interrupted; latest estimates returned."
                            ElseIf result IsNot Nothing AndAlso Not Double.IsNaN(result.ExecutionTimeMs) AndAlso Not Double.IsInfinity(result.ExecutionTimeMs) Then
                                Me.lblProgress.Text = "Elapsed Time: " & (result.ExecutionTimeMs / 1000.0).ToString("0.000", Globalization.CultureInfo.InvariantCulture) & " s"
                            Else
                                Me.lblProgress.Text = If(success, "Completed", "Failed")
                            End If

                            Me.btCalculate.Enabled = True
                            Me.btInterrupt.Enabled = False
                            pLmmProgressStopwatch = Nothing
                            RefreshLMMProgressControls(True)
                        End Sub)
        Catch
            Try
                InvokeLMMUi(Sub()
                                Me.btCalculate.Enabled = True
                                Me.btInterrupt.Enabled = False
                                pLmmProgressStopwatch = Nothing
                            End Sub)
            Catch
            End Try
        End Try
    End Sub

    Private Shared Sub ReleaseLMMLargeRunReferences(result As regression.MixedModelResult, model As regression.LMM,
                                                    req As regression.MixedModelFitRequest)
        Try
            If req IsNot Nothing Then
                req.ProgressReporter = Nothing
                req.CancellationRequested = Nothing
                req.InterruptionRequested = Nothing
                req.Data = Nothing
            End If

            If result IsNot Nothing Then result.ReleaseLargePostEstimationWorkspaces()
            If model IsNot Nothing Then model.ReleaseFitState(releaseResultWorkspaces:=True, clearRequestRuntimeReferences:=True)

        Catch
        End Try
    End Sub

    Private Sub Ui19LMM_FormClosed(sender As Object, e As Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ClearLMMDialogReferences()
        RequestLMMManagedCleanup()
    End Sub

    Private Sub ClearLMMDialogReferences()
        Try
            pWorksheet = Nothing
            pWorkbook = Nothing

            If VariableColumnsInfo IsNot Nothing Then VariableColumnsInfo.Clear()
            If FixedTermSpecs IsNot Nothing Then FixedTermSpecs.Clear()
            If RandomTermSpecs IsNot Nothing Then RandomTermSpecs.Clear()

            pLmmProgressStopwatch = Nothing
            pLmmProgressRefreshActive = False
            pLastLmmProgressRefreshTimestamp = 0
        Catch
        End Try
    End Sub

    Private Shared Sub RequestLMMManagedCleanup()
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