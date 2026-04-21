Imports System.Security.Cryptography
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel


Public Class UiTwoInputRefedits

    Sub New(analysis As String)
        ' This call is required by the designer.
        InitializeComponent()

        Me.RefEdit1.ExcelConnector = AppGlobals.app
        Me.RefEdit2.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.RefEditDemingSDx.ExcelConnector = AppGlobals.app
        Me.RefEditDemingSDy.ExcelConnector = AppGlobals.app
        Me.Text = analysis
        Me.spinBtnAlphaGlobal.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlphaGlobal.Minimum, Me.spinBtnAlphaGlobal.Maximum)
        Me.spinBtnAlphaDeming.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlphaDeming.Minimum, Me.spinBtnAlphaDeming.Maximum)
        Me.spinBtnAlphaHottelings.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlphaHottelings.Minimum, Me.spinBtnAlphaHottelings.Maximum)
        Me.spinBtnAlphaLinCCC.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlphaLinCCC.Minimum, Me.spinBtnAlphaLinCCC.Maximum)
        Me.spinBtnAlphaKappa.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlphaKappa.Minimum, Me.spinBtnAlphaKappa.Maximum)

        Me.TabPageOptionsHotteling.Parent = Nothing
        Me.TabPageOptions.Parent = Nothing
        Me.TabPageOptionsLinCCC.Parent = Nothing
        Me.TabPageOptionsKappa.Parent = Nothing
        Me.TabPageOptionsDeming.Parent = Nothing
        Me.ckFirstRow.Visible = False
        Me.SetDemingExtendedControlsVisible(False)

        If Me.Text = "Hotelling's T-Squared Test" Then
            Me.TabPageOptionsHotteling.Parent = Me.TabMultipage

        ElseIf Me.Text = "Deming Regression" Then
            Me.TabPageOptionsDeming.Parent = Me.TabMultipage
            Me.lblRefedit1.Text = "Reference method (X)"
            Me.lblRefedit2.Text = "Test method (Y)"
            Me.cmbDemingVarianceModel.Items.AddRange(New Object() {"Constant lambda", "Constant CV", "Known pointwise SD"})
            Me.cmbDemingVarianceModel.SelectedIndex = 0
            Me.SetDemingExtendedControlsVisible(True)
            Me.ApplyDemingControlState()

        ElseIf analysis = "Kendall's Rank Correlation" Or
               analysis = "Spearman Rank Correlation" Or
               analysis = "Theil-Sen Simple Regression" Then
            Me.TabPageOptions.Parent = Me.TabMultipage
            Me.ckSignTest.Visible = False
            Me.lblAlphaGlobal.Visible = True
            Me.lblAlphaGlobal.Location = New System.Drawing.Point(19, 45)
            Me.spinBtnAlphaGlobal.Visible = True
            Me.spinBtnAlphaGlobal.Location = New System.Drawing.Point(67, 43)

        ElseIf analysis = "Wilcoxon Signed Rank Test" Then
            Me.TabPageOptions.Parent = Me.TabMultipage
            Me.ckSignTest.Visible = True

        ElseIf Me.Text = "Paired T-test" Then
            Me.TabPageOptions.Parent = Me.TabMultipage
            Me.ckSignTest.Visible = False
            Me.lblAlphaGlobal.Visible = False
            Me.spinBtnAlphaGlobal.Visible = False

        ElseIf Me.Text = "Lin's Concordance Correlation Coefficient" Then
            Me.TabPageOptionsLinCCC.Parent = Me.TabMultipage
            Me.lblRefedit1.Text = "Reference method (X)"
            Me.lblRefedit2.Text = "Test method (Y)"
            Me.ApplyLinCCCControlState()

        ElseIf Me.Text = "Cohen's / Weighted Kappa" Then
            Me.TabPageOptionsKappa.Parent = Me.TabMultipage
            Me.cmbWeightingSchemeKappa.Items.AddRange(New Object() {"Unweighted (Cohen's Kappa)", "Linear weights", "Quadratic weights", "Cicchetti-Allison", "Fleiss-Cohen"})
            Me.cmbWeightingSchemeKappa.SelectedIndex = 0
            Me.ckFirstRow.Visible = True
            ApplyKappaControlState()

        End If

        Me.RefEdit1.txtAddress.Select()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Function getData(ByRef strErr As String) As TwoGroupsPairedData
        Dim out = New TwoGroupsPairedData
        Dim columData = New DataObj
        'for by ID declarations
        Dim refId As String, refData As String, refFinal As String

        If WorksheetNameFromRefAdress(Me.RefEdit1.Address, True, Me.RefEdit1.ExcelWorkBook) <>
            WorksheetNameFromRefAdress(Me.RefEdit2.Address, True, Me.RefEdit2.ExcelWorkBook) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
        End If

        refId = prepareRef2D(Me.RefEdit1.Address, Me.RefEdit1.ExcelWorkBook)
        refData = prepareRef2D(Me.RefEdit2.Address, Me.RefEdit2.ExcelWorkBook)
        'join reference address into one (remove sheet name from the second and concatenate).
        'Data can be only form one sheet because of the above check
        refFinal = refId & ", " & Replace(refData, WorksheetNameFromRefAdress(refData, True, Me.RefEdit2.ExcelWorkBook) & "!", String.Empty) 'Remove "Sheet1!" from string
        columData.DataInport(refFinal, True)

        'get unique group IDs
        out.X = columData.DataDbl
        out.name1 = columData.varNames(0)
        out.name2 = columData.varNames(1)

        Return out
    End Function

    Private Function getPairedCategoricalData(ByRef strErr As String) As (x As Object(), y As Object(), name1 As String, name2 As String)
        Dim columData = New DataObj
        Dim refId As String, refData As String, refFinal As String

        If WorksheetNameFromRefAdress(Me.RefEdit1.Address, True, Me.RefEdit1.ExcelWorkBook) <>
            WorksheetNameFromRefAdress(Me.RefEdit2.Address, True, Me.RefEdit2.ExcelWorkBook) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            Return (Nothing, Nothing, Nothing, Nothing)
        End If

        refId = prepareRef2D(Me.RefEdit1.Address, Me.RefEdit1.ExcelWorkBook)
        refData = prepareRef2D(Me.RefEdit2.Address, Me.RefEdit2.ExcelWorkBook)
        refFinal = refId & ", " & Replace(refData, WorksheetNameFromRefAdress(refData, True, Me.RefEdit2.ExcelWorkBook) & "!", String.Empty)

        'Allow character values in both paired columns for categorical agreement analysis.
        Dim iStart As Integer = If(Me.ckFirstRow.Checked, 1, 0)
        columData.DataInport(refFinal, True, CharCols:=1, iStart)

        If columData.bZeroValid Then
            strErr = "No valid paired categorical observations in the input ranges."
            Return (Nothing, Nothing, Nothing, Nothing)
        End If

        If Not Me.ckFirstRow.Checked Then
            columData.varNames(0) = "Group 1"
            columData.varNames(1) = "Group 2"
        End If

        Return (Matrix.GetColumnFrom2Darray(columData.FinalData, 0),
                Matrix.GetColumnFrom2Darray(columData.FinalData, 1),
                columData.varNames(0),
                columData.varNames(1))
    End Function

    Private Function getDataHotteling(ByRef strErr As String) As (DataObj, DataObj)
        Dim Gr1Data = New DataObj, Gr2Data = New DataObj
        Dim refGr1 As String, refGr2 As String

        If WorksheetNameFromRefAdress(Me.RefEdit1.Address, True, Me.RefEdit1.ExcelWorkBook) <>
            WorksheetNameFromRefAdress(Me.RefEdit2.Address, True, Me.RefEdit2.ExcelWorkBook) Then
            strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            Return (Nothing, Nothing)
        End If

        refGr1 = prepareRef2D(Me.RefEdit1.Address, Me.RefEdit1.ExcelWorkBook)
        Gr1Data.DataInport(refGr1, True)
        Dim x1(,) As Double = Gr1Data.DataDbl
        If Gr1Data.bZeroValid Then
            strErr = "No valid data in the 1st input."
            Return (Nothing, Nothing)
        End If

        refGr2 = prepareRef2D(Me.RefEdit2.Address, Me.RefEdit2.ExcelWorkBook)
        Gr2Data.DataInport(refGr2, True)
        Dim x2(,) As Double = Gr2Data.DataDbl
        If Gr2Data.bZeroValid Then
            strErr = "No valid data in the 2nd input."
            Return (Nothing, Nothing)
        End If

        If UBound(x1, 2) <> UBound(x2, 2) Then
            strErr = $"Input data ranges must have the same number of columns. Group1 #columns= {x1.GetLength(1)} Group2 #columns={x2.GetLength(1)}."
            Return (Nothing, Nothing)
        End If

        If Me.optSingle.Checked Then 'only one row is expected
            If UBound(x2, 1) <> 0 Then
                strErr = "Input data range 2 should contain only one row."
                Return (Nothing, Nothing)
            End If
        End If

        If optPaired.Checked Then
            'get only common rows in both ranges. This is required only for the Paired versoin  of Hotelling's T2
            Dim commonRows() As Integer = Gr1Data.RowIds.Intersect(Gr2Data.RowIds).ToArray()
            Gr1Data.SubsetByRowIdValues(CommonItems(Gr1Data.RowIds, commonRows))
            Gr2Data.SubsetByRowIdValues(CommonItems(Gr2Data.RowIds, commonRows))
        End If

        Return (Gr1Data, Gr2Data)
    End Function

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim data As TwoGroupsPairedData = Nothing, errText As String = String.Empty
            Dim D1 As (DataObj, DataObj) = Nothing 'for Hoteling's T
            'Validate Inputs
            If Me.checkInputs() Then Exit Sub

            'Get Data
            If Me.Text = "Hotelling's T-Squared Test" Then
                'We have two multicolumn inptus
                D1 = Me.getDataHotteling(errText)
            ElseIf Me.Text = "Cohen's / Weighted Kappa" Then
                ' Kappa loads its own categorical data inside RunCohensKappa().
            Else
                data = Me.getData(errText)
            End If

            If errText <> String.Empty Then
                MsgBox(errText, vbExclamation)
                Exit Sub
            End If

            If Me.Text = "Wilcoxon Signed Rank Test" Then
                Me.RunWilcoxon(data)
            ElseIf Me.Text = "Spearman Rank Correlation" Then
                Me.RunSpearman(data)
            ElseIf Me.Text = "Kendall's Rank Correlation" Then
                Me.RunKendall(data)
            ElseIf Me.Text = "Theil-Sen Simple Regression" Then
                Me.RunTheilSen(data)
            ElseIf Me.Text = "Paired T-test" Then
                Me.RunPairedTtest(data)
            ElseIf Me.Text = "Hotelling's T-Squared Test" Then
                Me.RunHotteling(D1.Item1, D1.Item2)
            ElseIf Me.Text = "Deming Regression" Then
                Me.RunDeming(data)
            ElseIf Me.Text = "Lin's Concordance Correlation Coefficient" Then
                Me.RunLinsCCC(data)
            ElseIf Me.Text = "Cohen's / Weighted Kappa" Then
                Me.RunCohensKappa()
            End If
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunCohensKappa()
        Dim errText As String = String.Empty
        Dim catData = Me.getPairedCategoricalData(errText)
        If errText <> String.Empty Then
            MsgBox(errText, vbExclamation)
            Exit Sub
        End If

        Dim WriteRes As New WriteResults
        Dim tt = New Agreement.WeightedKappaAgreement(catData.x, catData.y, catData.name1, catData.name2)

        Dim opts As New Agreement.KappaOptions With {
                .Alpha = CDbl(Me.spinBtnAlphaKappa.Value),
                .BootstrapReplicates = CInt(Me.spinBtnBootstrapReplicatesKappa.Value)
            }

        Select Case Me.cmbWeightingSchemeKappa.SelectedIndex
            Case 0 : opts.Weighting = Agreement.KappaWeightingScheme.Unweighted
            Case 1 : opts.Weighting = Agreement.KappaWeightingScheme.Linear
            Case 2 : opts.Weighting = Agreement.KappaWeightingScheme.Quadratic
            Case 3 : opts.Weighting = Agreement.KappaWeightingScheme.CicchettiAllison
            Case 4 : opts.Weighting = Agreement.KappaWeightingScheme.FleissCohen
            Case Else : opts.Weighting = Agreement.KappaWeightingScheme.Unweighted
        End Select

        If Me.optKappaAnalytical.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.Analytical
        ElseIf Me.optKappaJackknife.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.Jackknife
        ElseIf Me.optKappaBootstrapPercentile.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.BootstrapPercentile
        ElseIf Me.optKappaBootstrapBCa.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.BootstrapBCa
        Else
            opts.CiMethod = Agreement.AgreementCiMethod.Analytical
        End If

        Try
            tt.Options = opts

            Dim useResampling As Boolean = (opts.CiMethod = Agreement.AgreementCiMethod.BootstrapPercentile OrElse opts.CiMethod = Agreement.AgreementCiMethod.BootstrapBCa OrElse
                               opts.CiMethod = Agreement.AgreementCiMethod.Jackknife)
            If useResampling Then
                Me.progressBarExactCalc.Visible = True
                Me.progressBarExactCalc.Value = 0
                tt.Fit(Me.progressBarExactCalc)
            Else
                Me.progressBarExactCalc.Visible = False
                tt.Fit()
            End If

            Dim res = tt.wrapResults()
            WriteRes = GetResultWriter()
            Dim rr = New ProcessListofResultTables(res)
            Dim totrows As Integer = rr.TotRows + res.Count - 1
            Dim totcols As Integer = rr.TotCols

            If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
                If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                    Exit Sub
                End If
            End If

            rr.writeToSheet(WriteRes, True)
        Finally
            Me.progressBarExactCalc.Value = 0
            Me.progressBarExactCalc.Visible = False
        End Try
    End Sub

    Private Sub RunLinsCCC(data As TwoGroupsPairedData)
        Dim WriteRes As New WriteResults

        Dim tt = New Agreement.LinConcordanceCorrelation(
        Matrix.GetColumnFrom2Darray(data.X, 0),
        Matrix.GetColumnFrom2Darray(data.X, 1),
        data.name1,
        data.name2)

        Dim opts As New Agreement.LinConcordanceOptions With {
            .Alpha = CDbl(Me.spinBtnAlphaLinCCC.Value),
            .BootstrapReplicates = CInt(Me.spinBtnBootstrapReplicatesLinCCC.Value),
            .NullConcordance = CDbl(Me.spinBtnNullConcordanceLinCCC.Value)
        }

        If Me.optLinCCCAnalytical.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.Analytical
        ElseIf Me.optLinCCCBootstrapPercentile.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.BootstrapPercentile
        ElseIf Me.optLinCCCBootstrapBCa.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.BootstrapBCa
        Else
            opts.CiMethod = Agreement.AgreementCiMethod.Analytical
        End If

        Dim useBootstrap As Boolean = (opts.CiMethod = Agreement.AgreementCiMethod.BootstrapPercentile OrElse opts.CiMethod = Agreement.AgreementCiMethod.BootstrapBCa)

        tt.Options = opts

        Try
            If useBootstrap Then
                Me.progressBarExactCalc.Visible = True
                Me.progressBarExactCalc.Value = 0
                tt.Fit(Me.progressBarExactCalc)
            Else
                Me.progressBarExactCalc.Visible = False
                tt.Fit()
            End If

            Dim res = tt.wrapResults()

            WriteRes = GetResultWriter()
            Dim rr = New ProcessListofResultTables(res)
            Dim totrows As Integer = rr.TotRows + res.Count - 1
            Dim totcols As Integer = rr.TotCols

            If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
                If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                    Exit Sub
                End If
            End If

            rr.writeToSheet(WriteRes, True)
            tt.AddPlot(WriteRes.ws)
        Finally
            Me.progressBarExactCalc.Value = 0
            Me.progressBarExactCalc.Visible = False
        End Try
    End Sub

    Private Sub RunDeming(data As TwoGroupsPairedData)
        Dim WriteRes = New WriteResults
        Dim tt = New Agreement.WeightedDemingRegression(Matrix.GetColumnFrom2Darray(data.X, 0), Matrix.GetColumnFrom2Darray(data.X, 1), data.name1, data.name2)
        Dim opts As New Agreement.DemingOptions With {
                .Alpha = CDbl(Me.spinBtnAlphaDeming.Value),
                .BootstrapReplicates = CInt(Me.spinBtnBootstrapReplicatesDeming.Value),
                .FitIntercept = Me.ckDemingFitIntercept.Checked
            }

        Select Case Me.cmbDemingVarianceModel.SelectedIndex
            Case 1
                opts.VarianceModel = Agreement.DemingVarianceModel.ConstantCV
                opts.CVx = CDbl(Me.spinBtnDemingCVx.Value)
                opts.CVy = CDbl(Me.spinBtnDemingCVy.Value)
            Case 2
                opts.VarianceModel = Agreement.DemingVarianceModel.KnownPointwiseSD
                Dim errText As String = String.Empty
                opts.SDx = Me.GetDemingStandardDeviationVector(Me.RefEditDemingSDx, data.X.GetLength(0), "SDx", errText)
                If errText <> String.Empty Then
                    MsgBox(errText, vbExclamation)
                    Me.TabMultipage.SelectedTab = Me.TabPageOptionsDeming
                    Exit Sub
                End If
                opts.SDy = Me.GetDemingStandardDeviationVector(Me.RefEditDemingSDy, data.X.GetLength(0), "SDy", errText)
                If errText <> String.Empty Then
                    MsgBox(errText, vbExclamation)
                    Me.TabMultipage.SelectedTab = Me.TabPageOptionsDeming
                    Exit Sub
                End If
            Case Else
                opts.VarianceModel = Agreement.DemingVarianceModel.ConstantLambda
                opts.Lambda = CDbl(Me.spinBtnErrorRatio.Value)
        End Select

        If Me.optDemingBootstrapPercentile.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.BootstrapPercentile
        ElseIf Me.optDemingBootstrapBCa.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.BootstrapBCa
        ElseIf Me.optAnalyticalClosedForm.Checked OrElse Me.optAnalyticalLinnet.Checked Then
            opts.CiMethod = Agreement.AgreementCiMethod.Analytical
        Else
            opts.CiMethod = Agreement.AgreementCiMethod.Jackknife
        End If

        tt.Options = opts

        Dim useBootstrap As Boolean = (opts.CiMethod = Agreement.AgreementCiMethod.BootstrapPercentile OrElse opts.CiMethod = Agreement.AgreementCiMethod.BootstrapBCa)

        Try
            If opts.CiMethod = Agreement.AgreementCiMethod.Analytical AndAlso Me.optAnalyticalLinnet.Checked Then
                tt.DemingAnalyticalCI_MCR()
            ElseIf opts.CiMethod = Agreement.AgreementCiMethod.Analytical Then
                tt.DemingAnalyticalCI()
            ElseIf opts.CiMethod = Agreement.AgreementCiMethod.Jackknife Then
                tt.FitJackknifeCI()
            ElseIf useBootstrap Then
                Me.progressBarExactCalc.Visible = True
                Me.progressBarExactCalc.Value = 0
                tt.Fit(Me.progressBarExactCalc)
            Else
                Me.progressBarExactCalc.Visible = False
                tt.Fit()
            End If

            Dim res = tt.wrapResults()

            'Dump outputs
            WriteRes = GetResultWriter() 'pass just table from the main test output
            Dim rr = New ProcessListofResultTables(res)
            Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
            Dim totcols As Integer = rr.TotCols
            If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
                If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                    Exit Sub
                End If
            End If

            rr.writeToSheet(WriteRes, True)
            tt.AddPlot(WriteRes.ws)
        Finally
            Me.progressBarExactCalc.Value = 0
            Me.progressBarExactCalc.Visible = False
        End Try
    End Sub

    Private Sub RunHotteling(D1 As DataObj, D2 As DataObj)
        Dim res As List(Of ResultTable) = Nothing

        If Me.optPaired.Checked Then
            Dim HT2 = New parametric.HotelingsT_paired(D1.DataDbl, D2.DataDbl, D1.varNames)
            HT2.calculate()
            HT2.CI(Me.spinBtnAlphaHottelings.Value)
            res = HT2.wrapResults()

        ElseIf Me.optSingle.Checked Then
            Dim HT2 = New parametric.HotelingsT_single(D1.DataDbl, Matrix.rowFromArray(D2.DataDbl, 0), D1.varNames)
            HT2.calculate()
            HT2.CI(Me.spinBtnAlphaHottelings.Value)
            res = HT2.wrapResults()

        ElseIf Me.optIndependent.Checked Then
            Dim HT2 = New parametric.HotelingsT_independent(D1.DataDbl, D2.DataDbl, D1.varNames)

            'Box M test for equality of covariance matrices
            Dim tmp1(,) As Double = Matrix.MatCovar(D1.DataDbl)
            Dim tmp2(,) As Double = Matrix.MatCovar(D2.DataDbl)
            Dim p As Integer = UBound(tmp1, 1) + 1
            Dim CovMats(1, p - 1, p - 1) As Double
            For i = 0 To p - 1
                For j = 0 To p - 1
                    CovMats(0, i, j) = tmp1(i, j)
                    CovMats(1, i, j) = tmp2(i, j)
                Next
            Next

            Dim BoxStat As TestResult = assumptions.BoxM(CovMats, {D1.nRows, D2.nRows})
            Dim bEqCov As Boolean = If(BoxStat.Pvalue > AppGlobals.DefaultAlpha, True, False)
            HT2.calculate(True)
            HT2.CI(Me.spinBtnAlphaHottelings.Value)
            res = HT2.wrapResults()

            Dim t As New ResultTable
            t.AddHeaderTopRow({"Box 's test of Equality of Cov. Mat.", ""})
            t.AddHeaderLeftRow({"M", "Two-sided p-value"})
            t.SetBody({{BoxStat.TestStatistics1}, {BoxStat.Pvalue}})
            res.Add(t)
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)
    End Sub


    Private Sub RunPairedTtest(data As TwoGroupsPairedData)
        Dim WriteRes = New WriteResults
        Dim tt = New parametric.PairedTtest(data.X, {data.name1, data.name2})
        tt.compute()
        Dim res = tt.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then Res.Add(Me.ComputeDescriptiveStatistics(data, tt.Differences))

        'Dump outputs
        WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunTheilSen(data As TwoGroupsPairedData)
        Dim WriteRes = New WriteResults
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaGlobal.Value)
        Dim ts = New nonparametric.TheilSen(data.X, {data.name1, data.name2})
        ts.compute(alphaValue)
        Dim res = ts.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStatistics(data))

        'Dump outputs
        WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)
        ts.AddPlot(WriteRes.ws)
    End Sub

    Private Sub RunKendall(data As TwoGroupsPairedData)
        Dim WriteRes = New WriteResults
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaGlobal.Value)
        Dim kendall = New nonparametric.KendallsTau(Matrix.GetColumnFrom2Darray(data.X, 0), Matrix.GetColumnFrom2Darray(data.X, 1), data.name1, data.name2)
        kendall.compute(Me.progressBarExactCalc, alphaValue)
        Dim res = kendall.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then Res.Add(Me.ComputeDescriptiveStatistics(data))

        'Dump outputs
        WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(Res)
        Dim totrows As Integer = rr.TotRows + Res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunSpearman(data As TwoGroupsPairedData)
        Dim WriteRes = New WriteResults
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaGlobal.Value)

        Dim spearman = New nonparametric.SpearmanRho(Matrix.GetColumnFrom2Darray(data.X, 0), Matrix.GetColumnFrom2Darray(data.X, 1), data.name1, data.name2)
        spearman.Compute(Me.progressBarExactCalc, alphaValue)
        Dim res = spearman.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStatistics(data))

        'Dump outputs
        WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunWilcoxon(data As TwoGroupsPairedData)
        Dim WriteRes = New WriteResults
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaGlobal.Value)

        'Compute test
        Dim Wilcoxon = New nonparametric.WilcoxonTest(data.X, data.name1, data.name2)
        Wilcoxon.Compute(Me.progressBarExactCalc)
        Wilcoxon.ComputeShift(alphaValue)
        If Me.ckSignTest.Checked Then Wilcoxon.signTest()

        Dim res = Wilcoxon.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStatistics(data, Wilcoxon.Differences))

        'Dump outputs
        WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If CheckRefEdit(Me.RefEdit1.Address, If(Me.Text = "Hotelling's T-Squared Test", False, True)) Then
            Me.TabMultipage.SelectedIndex = 0
            RefEditReset(Me.RefEdit1)
            bOut = True
        End If
        If CheckRefEdit(Me.RefEdit2.Address, If(Me.Text = "Hotelling's T-Squared Test", False, True)) Then
            Me.TabMultipage.SelectedIndex = 0
            RefEditReset(Me.RefEdit2)
            bOut = True
        End If
        If Me.optOutputRange.Checked Then
            If CheckRefEdit(Me.RefEditOutput.Address) Then
                Me.TabMultipage.SelectedIndex = 0
                RefEditReset(Me.RefEditOutput)
                bOut = True
            End If
        End If
        Return bOut
    End Function

    Private Function ComputeDescriptiveStatistics(data As TwoGroupsPairedData, Optional diffs() As Double = Nothing) As ResultTable
        Dim descTable = New ResultTable
        Dim ds1 = New DescriptiveStat(Matrix.GetColumnFrom2Darray(data.X, 0))
        Dim ds2 = New DescriptiveStat(Matrix.GetColumnFrom2Darray(data.X, 1))
        ds1.compute()
        ds2.compute()

        If diffs IsNot Nothing Then
            Dim dsDiff = New DescriptiveStat(diffs)
            dsDiff.compute()
            descTable.SetBody(Matrix.VerticalStackArrays(Matrix.VerticalStackArrays(ds1.wrapSelf(True),
                                                                      ds2.wrapSelf(False)),
                                                  dsDiff.wrapSelf(False)))
            descTable.AddHeaderTopRow({"", data.name1, data.name2, "Difference: " & data.name1 & " - " & data.name2})
        Else
            descTable.SetBody(Matrix.VerticalStackArrays(ds1.wrapSelf(True), ds2.wrapSelf(False)))
            descTable.AddHeaderTopRow({"", data.name1, data.name2})
        End If
        descTable.AddTitle("Descriptive Statistics")

        Return descTable
    End Function

    Private Function GetResultWriter() As WriteResults
        Dim WriteRes = New WriteResults, rRange As Range
        If Me.optWorkbook.Checked Then
            WriteRes.wb = AppGlobals.app.Workbooks.Add()
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        ElseIf Me.optWorksheet.Checked Then
            WriteRes.wb = AppGlobals.app.ActiveWorkbook
            WriteRes.wb.Worksheets.Add()
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        Else
            WriteRes.wb = AppGlobals.app.ActiveWorkbook
            WriteRes.ws = WorksheetFromRefAdress(Me.RefEditOutput.Address)
            rRange = WriteRes.ws.Range(Me.RefEditOutput.Address)
            WriteRes.setRowPointer(rRange.Row)
            WriteRes.setColumnPointer(rRange.Column)
        End If

        Return WriteRes
    End Function

    Private Function GetDemingStandardDeviationVector(refEdit As Excel2007RefEdit,
                                                      expectedLength As Integer,
                                                      labelText As String,
                                                      ByRef errText As String) As Double()
        errText = String.Empty
        If String.IsNullOrWhiteSpace(refEdit.Address) Then
            errText = $"Please specify the {labelText} range on the Options tab."
            Return Nothing
        End If

        Try
            Dim sdData As New DataObj
            Dim refText As String = prepareRef2D(refEdit.Address, refEdit.ExcelWorkBook)
            sdData.DataInport(refText, True)

            If sdData.bZeroValid Then
                errText = $"The {labelText} range does not contain valid numeric data."
                Return Nothing
            End If
            If sdData.nCols <> 1 Then
                errText = $"The {labelText} range must contain exactly one numeric column."
                Return Nothing
            End If

            Dim vals As Double() = Matrix.GetColumnFrom2Darray(sdData.DataDbl, 0)
            If vals.Length <> expectedLength Then
                errText = $"The {labelText} range must contain the same number of rows as the paired X/Y input columns ({expectedLength})."
                Return Nothing
            End If

            Return vals
        Catch ex As Exception
            errText = $"Unable to import the {labelText} range. {ex.Message}"
            Return Nothing
        End Try
    End Function

    Private Sub SetDemingExtendedControlsVisible(isVisible As Boolean)
        Me.lblDemingVarianceModel.Visible = isVisible
        Me.cmbDemingVarianceModel.Visible = isVisible
        Me.ckDemingFitIntercept.Visible = isVisible
        Me.optDemingBootstrapPercentile.Visible = isVisible
        Me.optDemingBootstrapBCa.Visible = isVisible
        Me.lblBootstrapReplicatesDeming.Visible = isVisible
        Me.spinBtnBootstrapReplicatesDeming.Visible = isVisible
        Me.lblDemingCVx.Visible = isVisible
        Me.spinBtnDemingCVx.Visible = isVisible
        Me.lblDemingCVy.Visible = isVisible
        Me.spinBtnDemingCVy.Visible = isVisible
        Me.lblDemingSDx.Visible = isVisible
        Me.RefEditDemingSDx.Visible = isVisible
        Me.lblDemingSDy.Visible = isVisible
        Me.RefEditDemingSDy.Visible = isVisible
    End Sub

    Private Sub ApplyDemingControlState()
        If Me.Text <> "Deming Regression" Then Exit Sub

        Dim modelIndex As Integer = Me.cmbDemingVarianceModel.SelectedIndex
        If modelIndex < 0 Then modelIndex = 0

        Dim useBootstrap As Boolean = Me.optDemingBootstrapPercentile.Checked OrElse Me.optDemingBootstrapBCa.Checked
        Me.lblBootstrapReplicatesDeming.Enabled = useBootstrap
        Me.spinBtnBootstrapReplicatesDeming.Enabled = useBootstrap

        Dim allowAnalytical As Boolean = (modelIndex = 0 AndAlso Me.ckDemingFitIntercept.Checked)
        Me.optAnalyticalClosedForm.Enabled = allowAnalytical
        Me.optAnalyticalLinnet.Enabled = allowAnalytical
        If Not allowAnalytical AndAlso (Me.optAnalyticalClosedForm.Checked OrElse Me.optAnalyticalLinnet.Checked) Then
            Me.optJackknife.Checked = True
        End If

        Me.lblErrorRatio.Enabled = (modelIndex = 0)
        Me.spinBtnErrorRatio.Enabled = (modelIndex = 0)

        Dim showCV As Boolean = (modelIndex = 1)
        Me.lblDemingCVx.Enabled = showCV
        Me.spinBtnDemingCVx.Enabled = showCV
        Me.lblDemingCVy.Enabled = showCV
        Me.spinBtnDemingCVy.Enabled = showCV

        Dim showPointwise As Boolean = (modelIndex = 2)
        Me.lblDemingSDx.Enabled = showPointwise
        Me.RefEditDemingSDx.Enabled = showPointwise
        Me.lblDemingSDy.Enabled = showPointwise
        Me.RefEditDemingSDy.Enabled = showPointwise
    End Sub

    Private Sub DemingControlStateChanged(sender As Object, e As System.EventArgs) _
    Handles cmbDemingVarianceModel.SelectedIndexChanged,
            ckDemingFitIntercept.CheckedChanged,
            optJackknife.CheckedChanged,
            optAnalyticalLinnet.CheckedChanged,
            optAnalyticalClosedForm.CheckedChanged,
            optDemingBootstrapPercentile.CheckedChanged,
            optDemingBootstrapBCa.CheckedChanged
        Me.ApplyDemingControlState()
    End Sub

    Private Sub ApplyLinCCCControlState()
        Dim useBootstrap As Boolean = Me.optLinCCCBootstrapPercentile.Checked OrElse Me.optLinCCCBootstrapBCa.Checked
        Me.lblBootstrapReplicatesLinCCC.Enabled = useBootstrap
        Me.spinBtnBootstrapReplicatesLinCCC.Enabled = useBootstrap
    End Sub

    Private Sub optLinCCCAnalytical_CheckedChanged(sender As Object, e As System.EventArgs) _
    Handles optLinCCCAnalytical.CheckedChanged, optLinCCCBootstrapPercentile.CheckedChanged, optLinCCCBootstrapBCa.CheckedChanged

        Me.ApplyLinCCCControlState()
    End Sub

    Private Sub ApplyKappaControlState()
        Dim useBootstrap As Boolean = Me.optKappaBootstrapPercentile.Checked OrElse Me.optKappaBootstrapBCa.Checked
        Me.lblBootstrapReplicatesKappa.Enabled = useBootstrap
        Me.spinBtnBootstrapReplicatesKappa.Enabled = useBootstrap
    End Sub

    Private Sub optKappaAnalytical_CheckedChanged(sender As Object, e As System.EventArgs) _
        Handles optKappaAnalytical.CheckedChanged, optKappaJackknife.CheckedChanged, optKappaBootstrapPercentile.CheckedChanged, optKappaBootstrapBCa.CheckedChanged

        Me.ApplyKappaControlState()
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

End Class