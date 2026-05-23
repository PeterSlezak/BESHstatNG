Option Explicit On
Imports System.IO
Imports System.Security.Cryptography
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop.Excel

Public Class Ui0OneRefeditMulticol
    Sub New(analysis As String)
        ' This call is required by the designer.
        InitializeComponent()

        Me.RefEdit1.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.Text = analysis
        Me.spinBtnAlphaICC.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlphaICC.Minimum, Me.spinBtnAlphaICC.Maximum)

        Me.TabPage_Options.Parent = Nothing
        Me.TabPage_OptionsRxC.Parent = Nothing
        Me.TabPage_OptionsICC.Parent = Nothing

        If Me.Text = "Friedman Test" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True

        ElseIf Me.Text = "One-Way Repeated-Measures ANOVA" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.grpRmANOVAsphericity.Visible = True
            Me.grpMCP.Visible = True

        ElseIf Me.Text = "Cochran's Q Test" Then
        ElseIf Me.Text = "RxC Table" Then
            Me.TabPage_OptionsRxC.Parent = Me.TabMultipage
            ' Reuse the existing alpha controls for ordinal-association confidence intervals.
            Me.TabPage_OptionsRxC.Controls.Add(Me.lblAlphaICC)
            Me.TabPage_OptionsRxC.Controls.Add(Me.spinBtnAlphaICC)
            Me.lblAlphaICC.Visible = True
            Me.spinBtnAlphaICC.Visible = True
            Me.lblAlphaICC.Location = New System.Drawing.Point(20, 132)
            Me.spinBtnAlphaICC.Location = New System.Drawing.Point(68, 130)

        ElseIf Me.Text = "Mantel-Haenszel Test" Then
            Me.TabPage_OptionsICC.Parent = Me.TabMultipage
            Me.grpICCtype.Visible = False
            Me.ckRepeatabilityCoefficient.Visible = False
            Me.lblAlphaICC.Visible = True
            Me.spinBtnAlphaICC.Visible = True
            Me.lblAlphaICC.Location = New System.Drawing.Point(20, 20)
            Me.spinBtnAlphaICC.Location = New System.Drawing.Point(68, 18)

        ElseIf Me.Text = "Skillings-Mack Test" Then
        ElseIf Me.Text = "Correspondence Analysis" Then
            Me.ckLabels.Visible = True

        ElseIf Me.Text = "Intraclass Correlation Coefficients" Then
            Me.TabPage_OptionsICC.Parent = Me.TabMultipage

        End If
        Me.RefEdit1.txtAddress.Select()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data1------------------------------
        If CheckRefEdit(Me.RefEdit1.Address) Then
            Me.TabMultipage.SelectedIndex = 0
            RefEditReset(Me.RefEdit1)
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

    Private Function getDataMultipleGroups(ByRef strErr As String) As MultiGroupsPairedData
        If Me.Text <> "Skillings-Mack Test" Then
            Dim out = New MultiGroupsPairedData
            Dim columData = New DataObj
            Dim ref As String = prepareRef2D(Me.RefEdit1.Address, Me.RefEdit1.ExcelWorkBook)

            ExcelDnaDataImporter.ImportInto(columData, ref, True)
            out.X = columData.DataDbl()
            out.varNames = columData.varNames

            Return out
        Else
            Return Nothing
        End If
    End Function

    Private Function getCAdata(ByRef strErr As String) As DataObj
        Dim out = New MultiGroupsPairedData
        Dim columData = New DataObj
        Dim ref As String = prepareRef2D(Me.RefEdit1.Address, Me.RefEdit1.ExcelWorkBook)

        If Me.ckLabels.Checked Then
            ExcelDnaDataImporter.ImportInto(columData, ref, True, 0) 'first column will contain row labels
        Else
            ExcelDnaDataImporter.ImportInto(columData, ref, True)
        End If

        Return columData
    End Function

    Private Function getDataICC11(ByRef strErr As String) As MultiGroupsUnpairedData

        Dim columData = New DataObj
        Dim ref As String = prepareRef2D(Me.RefEdit1.Address, Me.RefEdit1.ExcelWorkBook)
        Dim out = New MultiGroupsUnpairedData

        columData.bAllowMissing = True 'allow missing values
        ExcelDnaDataImporter.ImportInto(columData, ref, True)
        Dim tmp(,) As Double = columData.DataDbl()
        Dim g As Integer = tmp.GetLength(0)
        Dim t As Integer = tmp.GetLength(1)
        Dim outData(g - 1)() As Double

        For i = 0 To g - 1
            Dim tm(t - 1) As Double
            Dim k = 0
            For j = 0 To t - 1
                If Not Double.IsNaN(tmp(i, j)) Then
                    tm(k) = tmp(i, j)
                    k += 1
                End If
            Next
            ReDim Preserve tm(k - 1)
            outData(i) = tm
        Next

        out.X = outData
        Dim varnames(g - 1) As String
        out.varNames = varnames
        Return out
    End Function

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim errText As String = String.Empty
            Dim data As MultiGroupsPairedData = Nothing, CAdata As DataObj = Nothing

            'Validate Inputs
            If Me.checkInputs() Then Exit Sub

            'Get Data
            If Me.Text = "Correspondence Analysis" Then
                CAdata = getCAdata(errText)
            Else
                data = Me.getDataMultipleGroups(errText)
            End If
            If errText <> String.Empty Then
                MsgBox(errText, vbExclamation)
                Exit Sub
            End If

            If Me.Text = "Friedman Test" Then
                Me.RunFriedman(data)
            ElseIf Me.Text = "One-Way Repeated-Measures ANOVA" Then
                Me.Run1WayRmANOVA(data)
            ElseIf Me.Text = "Cochran's Q Test" Then
                Me.RunCochran(data)
            ElseIf Me.Text = "RxC Table" Then
                Me.RunRxC(data)
            ElseIf Me.Text = "Mantel-Haenszel Test" Then
                Me.RunMantelHaenszel(data)
            ElseIf Me.Text = "Skillings-Mack Test" Then
                Me.RunSkillingsMack()
            ElseIf Me.Text = "Correspondence Analysis" Then
                Me.RunCA(CAdata)
            ElseIf Me.Text = "Intraclass Correlation Coefficients" Then
                Me.RunICC()
            End If
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True)
        End Try
    End Sub



    Private Sub RunICC()
        Dim errText As String = String.Empty
        Dim icc As New Agreement.IntraclassCorrelation
        Dim iccNum As ConfidenceIntervalResult = Nothing
        Dim data1 As MultiGroupsUnpairedData = Nothing
        Dim data2 As MultiGroupsPairedData = Nothing

        If Me.optICC11.Checked Or Me.optICC1k.Checked Then
            data1 = getDataICC11(errText)
        Else
            data2 = Me.getDataMultipleGroups(errText)
        End If

        If errText <> String.Empty Then
            MsgBox(errText, vbExclamation)
            Exit Sub
        End If

        Dim strIccType As String = String.Empty
        If Me.optICC11.Checked Then
            iccNum = icc.ICC11(data1.X, Me.spinBtnAlphaICC.Value)
            If Me.ckRepeatabilityCoefficient.Checked Then icc.RepeatabilityCoefficient_OneWay(data1.X, False, Me.spinBtnAlphaICC.Value)
            strIccType = "ICC(1,1)"

        ElseIf Me.optICC1k.Checked Then
            iccNum = icc.ICC1k(data1.X, Me.spinBtnAlphaICC.Value)
            If Me.ckRepeatabilityCoefficient.Checked Then icc.RepeatabilityCoefficient_OneWay(data1.X, True, Me.spinBtnAlphaICC.Value)
            strIccType = "ICC(1,k)"

        ElseIf Me.optICC21.Checked Then
            iccNum = icc.ICC21(data2.X, Me.spinBtnAlphaICC.Value)
            If Me.ckRepeatabilityCoefficient.Checked Then icc.RepeatabilityCoefficient_TwoWay(data2.X, True, False, Me.spinBtnAlphaICC.Value)
            strIccType = "ICC(2,1)"

        ElseIf Me.optICC2k.Checked Then
            iccNum = icc.ICC2k(data2.X, Me.spinBtnAlphaICC.Value)
            If Me.ckRepeatabilityCoefficient.Checked Then icc.RepeatabilityCoefficient_TwoWay(data2.X, True, True, Me.spinBtnAlphaICC.Value)
            strIccType = $"ICC(2,{data2.X.GetLength(1)})"

        ElseIf Me.optICC31.Checked Then
            iccNum = icc.ICC31(data2.X, Me.spinBtnAlphaICC.Value)
            If Me.ckRepeatabilityCoefficient.Checked Then icc.RepeatabilityCoefficient_TwoWay(data2.X, False, False, Me.spinBtnAlphaICC.Value)
            strIccType = "ICC(3,1)"

        ElseIf Me.optICC3k.Checked Then
            iccNum = icc.ICC3k(data2.X, Me.spinBtnAlphaICC.Value)
            If Me.ckRepeatabilityCoefficient.Checked Then icc.RepeatabilityCoefficient_TwoWay(data2.X, False, True, Me.spinBtnAlphaICC.Value)
            strIccType = $"ICC(3,{data2.X.GetLength(1)})"

        End If

        Dim res = icc.wrapResults(iccNum, strIccType)

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

    Private Sub RunCA(data As DataObj)
        Dim ca As New Multivariate.CA, TableData(,) As Integer

        If Me.ckLabels.Checked Then
            Dim RowLabels() As String = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(data.FinalData, 0))
            ReDim TableData(data.nRows - 1, data.nCols - 2)
            For i = 0 To UBound(data.FinalData, 1)
                For j = 1 To UBound(data.FinalData, 2) '1st column is Row labels
                    TableData(i, j - 1) = CInt(data.FinalData(i, j))
                Next
            Next
            ca.data(TableData, RowLabels, data.varNames.Skip(1).ToArray())
            ca.Calculate()
        Else
            ReDim TableData(data.nRows - 1, data.nCols - 1)
            For i = 0 To UBound(data.FinalData, 1)
                For j = 0 To UBound(data.FinalData, 2) '1st column is Row labels
                    TableData(i, j - 1) = CInt(data.FinalData(i, j))
                Next
            Next

            ca.data(TableData)
            ca.Calculate()
        End If
        Dim res = ca.wrapResults()

        Dim t As New ResultTable
        t.AddTitle("Analyzed contingency table")
        t.SetBody(TableData)
        t.AddHeaderLeftRow(ca.rowNames)
        t.AddHeaderTopRow(ca.ColumNames)
        res.Insert(0, t)

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

        'Add Plots
        If Me.optWorkbook.Checked Then
            graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(ca, 0, True)
            graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(ca, 1, True)
            graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(ca, 0, False)
            graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(ca, 1, False)
            graphics.CorrespondenceAnalysisPlotExcel.CorrespondencePlot(ca)
        Else
            graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(ca, 0, True, WriteRes.ws)
            graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(ca, 1, True, WriteRes.ws)
            graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(ca, 0, False, WriteRes.ws)
            graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(ca, 1, False, WriteRes.ws)
            graphics.CorrespondenceAnalysisPlotExcel.CorrespondencePlot(ca, WriteRes.ws)
        End If
    End Sub

    Private Sub RunSkillingsMack()
        Dim res = New List(Of ResultTable), t = New ResultTable, tdata = New ResultTable
        Dim columData = New DataObj
        Dim ref As String = prepareRef2D(Me.RefEdit1.Address)
        Dim out = New MultiGroupsPairedData

        columData.bAllowMissing = True 'allow missing values
        ExcelDnaDataImporter.ImportInto(columData, ref, True)

        'analyzed table
        Dim head(UBound(columData.FinalData, 2)) As String
        head(0) = "Analyzed table"
        tdata.AddHeaderTopRow(head)
        tdata.SetBody(columData.FinalData)

        Dim sm = nonparametric.SkillingsMack(columData.DataDbl())
        t.AddHeaderTopRow({"Skillings-Mack", ""})
        t.SetBody({{"Test Statistic", sm.TestStatistics1}, {"two-sided P-value", sm.Pvalue}})
        res.Add(t)
        res.Add(tdata)

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

    Private Sub RunRxC(data As MultiGroupsPairedData)
        Dim tab = Matrix.Array2intArray(data.X)
        Dim res = New List(Of ResultTable), t = New ResultTable
        Dim head(UBound(tab, 2)) As String
        head(0) = "Analyzed table"
        t.AddHeaderTopRow(head)
        t.SetBody(tab)
        res.Add(t)

        'compute outputs
        Dim chi2indep = contingencytable.Chi2TESTindependence(tab)
        t = New ResultTable
        t.AddHeaderTopRow({"Pearson's Chi-squared Test", ""})
        t.SetBody({{"Chi-Square", chi2indep.Item1.TestStatistics1}, {"two-sided P-value", chi2indep.Item1.Pvalue}})
        res.Add(t)

        If Me.ckNominalAssociation.Checked Then
            t = New ResultTable
            t.AddHeaderTopRow({"Measures of Nominal Association", ""})
            t.SetBody({{"Cramer's V", chi2indep.Item2},
                           {"Pearson's contingency coefficient", chi2indep.Item3},
                           {"Phi", chi2indep.Item4}})
            res.Add(t)
        End If

        If Me.ckFFH.Checked Then
            Dim strP As String
            Try
                Dim fexact = New contingencytable.FisherExactEngine(tab)
                fexact.Run()
                strP = CStr(fexact.PValue)
            Catch
                strP = ".error. not possible to compute"
            End Try

            t = New ResultTable
            t.AddHeaderTopRow({"Fisher-Freeman-Halton Exact Test", ""})
            t.SetBody({{"two-sided P-value", strP}})
            res.Add(t)
        End If

        If Me.ckOrdinal.Checked Then
            Dim alphaValue As Double = CDbl(Me.spinBtnAlphaICC.Value)
            Dim ciLabel As String = $"{(1.0 - alphaValue) * 100.0:0.##}% CI"
            Dim ordinal = contingencytable.cTableORDINALassoc(tab, alphaValue)

            t = New ResultTable
            t.AddHeaderTopRow({"Measures of Ordinal Association", ""})
            t.SetBody({{"Kendall's tau-b", ordinal.Item1.TestStatistics1},
                       {"Std.Err.", ordinal.Item1.DF1},
                       {ciLabel, ordinal.Item1.strSpecialInformation},
                       {"two-sided P-value", ordinal.Item1.Pvalue},
                       {"Kendall's tau-c", ordinal.Item2.TestStatistics1},
                       {"Std.Err.", ordinal.Item2.DF1},
                       {ciLabel, ordinal.Item2.strSpecialInformation},
                       {"two-sided P-value", ordinal.Item2.Pvalue},
                       {"Goodman-Kruskal's Gamma", ordinal.Item3.TestStatistics1},
                       {"Std.Err.", ordinal.Item3.DF1},
                       {ciLabel, ordinal.Item3.strSpecialInformation},
                       {"two-sided P-value", ordinal.Item3.Pvalue},
                       {"Somers' D (columns as dependent var.)", ordinal.Item4.TestStatistics1},
                       {"Std.Err.", ordinal.Item4.DF1},
                       {ciLabel, ordinal.Item4.strSpecialInformation},
                       {"two-sided P-value", ordinal.Item4.Pvalue}})
            res.Add(t)
        End If

        If Me.ckCochranArmitage.Checked Then
            Dim Cochran As TestResult = Nothing
            '1st chcek for proper table dimensions.
            'One of the dimensions have to be 2 and table must be larger then 2x2
            Dim rows As Integer = UBound(tab, 1) + 1
            Dim cols As Integer = UBound(tab, 2) + 1
            If Not ((rows = 2) Or (cols = 2)) Or (rows + cols < 5) Then
                MsgBox("Cannot compute Cochran-Armitage test for linear trend because of inapropriate table dimensions.", vbOKOnly, "Cochran-Armitage test")
            ElseIf rows = 2 Then
                Cochran = contingencytable.CochranArmitage(Matrix.Array2intArray(Matrix.trans(data.X)))
            Else
                Cochran = contingencytable.CochranArmitage(tab)
            End If

            If Cochran IsNot Nothing Then
                t = New ResultTable
                t.AddHeaderTopRow({"Cochran-Armitage Test for Linear Trend", ""})
                t.SetBody({{"Chi2 for Linear Trend", Cochran.TestStatistics1},
                       {"two-sided P-value for Linear Trend", Cochran.Pvalue},
                       {"Chi2 for Departure from Linear Trend", Cochran.TestStatistics2},
                       {"two-sided P-value for Linear Trend", Cochran.Pvalue2}})
                res.Add(t)
            End If
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

    Private Sub RunMantelHaenszel(data As MultiGroupsPairedData)
        Dim res = New List(Of ResultTable)
        Dim rowsNo As Integer = UBound(data.X, 1) + 1
        'check validity of the input
        If UBound(data.X, 2) <> 1 Or (rowsNo Mod 2 <> 0) Then
            AppInfrastructure.CoreServices.Log($"Wrong dimensions of the input table. {Matrix.array2str(data.X)}", AppInfrastructure.LogMsgType.Warn)
            MsgBox("Wrong dimensions of the input table.", vbOKOnly, AppGlobals.gsAPP_TITLE)
            Exit Sub
        End If

        Dim t = New ResultTable
        t.SetBody(Matrix.Array2intArray(data.X))
        t.AddHeaderTopRow({"Analyzed contingency tables", ""})
        res.Add(t)

        t = New ResultTable
        Dim alphaValue As Double = CDbl(Me.spinBtnAlphaICC.Value)
        Dim mh = contingencytable.MantelHaenszel(data.X, alphaValue)
        t.SetBody({{"Chi-square", mh.Item1.TestStatistics1},
                    {"two-sided P-value", mh.Item1.Pvalue},
                    {"pooled Or", mh.Item2.Estimate},
                    {mh.Item2.CIlabel, mh.Item2.strConfidenceInterval(CIformat.LL_to_UL)}})
        t.AddHeaderTopRow({"Mantel-Haenszel test", ""})
        res.Add(t)

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

    Private Sub RunCochran(data As MultiGroupsPairedData)
        Dim strErr As String = Nothing
        Dim prcRes = New ResultTable
        Dim WriteRes = New ExcelDnaResultWriter
        Dim res = New List(Of ResultTable)

        'check if data1 are 1/0
        Dim NoColumns As Integer = UBound(data.X, 2) + 1
        Dim NoBlocks As Integer = UBound(data.X, 1) + 1
        Dim ar1Counts(NoColumns - 1) As Integer, ar1Per(NoColumns - 1, 0) As Object
        'check if we have binary data1
        For i = 0 To NoBlocks - 1
            For j = 0 To NoColumns - 1
                If Not (data.X(i, j) = 1 Or data.X(i, j) = 0) Then
                    strErr = "ThenInput data1 must be 1/0 but value =" & CStr(data.X(i, j)) & " was observed. Please select 1/0 data1 only."
                    Exit For
                End If
                If strErr <> String.Empty Then Exit For
                ar1Counts(j) += data.X(i, j)
            Next j
        Next i

        If strErr <> String.Empty Then
            MsgBox(strErr, vbExclamation, AppGlobals.gsAPP_TITLE)
            Exit Sub
        End If

        'compute cochran q
        Dim F = New nonparametric.Friedman(data.X, data.varNames)
        Dim tr = F.compute()
        Dim t = New ResultTable
        t.SetBody({{"Q", tr.TestStatistics1},
                   {"Two-sided p-value", tr.Pvalue}})
        t.AddHeaderTopRow({"Cochran's Q Test", ""})
        res.Add(t)

        'compute percentage of 1's
        For i = 0 To NoColumns - 1
            ar1Per(i, 0) = 100.0# * CDbl(ar1Counts(i)) / CDbl(NoBlocks)
        Next i
        prcRes.SetBody(ar1Per)
        prcRes.AddHeaderTopRow({"Sample", "Percent of 1's"})
        prcRes.AddHeaderLeftRow(data.varNames)
        res.Add(prcRes)

        'Dump outputs
        WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(Res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub Run1WayRmANOVA(data As MultiGroupsPairedData)
        Dim Mtest As TestResult, Mout As ResultTable
        Dim box As graphics.BoxPlot = Nothing

        Dim anova = New parametric.OneWayRmANOVA(data.X, data.varNames)
        anova.compute()
        If Me.ckGreenhouse.Checked Then anova.GreenhouseGeisser()
        If Me.ckHuyhn.Checked Then anova.HuyhnFeldt()
        If Me.ckTukey.Checked Then
            anova.TukeyKramerRM2() 'no sphericity asumption
            anova.Tukey()
        End If
        Dim res = anova.wrapResults()

        If Me.ckMauchly.Checked Then
            Mtest = assumptions.MauchlyTest(data.X)
            Mout = New ResultTable
            Mout.SetBody({{"Chi2", Mtest.TestStatistics1}, {"P-value", Mtest.Pvalue}})
            Mout.AddHeaderTopRow({"Mauchly 's Test of Sphericity", ""})
            res.Add(Mout)
        End If

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(ComputeDescriptiveStatistics(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
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

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Sub RunFriedman(data As MultiGroupsPairedData)
        Dim box As graphics.BoxPlot = Nothing

        'Compute test
        Dim F = New nonparametric.Friedman(data.X, data.varNames)
        F.compute()
        F.MCP()
        Dim res = F.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(ComputeDescriptiveStatistics(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
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

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Function ComputeDescriptiveStatistics(data As MultiGroupsPairedData) As ResultTable
        Dim descTable = New ResultTable
        Dim ds1 = New DescriptiveStat(Matrix.GetColumnFrom2Darray(data.X, 0))
        ds1.compute()
        Dim tableBody = ds1.wrapSelf(True)
        For i = 1 To UBound(data.X, 2)
            Dim ds2 = New DescriptiveStat(Matrix.GetColumnFrom2Darray(data.X, i))
            ds2.compute()
            tableBody = Matrix.VerticalStackArrays(tableBody, ds2.wrapSelf(False))
        Next
        descTable.SetBody(tableBody)
        descTable.AddTitle("Descriptive Statistics")
        descTable.AddHeaderTopRow(Matrix.ConcatArrays({""}, data.varNames))
        Return descTable
    End Function

    Private Function GetResultWriter() As ExcelDnaResultWriter
        Dim WriteRes = New ExcelDnaResultWriter, rRange As Range
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