Imports System.Security.Cryptography
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui9ANOVA2nested
    Sub New(analysis As String)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Text = analysis

        ' Add any initialization after the InitializeComponent() call.
        Me.RefEdit1_Group.ExcelConnector = AppInfrastructure.AppGlobals.app
        Me.RefEdit2_nested.ExcelConnector = AppInfrastructure.AppGlobals.app
        Me.RefEdit3_Data.ExcelConnector = AppInfrastructure.AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppInfrastructure.AppGlobals.app
        Me.TabPageOptionsBlandAltman.Parent = Nothing
        Me.TabPageDecisionLimitsBlandAltman.Parent = Nothing

        If Me.Text = "Passing-Bablok Regression" Then
            Me.lblRefedit1_Group.Text = "Group (optional)"
            Me.lblRefedit2_Nested.Text = "Reference method (X)"
            Me.lblRefedit3_Data.Text = "Test method (Y)"

        ElseIf Me.Text = "Bland–Altman Analysis" Then
            Me.TabPageOptionsBlandAltman.Parent = Me.TabControl1
            Me.TabPageDecisionLimitsBlandAltman.Parent = Me.TabControl1
            Me.lblRefedit1_Group.Text = "Subject ID (optional)"
            Me.lblRefedit2_Nested.Text = "Reference method (X)"
            Me.lblRefedit3_Data.Text = "Test method (Y)"
            Me.cmbBlandMode.Items.AddRange(New Object() {"Auto", "Simple pairs", "Repeated by subject"})
            Me.cmbBlandScale.Items.AddRange(New Object() {"Raw difference", "% of paired mean", "% of reference", "% of test", "Log ratio"})
            Me.cmbBlandXAxis.Items.AddRange(New Object() {"Mean of methods", "Reference method", "Test method"})
            Me.cmbBlandPlotMode.Items.AddRange(New Object() {"All observations", "Subject means only", "All observations + subject means"})
            Me.cmbBlandPlotMode.SelectedIndex = 2
            Me.cmbBlandXAxis.SelectedIndex = 0
            Me.cmbBlandScale.SelectedIndex = 0
            Me.cmbBlandMode.SelectedIndex = 0
            Me.spinBtnBlandAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnBlandAlpha.Minimum, Me.spinBtnBlandAlpha.Maximum)
            Me.ApplyBlandDecisionLimitState()

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
        ElseIf (Me.Text = "Passing-Bablok Regression" OrElse Me.Text = "Bland–Altman Analysis") And Me.RefEdit1_Group.Address <> String.empty Then
            If CheckRefEdit(Me.RefEdit1_Group.Address, True) Then
                RefEditReset(Me.RefEdit1_Group)
                bOut = True
            End If
        End If

        If CheckRefEdit(Me.RefEdit2_nested.Address, True) Then
            RefEditReset(Me.RefEdit2_nested)
            bOut = True
        End If

        Dim bOneColumn As Boolean = True
        If Me.Text = "Two-Way Nested ANOVA" Then
            bOneColumn = False
        ElseIf Me.Text = "Passing-Bablok Regression" OrElse Me.Text = "Bland–Altman Analysis" Then
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
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit2_nested.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit3_Data.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If

        refGrp = prepareRef2D(Me.RefEdit1_Group.Address)
        refNest = prepareRef2D(Me.RefEdit2_nested.Address)
        refData = prepareRef2D(Me.RefEdit3_Data.Address)

        refFinal = refGrp & ", " &
               Replace(refNest, wks & "!", String.Empty) & ", " &
               Replace(refData, wks & "!", String.Empty)  'Remove "Sheet1!" from string

        ExcelDnaDataImporter.ImportInto(byIdData, refFinal, True, 1)

        If byIdData.varNames.Length = 0 Then
            strErr = "Zero valid data!"
            AppInfrastructure.CoreServices.Log("Zero valid data!", AppInfrastructure.LogMsgType.Warn)
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

        Dim wks As String = WorksheetNameFromRefAdress(Me.RefEdit2_nested.Address, True)
        If wks <> WorksheetNameFromRefAdress(Me.RefEdit3_Data.Address, True) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If

        refX = prepareRef2D(Me.RefEdit2_nested.Address)
        refY = prepareRef2D(Me.RefEdit3_Data.Address)

        If Me.RefEdit1_Group.Address <> String.Empty Then 'optional
            If wks <> WorksheetNameFromRefAdress(Me.RefEdit1_Group.Address, True) Then
                strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            End If

            refGrp = prepareRef2D(Me.RefEdit1_Group.Address)

            refFinal = refGrp & ", " &
                       Replace(refX, wks & "!", String.Empty) & ", " &
                       Replace(refY, wks & "!", String.Empty)  'Remove "Sheet1!" from string
            ExcelDnaDataImporter.ImportInto(byIdData, refFinal, True, 1) 'first column can be character
        Else
            refFinal = refX & ", " &
                       Replace(refY, wks & "!", String.Empty)
            ExcelDnaDataImporter.ImportInto(byIdData, refFinal, True)
        End If


        If byIdData.varNames.Length = 0 Then
            strErr = "Zero valid data!"
            AppInfrastructure.CoreServices.Log("Zero valid data!", AppInfrastructure.LogMsgType.Warn)
            Return Nothing
        End If

        out.X = byIdData.FinalData
        out.varNames = byIdData.varNames

        Return out
    End Function

    Private Sub RunBlandAltman(d As MultiGroupsPairedDataObj)
        Dim WriteRes As New ExcelDnaResultWriter
        Dim x() As Double = Nothing, y() As Double = Nothing
        Dim subIds() As Object = Nothing

        If d Is Nothing OrElse d.X Is Nothing Then Exit Sub

        Dim hasSubjectIds As Boolean = (d.X.GetLength(1) = 3)
        If hasSubjectIds Then
            subIds = Matrix.GetColumnFrom2Darray(d.X, 0)
            x = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 1))
            y = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 2))
        Else
            x = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 0))
            y = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 1))
        End If

        Dim lowerAllowable As Double = Double.NaN
        Dim upperAllowable As Double = Double.NaN
        Dim decisionErr As String = String.Empty
        Dim runDecisionLimits As Boolean = Me.ckBlandDecisionLimitsEnable.Checked

        If runDecisionLimits Then
            If Not Me.TryGetBlandDecisionLimitInputs(lowerAllowable, upperAllowable, hasSubjectIds, decisionErr) Then
                MsgBox(decisionErr, vbExclamation)
                Exit Sub
            End If
        End If

        Dim opts As New Agreement.BlandAltmanOptions With {
        .Alpha = CDbl(Me.spinBtnBlandAlpha.Value),
        .UseTDistribution = Me.ckBlandUseTDistribution.Checked,
        .BootstrapReplicates = CInt(Val(Me.tbBlandBootstrapReps.Text)),
        .SubjectIds = subIds,
        .ExcludeSingletonSubjects = Me.ckBlandExcludeSingletonSubjects.Checked,
        .MinSubjects = CInt(Me.spinBtnBlandMinSubjects.Value),
        .MinPairsPerSubject = CInt(Me.spinBtnBlandMinPairs.Value),
        .CheckProportionalBias = Me.ckBlandCheckProportionalBias.Checked,
        .AllowFallbackToSimple = Me.ckBlandAllowFallback.Checked
    }

        Select Case Me.cmbBlandMode.SelectedIndex
            Case 1 : opts.Mode = Agreement.RepeatedBlandAltmanMode.SimplePairs
            Case 2 : opts.Mode = Agreement.RepeatedBlandAltmanMode.RepeatedBySubject
            Case Else : opts.Mode = Agreement.RepeatedBlandAltmanMode.Auto
        End Select

        Select Case Me.cmbBlandScale.SelectedIndex
            Case 1 : opts.Scale = Agreement.BlandAltmanScale.PercentOfMean
            Case 2 : opts.Scale = Agreement.BlandAltmanScale.PercentOfReference
            Case 3 : opts.Scale = Agreement.BlandAltmanScale.PercentOfTest
            Case 4 : opts.Scale = Agreement.BlandAltmanScale.LogRatio
            Case Else : opts.Scale = Agreement.BlandAltmanScale.RawDifference
        End Select

        Select Case Me.cmbBlandXAxis.SelectedIndex
            Case 1 : opts.XAxisMode = Agreement.BlandAltmanXAxisMode.ReferenceMethod
            Case 2 : opts.XAxisMode = Agreement.BlandAltmanXAxisMode.TestMethod
            Case Else : opts.XAxisMode = Agreement.BlandAltmanXAxisMode.MeanOfMethods
        End Select

        Select Case Me.cmbBlandPlotMode.SelectedIndex
            Case 1 : opts.PlotMode = Agreement.RepeatedBlandAltmanPlotMode.SubjectMeansOnly
            Case 2 : opts.PlotMode = Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans
            Case Else : opts.PlotMode = Agreement.RepeatedBlandAltmanPlotMode.AllObservations
        End Select

        If Me.optBlandBootstrap.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.BootstrapPercentile
        ElseIf Me.optBlandBootstrapBCa.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.BootstrapBCa
        ElseIf Me.optBlandJackknife.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.Jackknife
        Else
            opts.CiMethod = Agreement.AgreementCiMethod.Analytical
        End If

        If opts.BootstrapReplicates <= 0 Then opts.BootstrapReplicates = 2000

        Dim ba As New Agreement.BlandAltmanAgreement(x, y,
                                             If(hasSubjectIds, d.varNames(1), d.varNames(0)),
                                             If(hasSubjectIds, d.varNames(2), d.varNames(1)),
                                             opts)
        Dim fit = ba.Fit()
        Dim res = ba.wrapResults()

        If runDecisionLimits Then
            Dim biasAssessment = equivalencetests.EquivalenceNonInferiorityMethods.AssessAllowableBias(
            fit, lowerAllowable, upperAllowable)

            Dim loaAssessment = equivalencetests.EquivalenceNonInferiorityMethods.AssessBlandAltmanAgainstDecisionLimits(
            fit, lowerAllowable, upperAllowable)

            Dim scaleText As String = If(String.IsNullOrWhiteSpace(Me.cmbBlandScale.Text), "Raw difference", Me.cmbBlandScale.Text)
            res.Add(Me.BuildBlandAllowableBiasTable(biasAssessment, scaleText, fit.UsedRepeatedModel))
            res.Add(Me.BuildBlandDecisionLimitTable(loaAssessment, scaleText, fit.UsedRepeatedModel))
        End If

        Dim rr = New ProcessListofResultTables(res)

        WriteRes = GetResultWriter()
        Dim totrows As Integer = rr.TotRows + res.Count + res.Count - 1
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then Exit Sub
        End If

        rr.writeToSheet(WriteRes, True)
        ba.AddPlot(WriteRes.ws)
    End Sub

    Private Sub RunPassingBablok(d As MultiGroupsPairedDataObj)
        Dim WriteRes = New ExcelDnaResultWriter
        Dim bGrouped As Boolean = False
        Dim x() As Double = Nothing, y() As Double = Nothing, grp() As Object = Nothing
        Dim pb As Agreement.PassinbBablok = Nothing

        If d.X.GetLength(1) = 2 Then
            bGrouped = False
        Else
            bGrouped = True
        End If

        If bGrouped Then
            grp = Matrix.GetColumnFrom2Darray(d.X, 0)
            x = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 1))
            y = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 2))
            pb = New Agreement.PassinbBablok(x, y, d.varNames(1), d.varNames(2), grp, d.varNames(0))
            pb.GroupedBlockPassingBablok()
        Else
            x = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 0))
            y = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(d.X, 1))
            pb = New Agreement.PassinbBablok(x, y, d.varNames(0), d.varNames(1))
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
        Dim WriteRes = New ExcelDnaResultWriter
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

    Private Function GetResultWriter() As ExcelDnaResultWriter
        Dim WriteRes = New ExcelDnaResultWriter, rRange As Range
        If Me.optWorkbook.Checked Then
            WriteRes.wb = AppInfrastructure.AppGlobals.app.Workbooks.Add()
            WriteRes.ws = AppInfrastructure.AppGlobals.app.ActiveWorkbook.ActiveSheet
        ElseIf Me.optWorksheet.Checked Then
            WriteRes.wb = AppInfrastructure.AppGlobals.app.ActiveWorkbook
            WriteRes.wb.Worksheets.Add()
            WriteRes.ws = AppInfrastructure.AppGlobals.app.ActiveWorkbook.ActiveSheet
        Else
            WriteRes.wb = AppInfrastructure.AppGlobals.app.ActiveWorkbook
            WriteRes.ws = WorksheetFromRefAdress(Me.RefEditOutput.Address)
            rRange = WriteRes.ws.Range(Me.RefEditOutput.Address)
            WriteRes.setRowPointer(rRange.Row)
            WriteRes.setColumnPointer(rRange.Column)
        End If

        Return WriteRes
    End Function

    Private Sub UpdateBlandAltmanOptionState() Handles cmbBlandMode.SelectedIndexChanged,
                                                   cmbBlandScale.SelectedIndexChanged,
                                                   optBlandAnalytical.CheckedChanged,
                                                   optBlandJackknife.CheckedChanged,
                                                   optBlandBootstrap.CheckedChanged,
                                                   optBlandBootstrapBCa.CheckedChanged,
                                                   ckBlandDecisionLimitsEnable.CheckedChanged
        If Me.cmbBlandMode Is Nothing Then Exit Sub

        Dim repeatedRequested As Boolean = (Me.cmbBlandMode.SelectedIndex <> 1)
        Me.cmbBlandPlotMode.Enabled = repeatedRequested
        Me.ckBlandExcludeSingletonSubjects.Enabled = repeatedRequested
        Me.ckBlandAllowFallback.Enabled = repeatedRequested
        Me.spinBtnBlandMinSubjects.Enabled = repeatedRequested
        Me.spinBtnBlandMinPairs.Enabled = repeatedRequested
        Me.lblBlandMinSubjects.Enabled = repeatedRequested
        Me.lblBlandMinPairs.Enabled = repeatedRequested

        Dim useBootstrap As Boolean = Me.optBlandBootstrap.Checked OrElse Me.optBlandBootstrapBCa.Checked
        Me.tbBlandBootstrapReps.Enabled = useBootstrap
        Me.lblBlandBootstrapReps.Enabled = useBootstrap
        Me.ckBlandUseTDistribution.Enabled = Not useBootstrap

        Me.ApplyBlandDecisionLimitState()
    End Sub

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim errText As String = String.Empty
            Dim Data As MultiGroupsPairedDataObj = Nothing

            'Validate Inputs
            If Me.checkInputs() Then Exit Sub

            'Get Data
            If Me.Text = "Two-Way Nested ANOVA" Then
                Data = Me.getData(errText)
            ElseIf Me.Text = "Passing-Bablok Regression" OrElse Me.Text = "Bland–Altman Analysis" Then
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
            ElseIf Me.Text = "Bland–Altman Analysis" Then
                Me.RunBlandAltman(Data)
            End If

        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True)
        End Try
    End Sub

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

    Private Function TryGetBlandDecisionLimitInputs(ByRef lowerAllowable As Double, ByRef upperAllowable As Double,
                                                    hasSubjectIds As Boolean, ByRef errText As String) As Boolean
        errText = String.Empty
        lowerAllowable = Double.NaN
        upperAllowable = Double.NaN

        If Not Me.ckBlandDecisionLimitsEnable.Checked Then Return True

        lowerAllowable = ParseUiDouble(Me.tbBlandLowerAllowable.Text, "Lower acceptable limit")
        upperAllowable = ParseUiDouble(Me.tbBlandUpperAllowable.Text, "Upper acceptable limit")

        If lowerAllowable > upperAllowable Then
            errText = "The lower acceptable limit must not exceed the upper acceptable limit."
            Return False
        End If
        Return True
    End Function

    Private Sub ApplyBlandDecisionLimitState()
        Dim enabled As Boolean = Me.ckBlandDecisionLimitsEnable.Checked

        Me.lblBlandLowerAllowable.Enabled = enabled
        Me.tbBlandLowerAllowable.Enabled = enabled
        Me.lblBlandUpperAllowable.Enabled = enabled
        Me.tbBlandUpperAllowable.Enabled = enabled
        Me.lblBlandDecisionLimitsHelp.Enabled = enabled

        If Not enabled Then
            Me.lblBlandDecisionLimitsHelp.Text = "Enable this to compare the fitted bias and limits of agreement with pre-specified lower and upper acceptable limits."
            Exit Sub
        End If

        Dim scaleText As String = If(Me.cmbBlandScale Is Nothing OrElse Me.cmbBlandScale.SelectedIndex < 0, "the active analysis scale", Me.cmbBlandScale.Text)
        Dim repeatedRequested As Boolean = False
        If Me.cmbBlandMode IsNot Nothing Then repeatedRequested = (Me.cmbBlandMode.SelectedIndex = 2 OrElse Me.cmbBlandMode.SelectedIndex = 0)

        Dim scaleHint As String
        Select Case Me.cmbBlandScale.SelectedIndex
            Case 1, 2, 3
                scaleHint = "Enter the allowable lower and upper limits on the selected percent scale."
            Case 4
                scaleHint = "Enter the allowable lower and upper limits on the selected log-ratio scale."
            Case Else
                scaleHint = "Enter the allowable lower and upper limits on the raw-difference scale."
        End Select

        Dim repeatedHint As String = If(repeatedRequested,
                                    " If the repeated-measures model is used, the assessment is based on the repeated-measures Bland–Altman bias and limits of agreement.",
                                    String.Empty)

        Me.lblBlandDecisionLimitsHelp.Text =
            $"Decision-limit / allowable-bias reporting now follows the current Bland–Altman analysis settings. " &
            $"Limits are interpreted on the current scale ({scaleText})." & Environment.NewLine &
            scaleHint & repeatedHint
    End Sub

    Private Function BuildBlandAllowableBiasTable(resBias As equivalencetests.MarginCiAssessmentResult,
                                              analysisScaleText As String,
                                              usedRepeatedModel As Boolean) As ResultTable
        Dim t As New ResultTable
        t.AddHeaderTopRow({"Allowable-bias assessment", ""})
        t.SetBody({
                {"Analysis scale", analysisScaleText},
                {"Repeated-measures model used", usedRepeatedModel},
                {"Lower acceptable limit", resBias.LowerMargin},
                {"Upper acceptable limit", resBias.UpperMargin},
                {"Bias estimate", resBias.Estimate},
                {"Bias confidence interval", resBias.ConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Point estimate within limits", resBias.IsPointEstimateWithinMargins},
                {"Confidence interval within limits", resBias.IsConfidenceIntervalWithinMargins},
                {"Lower-bound noninferiority supported", resBias.SupportsLowerNonInferiority},
                {"Upper-bound noninferiority supported", resBias.SupportsUpperNonInferiority},
                {"Conclusion", resBias.Conclusion}
            })
        t.AddFootnote("The acceptable limits are interpreted on the active Bland–Altman analysis scale.")
        Return t
    End Function

    Private Function BuildBlandDecisionLimitTable(resDecision As equivalencetests.BlandAltmanDecisionLimitAssessmentResult,
                                              analysisScaleText As String,
                                              usedRepeatedModel As Boolean) As ResultTable
        Dim t As New ResultTable
        t.AddHeaderTopRow({"Bland–Altman decision-limit assessment", ""})
        t.SetBody({
                {"Analysis scale", analysisScaleText},
                {"Repeated-measures model used", usedRepeatedModel},
                {"Lower acceptable limit", resDecision.LowerAllowableLimit},
                {"Upper acceptable limit", resDecision.UpperAllowableLimit},
                {"Bias estimate", resDecision.BlandAltman.BiasCI.Estimate},
                {"Bias confidence interval", resDecision.BlandAltman.BiasCI.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Lower limit of agreement", resDecision.BlandAltman.LowerLoACI.Estimate},
                {"Lower LoA confidence interval", resDecision.BlandAltman.LowerLoACI.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Upper limit of agreement", resDecision.BlandAltman.UpperLoACI.Estimate},
                {"Upper LoA confidence interval", resDecision.BlandAltman.UpperLoACI.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Observed LoA within allowable limits", resDecision.AreObservedLoAWithinAllowableLimits},
                {"LoA confidence intervals within allowable limits", resDecision.AreLoAConfidenceIntervalsWithinAllowableLimits},
                {"Conclusion", resDecision.Conclusion}
            })
        t.AddFootnote("The acceptable limits are interpreted on the active Bland–Altman analysis scale.")
        Return t
    End Function
End Class