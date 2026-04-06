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
        Me.Text = analysis
        Me.spinBtnAlphaDeming.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlphaDeming.Minimum, Me.spinBtnAlphaDeming.Maximum)
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)

        Me.TabPageOptionsHotteling.Parent = Nothing
        Me.TabPageOptions.Parent = Nothing
        If Me.Text = "Hotelling's T-Squared Test" Then
            Me.TabPageOptionsHotteling.Parent = Me.TabMultipage
            Me.grpCItype.Visible = False
            Me.lblAlpha.Visible = False
            Me.spinBtnAlphaDeming.Visible = False
            Me.lblErrorRatio.Visible = False
            Me.spinBtnErrorRatio.Visible = False

        ElseIf Me.Text = "Deming Regression" Then
            Me.TabPageOptions.Parent = Me.TabMultipage
            Me.lblRefedit1.Text = "Reference method (X)"
            Me.lblRefedit2.Text = "Test method (Y)"
            Me.ckSignTest.Visible = False
            Me.ckDescriptiveStatistics.Visible = False
            Me.grpCItype.Visible = True
            Me.lblAlpha.Visible = True
            Me.spinBtnAlphaDeming.Visible = True
            Me.lblErrorRatio.Visible = True
            Me.spinBtnErrorRatio.Visible = True

        ElseIf analysis = "Kendall's Rank Correlation" Or
               analysis = "Spearman Rank Correlation" Or
               analysis = "Theil-Sen Simple Regression" Then
            Me.TabPageOptions.Parent = Me.TabMultipage
            Me.ckSignTest.Visible = False
            Me.grpCItype.Visible = False
            Me.lblErrorRatio.Visible = False
            Me.spinBtnErrorRatio.Visible = False
            Me.lblAlpha.Visible = True
            Me.lblAlpha.Location = New System.Drawing.Point(19, 45)
            Me.spinBtnAlphaDeming.Visible = True
            Me.spinBtnAlphaDeming.Location = New System.Drawing.Point(67, 43)

        ElseIf analysis = "Wilcoxon Signed rank test" Then
            Me.TabPageOptions.Parent = Me.TabMultipage
            Me.ckSignTest.Visible = True
            Me.grpCItype.Visible = False
            Me.lblErrorRatio.Visible = False
            Me.spinBtnErrorRatio.Visible = False
            Me.lblAlpha.Visible = True
            Me.lblAlpha.Location = New System.Drawing.Point(19, 68)
            Me.spinBtnAlphaDeming.Visible = True
            Me.spinBtnAlphaDeming.Location = New System.Drawing.Point(67, 66)

        ElseIf Me.Text = "Paired T-test" Then
            Me.TabPageOptions.Parent = Me.TabMultipage
            Me.ckSignTest.Visible = False
            Me.grpCItype.Visible = False
            Me.lblErrorRatio.Visible = False
            Me.spinBtnErrorRatio.Visible = False
            Me.lblAlpha.Visible = False
            Me.spinBtnAlphaDeming.Visible = False

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
            End If
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunDeming(data As TwoGroupsPairedData)
        Dim WriteRes = New WriteResults
        Dim tt = New Agreement.DemingRegression(Matrix.GetColumnFrom2Darray(data.X, 0), Matrix.GetColumnFrom2Darray(data.X, 1), data.name1, data.name2)
        tt.Lambda = Me.spinBtnErrorRatio.Value
        tt.alpha = Me.spinBtnAlphaDeming.Value

        If Me.optAnalyticalLinnet.Checked Then
            tt.DemingAnalyticalCI_MCR()
        ElseIf Me.optJackknife.Checked Then
            tt.FitJackknifeCI()
        ElseIf Me.optAnalyticalClosedForm.Checked Then
            tt.DemingAnalyticalCI()
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
    End Sub

    Private Sub RunHotteling(D1 As DataObj, D2 As DataObj)
        Dim res As List(Of ResultTable) = Nothing

        If Me.optPaired.Checked Then
            Dim HT2 = New parametric.HotelingsT_paired(D1.DataDbl, D2.DataDbl, D1.varNames)
            HT2.calculate()
            HT2.CI(Me.spinBtnAlpha.Value)
            res = HT2.wrapResults()

        ElseIf Me.optSingle.checked Then
            Dim HT2 = New parametric.HotelingsT_single(D1.DataDbl, Matrix.rowFromArray(D2.DataDbl, 0), D1.varNames)
            HT2.calculate()
            HT2.CI(Me.spinBtnAlpha.Value)
            res = HT2.wrapResults()

        ElseIf Me.optIndependent.checked Then
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
            HT2.CI(Me.spinBtnAlpha.Value)
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
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaDeming.Value)
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
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaDeming.Value)
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
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaDeming.Value)

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
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaDeming.Value)

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