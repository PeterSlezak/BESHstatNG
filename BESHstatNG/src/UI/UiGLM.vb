Option Explicit On
Imports System
Imports System.Diagnostics.Tracing
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports System.Windows.Forms.LinkLabel
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.regression

Public Class UiGLM

    Private pWorksheet As Object
    Private pWorkbook As Object
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information Of variable/column names inported into the input listbox
    'key = effects list item string (e.g. Age | VarA, or "Age | VarA"^2)
    'UiGLM owns the TermSpecs dictionary; the shared EffectsController mutates this same
    'instance by reference so add/remove/clear operations remain synchronized.
    Private TermSpecs As Dictionary(Of String, TermSpec)
    Private ReadOnly EffectsController As RegressionEffectsController

    'ZIP logistic-model authored effects
    'The logistic ZIP tab uses its own independent effect list and term-specification dictionary.
    Private TermSpecsLogistic As Dictionary(Of String, TermSpec)
    Private ReadOnly EffectsControllerLogistic As RegressionEffectsController

    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()
        Me.tbEps.Text = FormatUiDouble(0.000001)
        Me.Text = analysis
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)

        ' Add any initialization after the InitializeComponent() call.
        If Me.Text = "Generalized Linear Models" Then
            Me.TabPageLogisticModel.Parent = Nothing
            Me.TabPageOptions_LinearModel.Parent = Nothing
            Me.grpReference.Visible = False

            For Each sFam In regression.Family.FamiliesList
                Me.cbFamily.Items.Add(sFam)
            Next
            Me.cbFamily.SelectedIndex = 0
            RefreshLinkOptionsForSelectedFamily(FamilyUtils.GetCanonicalLinkFromDisplayName(Me.cbFamily.SelectedItem.ToString()))
            Me.tbClassificationTreshold.Text = FormatUiDouble(0.5)
            UpdateClassificationOptionsState(False)

        ElseIf Me.Text = "Negative Binomial Regression (NB2)" Then
            Me.TabPageLogisticModel.Parent = Nothing
            Me.TabPageOptions_LinearModel.Parent = Nothing
            Me.grpReference.Visible = False

            Me.cbFamily.Items.Add("Negative Binomial")
            Me.cbFamily.SelectedIndex = 0
            RefreshLinkOptionsForSelectedFamily(FamilyUtils.GetCanonicalLinkFromDisplayName(Me.cbFamily.SelectedItem.ToString()))

        ElseIf Me.Text = "Zero-Inflated Poisson Regression" Then
            Me.TabPageOptions_LinearModel.Parent = Nothing
            Me.grpModelSpecification.Visible = False
            Me.grpReference.Visible = False
            Me.TabPageLogisticModel.Parent = Me.TabControl1
            Me.TabPageBuildModel.Text = "Build Model - Poisson"
            Me.lblEMiterations.Enabled = True
            Me.tbEMiterations.Enabled = True

            Me.lblWeights.Enabled = False
            Me.lbWeights.Enabled = False
            Me.btAddWeights.Enabled = False
            Me.btRemoveWeights.Enabled = False

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

        'Poisson / primary-model term specifications.
        'This dictionary remains owned by UiGLM and is passed into the shared controller
        'so both the form and the controller operate on the same backing state.
        Me.TermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)

        'Shared effect-authoring controller for the primary Build Model tab.
        Me.EffectsController = New RegressionEffectsController(Me.lbSelectedVariables,
                                                               Me.lbSelectedEffectsList,
                                                               Me.TermSpecs)

        'ZIP logistic-model term specifications.
        Me.TermSpecsLogistic = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)

        'Shared effect-authoring controller for the ZIP Logistic Build Model tab.
        Me.EffectsControllerLogistic = New RegressionEffectsController(Me.lbSelectedVariablesLogistic,
                                                                       Me.lbSelectedEffectsListLogistic,
                                                                       Me.TermSpecsLogistic)
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

    Private Function IsCurrentBinomialGlmFamily() As Boolean
        If Not String.Equals(Me.Text, "Generalized Linear Models", StringComparison.Ordinal) Then Return False
        If Me.cbFamily.SelectedItem Is Nothing Then Return False
        Return String.Equals(GetFamilyCodeFromDisplayName(Me.cbFamily.SelectedItem.ToString()), "Binomial", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function GetClassificationThresholdUiValue() As Double
        Dim txt As String = Me.tbClassificationTreshold.Text.Trim()
        If txt = String.Empty Then
            Me.tbClassificationTreshold.Text = FormatUiDouble(0.5R)
            Return 0.5R
        End If
        Dim threshold As Double = ParseUiDouble(txt, "classification threshold")
        If threshold < 0.0R OrElse threshold > 1.0R Then Throw New FormatException("Classification threshold must be between 0 and 1.")
        Return threshold
    End Function

    Private Sub UpdateClassificationOptionsState(Optional bAlreadyInit As Boolean = True)
        Dim enabledForFamily As Boolean = IsCurrentBinomialGlmFamily()

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

    Private Sub UpdatePowerLinkState()
        Dim usePowerLink As Boolean =
            Me.cbLink.SelectedItem IsNot Nothing AndAlso
            Me.cbLink.SelectedItem.ToString() = "Power"

        Me.lblPower.Enabled = usePowerLink
        Me.tbPower.Enabled = usePowerLink
    End Sub

    Private Function GetData(Optional bZip As Boolean = False) As glmData
        Dim MyData As New glmData
        Dim keys As New List(Of String)
        '--- Response variable always first ---
        Dim yKey As String = CStr(Me.lbY.Items(0))
        keys.Add(yKey)

        If bZip Then
            'ZIP Logistic tab: import only the required RAW predictors for the authored effects.
            Dim rawXKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedEffectsListLogistic.Items, Me.TermSpecsLogistic)
            For Each xKey As String In rawXKeys
                keys.Add(xKey)
            Next

        Else
            '--- Build refs only from required RAW predictors ---
            Dim rawXKeys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(Me.lbSelectedEffectsList.Items, Me.TermSpecs)
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

    ''' <summary>
    ''' Builds the expanded regression matrix and aligned variable names for the
    ''' current non-ZIP regression model.
    ''' </summary>
    ''' <param name="MyData">
    ''' Raw imported regression data containing Y in column 0 and only required raw predictors thereafter.
    ''' </param>
    ''' <param name="fitData">
    ''' Returns the expanded matrix in the form [Y | expanded X].
    ''' </param>
    ''' <param name="fitVarNames">
    ''' Returns variable names aligned to <paramref name="fitData"/>.
    ''' </param>
    Private Sub BuildExpandedRegressionInputs(MyData As glmData,
                                              ByRef fitData(,) As Double,
                                              ByRef fitVarNames() As String)

        RegressionDesignCore.BuildExpandedRegressionDataMatrix(raw:=MyData,
                                                       yKey:=CStr(Me.lbY.Items(0)),
                                                       effectItems:=Me.lbSelectedEffectsList.Items,
                                                       termSpecs:=Me.TermSpecs,
                                                       omitCategoricalReference:=Me.ckIntercept.Checked,
                                                       outData:=fitData,
                                                       outVarNames:=fitVarNames)
    End Sub

    ''' <summary>
    ''' Builds the expanded ZIP Poisson-part regression matrix and aligned variable names.
    ''' </summary>
    Private Sub BuildExpandedZIPPoissonInputs(MyData As glmData,
                                              ByRef fitData(,) As Double,
                                              ByRef fitVarNames() As String)

        RegressionDesignCore.BuildExpandedRegressionDataMatrix(raw:=MyData,
                                                       yKey:=CStr(Me.lbY.Items(0)),
                                                       effectItems:=Me.lbSelectedEffectsList.Items,
                                                       termSpecs:=Me.TermSpecs,
                                                       omitCategoricalReference:=Me.ckIntercept.Checked,
                                                       outData:=fitData,
                                                       outVarNames:=fitVarNames)
    End Sub

    ''' <summary>
    ''' Builds the expanded ZIP Logistic-part regression matrix and aligned variable names.
    ''' </summary>
    Private Sub BuildExpandedZIPLogisticInputs(MyData As glmData,
                                               ByRef fitData(,) As Double,
                                               ByRef fitVarNames() As String)

        RegressionDesignCore.BuildExpandedRegressionDataMatrix(raw:=MyData,
                                                       yKey:=CStr(Me.lbY.Items(0)),
                                                       effectItems:=Me.lbSelectedEffectsListLogistic.Items,
                                                       termSpecs:=Me.TermSpecsLogistic,
                                                       omitCategoricalReference:=Me.ckInterceptLogistic.Checked,
                                                       outData:=fitData,
                                                       outVarNames:=fitVarNames)
    End Sub

    ''' <summary>
    ''' Counts the number of distinct response categories in the first column of a
    ''' regression data matrix.
    ''' </summary>
    ''' <param name="data">
    ''' A regression matrix whose first column contains the response values.
    ''' </param>
    ''' <returns>
    ''' The number of distinct response categories.
    ''' </returns>
    Private Function CountDistinctResponseCategories(data(,) As Double) As Integer
        Dim cats As New Dictionary(Of Integer, Byte)

        For i As Integer = 0 To UBound(data, 1)
            Dim yVal As Integer = CInt(Math.Round(data(i, 0)))
            If Not cats.ContainsKey(yVal) Then cats.Add(yVal, 0)
        Next

        Return cats.Count
    End Function

    ''' <summary>
    ''' Validates the exact number of user-supplied initial values after effect expansion.
    ''' </summary>
    ''' <param name="expectedCount">
    ''' The exact number of parameters expected by the fitted model.
    ''' </param>
    ''' <param name="modelCaption">
    ''' The display name of the model used in the validation message.
    ''' </param>
    ''' <param name="includeInterceptFirstNote">
    ''' If <see langword="True"/>, appends a note that the intercept initial value should be first.
    ''' </param>
    ''' <param name="extraNote">
    ''' Optional extra explanatory text appended to the validation message.
    ''' </param>
    ''' <param name="targetTextBox">
    ''' The textbox containing the starting values. If omitted, uses <see cref="tbInitValues"/>.
    ''' </param>
    ''' <returns>
    ''' <see langword="True"/> when the supplied initial values are valid; otherwise <see langword="False"/>.
    ''' </returns>
    Private Function ValidateExpandedInitialValuesCount(expectedCount As Integer,
                                                        modelCaption As String,
                                                        Optional includeInterceptFirstNote As Boolean = False,
                                                        Optional extraNote As String = "",
                                                        Optional targetTextBox As TextBox = Nothing) As Boolean
        Dim tb As TextBox = If(targetTextBox, Me.tbInitValues)

        If tb.Text = String.Empty Then Return True

        setTextBoxProperties(tb, Color.White, String.Empty)

        Dim bErr As Boolean = False
        Dim vals() As Double = GetNumbersFromStrList(tb.Text, bErr)

        If bErr Then
            Dim msg As String = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers without thousands separators."
            setTextBoxProperties(tb, Color.Red, msg)
            MsgBox(msg, vbExclamation, "Input Error!")
            Return False
        End If

        If vals.Length <> expectedCount Then
            Dim msg As String = $"Number of initial values does not match the number of estimated parameters for {modelCaption}." &
                                vbNewLine &
                                $"Expected {expectedCount}, received {vals.Length}."

            If includeInterceptFirstNote Then
                msg &= vbNewLine & "Initial value for the intercept should be the first one in the list."
            End If

            If extraNote <> String.Empty Then
                msg &= vbNewLine & extraNote
            End If

            setTextBoxProperties(tb, Color.Red, msg)
            MsgBox(msg, vbExclamation, "Input Error!")
            Return False
        End If

        Return True
    End Function

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

            'Do not validate parameter count here for GLM/NB2/ordinal/multinomial.
            'The exact count depends on the expanded design matrix and, for ordinal/multinomial,
            'also on the number of observed response categories. Exact validation is performed
            'after effect expansion inside the corresponding fit routine.
        End If

        'ZIP logistic initial parameter values
        If Me.Text = "Zero-Inflated Poisson Regression" AndAlso Me.tbInitValuesLogistic.Text <> String.Empty Then
            setTextBoxProperties(Me.tbInitValuesLogistic, Color.White, String.Empty)
            vals = GetNumbersFromStrList(Me.tbInitValuesLogistic.Text, bErr)
            If bErr Then
                strErr = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers without thousands separators."
                setTextBoxProperties(Me.tbInitValuesLogistic, Color.Red, strErr)
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
        If Me.lbSelectedEffectsList.Items.Count = 0 AndAlso Not Me.ckIntercept.Checked Then
            strErr = "No Intercept and Effects were specified."
            bWait = True
            Exit Sub
        End If

        If Me.lbSelectedEffectsList.Items.Count = 0 AndAlso Me.ckIntercept.Checked Then
            If MsgBox("Do you want to fit intercept only model?", vbYesNo + vbExclamation, AppGlobals.gsAPP_TITLE) = vbNo Then
                bWait = True
                Exit Sub
            End If
        ElseIf Me.lbSelectedEffectsList.Items.Count = 0 Then
            strErr = "No Effects were specified."
            bWait = True
            Exit Sub
        End If

        If Me.Text = "Zero-Inflated Poisson Regression" Then
            If Me.lbSelectedEffectsListLogistic.Items.Count = 0 AndAlso Not Me.ckInterceptLogistic.Checked Then
                strErr = "No Intercept and Effects were specified for the Logistic part of the ZIP model."
                bWait = True
                Exit Sub
            End If

            If Me.lbSelectedEffectsListLogistic.Items.Count = 0 AndAlso Me.ckInterceptLogistic.Checked Then
                If MsgBox("Do you want to fit intercept only model for the Logistic part?", vbYesNo + vbExclamation, AppGlobals.gsAPP_TITLE) = vbNo Then
                    bWait = True
                    Exit Sub
                End If
            End If
        End If

        If IsCurrentBinomialGlmFamily() Then
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
                Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)

                If Me.Text = "Zero-Inflated Poisson Regression" Then
                    Me.lbSelectedVariablesLogistic.Items.Clear()
                    Remove_Item(Me.lbSelectedEffectsListLogistic, "all", Me.TermSpecsLogistic)
                End If
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
                    AppGlobals.BSlogg.Log("Cannot extract initial parameter values. They will be ignored.")
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
                        AppGlobals.BSlogg.Log("Cannot extract initial parameter values. They will be ignored.")
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
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunOLS(MyData As glmData, bInitialValues As Boolean)
        Dim lm As New regression.LinearModel()
        Dim expanded(,) As Double = Nothing
        Dim expandedNames() As String = Nothing
        Dim customGroups As Dictionary(Of String, Integer()) = Nothing

        RegressionDesignCore.BuildExpandedLmDataMatrix(raw:=MyData,
                                       yKey:=CStr(Me.lbY.Items(0)),
                                       effectItems:=Me.lbSelectedEffectsList.Items,
                                       termSpecs:=Me.TermSpecs,
                                       includeIntercept:=Me.ckIntercept.Checked,
                                       outData:=expanded,
                                       outVarNames:=expandedNames,
                                       outTermGroups:=customGroups)

        lm.Data(expanded, expandedNames, MyData.RowIds, If(MyData.bWeights, MyData.WeightData, Nothing))
        Dim codingNotes As List(Of String) = RegressionDesignCore.BuildCategoricalReferenceFootnotesForLm(
                                                                    raw:=MyData,
                                                                    effectItems:=Me.lbSelectedEffectsList.Items,
                                                                    termSpecs:=Me.TermSpecs,
                                                                    includeIntercept:=Me.ckIntercept.Checked)
        lm.SetPredictorCodingFootnotes(codingNotes)
        lm.bReturnCov = Me.ckCovarMatrixLM.Checked
        lm.bComputeResiduals = Me.ckResidualsLM.Checked

        Dim ss As regression.TermSumOfSquaresType
        If Me.optTypeISS.Checked Then
            ss = regression.TermSumOfSquaresType.TypeI
        ElseIf Me.optTypeIIISS.Checked Then
            ss = regression.TermSumOfSquaresType.TypeIII
        End If

        lm.Fit(Me.ckIntercept.Checked, customGroups, ss)

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = AppGlobals.app.Workbooks.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
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
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "LM"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

    End Sub

    Private Sub RunOrdLogit(MyData As glmData, bInitialValues As Boolean)
        Dim ordL = New regression.OrdinalLogitModel()
        Dim alphaValue As Double = Me.spinBtnAlpha.Value
        Dim fitData(,) As Double = Nothing
        Dim fitVarNames() As String = Nothing

        BuildExpandedRegressionInputs(MyData, fitData, fitVarNames)

        Dim predictorCount As Integer = fitVarNames.Length - 1
        Dim categoryCount As Integer = CountDistinctResponseCategories(fitData)

        If bInitialValues Then
            Dim expectedCount As Integer = predictorCount + (categoryCount - 1)

            If Not ValidateExpandedInitialValuesCount(
                    expectedCount:=expectedCount,
                    modelCaption:="Ordinal Logistic Regression",
                    extraNote:="Expected one value per expanded predictor plus one threshold for each adjacent outcome split.") Then
                Exit Sub
            End If
        End If

        ordL.Data(fitData, fitVarNames, MyData.RowIds,
                   If(MyData.bOffset, MyData.OffsetData, Nothing),
                   If(MyData.bWeights, MyData.WeightData, Nothing))
        ordL.bReturnCov = Me.ckCovarMatrix.Checked
        ordL.bComputeResiduals = Me.ckResiduals.Checked
        ordL.bIterationDetails = Me.ckIterationsDetails.Checked
        ordL.SettingInputs(alphaValue,
                           ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations"),
                           ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"))
        If bInitialValues Then ordL.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'validated above
        Dim refCat = If(Me.optFirst.Checked, regression.ReferenceCategory.First, regression.ReferenceCategory.Last)
        ordL.Fit(refCat, bInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
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
        WriteRes.shiftColumnPointer(fitVarNames.Length)
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
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Ordinal_LR"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunMultiLogit(MyData As glmData, bInitialValues As Boolean)
        Dim multL = New regression.MultinomialLogitModel()
        Dim alphaValue As Double = Me.spinBtnAlpha.Value
        Dim fitData(,) As Double = Nothing
        Dim fitVarNames() As String = Nothing

        BuildExpandedRegressionInputs(MyData, fitData, fitVarNames)

        Dim predictorCount As Integer = fitVarNames.Length - 1
        Dim categoryCount As Integer = CountDistinctResponseCategories(fitData)
        Dim lIntercept As Integer = If(Me.ckIntercept.Checked, 1, 0)

        If bInitialValues Then
            Dim expectedCount As Integer = (predictorCount + lIntercept) * (categoryCount - 1)

            If Not ValidateExpandedInitialValuesCount(
                    expectedCount:=expectedCount,
                    modelCaption:="Multinomial Logistic Regression",
                    extraNote:="Expected (expanded predictors + intercept, if included) × (number of non-reference outcome categories).") Then
                Exit Sub
            End If
        End If

        multL.data(fitData, fitVarNames, MyData.RowIds,
                   If(MyData.bOffset, MyData.OffsetData, Nothing),
                   If(MyData.bWeights, MyData.WeightData, Nothing))
        multL.bReturnCov = Me.ckCovarMatrix.Checked
        multL.bComputeResiduals = Me.ckResiduals.Checked
        multL.bIterationDetails = Me.ckIterationsDetails.Checked
        multL.settingInputs(alphaValue,
                            ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations"),
                            ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"))
        If bInitialValues Then multL.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'validated above
        Dim refCat = If(Me.optFirst.Checked, regression.ReferenceCategory.First, regression.ReferenceCategory.Last)
        multL.Fit(lIntercept, refCat, bInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
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
        WriteRes.shiftColumnPointer(fitVarNames.Length)
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
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Multinomial_LR"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunZIP(PoissonData As glmData, bPoissonInitialValues As Boolean,
                       LogisticData As glmData, bLogisticInitialValues As Boolean)
        Dim zipFit = New ZeroInflatedPoisson
        Dim alphaValue As Double = Me.spinBtnAlpha.Value

        Dim fitDataPois(,) As Double = Nothing
        Dim fitVarNamesPois() As String = Nothing
        Dim fitDataLog(,) As Double = Nothing
        Dim fitVarNamesLog() As String = Nothing

        BuildExpandedZIPPoissonInputs(PoissonData, fitDataPois, fitVarNamesPois)
        BuildExpandedZIPLogisticInputs(LogisticData, fitDataLog, fitVarNamesLog)

        Dim predictorCountPois As Integer = fitVarNamesPois.Length - 1
        Dim predictorCountLog As Integer = fitVarNamesLog.Length - 1
        Dim interceptPois As Integer = If(Me.ckIntercept.Checked, 1, 0)
        Dim interceptLog As Integer = If(Me.ckInterceptLogistic.Checked, 1, 0)

        If bPoissonInitialValues Then
            If Not ValidateExpandedInitialValuesCount(
                    expectedCount:=predictorCountPois + interceptPois,
                    modelCaption:="Zero-Inflated Poisson Regression (Poisson part)",
                    includeInterceptFirstNote:=(interceptPois = 1),
                    targetTextBox:=Me.tbInitValues) Then
                Exit Sub
            End If
        End If

        If bLogisticInitialValues Then
            If Not ValidateExpandedInitialValuesCount(
                    expectedCount:=predictorCountLog + interceptLog,
                    modelCaption:="Zero-Inflated Poisson Regression (Logistic part)",
                    includeInterceptFirstNote:=(interceptLog = 1),
                    targetTextBox:=Me.tbInitValuesLogistic) Then
                Exit Sub
            End If
        End If

        zipFit.dataInputs(fitDataPois,
                          fitDataLog,
                          fitVarNamesPois,
                          fitVarNamesLog,
                          PoissonData.RowIds,
                          If(PoissonData.bOffset, PoissonData.OffsetData, Nothing),
                          If(PoissonData.bOffset, PoissonData.OffsetVarName, Nothing))

        zipFit.bComputeResiduals = Me.ckResiduals.Checked
        zipFit.bIterationDetails = Me.ckIterationsDetails.Checked
        zipFit.bReturnCov = Me.ckCovarMatrix.Checked
        zipFit.settingInputs(alphaValue,
                             ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations"),
                             ParseUiInteger(Me.tbEMiterations.Text, "EM iterations"),
                             ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"))

        If bPoissonInitialValues Then
            zipFit.startParamsPois = GetNumbersFromStrList(Me.tbInitValues.Text, False)
        End If

        If bLogisticInitialValues Then
            zipFit.startParamsLog = GetNumbersFromStrList(Me.tbInitValuesLogistic.Text, False)
        End If

        zipFit.Fit(interceptPois,
                   interceptLog,
                   bPoissonInitialValues,
                   bLogisticInitialValues,
                   Me.ProgressBar1,
                   Me.lblProgress)

        'Dump results
        Dim WriteRes As WriteResults = New WriteResults
        WriteRes.wb = AppGlobals.app.Workbooks.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

        WriteRes.write({"Poisson"})
        WriteRes.write({"Row ID"})
        WriteRes.write(PoissonData.RowIds, bTall:=True)
        WriteRes.setRowPointer(2)
        WriteRes.setColumnPointer(2)
        WriteRes.write(fitVarNamesPois)
        WriteRes.write(fitDataPois)
        WriteRes.shiftColumnPointer(fitVarNamesPois.Length)
        WriteRes.setRowPointer(1)

        WriteRes.write({"Logistic"})
        WriteRes.write(fitVarNamesLog)
        WriteRes.write(fitDataLog)
        WriteRes.shiftColumnPointer(fitVarNamesLog.Length)
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
        Dim res = zipFit.wrapResults(If(PoissonData.bOffset, PoissonData.OffsetVarName, Nothing), Nothing)

        WriteRes = New WriteResults
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Zero-Inflated Poisson"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunGLMNB2(MyData As glmData, bInitialValues As Boolean)
        Dim lnk As regression.Link
        Dim alphaValue As Double = Me.spinBtnAlpha.Value
        Dim fitData(,) As Double = Nothing
        Dim fitVarNames() As String = Nothing

        BuildExpandedRegressionInputs(MyData, fitData, fitVarNames)

        If Me.cbLink.SelectedItem = "Power" Then
            lnk = regression.createLink(Me.cbLink.SelectedItem, ParseUiDouble(Me.tbPower.Text, "Power link parameter"))
        Else
            lnk = regression.createLink(Me.cbLink.SelectedItem)
        End If

        Dim nb2 = New GLM_NB(lnk)
        Dim lIntercept As Integer = If(Me.ckIntercept.Checked, 1, 0)
        Dim predictorCount As Integer = fitVarNames.Length - 1

        If bInitialValues Then
            If Not ValidateExpandedInitialValuesCount(
                    expectedCount:=predictorCount + lIntercept,
                    modelCaption:="Negative Binomial Regression (NB2)",
                    includeInterceptFirstNote:=(lIntercept = 1)) Then
                Exit Sub
            End If
        End If

        nb2.data(fitData, MyData.RowIds,
                 If(MyData.bOffset, MyData.OffsetData, Nothing),
                 If(MyData.bWeights, MyData.WeightData, Nothing))
        nb2.setVarNames(fitVarNames)
        nb2.bReturnCov = Me.ckCovarMatrix.Checked
        nb2.bComputeResiduals = Me.ckResiduals.Checked
        nb2.bIterationDetails = Me.ckIterationsDetails.Checked
        nb2.settingInputs(alphaValue,
                          ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations"),
                          ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"))
        If bInitialValues Then nb2.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'validated above
        nb2.Fit(lIntercept, bInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
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
        WriteRes.shiftColumnPointer(fitVarNames.Length)
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
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "GLM NB2"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunGLM(MyData As glmData, bInitialValues As Boolean)
        Dim fitGlm As GLM
        Dim alphaValue As Double = Me.spinBtnAlpha.Value
        Dim fitData(,) As Double = Nothing
        Dim fitVarNames() As String = Nothing

        BuildExpandedRegressionInputs(MyData, fitData, fitVarNames)

        Dim fam = regression.createFamily(regression.Family.FamiliesCodes(Me.cbFamily.SelectedIndex))
        If Me.tbDispersionParameterNB2.Text <> String.Empty Then
            Try
                Dim dispParam As Double = ParseUiDouble(Me.tbDispersionParameterNB2.Text, "Dispersion parameter")
                If dispParam > 0 Then fam.pdAlpha = dispParam
            Catch
            End Try
        End If

        Dim lnk As regression.Link
        If Me.cbLink.SelectedItem = "Power" Then
            lnk = regression.createLink(Me.cbLink.SelectedItem, ParseUiDouble(Me.tbPower.Text, "Power link parameter"))
        Else
            lnk = regression.createLink(Me.cbLink.SelectedItem)
        End If

        fitGlm = New GLM(fam, lnk)

        Dim lIntercept As Integer = If(Me.ckIntercept.Checked, 1, 0)
        Dim predictorCount As Integer = fitVarNames.Length - 1

        If bInitialValues Then
            If Not ValidateExpandedInitialValuesCount(
                    expectedCount:=predictorCount + lIntercept,
                    modelCaption:="Generalized Linear Models",
                    includeInterceptFirstNote:=(lIntercept = 1)) Then
                Exit Sub
            End If
        End If

        fitGlm.data(fitData, MyData.RowIds,
                        If(MyData.bOffset, MyData.OffsetData, Nothing),
                        If(MyData.bWeights, MyData.WeightData, Nothing))
        fitGlm.setVarNames(fitVarNames)
        fitGlm.bReturnCov = Me.ckCovarMatrix.Checked
        fitGlm.bComputeResiduals = Me.ckResiduals.Checked
        fitGlm.bIterationDetails = Me.ckIterationsDetails.Checked
        fitGlm.settingInputs(alphaValue,
                             ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations"),
                             ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"))
        If bInitialValues Then fitGlm.startParams = GetNumbersFromStrList(Me.tbInitValues.Text, False) 'validated above
        fitGlm.Fit(lIntercept, bInitialValues, Me.ProgressBar1, Me.lblProgress)

        'Dump results
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
        WriteRes.shiftColumnPointer(UBound(fitData, 2) + 1)

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
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "GLM"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

        If IsCurrentBinomialGlmFamily() AndAlso Me.grpClassification.Enabled And cbPerformClasification.Checked Then
            Dim y() As Double = fitGlm.ObservedResponses()
            Dim p() As Double = fitGlm.PredictedResponses()
            Dim weights() As Double = fitGlm.ObservationWeights()
            Dim threshold As Double = GetClassificationThresholdUiValue()

            regression.BinaryClassificationReporting.ValidateBinaryInputs(y, p, weights)

            Dim summary As regression.BinaryClassificationSummary = regression.BinaryClassificationReporting.ComputeBinarySummary(y, p, threshold, weights)

            Dim thresholdRows As List(Of regression.BinaryThresholdRow) = Nothing
            If Me.cbOutputTresholdTable.Checked Then thresholdRows = regression.BinaryClassificationReporting.BuildThresholdTable(y, p, Nothing, weights)
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
                summary, thresholdRows, calibrationRows, brier, eventRate, "GLM Binary Classification")

            If clsRes IsNot Nothing AndAlso clsRes.Count > 0 Then
                WriteRes = New WriteResults
                AppGlobals.app.ActiveWorkbook.Worksheets.Add(After:=AppGlobals.app.ActiveWorkbook.Worksheets(AppGlobals.app.ActiveWorkbook.Worksheets.Count))
                AppGlobals.app.ActiveWorkbook.ActiveSheet.Name = "GLM Classification"
                WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet

                Dim rrClass = New ProcessListofResultTables(clsRes)
                rrClass.writeToSheet(WriteRes, True)

                AddRocResultsAndPlotToClassificationSheet(WriteRes, y, p, CDbl(Me.spinBtnAlpha.Value))

                If calibrationRows IsNot Nothing Then
                    Dim cp As New graphics.CalibrationPlot(calibrationRows)
                    Dim cht = cp.addCalibrationPlot(WriteRes.ws)
                End If
            End If
        End If

    End Sub

    Private Sub tbInitValues_Leave(sender As Object, e As System.EventArgs) Handles tbInitValues.Leave
        Dim vals() As Double, bErr As Boolean, tiptext As String

        setTextBoxProperties(Me.tbInitValues, Color.White, String.Empty)
        vals = GetNumbersFromStrList(Me.tbInitValues.Text, bErr)
        If bErr Then
            tiptext = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers without thousands separators."
            setTextBoxProperties(Me.tbInitValues, Color.Red, tiptext)
            Exit Sub
        End If

        'Exact initial-value length validation is performed after predictor expansion
        'during model fitting.
    End Sub

    Private Sub tbInitValuesLogistic_Leave(sender As Object, e As System.EventArgs) Handles tbInitValuesLogistic.Leave
        Dim vals() As Double, bErr As Boolean, tiptext As String

        setTextBoxProperties(Me.tbInitValuesLogistic, Color.White, String.Empty)
        vals = GetNumbersFromStrList(Me.tbInitValuesLogistic.Text, bErr)
        If bErr Then
            tiptext = "Cannot convert provided string to the array of double digits. Please provide space separated list of numbers without thousands separators."
            setTextBoxProperties(Me.tbInitValuesLogistic, Color.Red, tiptext)
            Exit Sub
        End If

        'Exact initial-value length validation is performed after predictor expansion
        'during model fitting.
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
                        If Me.lbSelectedEffectsList.Items.Count > 0 Then Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
                    End If
                End If
            End If
            If Me.Text = "Zero-Inflated Poisson Regression" Then
                If Not IsEqualListBox(Me.lbXs, Me.lbSelectedVariablesLogistic) Then
                    'values on 1st tab changed so refresh it with new values
                    If Me.lbSelectedVariablesLogistic.Items.Count > 0 Then
                        Remove_Item(Me.lbSelectedVariablesLogistic)
                    End If
                    For i = 0 To Me.lbXs.Items.Count - 1
                        Me.lbSelectedVariablesLogistic.Items.Add(Me.lbXs.Items(i))
                    Next i
                    If Not IsSubsetListBox(Me.lbSelectedVariablesLogistic, Me.lbSelectedEffectsListLogistic, bOnlyMain:=True) Then
                        If MsgBox("There is a variable in selected effects list that was removed from the predictor variable(s) list." & vbNewLine & vbNewLine &
                              "Clear selected effects list?", vbYesNo + vbExclamation, "Clear selected effects list?") = vbYes Then
                            If Me.lbSelectedEffectsListLogistic.Items.Count > 0 Then
                                Remove_Item(Me.lbSelectedEffectsListLogistic, "all", Me.TermSpecsLogistic)
                            End If
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
        Remove_Item(Me.lbSelectedEffectsList, "all", Me.TermSpecs)
    End Sub

    Private Sub tbRemoveSelectedEffects_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffects.Click
        Remove_Item(Me.lbSelectedEffectsList, "selected", Me.TermSpecs)
    End Sub

    Private Sub btAddEffect_Click(sender As Object, e As System.EventArgs) Handles btAddEffect.Click
        Me.EffectsController.AddMainEffectsFromSelectedVars()
    End Sub

    Private Sub btAddEffectLogistic_Click(sender As Object, e As System.EventArgs) Handles btAddEffectLogistic.Click
        Me.EffectsControllerLogistic.AddMainEffectsFromSelectedVars()
    End Sub

    Private Sub btAddEffectCategoricalFactorLogistic_Click(sender As Object, e As System.EventArgs) Handles btAddEffectCategoricalFactorLogistic.Click
        Me.EffectsControllerLogistic.AddCategoricalEffectsFromSelectedVars()
    End Sub

    Private Sub btnPolyLogistic_Click(sender As Object, e As System.EventArgs) Handles btnPolyLogistic.Click
        Me.EffectsControllerLogistic.AddPolynomialEffectsFromSelectedVars(CInt(Me.spinBtnPolyLogistic.Value))
    End Sub

    Private Sub btn2InteractionsLogistic_Click(sender As Object, e As System.EventArgs) Handles btn2InteractionsLogistic.Click
        Me.EffectsControllerLogistic.AddTwoWayInteractionsFromSelectedVars()
    End Sub

    Private Sub btnCustomInteractionLogistic_Click(sender As Object, e As System.EventArgs) Handles btnCustomInteractionLogistic.Click
        Me.EffectsControllerLogistic.AddCustomInteractionFromSelectedVars()
    End Sub

    Private Sub tbRemoveSelectedEffectsLogistic_Click(sender As Object, e As System.EventArgs) Handles tbRemoveSelectedEffectsLogistic.Click
        Remove_Item(Me.lbSelectedEffectsListLogistic, "selected", Me.TermSpecsLogistic)
    End Sub

    Private Sub btClearAllSelectedEffectsLogistic_Click(sender As Object, e As System.EventArgs) Handles btClearAllSelectedEffectsLogistic.Click
        Remove_Item(Me.lbSelectedEffectsListLogistic, "all", Me.TermSpecsLogistic)
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

    Private Sub btAddEffectCategoricalFactor_Click(sender As Object, e As System.EventArgs) Handles btAddEffectCategoricalFactor.Click
        Me.EffectsController.AddCategoricalEffectsFromSelectedVars()
    End Sub

    Private Sub cbFamily_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbFamily.SelectedIndexChanged
        RefreshLinkOptionsForSelectedFamily(GetCanonicalLinkFromDisplayName(Me.cbFamily.SelectedItem.ToString()))
    End Sub

    Private Sub cbLink_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbLink.SelectedIndexChanged
        UpdatePowerLinkState()
    End Sub

    Private Sub cbOutputCalibrationTable_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbOutputCalibrationTable.CheckedChanged
        UpdateClassificationOptionsState()
    End Sub

    Private Sub cbPerformClasification_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbPerformClasification.CheckedChanged
        If Me.cbPerformClasification.Checked And IsCurrentBinomialGlmFamily() Then
            Me.grpClassification.Enabled = True
        ElseIf Not Me.cbPerformClasification.Checked And IsCurrentBinomialGlmFamily() Then
            Me.grpClassification.Enabled = False
        End If
    End Sub
End Class